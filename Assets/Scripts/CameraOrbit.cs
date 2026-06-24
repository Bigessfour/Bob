using UnityEngine;

/// <summary>
/// Simple orbit controller for the training default sideline view (CameraRig).
/// F1 resets to exact lab defaults. Mouse drag (right) orbits around the fixed look-at point.
/// Child "Main Camera" is kept at local identity so world pose always matches the rig at the documented position.
/// Keeps Bob + basketball + hoop framed; west-wall scoreboard remains visible within yaw/pitch clamps.
/// </summary>
public class CameraOrbit : MonoBehaviour
{
    [Header("Look-At Target (fixed pivot)")]
    public Vector3 lookAtPoint = new Vector3(0f, 2f, -4.5f);

    [Header("Default View (exact values from prompt / SimpleArcAcademyArena)")]
    public Vector3 defaultPosition = new Vector3(13f, 3.2f, -3.5f);
    public float defaultFov = 52f;

    [Header("Orbit Tuning")]
    public float lookSensitivity = 2.5f;
    public float minPitch = 2f;
    public float maxPitch = 55f;
    public float yawLimit = 30f; // +/- around default to keep subjects + scoreboard framed

    private Camera childCamera;
    private Transform childCamTransform;

    private float distance;
    private float yaw;
    private float pitch;
    private float defaultYaw;
    private float defaultPitch;

    private void Awake()
    {
        ResolveChildCamera();

        // Compute initial spherical coords from the desired default (used for reset + limits)
        ComputeSphericalFromOffset(defaultPosition - lookAtPoint, out defaultYaw, out defaultPitch, out distance);
        yaw = defaultYaw;
        pitch = defaultPitch;
    }

    private void Start()
    {
        // Ensure we start exactly on the rational default even if transform was tweaked in scene
        ResetToDefault();
    }

    private void ResolveChildCamera()
    {
        if (childCamera != null)
        {
            return;
        }

        var child = transform.Find("Main Camera");
        if (child != null)
        {
            childCamTransform = child;
            childCamera = child.GetComponent<Camera>();
        }
        else
        {
            // Fallback: first camera in children (for safety during incremental edits)
            childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
            {
                childCamTransform = childCamera.transform;
            }
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ResetToDefault();
        }

        // Right mouse drag for orbit (matches existing demo camera convention)
        if (Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            yaw += dx * lookSensitivity;
            pitch -= dy * lookSensitivity;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            yaw = Mathf.Clamp(yaw, defaultYaw - yawLimit, defaultYaw + yawLimit);

            ApplyOrbitTransform();
        }
    }

    private void LateUpdate()
    {
        // Keep child camera exactly at local identity so:
        // - rig reports the "at position (13,3.2,-3.5)" as requested
        // - Camera.main world values are authoritative for billboards / other readers
        if (childCamTransform != null)
        {
            childCamTransform.localPosition = Vector3.zero;
            childCamTransform.localRotation = Quaternion.identity;
        }
    }

    public void ResetToDefault()
    {
        ResolveChildCamera();

        // Exact values per prompt
        transform.position = defaultPosition;
        Vector3 dir = (lookAtPoint - defaultPosition).normalized;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        if (childCamera != null)
        {
            childCamera.fieldOfView = defaultFov;
        }

        // Recompute spherical for orbit state
        Vector3 offset = defaultPosition - lookAtPoint;
        ComputeSphericalFromOffset(offset, out yaw, out pitch, out distance);

        // Re-assert child local zero (defensive)
        if (childCamTransform != null)
        {
            childCamTransform.localPosition = Vector3.zero;
            childCamTransform.localRotation = Quaternion.identity;
        }
    }

    private void ApplyOrbitTransform()
    {
        ResolveChildCamera();

        // Rebuild position from spherical around the fixed lookAt
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rot * (Vector3.back * Mathf.Max(0.1f, distance));
        transform.position = lookAtPoint + offset;

        // Face the look-at point (keeps framing consistent)
        transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position, Vector3.up);

        if (childCamera != null)
        {
            childCamera.fieldOfView = defaultFov;
        }
    }

    private static void ComputeSphericalFromOffset(Vector3 offset, out float outYaw, out float outPitch, out float outDist)
    {
        outDist = offset.magnitude;
        if (outDist < 0.0001f)
        {
            outDist = 13f;
            outYaw = -90f; // roughly -x
            outPitch = 0f;
            return;
        }

        // ApplyOrbitTransform rebuilds position with Quaternion.Euler(pitch, yaw, 0) * Vector3.back.
        // Atan2(x, z) yields the yaw for a Vector3.forward basis, so subtract 180 degrees to
        // match the Vector3.back basis. Without this the camera flips to the opposite side of the
        // room on the first Update (pointing at the floor with the scoreboard behind it).
        outYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg - 180f;
        outPitch = Mathf.Asin(offset.y / outDist) * Mathf.Rad2Deg;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(lookAtPoint, 0.15f);
        Gizmos.DrawLine(transform.position, lookAtPoint);
    }
#endif
}
