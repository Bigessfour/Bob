#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Resets the Game view toolbar Scale slider. Values like 4x make the Game tab look
/// pixelated even when Scene view is fine — a common Unity Editor UX footgun.
/// </summary>
[InitializeOnLoad]
public static class BobGameViewScaleFix
{
    private const float TargetScale = 1f;

    static BobGameViewScaleFix()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode
            || state == PlayModeStateChange.EnteredEditMode)
        {
            // Defer one tick so GameView finishes layout after domain reload / play enter.
            // delayCall requires Action (void); ResetGameViewScaleToOne returns bool.
            EditorApplication.delayCall += () => ResetGameViewScaleToOne();
        }
    }

    [MenuItem("Bob/Polish/Reset Game View Scale (1x)")]
    public static void MenuResetGameViewScale()
    {
        if (ResetGameViewScaleToOne())
        {
            EditorUtility.DisplayDialog(
                "Game View Scale",
                "Game view Scale set to 1x.\n\n"
                + "If the Game tab looked pixelated while Scene looked fine, "
                + "the Scale slider (top of Game view) was zoomed in — not the scene assets.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Game View Scale",
                "Could not find the Game view window.\n"
                + "Open the Game tab, then drag the Scale slider to 1x manually.",
                "OK");
        }
    }

    public static bool ResetGameViewScaleToOne()
    {
        try
        {
            Type gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            if (gameViewType == null)
            {
                return false;
            }

            EditorWindow gameView = null;
            var windows = Resources.FindObjectsOfTypeAll(gameViewType);
            if (windows != null && windows.Length > 0)
            {
                gameView = windows[0] as EditorWindow;
            }

            if (gameView == null)
            {
                gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
            }

            if (gameView == null)
            {
                return false;
            }

            FieldInfo zoomAreaField = gameViewType.GetField(
                "m_ZoomArea",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (zoomAreaField == null)
            {
                return false;
            }

            object zoomArea = zoomAreaField.GetValue(gameView);
            if (zoomArea == null)
            {
                return false;
            }

            FieldInfo scaleField = zoomArea.GetType().GetField(
                "m_Scale",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (scaleField == null)
            {
                return false;
            }

            scaleField.SetValue(zoomArea, new Vector2(TargetScale, TargetScale));

            FieldInfo defaultScaleField = gameViewType.GetField(
                "m_defaultScale",
                BindingFlags.Instance | BindingFlags.NonPublic);
            defaultScaleField?.SetValue(gameView, TargetScale);

            gameView.Repaint();
            Debug.Log("BOB_GAME_VIEW_SCALE_OK: Game view Scale set to 1x.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"BOB_GAME_VIEW_SCALE_WARN: {ex.Message}");
            return false;
        }
    }
}
#endif
