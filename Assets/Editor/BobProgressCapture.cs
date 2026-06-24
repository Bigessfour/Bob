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

    [MenuItem("Bob/Test/Play Single Shot")]
    public static void PlaySingleShotMenu()
    {
        PlaySingleShotSession.Start();
    }

    public static void PlaySingleShotFromCli()
    {
        PlaySingleShotSession.Start();
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

    private const int DefaultPlayCaptureFrames = 180;

    /// <summary>
    /// Runtime-safe view prep before play-mode PNG capture. Do not call ArcTrainingViewValidator here
    /// (uses DestroyImmediate). Mirrors ArcAcademyLabPlayFix + single-shot camera reset.
    /// </summary>
    private static void PreparePlayCaptureView()
    {
        if (SimpleArcAcademyArena.IsLabViewActive)
        {
            ArcAcademyLabSceneCleanup.EnsureLabCamera();
        }

        var orbit = UnityEngine.Object.FindAnyObjectByType<CameraOrbit>();
        orbit?.ResetToDefault();

        TrainingHoopDetail.UpgradeActiveHoop();
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
            SessionState.SetInt(FramesKey, ParseEnvInt("BOB_CAPTURE_PLAY_FRAMES", DefaultPlayCaptureFrames));

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
                    SessionState.SetInt(FramesKey, ParseEnvInt("BOB_CAPTURE_PLAY_FRAMES", DefaultPlayCaptureFrames));
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

            PreparePlayCaptureView();

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

    /// <summary>
    /// Single-shot test flow for Prompt 6: reset to clean start state, EnterPlaymode,
    /// auto-launch ball ("click START") after settle, capture exactly "docs/TrainingView_Success.png" after 3 real seconds,
    /// exit play. Reuses the same play + capture timing pattern as PlayCaptureSession.
    /// </summary>
    private static class PlaySingleShotSession
    {
        private const string ActiveKey = "BobSingleShot.Active";
        private const string DeadlineKey = "BobSingleShot.Deadline";
        private const string LaunchedKey = "BobSingleShot.Launched";

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

        public static void Start()
        {
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(LaunchedKey, false);
            SessionState.SetFloat(DeadlineKey, 0f);

            EditorSceneManager.OpenScene(ScenePath);
            PrepareSingleShotStartState();

            // Run the full one-click polish (Prompt 5) so eyes, camera, hoop, scoreboard, trails are guaranteed
            ArcTrainingViewValidator.FixTrainingView(showDialog: false);

            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!Active) return;

            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    SessionState.SetBool(LaunchedKey, false);
                    SessionState.SetFloat(
                        DeadlineKey,
                        (float)(EditorApplication.timeSinceStartup + 3.0));
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
                return;

            double deadline = SessionState.GetFloat(DeadlineKey, 0f);
            bool launched = SessionState.GetBool(LaunchedKey, false);

            double now = EditorApplication.timeSinceStartup;

            // Launch ball ~0.9s after play for nice flying arc by 3s capture
            if (!launched && (now > (deadline - 2.1)))
            {
                SessionState.SetBool(LaunchedKey, true);
                // Trigger the "START"
                var shooter = UnityEngine.Object.FindAnyObjectByType<BobShootingInput>();
                if (shooter != null)
                {
                    shooter.ForceTestLaunchForSnapshot();
                }
                else
                {
                    // Fallback direct launch of existing ball
                    TryDirectBallLaunch();
                }
            }

            if (now >= deadline)
            {
                EditorApplication.update -= OnEditorUpdate;

                PreparePlayCaptureView();

                var camera = Camera.main;
                if (camera != null)
                {
                    var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? ".";
                    var outPath = Path.Combine(projectRoot, "docs", "TrainingView_Success.png");
                    Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                    bool ok = CaptureCameraToPng(camera, 1280, 720, outPath);
                    if (!ok)
                    {
                        Debug.LogError("SINGLE_SHOT_FAIL: capture failed");
                        ClearSession();
                        EditorApplication.ExitPlaymode();
                        EditorApplication.delayCall += () => EditorApplication.Exit(1);
                        return;
                    }

                    Debug.Log($"SINGLE_SHOT_OK: {outPath}");
                }
                else
                {
                    Debug.LogError("SINGLE_SHOT_FAIL: Main Camera not found");
                    ClearSession();
                    EditorApplication.ExitPlaymode();
                    EditorApplication.delayCall += () => EditorApplication.Exit(1);
                    return;
                }

                ClearSession();
                EditorApplication.ExitPlaymode();
                EditorApplication.delayCall += () => EditorApplication.Exit(0);
            }
        }

        private static void ClearSession()
        {
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseFloat(DeadlineKey);
            SessionState.EraseBool(LaunchedKey);
        }

        private static void TryDirectBallLaunch()
        {
            var ball = UnityEngine.Object.FindAnyObjectByType<SimpleBasketball>();
            if (ball == null) return;
            var rb = ball.GetComponent<Rigidbody>();
            if (rb == null) return;

            // Simple visible arc from near spawn area toward hoop
            Vector3 dir = (new Vector3(0f, 3.5f, -5.5f) - rb.position).normalized;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(dir * 10.5f + Vector3.up * 3.5f, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Resets the training scene to a pristine single-shot start state for the test snapshot.
    /// Bob at spawn, ball at release point with zero velocity, stats cleared, camera to default rig view,
    /// hoop/net upgraded, clean single instances.
    /// </summary>
    private static void PrepareSingleShotStartState()
    {
        // Prefer simple arena for clean AI-Warehouse single training shot look
        try
        {
            SimpleArcAcademyArenaBuilder.ApplySilently();
        }
        catch { /* best effort if already applied */ }

        // Reset metrics
        var stats = UnityEngine.Object.FindAnyObjectByType<BobTrainingStats>();
        stats?.ResetSession();

        // Reset positions + episode
        var manager = UnityEngine.Object.FindAnyObjectByType<SimpleArcArenaManager>();
        if (manager != null)
        {
            manager.ResetEpisode();
        }

        // Position Bob + clear physics
        var bob = UnityEngine.Object.FindAnyObjectByType<BobAgent>();
        if (bob != null)
        {
            Vector3 spawn = (manager != null) ? manager.GetBobSpawnPosition() : bob.transform.position;
            bob.transform.position = spawn;
            var bobRb = bob.GetComponent<Rigidbody>();
            if (bobRb != null) BobPhysicsUtility.ClearVelocitiesIfDynamic(bobRb);
        }

        // Ball at exact release, zeroed
        var spawnGo = GameObject.Find(SimpleArcAcademyArena.SpawnPointName);
        Vector3 bobSpawnForRelease = (bob != null) ? bob.transform.position :
            (spawnGo != null ? spawnGo.transform.position : Vector3.zero);
        Vector3 releasePos = BasketballProjectileSetup.GetReleasePosition(bobSpawnForRelease);

        var ball = UnityEngine.Object.FindAnyObjectByType<SimpleBasketball>();
        if (ball != null)
        {
            var ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                ballRb.isKinematic = false;
                ball.transform.position = releasePos;
                BobPhysicsUtility.ClearVelocitiesIfDynamic(ballRb);
            }
        }
        else
        {
            // Ensure one if missing
            var parent = (bob != null ? bob.transform.parent : null) ?? (manager != null ? manager.transform : null);
            BasketballProjectileSetup.EnsureBasketball(parent, releasePos);
        }

        // Camera to the rational default (CameraRig view)
        var orbit = UnityEngine.Object.FindAnyObjectByType<CameraOrbit>();
        orbit?.ResetToDefault();

        // Hoop/net fully detailed
        TrainingHoopDetail.UpgradeActiveHoop();

        Debug.Log("SINGLE_SHOT_START_STATE: Bob, ball, stats, camera, hoop prepared for clean warehouse snapshot.");
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

            if (mode == "play" && Application.isPlaying)
            {
                PreparePlayCaptureView();
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
        if (volume != null && volume.profile != null)
        {
            // Clone to avoid destroyed override refs (see ArcAcademyLabRenderPreset SSR fix).
            if (volume.profile == volume.sharedProfile)
            {
                volume.profile = UnityEngine.Object.Instantiate(volume.profile);
            }
            if (volume.profile.TryGet(out Exposure exposure))
            {
                restoredExposure = exposure.fixedExposure.value;
                exposure.fixedExposure.overrideState = true;
                exposure.fixedExposure.value = 9f;
            }
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
        if (volume != null && volume.profile != null)
        {
            if (volume.profile == volume.sharedProfile)
            {
                volume.profile = UnityEngine.Object.Instantiate(volume.profile);
            }
            if (volume.profile.TryGet(out Exposure exposureRestore))
            {
                exposureRestore.fixedExposure.value = restoredExposure.Value;
            }
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

    /// <summary>
    /// Snapshot-specific capture wrapper (re-uses private impl + HDRP prep).
    /// </summary>
    private static bool CaptureCameraToPngForSnapshot(Camera camera, int width, int height, string outputPath)
    {
        return CaptureCameraToPng(camera, width, height, outputPath);
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
