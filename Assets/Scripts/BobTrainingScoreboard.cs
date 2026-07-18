using UnityEngine;

/// <summary>
/// On-screen training scoreboard fallback for warehouse scenes without in-scene HUDs.
/// In Simple Arc Academy lab view, <see cref="BobNearBobTrainingHud"/> + wall console are canonical.
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

        if (BobWallTrainingHud.Instance != null || BobNearBobTrainingHud.Instance != null)
        {
            return;
        }

        // Modern Canvas/TMP HUD replaces this OnGUI fallback when present.
        if (BobTrainingHUD.Instance != null || Object.FindAnyObjectByType<BobTrainingHUD>() != null)
        {
            return;
        }

        var stats = BobTrainingStats.Instance;
        if (stats == null)
        {
            return;
        }

        EnsureStyles();

        var width = BobScoreboardDisplay.OnGuiPanelWidth;
        var height = BobScoreboardDisplay.OnGuiPanelHeight;
        var rect = new Rect(Screen.width - width - 14f, 14f, width, height);

        GUILayout.BeginArea(rect, panelStyle);
        GUILayout.Label("Arc Academy Scoreboard", titleStyle);
        GUILayout.Space(4f);
        GUILayout.Label($"{BobScoreboardDisplay.EpisodesLabel}: {stats.TotalIterations}", lineStyle);
        GUILayout.Label($"{BobScoreboardDisplay.ScoreLabel} (baskets): {stats.BasketballPoints}", highlightStyle);
        GUILayout.Label(
            $"{BobScoreboardDisplay.SuccessLabel}: {stats.SessionSuccessRate:P1}  ·  " +
            $"{BobScoreboardDisplay.RollingLabel}: {stats.RollingSuccessRate:P1}",
            lineStyle);
        var monitor = BobTrainingConnectionMonitor.Instance;
        if (monitor != null)
        {
            var statusStyle = new GUIStyle(lineStyle)
            {
                fontStyle = FontStyle.Italic,
                normal =
                {
                    textColor = BobScoreboardDisplay.StatusColor(monitor.IsTrainingConnected),
                },
            };
            GUILayout.Label(monitor.StatusLabel, statusStyle);
        }

        GUILayout.Space(2f);
        GUILayout.Label($"{BobScoreboardDisplay.RewardsLabel} (RL): +{stats.TotalRewards:F2}", lineStyle);
        GUILayout.Label($"{BobScoreboardDisplay.PenaltiesLabel} (RL): -{stats.TotalPenalties:F2}", lineStyle);
        GUILayout.Label($"{BobScoreboardDisplay.NetRlLabel}: {stats.NetSessionReward:+0.00;-0.00}", lineStyle);
        GUILayout.Label(
            $"{BobScoreboardDisplay.LastShotRlLabel}: {stats.LastEpisodeNetReward:+0.00;-0.00}  ·  " +
            $"{BobScoreboardDisplay.ArcLabel}: {stats.LastEpisodePeakArcQuality:P0}",
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
            padding = new RectOffset(14, 14, 12, 12),
            normal = { background = MakeTex(2, 2, BobScoreboardDisplay.PanelBackgroundColor) },
        };
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = BobScoreboardDisplay.OnGuiTitleFontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = BobScoreboardDisplay.BodyColor },
        };
        lineStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = BobScoreboardDisplay.OnGuiBodyFontSize,
            normal = { textColor = BobScoreboardDisplay.BodyColor },
        };
        highlightStyle = new GUIStyle(lineStyle)
        {
            fontSize = BobScoreboardDisplay.OnGuiHighlightFontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = BobScoreboardDisplay.ScoreAccentColor },
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
