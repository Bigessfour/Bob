using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared typography and label strings for wall HUD and OnGUI fallback scoreboards.
/// </summary>
public static class BobScoreboardDisplay
{
    public const string EpisodesLabel = "Episodes";
    public const string SuccessLabel = "Success";
    public const string ArcLabel = "Arc";
    public const string ScoreLabel = "Score";

    public const int HeadlineFontSize = 24;
    public const int BodyFontSize = 15;
    public const int DetailFontSize = 12;
    public const int TitleFontSize = 18;

    public const float CanvasReferencePixelsPerUnit = 100f;
    public static readonly Color HeadlineColor = Color.white;
    public static readonly Color BodyColor = new(0.92f, 0.94f, 0.98f);
    public static readonly Color ScoreAccentColor = new(1f, 0.82f, 0.35f);
    public static readonly Color OutlineColor = Color.black;
    public static readonly Vector2 OutlineDistance = new(1.2f, -1.2f);

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

    public static void ApplyReadableTextStyle(Text text, bool headline)
    {
        if (text == null)
        {
            return;
        }

        text.color = headline ? HeadlineColor : BodyColor;
        text.fontSize = headline ? HeadlineFontSize : BodyFontSize;
        if (headline)
        {
            text.fontStyle = FontStyle.Bold;
        }

        var outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = OutlineColor;
        outline.effectDistance = OutlineDistance;
        outline.useGraphicAlpha = true;
    }

    public static void ApplyDetailTextStyle(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.color = BodyColor;
        text.fontSize = DetailFontSize;

        var outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = OutlineColor;
        outline.effectDistance = new Vector2(0.8f, -0.8f);
        outline.useGraphicAlpha = true;
    }
}
