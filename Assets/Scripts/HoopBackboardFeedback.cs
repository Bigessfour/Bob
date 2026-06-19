using UnityEngine;

/// <summary>
/// Forwards backboard/rim clanks to ArcAcademyManager and flashes the backboard on hard hits.
/// </summary>
public class HoopBackboardFeedback : MonoBehaviour
{
    [SerializeField] private float flashDurationSeconds = 0.25f;
    [SerializeField] private float flashIntensity = 0.75f;

    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private Renderer backboardRenderer;
    private Color baseEmissive;
    private float flashTimer;

    private void Awake()
    {
        backboardRenderer = GetComponent<Renderer>();
        if (backboardRenderer != null && backboardRenderer.material.HasProperty(EmissiveColorId))
        {
            baseEmissive = backboardRenderer.material.GetColor(EmissiveColorId);
        }
    }

    private void Update()
    {
        if (flashTimer <= 0f || backboardRenderer == null)
        {
            return;
        }

        flashTimer -= Time.deltaTime;
        float t = flashTimer / flashDurationSeconds;
        backboardRenderer.material.SetColor(
            EmissiveColorId,
            Color.Lerp(baseEmissive, Color.white * flashIntensity, t));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<BobAgent>() == null)
        {
            return;
        }

        ArcAcademyManager.Instance?.NotifyBackboardHit(collision.relativeVelocity.magnitude);

        if (collision.relativeVelocity.magnitude > 1.5f && backboardRenderer != null)
        {
            flashTimer = flashDurationSeconds;
        }
    }
}
