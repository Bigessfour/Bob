using UnityEngine;

/// <summary>
/// Portfolio trajectory arcs from spawn pad toward hoop targets (visual only — no RL impact).
/// </summary>
public class ArcTrajectoryVisual : MonoBehaviour
{
    [SerializeField] private int arcSegments = ArcAcademyLayout.TrajectoryArcSegments;
    [SerializeField] private float arcHeight = ArcAcademyLayout.TrajectoryArcHeight;
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private Color arcColor = new(0.85f, 0.9f, 1f);
    [SerializeField] private float emissiveIntensity = ArcAcademyLayout.ArcLineEmissiveIntensity;

    public void ConfigureStaticArcs(Vector3 start, Vector3[] targets)
    {
        ClearChildren();

        if (targets == null || targets.Length == 0)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            CreateArcLine($"Arc_{i + 1}", start, targets[i], i);
        }
    }

    private void CreateArcLine(string name, Vector3 start, Vector3 end, int index)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = arcSegments + 1;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth * 0.6f;
        line.numCapVertices = 4;

        var tint = Color.Lerp(arcColor, new Color(0.6f, 0.35f, 1f), index * 0.25f);
        line.sharedMaterial = CreateArcMaterial(tint);

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
        var mat = new Material(Shader.Find("Standard"));
        mat.EnableKeyword("_EMISSION");
        mat.color = color;
        mat.SetColor("_EmissionColor", color * emissiveIntensity);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        return mat;
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
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
