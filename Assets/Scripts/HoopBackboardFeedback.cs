using UnityEngine;

/// <summary>
/// Forwards backboard/rim clanks to ArcAcademyManager and flashes the backboard on hard hits.
/// Uses unscaled time so the flash stays readable during 20× training time scale.
/// </summary>
public class HoopBackboardFeedback : MonoBehaviour
{
    // Wall-clock duration — Time.timeScale must not shrink this at 20× training.
    [SerializeField] private float flashDurationSeconds = 0.45f;
    // Peak HDRP emissive multiplier (paired with _EmissiveIntensity below).
    [SerializeField] private float flashIntensity = 4.5f;
    [SerializeField] private float minImpactSpeed = 1.5f;
    [SerializeField] private float maxImpactSpeed = 12f;

    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private static readonly int EmissiveIntensityId = Shader.PropertyToID("_EmissiveIntensity");
    private static readonly Color FlashTint = new(1f, 0.95f, 0.75f);

    private Renderer backboardRenderer;
    private Material backboardMaterial;
    private Color baseEmissive;
    private float baseEmissiveIntensity;
    private bool hasEmissiveIntensity;
    private float flashTimer;
    private float activeFlashPeak = 1f;

    private void Awake()
    {
        // Scene instances may still carry pre-v4.1 serialized defaults; lift them so
        // existing Backboard components get the stronger 20×-readable flash.
        flashDurationSeconds = Mathf.Max(flashDurationSeconds, 0.45f);
        flashIntensity = Mathf.Max(flashIntensity, 4.5f);

        backboardRenderer = GetComponent<Renderer>();
        if (backboardRenderer == null)
        {
            return;
        }

        // Instance material so flash does not mutate the shared gym-pro glass asset.
        backboardMaterial = backboardRenderer.material;
        if (backboardMaterial.HasProperty(EmissiveColorId))
        {
            baseEmissive = backboardMaterial.GetColor(EmissiveColorId);
        }

        hasEmissiveIntensity = backboardMaterial.HasProperty(EmissiveIntensityId);
        if (hasEmissiveIntensity)
        {
            baseEmissiveIntensity = backboardMaterial.GetFloat(EmissiveIntensityId);
        }
    }

    private void Update()
    {
        if (flashTimer <= 0f || backboardMaterial == null)
        {
            return;
        }

        flashTimer -= Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(flashTimer / flashDurationSeconds);
        // Ease-out so the hit pops hard then settles (readable in a single glance at high speed).
        float envelope = t * t;

        Color peak = FlashTint * (flashIntensity * activeFlashPeak);
        backboardMaterial.SetColor(EmissiveColorId, Color.Lerp(baseEmissive, peak, envelope));

        if (hasEmissiveIntensity)
        {
            // HDRP lit glass needs intensity boost; color alone is nearly invisible.
            float peakIntensity = baseEmissiveIntensity + flashIntensity * activeFlashPeak * 300f;
            backboardMaterial.SetFloat(
                EmissiveIntensityId,
                Mathf.Lerp(baseEmissiveIntensity, peakIntensity, envelope));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<BobAgent>() == null
            && collision.collider.GetComponent<SimpleBasketball>() == null)
        {
            return;
        }

        float impact = collision.relativeVelocity.magnitude;
        ArcAcademyManager.Instance?.NotifyBackboardHit(impact);

        if (impact > minImpactSpeed && backboardMaterial != null)
        {
            activeFlashPeak = Mathf.Clamp01(
                Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impact));
            // Soft floor so even light board taps still register visually.
            activeFlashPeak = Mathf.Max(0.35f, activeFlashPeak);
            flashTimer = flashDurationSeconds;
        }
    }
}
