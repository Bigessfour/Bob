using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rolling success-rate line graph for the training HUD (AI Warehouse–style progress chart).
/// </summary>
public class BobTrainingSuccessGraph : MonoBehaviour
{
    [SerializeField] private int windowSize = 24;
    [SerializeField] private float graphWidth = 280f;
    [SerializeField] private float graphHeight = 72f;
    [SerializeField] private float marginRight = 14f;
    [SerializeField] private float marginTop = 190f;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private Texture2D lineTex;

    private void OnGUI()
    {
        if (SimpleArcAcademyArena.IsLabViewActive)
        {
            return;
        }

        if (!Application.isPlaying)
        {
            return;
        }

        if (SimpleArcAcademyArena.IsLabViewActive && BobWallTrainingHud.Instance != null)
        {
            return;
        }

        var stats = BobTrainingStats.Instance;
        if (stats == null)
        {
            return;
        }

        EnsureStyles();

        var rect = new Rect(
            Screen.width - graphWidth - marginRight,
            marginTop,
            graphWidth,
            graphHeight + 28f);

        GUILayout.BeginArea(rect, panelStyle);
        GUILayout.Label(
            $"Success rate (last {windowSize}): {stats.RollingSuccessRate:P0}  ·  Session: {stats.SessionSuccessRate:P0}",
            titleStyle);

        var graphRect = new Rect(8f, 26f, graphWidth - 16f, graphHeight);
        DrawGraph(graphRect, stats.GetRecentOutcomes(windowSize));
        GUILayout.EndArea();
    }

    private void DrawGraph(Rect rect, IReadOnlyList<float> samples)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        GUI.DrawTexture(rect, panelStyle.normal.background, ScaleMode.StretchToFill);

        if (samples == null || samples.Count < 2)
        {
            GUI.Label(rect, "Collecting shots…", titleStyle);
            return;
        }

        var points = new List<Vector2>(samples.Count);
        for (int i = 0; i < samples.Count; i++)
        {
            float x = rect.xMin + (i / (float)(samples.Count - 1)) * rect.width;
            float y = rect.yMax - samples[i] * rect.height;
            points.Add(new Vector2(x, y));
        }

        var color = new Color(0.35f, 0.95f, 0.55f, 0.95f);
        for (int i = 1; i < points.Count; i++)
        {
            DrawLine(points[i - 1], points[i], color, 2f);
        }

        DrawLine(
            new Vector2(rect.xMin, rect.yMax - rect.height * 0.5f),
            new Vector2(rect.xMax, rect.yMax - rect.height * 0.5f),
            new Color(1f, 1f, 1f, 0.15f),
            1f);
    }

    private void DrawLine(Vector2 a, Vector2 b, Color color, float width)
    {
        if (lineTex == null)
        {
            lineTex = new Texture2D(1, 1);
            lineTex.SetPixel(0, 0, Color.white);
            lineTex.Apply();
        }

        var saved = GUI.color;
        GUI.color = color;
        var delta = b - a;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        float length = delta.magnitude;
        var matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, a);
        GUI.DrawTexture(new Rect(a.x, a.y - width * 0.5f, length, width), lineTex);
        GUI.matrix = matrix;
        GUI.color = saved;
    }

    private void EnsureStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(8, 8, 6, 6),
            normal = { background = MakeTex(2, 2, new Color(0.04f, 0.06f, 0.09f, 0.88f)) },
        };
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            normal = { textColor = new Color(0.85f, 0.9f, 0.95f) },
        };
    }

    private static Texture2D MakeTex(int width, int height, Color color)
    {
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        var tex = new Texture2D(width, height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
