using UnityEngine;

/// <summary>
/// Portfolio trajectory arcs from spawn pad toward hoop targets (visual only — no RL impact).
/// </summary>
public class ArcTrajectoryVisual : MonoBehaviour
{
    [SerializeField] private int arcSegments = ArcAcademyLayout.TrajectoryArcSegments;
    [SerializeField] private float arcHeight = ArcAcademyLayout.TrajectoryArcHeight;
    [SerializeField] private float lineWidth = 0.035f;
    [SerializeField] private Color arcColor = Color.white;
    [SerializeField] private float emissiveIntensity = ArcAcademyLayout.ArcLineEmissiveIntensity;
    [SerializeField] private Color previewColor = new(1f, 1f, 1f, 0.85f);

    private LineRenderer previewLine;

    public void ConfigureStaticArcs(Vector3 start, Vector3[] targets)
    {
        ClearStaticArcs();

        if (targets == null || targets.Length == 0)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            CreateArcLine($"Arc_{i + 1}", start, targets[i]);
        }
    }

    public void PreviewArc(Vector3 start, Vector3 end)
    {
        EnsurePreviewLine();
        previewLine.positionCount = arcSegments + 1;
        previewLine.enabled = true;

        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            previewLine.SetPosition(i, ParabolicPoint(start, end, t, arcHeight));
        }
    }

    public void ClearPreview()
    {
        if (previewLine != null)
        {
            previewLine.enabled = false;
        }
    }

    private void EnsurePreviewLine()
    {
        if (previewLine != null)
        {
            return;
        }

        var go = new GameObject("PreviewArc");
        go.transform.SetParent(transform, false);
        previewLine = go.AddComponent<LineRenderer>();
        previewLine.useWorldSpace = true;
        previewLine.startWidth = lineWidth * 1.1f;
        previewLine.endWidth = lineWidth * 0.5f;
        previewLine.numCapVertices = 4;
        previewLine.enabled = false;
        previewLine.sharedMaterial = CreateArcMaterial(previewColor);
    }

    private void CreateArcLine(string name, Vector3 start, Vector3 end)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = arcSegments + 1;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth * 0.6f;
        line.numCapVertices = 4;
        line.sharedMaterial = CreateArcMaterial(arcColor);

        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            line.SetPosition(i, ParabolicPoint(start, end, t, arcHeight));
        }
    }

    private static Vector3 ParabolicPoint(Vector3 start, Vector3 end, float t, float height)
    {
        var linear = Vector3.Lerp(start, end, t);
        float lift = 4f * height * t * (1f - t);
        linear.y += lift;
        return linear;
    }

    private Material CreateArcMaterial(Color color)
    {
        return ArcAcademyShaderUtility.CreateEmissiveLineMaterial(color, emissiveIntensity);
    }

    private void ClearStaticArcs()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "PreviewArc")
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
