using UnityEngine;

/// <summary>
/// Subtle idle animation for static robotic ball-launcher assemblies in training bays and decorative stations.
/// </summary>
public class RoboticLauncherVisual : MonoBehaviour
{
    [SerializeField] private float armPitchDegrees = 4f;
    [SerializeField] private float armPitchHz = 0.35f;
    [SerializeField] private float rootYawDegrees = 2f;
    [SerializeField] private float rootYawHz = 0.12f;

    private Transform armTransform;
    private Quaternion armBaseLocalRotation;
    private Quaternion rootBaseLocalRotation;
    private float phaseOffset;

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

        if (armTransform != null)
        {
            float pitch = Mathf.Sin(t * armPitchHz * Mathf.PI * 2f) * armPitchDegrees;
            armTransform.localRotation = armBaseLocalRotation * Quaternion.Euler(pitch, 0f, 0f);
        }

        float yaw = Mathf.Sin(t * rootYawHz * Mathf.PI * 2f) * rootYawDegrees;
        transform.localRotation = rootBaseLocalRotation * Quaternion.Euler(0f, yaw, 0f);
    }
}
