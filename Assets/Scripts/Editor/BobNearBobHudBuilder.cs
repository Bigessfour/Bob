#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor helper: large world-space floating scoreboard beside Bob at the free-throw line.
/// </summary>
public static class BobNearBobHudBuilder
{
    public static void EnsureNearBobTrainingHud(Transform arenaRoot)
    {
        if (arenaRoot == null)
        {
            Debug.LogWarning("BOB_NEAR_BOB_HUD_WARN: arena root missing.");
            return;
        }

        var existing = arenaRoot.Find(BobNearBobTrainingHud.RootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var hudRoot = new GameObject(BobNearBobTrainingHud.RootName);
        hudRoot.transform.SetParent(arenaRoot, false);
        hudRoot.transform.position = SimpleArcAcademyArena.NearBobHudWorldPosition;
        hudRoot.transform.localScale = Vector3.one;
        BobPhysicsLayers.SetLayerRecursively(hudRoot, BobPhysicsLayers.DecorationLayer);

        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(hudRoot.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        BobScoreboardDisplay.ConfigureCanvasScaler(canvasGo.AddComponent<CanvasScaler>());
        canvasGo.AddComponent<GraphicRaycaster>();

        var canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.sizeDelta = SimpleArcAcademyArena.NearBobHudCanvasSize;
        canvasRect.localScale = SimpleArcAcademyArena.NearBobHudCanvasScale;
        canvasRect.localPosition = Vector3.zero;
        canvasRect.localRotation = Quaternion.identity;

        var panel = CreateUiObject<RectTransform>("Panel", canvasGo.transform);
        var panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.03f, 0.04f, 0.07f, 0.94f);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;

        float pad = SimpleArcAcademyArena.NearBobHudPanelPadding;
        float contentWidth = SimpleArcAcademyArena.NearBobHudCanvasSize.x - (pad * 2f);
        float y = -pad;

        y = PlaceTopRow(panel.transform, "TitleText", "BOB · LIVE",
            BobScoreboardDisplay.FloatTitleFontSize, FontStyle.Bold, pad, y, contentWidth, 48f,
            new Color(0.92f, 0.94f, 1f));
        y -= 12f;

        y = PlaceTopRow(panel.transform, "EpisodesText", $"{BobScoreboardDisplay.EpisodesLabel}  0",
            BobScoreboardDisplay.FloatHeroFontSize, FontStyle.Bold, pad, y, contentWidth, 88f,
            BobScoreboardDisplay.HeadlineColor);
        y -= 10f;

        y = PlaceTopRow(panel.transform, "ScoreText", $"{BobScoreboardDisplay.ScoreLabel}  0",
            BobScoreboardDisplay.FloatScoreFontSize, FontStyle.Bold, pad, y, contentWidth, 78f,
            BobScoreboardDisplay.ScoreAccentColor);
        y -= 10f;

        y = PlaceTopRow(panel.transform, "SuccessText",
            $"{BobScoreboardDisplay.SuccessLabel}  0%   ·   Rolling 0%",
            BobScoreboardDisplay.FloatSuccessFontSize, FontStyle.Bold, pad, y, contentWidth, 70f,
            BobScoreboardDisplay.HeadlineColor);
        y -= 10f;

        y = PlaceTopRow(panel.transform, "StatusText", "Inference fallback — start ./scripts/train.sh",
            BobScoreboardDisplay.FloatDetailFontSize, FontStyle.Normal, pad, y, contentWidth, 44f,
            new Color(1f, 0.55f, 0.45f));
        y -= 8f;

        y = PlaceTopRow(panel.transform, "LastShotText", "Last  +0.0 RL   ·   Arc 0%   ·   —",
            BobScoreboardDisplay.FloatDetailFontSize, FontStyle.Normal, pad, y, contentWidth, 44f,
            BobScoreboardDisplay.BodyColor);
        y -= 8f;

        PlaceTopRow(panel.transform, "LaunchText", "Launch a=(0,0,0)  F=(0,0,0)  toward 0.00",
            BobScoreboardDisplay.FloatDetailFontSize, FontStyle.Normal, pad, y, contentWidth, 44f,
            BobScoreboardDisplay.BodyColor);

        hudRoot.AddComponent<BobNearBobTrainingHud>();
        if (hudRoot.GetComponent<CameraFacingBillboard>() == null)
        {
            hudRoot.AddComponent<CameraFacingBillboard>();
        }

        BobWallHudLayout.ApplyNearBobHudLayout(arenaRoot);
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
        Color color)
    {
        var rect = CreateUiObject<RectTransform>(name, parent);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);

        var text = rect.gameObject.AddComponent<Text>();
        text.text = defaultText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        BobScoreboardDisplay.ApplyFloatHeroTextStyle(text, fontSize, color, bold: style == FontStyle.Bold);
        return y - height;
    }

    private static T CreateUiObject<T>(string name, Transform parent) where T : Component
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<T>();
    }
}
#endif
