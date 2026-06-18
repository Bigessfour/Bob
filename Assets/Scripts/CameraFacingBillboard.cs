using UnityEngine;

/// <summary>
/// Yaw-only billboard so TextMesh labels on the spawn pad face the main camera.
/// </summary>
public class CameraFacingBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void LateUpdate()
    {
        var cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 toCamera = cam.transform.position - transform.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(-toCamera.normalized, Vector3.up);
    }
}
