using UnityEngine;

/// <summary>
/// Green arc preview on first ball launch each episode (simple arena sideline readability).
/// </summary>
[RequireComponent(typeof(BobAgent))]
public class BobShotArcPreview : MonoBehaviour
{
    [SerializeField] private BobAgent agent;
    [SerializeField] private int arcSegments = 24;
    [SerializeField] private float sampleDelta = 0.05f;
    [SerializeField] private float launchSpeedThreshold = 0.5f;
    [SerializeField] private float lineWidth = 0.04f;

    private LineRenderer lineRenderer;
    private bool previewShownThisEpisode;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<BobAgent>();
        }

        EnsureLineRenderer();
    }

    public void Bind(BobAgent bobAgent)
    {
        agent = bobAgent;
    }

    private void LateUpdate()
    {
        var rb = agent != null ? agent.ProjectileBody : null;
        if (rb == null)
        {
            ClearPreview();
            previewShownThisEpisode = false;
            return;
        }

        BasketballProjectileSetup.UpdateTrailEmit(rb);

        if (rb.linearVelocity.sqrMagnitude < 0.05f)
        {
            previewShownThisEpisode = false;
            ClearPreview();
            return;
        }

        if (!previewShownThisEpisode && rb.linearVelocity.sqrMagnitude > launchSpeedThreshold * launchSpeedThreshold)
        {
            ShowBallisticArc(rb.position, rb.linearVelocity);
            previewShownThisEpisode = true;
        }
    }

    private void ShowBallisticArc(Vector3 start, Vector3 velocity)
    {
        EnsureLineRenderer();
        lineRenderer.positionCount = arcSegments + 1;
        lineRenderer.enabled = true;

        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i * sampleDelta;
            Vector3 pos = start + velocity * t + 0.5f * Physics.gravity * (t * t);
            lineRenderer.SetPosition(i, pos);
        }
    }

    private void ClearPreview()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }
    }

    private void EnsureLineRenderer()
    {
        if (lineRenderer != null)
        {
            return;
        }

        var existingChild = transform.Find("ShotArcPreview");
        GameObject go;
        if (existingChild != null)
        {
            go = existingChild.gameObject;
            lineRenderer = go.GetComponent<LineRenderer>();
        }
        else
        {
            go = new GameObject("ShotArcPreview");
            go.transform.SetParent(transform, false);
        }

        if (lineRenderer == null)
        {
            lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth * 0.35f;
            lineRenderer.numCapVertices = 4;
            lineRenderer.enabled = false;
            lineRenderer.sharedMaterial = ArcAcademyShaderUtility.CreateEmissiveLineMaterial(
                new Color(0.25f, 0.95f, 0.35f, 0.9f),
                1.4f);
        }
    }
}
