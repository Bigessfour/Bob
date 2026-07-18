#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor helper: mounts compact world-space lab HUD on SimpleArcAcademyArena south wall.
/// Typography comes from <see cref="BobScoreboardDisplay"/> so rebuilds match runtime styles.
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
        panelImage.color = BobScoreboardDisplay.PanelBackgroundColor;
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;

        float pad = SimpleArcAcademyArena.LabHudPanelPadding;
        float contentWidth = SimpleArcAcademyArena.LabHudCanvasSize.x - (pad * 2f);
        float halfGap = 12f;
        float halfWidth = (contentWidth - halfGap) * 0.5f;
        float y = -pad;

        // Title
        y = PlaceTopRow(panel.transform, "TitleText", "Lab Console · RL",
            BobScoreboardDisplay.TitleFontSize, FontStyle.Bold, pad, y, contentWidth, 38f,
            BobScoreboardDisplay.BodyColor, LabelKind.Title);
        y -= 12f;

        // Connection status
        y = PlaceTopRow(panel.transform, "StatusText", "Play mode",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Italic, pad, y, contentWidth, 30f,
            BobScoreboardDisplay.StatusDisconnectedColor, LabelKind.Detail);
        y -= 10f;

        // Hero metrics — taller rows so WallMetricFontSize does not clip
        PlaceTopRow(panel.transform, "EpisodesText", $"{BobScoreboardDisplay.EpisodesLabel}: 0",
            BobScoreboardDisplay.WallMetricFontSize, FontStyle.Bold, pad, y, halfWidth, 36f,
            BobScoreboardDisplay.HeadlineColor, LabelKind.Metric);
        PlaceTopRow(panel.transform, "ScoreText", $"{BobScoreboardDisplay.ScoreLabel}: 0",
            BobScoreboardDisplay.WallMetricFontSize, FontStyle.Bold, pad + halfWidth + halfGap, y, halfWidth, 36f,
            BobScoreboardDisplay.ScoreAccentColor, LabelKind.Score);
        y -= 36f;
        y -= 8f;

        y = PlaceTopRow(panel.transform, "SuccessText",
            $"{BobScoreboardDisplay.SuccessLabel}: 0%  ·  {BobScoreboardDisplay.RollingLabel} 0%",
            BobScoreboardDisplay.WallMetricFontSize, FontStyle.Bold, pad, y, contentWidth, 36f,
            BobScoreboardDisplay.HeadlineColor, LabelKind.Metric);
        y -= 10f;

        PlaceTopRow(panel.transform, "RewardsText", $"{BobScoreboardDisplay.RewardsLabel}: +0.0",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad, y, halfWidth, 30f,
            BobScoreboardDisplay.MutedDetailColor, LabelKind.Detail);
        PlaceTopRow(panel.transform, "PenaltiesText", $"{BobScoreboardDisplay.PenaltiesLabel}: -0.0",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad + halfWidth + halfGap, y, halfWidth, 30f,
            BobScoreboardDisplay.MutedDetailColor, LabelKind.Detail);
        y -= 30f;
        y -= 8f;

        y = PlaceTopRow(panel.transform, "NetRlText", $"{BobScoreboardDisplay.NetRlLabel}: 0.0",
            BobScoreboardDisplay.BodyFontSize, FontStyle.Bold, pad, y, contentWidth, 34f,
            BobScoreboardDisplay.BodyColor, LabelKind.Body);
        y -= 8f;

        y = PlaceTopRow(panel.transform, "LastEpisodeText",
            $"{BobScoreboardDisplay.LastShotRlLabel}: 0.0  ·  {BobScoreboardDisplay.ArcLabel}: 0%",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad, y, contentWidth, 30f,
            BobScoreboardDisplay.MutedDetailColor, LabelKind.Detail);
        y -= 8f;

        y = PlaceTopRow(panel.transform, "ArcText",
            $"{BobScoreboardDisplay.ArcLabel} avg: 0%",
            BobScoreboardDisplay.BodyFontSize, FontStyle.Bold, pad, y, contentWidth, 34f,
            BobScoreboardDisplay.BodyColor, LabelKind.Body);
        y -= 8f;

        PlaceTopRow(panel.transform, "GraphLegendText",
            $"{BobScoreboardDisplay.SuccessLabel} · {BobScoreboardDisplay.ArcLabel} quality",
            BobScoreboardDisplay.DetailFontSize, FontStyle.Normal, pad, y, contentWidth, 26f,
            BobScoreboardDisplay.MutedDetailColor, LabelKind.Detail);

        var graphRect = CreateUiObject<RectTransform>("GraphImage", panel.transform);
        graphRect.anchorMin = new Vector2(0f, 0f);
        graphRect.anchorMax = new Vector2(1f, 0f);
        graphRect.pivot = new Vector2(0.5f, 0f);
        graphRect.anchoredPosition = new Vector2(0f, pad);
        graphRect.sizeDelta = new Vector2(-(pad * 2f), 220f);
        graphRect.gameObject.AddComponent<RawImage>().color = Color.white;

        hudRoot.AddComponent<BobWallTrainingHud>();
        if (hudRoot.GetComponent<CameraFacingBillboard>() == null)
        {
            hudRoot.AddComponent<CameraFacingBillboard>();
        }

        BobWallHudLayout.ApplyLabHudLayout(arenaRoot);
        EditorUtility.SetDirty(hudRoot);
    }

    private enum LabelKind
    {
        Title,
        Metric,
        Score,
        Body,
        Detail,
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
        LabelKind kind)
    {
        CreateLabel(parent, name, defaultText, fontSize, style,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(x, y), new Vector2(width, height), color, kind);
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
        LabelKind kind)
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

        switch (kind)
        {
            case LabelKind.Title:
                BobScoreboardDisplay.ApplyTitleTextStyle(text);
                break;
            case LabelKind.Metric:
                BobScoreboardDisplay.ApplyWallMetricTextStyle(text);
                break;
            case LabelKind.Score:
                BobScoreboardDisplay.ApplyScoreAccentTextStyle(text);
                break;
            case LabelKind.Detail:
                BobScoreboardDisplay.ApplyDetailTextStyle(text);
                break;
            default:
                BobScoreboardDisplay.ApplyReadableTextStyle(text, headline: false);
                break;
        }
    }
}
#endif
