using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Headless training session for batchmode: enter Play, wait for trainer handshake, run for a wall-clock duration, exit.
/// Usage: ./scripts/unity.sh -batchmode -executeMethod BobTrainingSessionRunner.RunFromCli
/// Env: BOB_TRAIN_SESSION_SECONDS (default 300), BOB_TRAIN_CONNECT_TIMEOUT_SECONDS (default 120)
/// Start ./scripts/train.sh first and wait for Listening on port 5004.
/// </summary>
public static class BobTrainingSessionRunner
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";
    private const string ActiveKey = "BobTrainSession.Active";
    private const string EndTimeKey = "BobTrainSession.EndTime";
    private const string ConnectedKey = "BobTrainSession.Connected";

    [InitializeOnLoadMethod]
    private static void Bootstrap()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        if (SessionState.GetBool(ActiveKey, false) && EditorApplication.isPlaying)
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }
    }

    public static void RunFromCli()
    {
        SessionState.SetBool(ActiveKey, true);
        SessionState.SetBool(ConnectedKey, false);
        SessionState.SetFloat(EndTimeKey, 0f);

        EditorSceneManager.OpenScene(ScenePath);
        ArcAcademyHdrpSetup.EnsureHdrpPipeline();
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(ActiveKey, false))
        {
            return;
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            int duration = ParseEnvInt("BOB_TRAIN_SESSION_SECONDS", 300);
            SessionState.SetFloat(EndTimeKey, (float)(EditorApplication.timeSinceStartup + duration));
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            Debug.Log($"BOB_TRAIN_SESSION_START: duration={duration}s (wall clock)");
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.update -= OnEditorUpdate;
            ClearSession();
        }
    }

    private static void OnEditorUpdate()
    {
        if (!SessionState.GetBool(ActiveKey, false) || !EditorApplication.isPlaying)
        {
            return;
        }

        var monitor = BobTrainingConnectionMonitor.Instance;
        if (monitor != null && monitor.IsTrainingConnected)
        {
            SessionState.SetBool(ConnectedKey, true);
        }

        float endTime = SessionState.GetFloat(EndTimeKey, 0f);
        float connectTimeout = ParseEnvInt("BOB_TRAIN_CONNECT_TIMEOUT_SECONDS", 120);

        if (!SessionState.GetBool(ConnectedKey, false)
            && EditorApplication.timeSinceStartup > connectTimeout)
        {
            Debug.LogError(
                "BOB_TRAIN_SESSION_FAIL: No trainer connection within timeout. " +
                "Start ./scripts/train.sh and wait for Listening on port 5004.");
            Finish(1);
            return;
        }

        if (SessionState.GetBool(ConnectedKey, false)
            && EditorApplication.timeSinceStartup >= endTime)
        {
            var stats = BobTrainingStats.Instance;
            int iterations = stats != null ? stats.TotalIterations : 0;
            int points = stats != null ? stats.BasketballPoints : 0;
            Debug.Log(
                $"BOB_TRAIN_SESSION_DONE: iterations={iterations} basketball_points={points}");
            Finish(0);
        }
    }

    private static void Finish(int exitCode)
    {
        EditorApplication.update -= OnEditorUpdate;
        ClearSession();
        EditorApplication.ExitPlaymode();
        EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
    }

    private static void ClearSession()
    {
        SessionState.EraseBool(ActiveKey);
        SessionState.EraseFloat(EndTimeKey);
        SessionState.EraseBool(ConnectedKey);
    }

    private static int ParseEnvInt(string name, int defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return int.TryParse(raw, out var value) && value > 0 ? value : defaultValue;
    }
}
