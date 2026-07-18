using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared typography, colors, and label strings for wall HUD, near-Bob float, and OnGUI fallback.
/// Keeps long-run training readability consistent across all scoreboard surfaces.
/// </summary>
public static class BobScoreboardDisplay
{
    // --- Shared metric labels (wall + float + OnGUI) ---
    public const string EpisodesLabel = "Episodes";
    public const string SuccessLabel = "Success";
    public const string RollingLabel = "Rolling";
    public const string ArcLabel = "Arc";
    public const string ScoreLabel = "Score";
    public const string RewardsLabel = "Rewards";
    public const string PenaltiesLabel = "Penalties";
    public const string NetRlLabel = "Net RL";
    public const string LastShotRlLabel = "Last shot RL";

    // --- Wall / shared body hierarchy (lab console + OnGUI) ---
    public const int HeadlineFontSize = 34;
    public const int WallMetricFontSize = 28;
    public const int BodyFontSize = 22;
    public const int DetailFontSize = 17;
    public const int TitleFontSize = 30;

    // --- OnGUI fallback (warehouse scenes without world-space HUDs) ---
    public const int OnGuiTitleFontSize = 17;
    public const int OnGuiBodyFontSize = 14;
    public const int OnGuiHighlightFontSize = 15;
    public const float OnGuiPanelWidth = 340f;
    public const float OnGuiPanelHeight = 248f;

    /// <summary>Near-Bob floating board — oversized for lab-camera readability.</summary>
    public const int FloatHeroFontSize = 72;
    public const int FloatScoreFontSize = 64;
    public const int FloatSuccessFontSize = 56;
    public const int FloatDetailFontSize = 36;
    public const int FloatTitleFontSize = 40;
    public static readonly Vector2 FloatOutlineDistance = new(3f, -3f);

    public const float CanvasReferencePixelsPerUnit = 100f;

    public static readonly Color HeadlineColor = Color.white;
    public static readonly Color BodyColor = new(0.93f, 0.95f, 0.99f);
    public static readonly Color MutedDetailColor = new(0.78f, 0.82f, 0.90f);
    public static readonly Color ScoreAccentColor = new(1f, 0.84f, 0.32f);
    public static readonly Color OutlineColor = new(0f, 0f, 0f, 0.92f);
    public static readonly Color PanelBackgroundColor = new(0.03f, 0.04f, 0.07f, 0.90f);
    public static readonly Color StatusConnectedColor = new(0.45f, 0.95f, 0.55f);
    public static readonly Color StatusDisconnectedColor = new(1f, 0.55f, 0.45f);

    /// <summary>Stronger outline for wall metrics during long training takes.</summary>
    public static readonly Vector2 OutlineDistance = new(1.6f, -1.6f);
    public static readonly Vector2 DetailOutlineDistance = new(1.0f, -1.0f);

    public static void ConfigureCanvasScaler(CanvasScaler scaler)
    {
        if (scaler == null)
        {
            return;
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        scaler.referencePixelsPerUnit = CanvasReferencePixelsPerUnit;
    }

    public static void ApplyFloatHeroTextStyle(Text text, int fontSize, Color color, bool bold = true)
    {
        if (text == null)
        {
            return;
        }

        text.color = color;
        text.fontSize = fontSize;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        EnsureOutline(text, FloatOutlineDistance);
    }

    /// <summary>Primary wall headline (title / key section).</summary>
    public static void ApplyReadableTextStyle(Text text, bool headline)
    {
        if (text == null)
        {
            return;
        }

        text.color = headline ? HeadlineColor : BodyColor;
        text.fontSize = headline ? HeadlineFontSize : BodyFontSize;
        text.fontStyle = headline ? FontStyle.Bold : FontStyle.Normal;
        EnsureOutline(text, OutlineDistance);
    }

    /// <summary>Wall hero metrics: Episodes / Success — readable from LabHero camera.</summary>
    public static void ApplyWallMetricTextStyle(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.color = HeadlineColor;
        text.fontSize = WallMetricFontSize;
        text.fontStyle = FontStyle.Bold;
        EnsureOutline(text, OutlineDistance);
    }

    /// <summary>Basketball score accent — stands out vs RL reward lines.</summary>
    public static void ApplyScoreAccentTextStyle(Text text, int fontSize = WallMetricFontSize)
    {
        if (text == null)
        {
            return;
        }

        text.color = ScoreAccentColor;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        EnsureOutline(text, OutlineDistance);
    }

    public static void ApplyDetailTextStyle(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.color = MutedDetailColor;
        text.fontSize = DetailFontSize;
        text.fontStyle = FontStyle.Normal;
        EnsureOutline(text, DetailOutlineDistance);
    }

    public static void ApplyTitleTextStyle(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.color = BodyColor;
        text.fontSize = TitleFontSize;
        text.fontStyle = FontStyle.Bold;
        EnsureOutline(text, OutlineDistance);
    }

    public static Color StatusColor(bool trainingConnected) =>
        trainingConnected ? StatusConnectedColor : StatusDisconnectedColor;

    private static void EnsureOutline(Text text, Vector2 distance)
    {
        var outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = OutlineColor;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }
}
