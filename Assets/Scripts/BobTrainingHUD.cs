using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Modern screen-space training HUD (Canvas + TextMeshPro).
/// Supplements lab wall/near-Bob HUDs and replaces the OnGUI scoreboard when present.
/// Pulls live metrics from <see cref="BobTrainingStats"/> — no reward/episode logic here.
/// </summary>
public class BobTrainingHUD : MonoBehaviour
{
    public const int OutcomeChipCount = 8;
    public const int GraphBarCount = 24;

    public static BobTrainingHUD Instance { get; private set; }

    [Tooltip("Shown in the header; overridden by RUN_ID env var when set.")]
    [SerializeField] private string runId = "local";

    private TextMeshProUGUI headerText;
    private TextMeshProUGUI successText;
    private TextMeshProUGUI economicsText;
    private TextMeshProUGUI outcomesLabel;
    private readonly Image[] outcomeChips = new Image[OutcomeChipCount];
    private readonly Image[] graphBars = new Image[GraphBarCount];

    private float highlightTimer;
    private TextMeshProUGUI highlightTarget;
    private Color highlightBaseColor;
    private string lastSuccessKey = "";
    private string lastEconomicsKey = "";
    private int lastIteration = -1;

    private static readonly Color PanelBg = new(0.04f, 0.05f, 0.08f, 0.92f);
    private static readonly Color MakeGreen = new(0.35f, 0.92f, 0.55f, 1f);
    private static readonly Color RimOrange = new(1f, 0.62f, 0.22f, 1f);
    private static readonly Color BadRed = new(0.95f, 0.32f, 0.32f, 1f);
    private static readonly Color ChipEmpty = new(0.22f, 0.24f, 0.30f, 0.85f);
    private static readonly Color GraphBar = new(0.35f, 0.75f, 1f, 0.9f);
    private static readonly Color AccentFlash = new(1f, 0.9f, 0.45f, 1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        var envRun = Environment.GetEnvironmentVariable("RUN_ID");
        if (!string.IsNullOrWhiteSpace(envRun))
        {
            runId = envRun.Trim();
        }

        EnsureUi();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Refresh();
        TickHighlight();
    }

    private void Refresh()
    {
        var stats = BobTrainingStats.Instance;
        if (stats == null || headerText == null)
        {
            return;
        }

        var monitor = BobTrainingConnectionMonitor.Instance;
        bool connected = monitor != null && monitor.IsTrainingConnected;
        string status = monitor != null ? monitor.StatusLabel : "Play mode";

        headerText.text = $"Run <b>{runId}</b>  ·  {status}";
        headerText.color = BobScoreboardDisplay.StatusColor(connected);

        string successKey =
            $"{stats.SessionSuccessRate:F4}|{stats.RollingSuccessRate:F4}|{stats.BasketballPoints}|{stats.TotalIterations}";
        successText.text =
            $"<size=150%>{stats.SessionSuccessRate:P1}</size>  session\n" +
            $"<size=130%>{stats.RollingSuccessRate:P1}</size>  rolling  ·  " +
            $"{BobScoreboardDisplay.ScoreLabel} {stats.BasketballPoints} / {stats.TotalIterations}";

        if (successKey != lastSuccessKey)
        {
            Pulse(successText, BobScoreboardDisplay.HeadlineColor);
            lastSuccessKey = successKey;
        }
        else if (highlightTarget != successText)
        {
            successText.color = BobScoreboardDisplay.HeadlineColor;
        }

        string econKey =
            $"{stats.TotalRewards:F2}|{stats.TotalPenalties:F2}|{stats.NetSessionReward:F2}|{stats.LastEpisodeNetReward:F2}";
        economicsText.text =
            $"{BobScoreboardDisplay.RewardsLabel} +{stats.TotalRewards:F1}  |  " +
            $"{BobScoreboardDisplay.PenaltiesLabel} −{stats.TotalPenalties:F1}  |  " +
            $"{BobScoreboardDisplay.NetRlLabel} {stats.NetSessionReward:+0.0;-0.0}  |  " +
            $"{BobScoreboardDisplay.LastShotRlLabel} {stats.LastEpisodeNetReward:+0.0;-0.0}";

        if (econKey != lastEconomicsKey)
        {
            Pulse(economicsText, BobScoreboardDisplay.MutedDetailColor);
            lastEconomicsKey = econKey;
        }
        else if (highlightTarget != economicsText)
        {
            economicsText.color = BobScoreboardDisplay.MutedDetailColor;
        }

        if (stats.TotalIterations != lastIteration)
        {
            lastIteration = stats.TotalIterations;
            RefreshOutcomeChips(stats);
            RefreshGraphBars(stats);
            outcomesLabel.text =
                $"Recent  ·  last {OutcomeChipCount}  ·  {stats.LastShotEndReason}";
        }
    }

    private void RefreshOutcomeChips(BobTrainingStats stats)
    {
        IReadOnlyList<string> reasons = stats.GetRecentEndReasons(OutcomeChipCount);
        for (int i = 0; i < OutcomeChipCount; i++)
        {
            var chip = outcomeChips[i];
            if (chip == null)
            {
                continue;
            }

            int reasonIndex = i - (OutcomeChipCount - reasons.Count);
            if (reasonIndex < 0 || reasonIndex >= reasons.Count)
            {
                chip.color = ChipEmpty;
                continue;
            }

            chip.color = ColorForEndReason(reasons[reasonIndex]);
        }
    }

    private void RefreshGraphBars(BobTrainingStats stats)
    {
        IReadOnlyList<float> series = stats.GetRecentOutcomes(GraphBarCount);
        for (int i = 0; i < GraphBarCount; i++)
        {
            var bar = graphBars[i];
            if (bar == null)
            {
                continue;
            }

            float value = i < series.Count ? Mathf.Clamp01(series[i]) : 0f;
            var rt = bar.rectTransform;
            // Bars grow upward from the row baseline (anchored bottom).
            rt.anchorMin = new Vector2(rt.anchorMin.x, 0f);
            rt.anchorMax = new Vector2(rt.anchorMax.x, Mathf.Max(0.08f, value));
            bar.color = value > 0.01f ? GraphBar : ChipEmpty;
        }
    }

    private static Color ColorForEndReason(string reason)
    {
        if (reason == "make" || reason == "swish")
        {
            return MakeGreen;
        }

        if (reason == "rim_miss")
        {
            return RimOrange;
        }

        return BadRed;
    }

    private void Pulse(TextMeshProUGUI target, Color baseColor)
    {
        highlightTarget = target;
        highlightBaseColor = baseColor;
        highlightTimer = 0.35f;
        target.color = AccentFlash;
    }

    private void TickHighlight()
    {
        if (highlightTimer <= 0f || highlightTarget == null)
        {
            return;
        }

        highlightTimer -= Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(highlightTimer / 0.35f);
        highlightTarget.color = Color.Lerp(highlightBaseColor, AccentFlash, t);
        if (highlightTimer <= 0f)
        {
            highlightTarget.color = highlightBaseColor;
            highlightTarget = null;
        }
    }

    private void EnsureUi()
    {
        if (headerText != null)
        {
            return;
        }

        var canvasGo = new GameObject("BobTrainingHUD_Canvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = PanelBg;
        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(1f, 1f);
        panelRt.anchorMax = new Vector2(1f, 1f);
        panelRt.pivot = new Vector2(1f, 1f);
        panelRt.anchoredPosition = new Vector2(-16f, -16f);
        panelRt.sizeDelta = new Vector2(420f, 320f);

        float y = -14f;
        headerText = CreateTmp(panelGo.transform, "HeaderText", 18f, FontStyles.Normal, y, 28f);
        y -= 34f;
        successText = CreateTmp(panelGo.transform, "SuccessText", 22f, FontStyles.Bold, y, 64f);
        y -= 70f;
        economicsText = CreateTmp(panelGo.transform, "EconomicsText", 15f, FontStyles.Normal, y, 40f);
        economicsText.color = BobScoreboardDisplay.MutedDetailColor;
        y -= 44f;

        outcomesLabel = CreateTmp(panelGo.transform, "OutcomesLabel", 14f, FontStyles.Normal, y, 22f);
        outcomesLabel.color = BobScoreboardDisplay.MutedDetailColor;
        y -= 26f;

        var chipsRow = CreateRow(panelGo.transform, "OutcomesRow", y, 22f);
        for (int i = 0; i < OutcomeChipCount; i++)
        {
            outcomeChips[i] = CreateChip(chipsRow, $"Chip_{i}", i, OutcomeChipCount);
        }

        y -= 36f;
        var graphLabel = CreateTmp(panelGo.transform, "GraphLabel", 13f, FontStyles.Normal, y, 20f);
        graphLabel.text = "Rolling success (placeholder)";
        graphLabel.color = BobScoreboardDisplay.MutedDetailColor;
        y -= 24f;

        var graphRow = CreateRow(panelGo.transform, "GraphRow", y, 48f);
        for (int i = 0; i < GraphBarCount; i++)
        {
            graphBars[i] = CreateGraphBar(graphRow, $"Bar_{i}", i, GraphBarCount);
        }
    }

    private static TextMeshProUGUI CreateTmp(
        Transform parent,
        string name,
        float fontSize,
        FontStyles style,
        float anchoredY,
        float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = BobScoreboardDisplay.BodyColor;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.enableWordWrapping = true;
        tmp.richText = true;
        tmp.raycastTarget = false;
        var rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, anchoredY);
        rt.sizeDelta = new Vector2(-28f, height);
        return tmp;
    }

    private static RectTransform CreateRow(Transform parent, string name, float anchoredY, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, anchoredY);
        rt.sizeDelta = new Vector2(-28f, height);
        return rt;
    }

    private static Image CreateChip(RectTransform row, string name, int index, int count)
    {
        var go = new GameObject(name);
        go.transform.SetParent(row, false);
        var image = go.AddComponent<Image>();
        image.color = ChipEmpty;
        image.raycastTarget = false;
        var rt = image.rectTransform;
        float pad = 0.01f;
        float width = (1f - pad * (count + 1)) / count;
        float x0 = pad + index * (width + pad);
        rt.anchorMin = new Vector2(x0, 0.15f);
        rt.anchorMax = new Vector2(x0 + width, 0.85f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return image;
    }

    private static Image CreateGraphBar(RectTransform row, string name, int index, int count)
    {
        var go = new GameObject(name);
        go.transform.SetParent(row, false);
        var image = go.AddComponent<Image>();
        image.color = ChipEmpty;
        image.raycastTarget = false;
        var rt = image.rectTransform;
        float pad = 0.005f;
        float width = (1f - pad * (count + 1)) / count;
        float x0 = pad + index * (width + pad);
        rt.anchorMin = new Vector2(x0, 0f);
        rt.anchorMax = new Vector2(x0 + width, 0.08f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return image;
    }
}
