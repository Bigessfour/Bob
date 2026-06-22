#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor helper: mounts world-space lab HUD on SimpleArcAcademyArena wall.
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
        hudRoot.transform.localPosition = SimpleArcAcademyArena.LabHudLocalPosition;
        hudRoot.transform.localRotation = Quaternion.Euler(SimpleArcAcademyArena.LabHudLocalRotation);
        hudRoot.transform.localScale = Vector3.one;
        BobPhysicsLayers.SetLayerRecursively(hudRoot, BobPhysicsLayers.DecorationLayer);

        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(hudRoot.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
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

        CreateLabel(panel.transform, "TitleText", "Arc Academy Lab", 22, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -12f), new Vector2(360f, 32f), new Color(0.92f, 0.94f, 1f));

        CreateLabel(panel.transform, "IterationsText", "Iterations: 0", 16, FontStyle.Normal,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -44f), new Vector2(320f, 24f), Color.white);
        CreateLabel(panel.transform, "ScoreText", "Score: 0", 18, FontStyle.Bold,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -68f), new Vector2(320f, 26f), new Color(1f, 0.82f, 0.35f));
        CreateLabel(panel.transform, "SuccessText", "Success: 0%", 15, FontStyle.Normal,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -92f), new Vector2(320f, 22f), Color.white);
        CreateLabel(panel.transform, "StatusText", "Play mode", 14, FontStyle.Italic,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -114f), new Vector2(320f, 20f), new Color(1f, 0.55f, 0.45f));
        CreateLabel(panel.transform, "RewardsText", "Rewards: +0", 14, FontStyle.Normal,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -138f), new Vector2(320f, 20f), Color.white);
        CreateLabel(panel.transform, "PenaltiesText", "Penalties: -0", 14, FontStyle.Normal,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -158f), new Vector2(320f, 20f), Color.white);
        CreateLabel(panel.transform, "NetRlText", "Net RL: 0", 14, FontStyle.Normal,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -178f), new Vector2(320f, 20f), Color.white);
        CreateLabel(panel.transform, "LastEpisodeText", "Last shot RL: 0", 13, FontStyle.Normal,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -198f), new Vector2(320f, 20f), new Color(0.85f, 0.88f, 0.95f));
        CreateLabel(panel.transform, "GraphLegendText", "Success · Arc quality", 12, FontStyle.Normal,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -222f), new Vector2(320f, 18f), new Color(0.75f, 0.8f, 0.88f));

        var graphRect = CreateUiObject<RectTransform>("GraphImage", panel.transform);
        graphRect.anchorMin = new Vector2(0f, 0f);
        graphRect.anchorMax = new Vector2(1f, 0f);
        graphRect.pivot = new Vector2(0.5f, 0f);
        graphRect.anchoredPosition = new Vector2(0f, 12f);
        graphRect.sizeDelta = new Vector2(-28f, 72f);
        graphRect.gameObject.AddComponent<RawImage>().color = Color.white;

        hudRoot.AddComponent<BobWallTrainingHud>();
        EditorUtility.SetDirty(hudRoot);
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
        Color color)
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
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }
}
#endif
