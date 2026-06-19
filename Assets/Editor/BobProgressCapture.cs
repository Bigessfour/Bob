using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public static class BobProgressCapture
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";
    private const string ProgressRoot = "docs/progress";
    private const int DefaultWidth = 1280;
    private const int DefaultHeight = 720;

    [MenuItem("Bob/Capture Progress Screenshot")]
    public static void CaptureFromMenu()
    {
        var label = EditorInputDialog.Show("Progress Screenshot", "Milestone label (slug):", "snapshot");
        if (string.IsNullOrWhiteSpace(label))
        {
            return;
        }

        CaptureWithLabel(label.Trim());
    }

    [MenuItem("Bob/Polish/Capture Wiley Demo")]
    public static void CaptureWileyDemo()
    {
        BobTrainingSceneBuilder.CreateTrainingSceneMenu();
        CaptureWithLabel("wiley-widget-demo");
    }

    private static void CaptureWithLabel(string label)
    {
        if (!TryCapture(label, "edit", out var folderPath, out var error))
        {
            EditorUtility.DisplayDialog("Capture Failed", error, "OK");
            return;
        }

        EditorUtility.DisplayDialog("Capture Saved", $"Screenshot saved to:\n{folderPath}", "OK");
    }

    public static void CaptureFromCli()
    {
        var label = ResolveCaptureLabel();
        if (!TryCapture(label, "edit", out var folderPath, out var error))
        {
            Debug.LogError($"CAPTURE_FAIL: {error}");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"CAPTURE_OK: {folderPath}");
        EditorApplication.Exit(0);
    }

    public static void CapturePlayModeFromCli()
    {
        if (PlayCaptureSession.Active)
        {
            Debug.LogError("CAPTURE_FAIL: Play-mode capture already in progress");
            EditorApplication.Exit(1);
            return;
        }

        var label = ResolveCaptureLabel();
        PlayCaptureSession.Start(label);
    }

    private static string ResolveCaptureLabel()
    {
        var label = Environment.GetEnvironmentVariable("BOB_CAPTURE_LABEL");
        return string.IsNullOrWhiteSpace(label) ? "snapshot" : label.Trim();
    }

    private static class PlayCaptureSession
    {
        private const string ActiveKey = "BobPlayCapture.Active";
        private const string LabelKey = "BobPlayCapture.Label";
        private const string FramesKey = "BobPlayCapture.FramesRemaining";

        public static bool Active => SessionState.GetBool(ActiveKey, false);

        [InitializeOnLoadMethod]
        private static void Bootstrap()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (Active && EditorApplication.isPlaying)
            {
                EditorApplication.update -= OnEditorUpdate;
                EditorApplication.update += OnEditorUpdate;
            }
        }

        public static void Start(string label)
        {
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetString(LabelKey, label);
            SessionState.SetInt(FramesKey, ParseEnvInt("BOB_CAPTURE_PLAY_FRAMES", 120));

            EditorSceneManager.OpenScene(ScenePath);
            ArcAcademyHdrpSetup.EnsureHdrpPipeline();
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!Active)
            {
                return;
            }

            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    SessionState.SetInt(FramesKey, ParseEnvInt("BOB_CAPTURE_PLAY_FRAMES", 120));
                    EditorApplication.update -= OnEditorUpdate;
                    EditorApplication.update += OnEditorUpdate;
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    EditorApplication.update -= OnEditorUpdate;
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    ClearSession();
                    break;
            }
        }

        private static void OnEditorUpdate()
        {
            if (!Active || !EditorApplication.isPlaying)
            {
                return;
            }

            var framesRemaining = SessionState.GetInt(FramesKey, 0);
            if (framesRemaining > 0)
            {
                SessionState.SetInt(FramesKey, framesRemaining - 1);
                return;
            }

            EditorApplication.update -= OnEditorUpdate;

            var label = SessionState.GetString(LabelKey, "snapshot");
            if (!TryCapture(label, "play", out var folderPath, out var error, openScene: false))
            {
                Debug.LogError($"CAPTURE_FAIL: {error}");
                ClearSession();
                EditorApplication.ExitPlaymode();
                EditorApplication.delayCall += () => EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"CAPTURE_OK: {folderPath}");
            ClearSession();
            EditorApplication.ExitPlaymode();
            EditorApplication.delayCall += () => EditorApplication.Exit(0);
        }

        private static void ClearSession()
        {
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseString(LabelKey);
            SessionState.EraseInt(FramesKey);
        }
    }

    private static bool TryCapture(
        string label,
        string mode,
        out string folderPath,
        out string error,
        bool openScene = true)
    {
        folderPath = null;
        error = null;

        try
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                error = "Could not resolve project root";
                return false;
            }

            var progressDir = Path.Combine(projectRoot, ProgressRoot);
            Directory.CreateDirectory(progressDir);

            if (openScene)
            {
                EditorSceneManager.OpenScene(ScenePath);
            }

            var camera = Camera.main;
            if (camera == null)
            {
                error = "Main Camera not found in training scene";
                return false;
            }

            var width = ParseEnvInt("BOB_CAPTURE_WIDTH", DefaultWidth);
            var height = ParseEnvInt("BOB_CAPTURE_HEIGHT", DefaultHeight);
            if (width <= 0 || height <= 0)
            {
                error = "Capture resolution must be positive";
                return false;
            }

            var sequence = GetNextSequence(progressDir);
            var slug = Slugify(label);
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var folderName = $"{sequence:D3}-{date}-{slug}";
            folderPath = Path.Combine(progressDir, folderName);
            Directory.CreateDirectory(folderPath);

            var imagePath = Path.Combine(folderPath, "capture.png");
            if (!CaptureCameraToPng(camera, width, height, imagePath))
            {
                error = "Failed to render camera to PNG";
                return false;
            }

            var meta = new CaptureMeta
            {
                sequence = sequence.ToString("D3", CultureInfo.InvariantCulture),
                label = slug,
                capturedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                mode = mode,
                scene = ScenePath,
                unityVersion = Application.unityVersion,
                gitCommit = Environment.GetEnvironmentVariable("BOB_CAPTURE_GIT_SHA") ?? string.Empty,
                resolution = $"{width}x{height}",
            };

            var metaJson = JsonUtility.ToJson(meta, true);
            File.WriteAllText(Path.Combine(folderPath, "meta.json"), metaJson, Encoding.UTF8);

            RegenerateReadme(progressDir);
            AssetDatabase.Refresh();

            folderPath = Path.Combine(ProgressRoot, folderName).Replace('\\', '/');
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static int ParseEnvInt(string name, int fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }

    private static int GetNextSequence(string progressDir)
    {
        if (!Directory.Exists(progressDir))
        {
            return 1;
        }

        var max = 0;
        foreach (var dir in Directory.GetDirectories(progressDir))
        {
            var name = Path.GetFileName(dir);
            if (name.Length >= 3 && int.TryParse(name.AsSpan(0, 3), out var seq))
            {
                max = Math.Max(max, seq);
            }
        }

        return max + 1;
    }

    private static string Slugify(string label)
    {
        var lower = label.Trim().ToLowerInvariant();
        lower = Regex.Replace(lower, @"[^a-z0-9]+", "-");
        lower = lower.Trim('-');
        return string.IsNullOrEmpty(lower) ? "snapshot" : lower;
    }

    private static float? PrepareHdrpCapture(Camera camera)
    {
        ArcAcademyHdrpSetup.EnsureHdrpPipeline();

        foreach (var probe in UnityEngine.Object.FindObjectsByType<ReflectionProbe>())
        {
            probe.RenderProbe();
        }

        for (int i = 0; i < 3; i++)
        {
            EditorApplication.QueuePlayerLoopUpdate();
            Thread.Sleep(100);
        }

        float? restoredExposure = null;
        Volume volume = UnityEngine.Object.FindAnyObjectByType<Volume>();
        if (volume != null && volume.profile != null && volume.profile.TryGet(out Exposure exposure))
        {
            restoredExposure = exposure.fixedExposure.value;
            exposure.fixedExposure.overrideState = true;
            exposure.fixedExposure.value = 9f;
        }

        return restoredExposure;
    }

    private static void RestoreHdrpExposure(float? restoredExposure)
    {
        if (!restoredExposure.HasValue)
        {
            return;
        }

        Volume volume = UnityEngine.Object.FindAnyObjectByType<Volume>();
        if (volume != null && volume.profile != null
            && volume.profile.TryGet(out Exposure exposureRestore))
        {
            exposureRestore.fixedExposure.value = restoredExposure.Value;
        }
    }

    private static bool CaptureCameraToPng(Camera camera, int width, int height, string outputPath)
    {
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        float? restoredExposure = null;

        var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        try
        {
            restoredExposure = PrepareHdrpCapture(camera);
            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture.active = renderTexture;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            var png = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);

            if (png == null || png.Length == 0)
            {
                return false;
            }

            File.WriteAllBytes(outputPath, png);
            return true;
        }
        finally
        {
            RestoreHdrpExposure(restoredExposure);
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    private static void RegenerateReadme(string progressDir)
    {
        var entries = new List<CaptureEntry>();
        foreach (var dir in Directory.GetDirectories(progressDir).OrderBy(Path.GetFileName))
        {
            var folderName = Path.GetFileName(dir);
            var metaPath = Path.Combine(dir, "meta.json");
            var imagePath = Path.Combine(dir, "capture.png");
            if (!File.Exists(metaPath) || !File.Exists(imagePath))
            {
                continue;
            }

            var metaJson = File.ReadAllText(metaPath, Encoding.UTF8);
            var meta = JsonUtility.FromJson<CaptureMeta>(metaJson);
            entries.Add(new CaptureEntry
            {
                FolderName = folderName,
                Meta = meta,
            });
        }

        var sb = new StringBuilder();
        sb.AppendLine("# Bob Build Progress");
        sb.AppendLine();
        sb.AppendLine("Chronological Unity scene screenshots documenting project milestones.");
        sb.AppendLine();
        sb.AppendLine("## How to capture");
        sb.AppendLine();
        sb.AppendLine("- **Unity Editor:** Bob → Capture Progress Screenshot");
        sb.AppendLine("- **CLI (edit):** `./scripts/capture-progress.sh <milestone-label>`");
        sb.AppendLine("- **CLI (play):** `./scripts/capture-progress.sh --play <milestone-label>`");
        sb.AppendLine();
        sb.AppendLine("See [unity-dev.md](../unity-dev.md#progress-screenshots) for details.");
        sb.AppendLine();

        if (entries.Count == 0)
        {
            sb.AppendLine("_No captures yet._");
        }
        else
        {
            sb.AppendLine("## Gallery");
            sb.AppendLine();
            sb.AppendLine("| # | Date | Label | Mode | Preview |");
            sb.AppendLine("|---|------|-------|------|---------|");
            foreach (var entry in entries)
            {
                var date = entry.Meta.capturedAt.Length >= 10
                    ? entry.Meta.capturedAt.Substring(0, 10)
                    : entry.Meta.capturedAt;
                var preview = $"![{entry.Meta.label}]({entry.FolderName}/capture.png)";
                sb.AppendLine(
                    $"| {entry.Meta.sequence} | {date} | {entry.Meta.label} | {entry.Meta.mode} | {preview} |");
            }
        }

        sb.AppendLine();
        File.WriteAllText(Path.Combine(progressDir, "README.md"), sb.ToString(), Encoding.UTF8);
    }

    [Serializable]
    private class CaptureMeta
    {
        public string sequence;
        public string label;
        public string capturedAt;
        public string mode;
        public string scene;
        public string unityVersion;
        public string gitCommit;
        public string resolution;
    }

    private class CaptureEntry
    {
        public string FolderName;
        public CaptureMeta Meta;
    }
}

internal static class EditorInputDialog
{
    public static string Show(string title, string message, string defaultValue)
    {
        return EditorInputDialogWindow.ShowModal(title, message, defaultValue);
    }
}

internal class EditorInputDialogWindow : EditorWindow
{
    private string _message;
    private string _input;
    private bool _submitted;

    public static string ShowModal(string title, string message, string defaultValue)
    {
        var window = CreateInstance<EditorInputDialogWindow>();
        window.titleContent = new GUIContent(title);
        window._message = message;
        window._input = defaultValue;
        window.minSize = new Vector2(360, 110);
        window.maxSize = new Vector2(360, 110);
        window.ShowModalUtility();
        return window._submitted ? window._input : null;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(_message);
        GUI.SetNextControlName("InputField");
        _input = EditorGUILayout.TextField(_input);

        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.FocusTextInControl("InputField");
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Cancel", GUILayout.Width(80)))
        {
            _submitted = false;
            Close();
        }

        if (GUILayout.Button("Capture", GUILayout.Width(80)))
        {
            _submitted = true;
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }
}
