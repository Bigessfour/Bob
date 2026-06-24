using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space lab wall HUD — Episodes, Success, Arc headlines, RL metrics, and dual-metric graph.
/// </summary>
public class BobWallTrainingHud : MonoBehaviour
{
    public const string RootName = "LabTrainingHud";

    public static BobWallTrainingHud Instance { get; private set; }

    [SerializeField] private int graphWindow = BobTrainingStats.DefaultRollingWindow;
    [SerializeField] private RawImage graphImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Text episodesText;
    [SerializeField] private Text successText;
    [SerializeField] private Text arcText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text rewardsText;
    [SerializeField] private Text penaltiesText;
    [SerializeField] private Text netRlText;
    [SerializeField] private Text lastEpisodeText;
    [SerializeField] private Text graphLegendText;

    private Texture2D graphTexture;
    private Color32[] graphPixels;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        BindMissingReferences();
        EnsureReadableStyles();
        EnsureGraphTexture(128, 48);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (graphTexture != null)
        {
            Destroy(graphTexture);
        }
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        var stats = BobTrainingStats.Instance;
        if (stats == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = "Arc Academy Lab";
        }

        if (episodesText != null)
        {
            episodesText.text = $"{BobScoreboardDisplay.EpisodesLabel}: {stats.TotalIterations}";
        }

        if (successText != null)
        {
            successText.text =
                $"{BobScoreboardDisplay.SuccessLabel}: {stats.SessionSuccessRate:P0}  ·  Rolling: {stats.RollingSuccessRate:P0}";
        }

        if (arcText != null)
        {
            arcText.text =
                $"{BobScoreboardDisplay.ArcLabel}: {stats.LastEpisodePeakArcQuality:P0}  ·  Avg: {stats.RollingAverageArcQuality:P0}";
        }

        if (scoreText != null)
        {
            scoreText.text = $"{BobScoreboardDisplay.ScoreLabel}: {stats.BasketballPoints}";
        }

        var monitor = BobTrainingConnectionMonitor.Instance;
        if (statusText != null)
        {
            statusText.text = monitor != null ? monitor.StatusLabel : "Play mode";
            statusText.color = monitor != null && monitor.IsTrainingConnected
                ? new Color(0.45f, 0.95f, 0.55f)
                : new Color(1f, 0.55f, 0.45f);
        }

        if (rewardsText != null)
        {
            rewardsText.text = $"Rewards: +{stats.TotalRewards:F1}";
        }

        if (penaltiesText != null)
        {
            penaltiesText.text = $"Penalties: -{stats.TotalPenalties:F1}";
        }

        if (netRlText != null)
        {
            netRlText.text = $"Net RL: {stats.NetSessionReward:+0.0;-0.0}";
        }

        if (lastEpisodeText != null)
        {
            lastEpisodeText.text =
                $"Last shot RL: {stats.LastEpisodeNetReward:+0.0;-0.0}  ·  {BobScoreboardDisplay.ArcLabel}: {stats.LastEpisodePeakArcQuality:P0}";
        }

        if (graphLegendText != null)
        {
            graphLegendText.text =
                $"{BobScoreboardDisplay.SuccessLabel} · {BobScoreboardDisplay.ArcLabel} quality (avg {stats.RollingAverageArcQuality:P0})";
        }

        DrawGraph(stats);
    }

    private void BindMissingReferences()
    {
        var panel = transform.Find("Canvas/Panel");
        if (panel == null)
        {
            return;
        }

        titleText ??= panel.Find("TitleText")?.GetComponent<Text>();
        episodesText ??= panel.Find("EpisodesText")?.GetComponent<Text>();
        episodesText ??= panel.Find("IterationsText")?.GetComponent<Text>();
        successText ??= panel.Find("SuccessText")?.GetComponent<Text>();
        arcText ??= panel.Find("ArcText")?.GetComponent<Text>();
        scoreText ??= panel.Find("ScoreText")?.GetComponent<Text>();
        statusText ??= panel.Find("StatusText")?.GetComponent<Text>();
        rewardsText ??= panel.Find("RewardsText")?.GetComponent<Text>();
        penaltiesText ??= panel.Find("PenaltiesText")?.GetComponent<Text>();
        netRlText ??= panel.Find("NetRlText")?.GetComponent<Text>();
        lastEpisodeText ??= panel.Find("LastEpisodeText")?.GetComponent<Text>();
        graphLegendText ??= panel.Find("GraphLegendText")?.GetComponent<Text>();
        graphImage ??= panel.Find("GraphImage")?.GetComponent<RawImage>();
    }

    private void EnsureReadableStyles()
    {
        if (episodesText != null)
        {
            BobScoreboardDisplay.ApplyReadableTextStyle(episodesText, headline: true);
        }

        if (successText != null)
        {
            BobScoreboardDisplay.ApplyReadableTextStyle(successText, headline: true);
        }

        if (arcText != null)
        {
            BobScoreboardDisplay.ApplyReadableTextStyle(arcText, headline: true);
        }

        if (scoreText != null)
        {
            BobScoreboardDisplay.ApplyReadableTextStyle(scoreText, headline: false);
        }

        foreach (var detail in new[] { statusText, rewardsText, penaltiesText, netRlText, lastEpisodeText, graphLegendText })
        {
            if (detail != null)
            {
                BobScoreboardDisplay.ApplyDetailTextStyle(detail);
            }
        }
    }

    private void EnsureGraphTexture(int width, int height)
    {
        if (graphTexture != null && graphTexture.width == width && graphTexture.height == height)
        {
            return;
        }

        if (graphTexture != null)
        {
            Destroy(graphTexture);
        }

        graphTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        graphPixels = new Color32[width * height];

        if (graphImage != null)
        {
            graphImage.texture = graphTexture;
        }
    }

    private void DrawGraph(BobTrainingStats stats)
    {
        if (graphTexture == null || graphPixels == null)
        {
            return;
        }

        int w = graphTexture.width;
        int h = graphTexture.height;
        var bg = new Color32(8, 10, 16, 220);
        for (int i = 0; i < graphPixels.Length; i++)
        {
            graphPixels[i] = bg;
        }

        DrawHLine(w, h, 0.5f, new Color32(255, 255, 255, 30));
        PlotSeries(stats.GetRecentOutcomes(graphWindow), new Color32(90, 240, 140, 240), w, h);
        PlotSeries(stats.GetRecentArcQuality(graphWindow), new Color32(90, 210, 255, 240), w, h);

        graphTexture.SetPixels32(graphPixels);
        graphTexture.Apply(false);
    }

    private void PlotSeries(IReadOnlyList<float> samples, Color32 color, int w, int h)
    {
        if (samples == null || samples.Count < 2)
        {
            return;
        }

        for (int i = 1; i < samples.Count; i++)
        {
            float x0 = (i - 1) / (float)(samples.Count - 1) * (w - 1);
            float x1 = i / (float)(samples.Count - 1) * (w - 1);
            float y0 = (1f - Mathf.Clamp01(samples[i - 1])) * (h - 1);
            float y1 = (1f - Mathf.Clamp01(samples[i])) * (h - 1);
            DrawLine((int)x0, (int)y0, (int)x1, (int)y1, color);
        }
    }

    private void DrawHLine(int w, int h, float normalizedY, Color32 color)
    {
        int y = Mathf.Clamp((int)(normalizedY * (h - 1)), 0, h - 1);
        for (int x = 0; x < w; x++)
        {
            graphPixels[y * w + x] = color;
        }
    }

    private void DrawLine(int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        int w = graphTexture.width;
        int h = graphTexture.height;

        while (true)
        {
            if (x0 >= 0 && x0 < w && y0 >= 0 && y0 < h)
            {
                graphPixels[y0 * w + x0] = color;
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
