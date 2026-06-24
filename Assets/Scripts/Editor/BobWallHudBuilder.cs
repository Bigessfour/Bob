#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor helper: mounts compact world-space lab HUD on SimpleArcAcademyArena south wall.
/// </summary>
public static class BobWallHudBuilder
{
    public static void EnsureWallTrainingHud(Transform arenaRoot)
    {
        var wall = arenaRoot.Find(SimpleArcAcademyArena.LabHudWallName);
        if (wall == null)
        {
            Debug.LogWarning("BOB_WALL_HUD_WARN: Lab HUD wall not found.");
            return;
        }

        var existing = wall.Find(BobWallTrainingHud.RootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var hudRoot = new GameObject(BobWallTrainingHud.RootName);
        hudRoot.transform.SetParent(wall, false);
        hudRoot.transform.localScale = Vector3.one;
        BobPhysicsLayers.SetLayerRecursively(hudRoot, BobPhysicsLayers.DecorationLayer);

        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(hudRoot.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        BobScoreboardDisplay.ConfigureCanvasScaler(canvasGo.AddComponent<CanvasScaler>());
        canvasGo.AddComponent<GraphicRaycaster>();

        var canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.sizeDelta = SimpleArcAcademyArena.LabHudCanvasSize;
        canvasRect.localScale = SimpleArcAcademyArena.LabHudCanvasScale;

        var panel = CreateUiObject<RectTransform>("Panel", canvasGo.transform);
        var panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.04f, 0.05f, 0.08f, 0.92f);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;

        float pad = SimpleArcAcademyArena.LabHudPanelPadding;
        float contentWidth = SimpleArcAcademyArena.LabHudCanvasSize.x - (pad * 2f);
        float halfGap = 6f;
        float halfWidth = (contentWidth - halfGap) * 0.5f;
        float y = -pad;

        y = PlaceTopRow(panel.transform, "TitleText", "Arc Academy Lab", BobScoreboardDisplay.TitleFontSize,
            FontStyle.Bold, pad, y, contentWidth, 22f, new Color(0.92f, 0.94f, 1f), headline: false);
        y -= 6f;

        y = PlaceTopRow(panel.transform, "EpisodesText", $"{BobScoreboardDisplay.EpisodesLabel}: 0",
            BobScoreboardDisplay.HeadlineFontSize, FontStyle.Bold, pad, y, contentWidth, 28f,
            BobScoreboardDisplay.HeadlineColor, headline: true);
        y -= 4f;

        y = PlaceTopRow(panel.transform, "SuccessText",
            $"{BobScoreboardDisplay.SuccessLabel}: 0%  ·  Rolling: 0%",
            BobScoreboardDisplay.HeadlineFontSize, FontStyle.Bold, pad, y, contentWidth, 28f,
            BobScoreboardDisplay.HeadlineColor, headline: true);
        y -= 4f;

        y = PlaceTopRow(panel.transform, "ArcText",
            $"{BobScoreboardDisplay.ArcLabel}: 0%  ·  Avg: 0%",
            BobScoreboardDisplay.HeadlineFontSize, FontStyle.Bold, pad, y, contentWidth, 28f,
            BobScoreboardDisplay.HeadlineColor, headline: true);
        y -= 4f;

        y = PlaceTopRow(panel.transform, "ScoreText", $"{BobScoreboardDisplay.ScoreLabel}: 0",
            BobScoreboardDisplay.BodyFontSize, FontStyle.Bold, pad, y, contentWidth, 22f,
            BobScoreboardDisplay.ScoreAccentColor, headline: false);
        y -= 4f;

        y = PlaceTopRow(panel.transform, "StatusText", "Play mode", BobScoreboardDisplay.DetailFontSize,
            FontStyle.Italic, pad, y, contentWidth, 18f, new Color(1f, 0.55f, 0.45f), headline: false,
            useDetailStyle: true);
        y -= 4f;

        PlaceTopRow(panel.transform, "RewardsText", "Rewards: +0.0",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad, y, halfWidth, 18f,
            BobScoreboardDisplay.BodyColor, headline: false, useDetailStyle: true);
        PlaceTopRow(panel.transform, "PenaltiesText", "Penalties: -0.0",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad + halfWidth + halfGap, y, halfWidth, 18f,
            BobScoreboardDisplay.BodyColor, headline: false, useDetailStyle: true);
        y -= 18f;
        y -= 4f;

        y = PlaceTopRow(panel.transform, "NetRlText", "Net RL: 0.0",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad, y, contentWidth, 18f,
            BobScoreboardDisplay.BodyColor, headline: false, useDetailStyle: true);
        y -= 4f;

        y = PlaceTopRow(panel.transform, "LastEpisodeText", "Last shot RL: 0.0  ·  Arc: 0%",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad, y, contentWidth, 18f,
            new Color(0.85f, 0.88f, 0.95f), headline: false, useDetailStyle: true);
        y -= 4f;

        PlaceTopRow(panel.transform, "GraphLegendText", "Success · Arc quality (avg 0%)",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad, y, contentWidth, 16f,
            new Color(0.75f, 0.8f, 0.88f), headline: false, useDetailStyle: true);

        var graphRect = CreateUiObject<RectTransform>("GraphImage", panel.transform);
        graphRect.anchorMin = new Vector2(0f, 0f);
        graphRect.anchorMax = new Vector2(1f, 0f);
        graphRect.pivot = new Vector2(0.5f, 0f);
        graphRect.anchoredPosition = new Vector2(0f, pad);
        graphRect.sizeDelta = new Vector2(-(pad * 2f), 48f);
        graphRect.gameObject.AddComponent<RawImage>().color = Color.white;

        hudRoot.AddComponent<BobWallTrainingHud>();
        if (hudRoot.GetComponent<CameraFacingBillboard>() == null)
        {
            hudRoot.AddComponent<CameraFacingBillboard>();
        }

        BobWallHudLayout.ApplyLabHudLayout(arenaRoot);
        EditorUtility.SetDirty(hudRoot);
    }

    private static float PlaceTopRow(
        Transform parent,
        string name,
        string defaultText,
        int fontSize,
        FontStyle style,
        float x,
        float y,
        float width,
        float height,
        Color color,
        bool headline,
        bool useDetailStyle = false)
    {
        CreateLabel(parent, name, defaultText, fontSize, style,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(x, y), new Vector2(width, height), color, headline, useDetailStyle);
        return y - height;
    }

    private static T CreateUiObject<T>(string name, Transform parent) where T : Component
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<T>();
    }

    private static void CreateLabel(
        Transform parent,
        string name,
        string defaultText,
        int fontSize,
        FontStyle style,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color color,
        bool headline,
        bool useDetailStyle = false)
    {
        var rect = CreateUiObject<RectTransform>(name, parent);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var text = rect.gameObject.AddComponent<Text>();
        text.text = defaultText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        if (useDetailStyle)
        {
            BobScoreboardDisplay.ApplyDetailTextStyle(text);
        }
        else if (headline)
        {
            BobScoreboardDisplay.ApplyReadableTextStyle(text, headline: true);
        }
        else
        {
            BobScoreboardDisplay.ApplyReadableTextStyle(text, headline: false);
        }
    }
}
#endif
