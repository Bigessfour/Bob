using UnityEngine;

/// <summary>
/// Shot arc preview: green = actual ballistic path after impulse;
/// cyan = <see cref="BobSwishLaunchSolver"/> ideal high-arc free throw for comparison.
/// </summary>
[RequireComponent(typeof(BobAgent))]
public class BobShotArcPreview : MonoBehaviour
{
    [SerializeField] private BobAgent agent;
    [SerializeField] private int arcSegments = 48;
    [SerializeField] private float maxFlightSeconds = 2.8f;
    [SerializeField] private float floorCutY = 0.05f;
    [SerializeField] private float launchSpeedThreshold = 0.5f;
    [SerializeField] private float lineWidth = 0.03f;

    private LineRenderer actualLine;
    private LineRenderer idealLine;
    private bool previewShownThisEpisode;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<BobAgent>();
        }

        EnsureLineRenderers();
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
            ShowBallisticArc(actualLine, rb.position, rb.linearVelocity);
            if (agent.TryGetIdealWorldImpulse(
                    BobSwishLaunchSolver.PreferredLaunchAngleDegrees, out Vector3 idealImpulse)
                && rb.mass > 0.01f)
            {
                // Impulse → Δv for ballistic sample (same as ForceMode.Impulse).
                Vector3 idealVelocity = idealImpulse / rb.mass;
                ShowBallisticArc(idealLine, rb.position, idealVelocity);
            }

            previewShownThisEpisode = true;
        }
    }

    private void ShowBallisticArc(LineRenderer line, Vector3 start, Vector3 velocity)
    {
        if (line == null)
        {
            return;
        }

        float g = Physics.gravity.y;
        float duration = EstimateFlightDuration(start.y, velocity.y, g);
        duration = Mathf.Clamp(duration, 0.35f, maxFlightSeconds);

        int count = Mathf.Max(arcSegments, 16) + 1;
        line.positionCount = count;
        line.enabled = true;

        int written = 0;
        for (int i = 0; i < count; i++)
        {
            float t = duration * (i / (float)(count - 1));
            Vector3 pos = start + velocity * t + 0.5f * Physics.gravity * (t * t);
            if (i > 0 && pos.y < floorCutY)
            {
                break;
            }

            line.SetPosition(written, pos);
            written++;
        }

        line.positionCount = Mathf.Max(written, 2);
    }

    private float EstimateFlightDuration(float startY, float vy, float gravityY)
    {
        float a = 0.5f * gravityY;
        float b = vy;
        float c = startY - floorCutY;
        float disc = b * b - 4f * a * c;
        if (disc < 0f || Mathf.Abs(a) < 1e-6f)
        {
            return maxFlightSeconds;
        }

        float sqrt = Mathf.Sqrt(disc);
        float t1 = (-b + sqrt) / (2f * a);
        float t2 = (-b - sqrt) / (2f * a);
        float t = Mathf.Max(t1, t2);
        if (t < 0.2f)
        {
            return maxFlightSeconds;
        }

        return Mathf.Min(t, maxFlightSeconds);
    }

    private void ClearPreview()
    {
        if (actualLine != null)
        {
            actualLine.enabled = false;
            actualLine.positionCount = 0;
        }

        if (idealLine != null)
        {
            idealLine.enabled = false;
            idealLine.positionCount = 0;
        }
    }

    private void EnsureLineRenderers()
    {
        if (actualLine == null)
        {
            actualLine = EnsureChildLine(
                "ShotArcPreview",
                new Color(0.25f, 0.95f, 0.35f, 0.9f),
                lineWidth);
        }

        if (idealLine == null)
        {
            idealLine = EnsureChildLine(
                "IdealFreeThrowArc",
                new Color(0.25f, 0.85f, 1f, 0.75f),
                lineWidth * 0.85f);
        }
    }

    private LineRenderer EnsureChildLine(string childName, Color color, float width)
    {
        var existingChild = transform.Find(childName);
        GameObject go;
        if (existingChild != null)
        {
            go = existingChild.gameObject;
        }
        else
        {
            go = new GameObject(childName);
            go.transform.SetParent(transform, false);
        }

        var line = go.GetComponent<LineRenderer>();
        if (line == null)
        {
            line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.startWidth = width;
            line.endWidth = width * 0.35f;
            line.numCapVertices = 4;
            line.enabled = false;
            line.sharedMaterial = ArcAcademyShaderUtility.CreateEmissiveLineMaterial(color, 1.4f);
        }

        return line;
    }
}
