using UnityEngine;

/// <summary>
/// On-screen training scoreboard fallback for warehouse scenes without a wall HUD.
/// In Simple Arc Academy lab view, <see cref="BobWallTrainingHud"/> on Wall_South is canonical.
/// </summary>
public class BobTrainingScoreboard : MonoBehaviour
{
    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle lineStyle;
    private GUIStyle highlightStyle;

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

        const float width = 300f;
        const float height = 218f;
        var rect = new Rect(Screen.width - width - 14f, 14f, width, height);

        GUILayout.BeginArea(rect, panelStyle);
        GUILayout.Label("Arc Academy Scoreboard", titleStyle);
        GUILayout.Label($"{BobScoreboardDisplay.EpisodesLabel}: {stats.TotalIterations}", lineStyle);
        GUILayout.Label($"{BobScoreboardDisplay.ScoreLabel} (baskets): {stats.BasketballPoints}", highlightStyle);
        GUILayout.Label($"{BobScoreboardDisplay.SuccessLabel}: {stats.SessionSuccessRate:P1}", lineStyle);
        var monitor = BobTrainingConnectionMonitor.Instance;
        if (monitor != null)
        {
            var statusStyle = new GUIStyle(lineStyle)
            {
                fontStyle = FontStyle.Italic,
                normal =
                {
                    textColor = monitor.IsTrainingConnected
                        ? new Color(0.45f, 0.95f, 0.55f)
                        : new Color(1f, 0.55f, 0.45f),
                },
            };
            GUILayout.Label(monitor.StatusLabel, statusStyle);
        }

        GUILayout.Label($"Rewards (RL): +{stats.TotalRewards:F2}", lineStyle);
        GUILayout.Label($"Penalties (RL): -{stats.TotalPenalties:F2}", lineStyle);
        GUILayout.Label($"Net RL reward: {stats.NetSessionReward:+0.00;-0.00}", lineStyle);
        GUILayout.Label(
            $"Last iteration: {stats.LastEpisodeNetReward:+0.00;-0.00}  ·  {BobScoreboardDisplay.ArcLabel}: {stats.LastEpisodePeakArcQuality:P0}",
            lineStyle);
        GUILayout.EndArea();
    }

    private void EnsureStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(12, 12, 10, 10),
            normal = { background = MakeTex(2, 2, new Color(0.04f, 0.05f, 0.08f, 0.82f)) },
        };
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.92f, 0.94f, 1f) },
        };
        lineStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            normal = { textColor = new Color(0.88f, 0.9f, 0.95f) },
        };
        highlightStyle = new GUIStyle(lineStyle)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.82f, 0.35f) },
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
