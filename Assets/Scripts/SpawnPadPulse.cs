using UnityEngine;

/// <summary>
/// Sinusoidal emissive pulse on the spawn pad glow ring; extra burst when a basket is scored.
/// </summary>
public class SpawnPadPulse : MonoBehaviour
{
    [SerializeField] private Renderer glowRenderer;
    [SerializeField] private Color baseEmissiveColor = new(0.55f, 0.2f, 0.95f);
    [SerializeField] private float baseIntensity = 2.2f;
    [SerializeField] private float pulseAmplitude = 0.45f;
    [SerializeField] private float pulseSpeed = 1.8f;

    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private float scoreBurstTimer;

    public void TriggerScoreBurst()
    {
        scoreBurstTimer = 0.6f;
    }

    private void Awake()
    {
        if (glowRenderer == null)
        {
            var glow = transform.Find("SpawnPadGlow");
            if (glow != null)
            {
                glowRenderer = glow.GetComponent<Renderer>();
            }
        }
    }

    private void Update()
    {
        if (glowRenderer == null)
        {
            return;
        }

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        float burst = scoreBurstTimer > 0f ? 1.2f : 0f;
        scoreBurstTimer = Mathf.Max(0f, scoreBurstTimer - Time.deltaTime);

        float intensity = baseIntensity + pulse + burst;
        glowRenderer.material.SetColor(EmissiveColorId, baseEmissiveColor * intensity);
    }
}
