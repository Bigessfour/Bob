using UnityEngine;

/// <summary>
/// Lightweight demo HUD — camera mode hint and reset view button.
/// </summary>
public class ArcAcademyDemoUi : MonoBehaviour
{
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        EnsureStyles();

        var camera = ArcAcademyDemoCamera.Instance;
        string modeLabel = camera != null ? camera.Mode.ToString() : "Hero";

        GUILayout.BeginArea(new Rect(12f, 12f, 280f, 90f), boxStyle);
        GUILayout.Label("Arc Academy Demo", labelStyle);
        GUILayout.Label($"Camera: {modeLabel}  ·  F1 cycle  ·  R reset", labelStyle);
        GUILayout.Label("Space shoot  ·  Drag to aim", labelStyle);
        GUILayout.EndArea();

        if (GUI.Button(new Rect(Screen.width - 132f, Screen.height - 48f, 120f, 36f), "Reset View", buttonStyle)
            && camera != null)
        {
            camera.ResetToHero();
        }
    }

    private void EnsureStyles()
    {
        if (boxStyle != null)
        {
            return;
        }

        boxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.45f)) },
        };
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            normal = { textColor = Color.white },
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
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
