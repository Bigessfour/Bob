using UnityEngine;

/// <summary>
/// Subtle idle bob and emissive breathing while Bob waits on the spawn pad.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BobIdleAnimation : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float bobAmplitude = 0.04f;
    [SerializeField] private float bobSpeed = 2.2f;
    [SerializeField] private float swayDegrees = 3f;
    [SerializeField] private float groundedSpeed = 0.35f;
    [SerializeField] private float spawnRadius = 1.5f;

    private Rigidbody rb;
    private BobEntranceController entrance;
    private Renderer bodyRenderer;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Color baseEmissive;
    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");

    public void Wire(Transform spawn)
    {
        spawnPoint = spawn;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        entrance = GetComponent<BobEntranceController>();
        bodyRenderer = GetComponent<Renderer>();
        baseLocalPosition = transform.localPosition;
        baseLocalRotation = transform.localRotation;

        if (bodyRenderer != null && bodyRenderer.material.HasProperty(EmissiveColorId))
        {
            baseEmissive = bodyRenderer.material.GetColor(EmissiveColorId);
        }
    }

    private void Update()
    {
        if (!CanIdle())
        {
            return;
        }

        float t = Time.time * bobSpeed;
        Vector3 offset = new Vector3(0f, Mathf.Sin(t) * bobAmplitude, 0f);
        transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, Mathf.Sin(t * 0.7f) * swayDegrees, 0f);

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position + offset;
        }
        else
        {
            transform.localPosition = baseLocalPosition + offset;
        }

        if (bodyRenderer != null)
        {
            float breathe = 1f + Mathf.Sin(t * 1.4f) * 0.08f;
            bodyRenderer.material.SetColor(EmissiveColorId, baseEmissive * breathe);
        }
    }

    private bool CanIdle()
    {
        if (entrance != null && entrance.IsActive)
        {
            return false;
        }

        if (rb == null || rb.linearVelocity.magnitude > groundedSpeed)
        {
            return false;
        }

        if (spawnPoint != null)
        {
            Vector3 flat = transform.position - spawnPoint.position;
            flat.y = 0f;
            if (flat.magnitude > spawnRadius)
            {
                return false;
            }
        }

        return true;
    }
}
