using UnityEngine;

/// <summary>
/// Subtle idle animation for static robotic ball-launcher assemblies in training bays and decorative stations.
/// </summary>
public class RoboticLauncherVisual : MonoBehaviour
{
    [SerializeField] private float armPitchDegrees = 4f;
    [SerializeField] private float armPitchHz = 0.35f;
    [SerializeField] private float rootYawDegrees = 2.5f;
    [SerializeField] private float rootYawHz = 0.12f;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private float aimBlend = 0.08f;

    private Transform armTransform;
    private Quaternion armBaseLocalRotation;
    private Quaternion rootBaseLocalRotation;
    private float phaseOffset;

    public void SetAimTarget(Transform target)
    {
        aimTarget = target;
    }

    private void Awake()
    {
        armTransform = transform.Find("LauncherArm");
        if (armTransform != null)
        {
            armBaseLocalRotation = armTransform.localRotation;
        }

        rootBaseLocalRotation = transform.localRotation;
        phaseOffset = transform.position.sqrMagnitude * 0.17f;
    }

    private void Update()
    {
        float t = Time.time + phaseOffset;

        float pitch = 0f;
        float yaw = Mathf.Sin(t * rootYawHz * Mathf.PI * 2f) * rootYawDegrees;

        if (aimTarget != null)
        {
            Vector3 localTarget = transform.InverseTransformPoint(aimTarget.position);
            float aimPitch = Mathf.Atan2(localTarget.y, localTarget.z) * Mathf.Rad2Deg;
            float aimYaw = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            pitch = Mathf.Lerp(0f, aimPitch * 0.35f, aimBlend);
            yaw = Mathf.Lerp(yaw, aimYaw * 0.25f, aimBlend);
        }

        if (armTransform != null)
        {
            float swayPitch = Mathf.Sin(t * armPitchHz * Mathf.PI * 2f) * armPitchDegrees;
            armTransform.localRotation = armBaseLocalRotation * Quaternion.Euler(pitch + swayPitch, 0f, 0f);
        }

        transform.localRotation = rootBaseLocalRotation * Quaternion.Euler(0f, yaw, 0f);
    }
}
