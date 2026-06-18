using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BobPlayModeSmokeTest
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";
    private const string ExpectedLog = "Bob the Free Throw Champion has entered the arena!";
    private static bool s_Running;
    private static bool s_Passed;

    public static void RunFromCli()
    {
        s_Running = true;
        s_Passed = false;
        Application.logMessageReceived += OnLogMessage;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorSceneManager.OpenScene(ScenePath);
        EditorApplication.EnterPlaymode();
    }

    private static void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (!s_Running || s_Passed)
        {
            return;
        }

        if (condition.Contains(ExpectedLog))
        {
            s_Passed = true;
            Finish(0);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!s_Running)
        {
            return;
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.delayCall += () =>
            {
                if (!s_Passed && Object.FindFirstObjectByType<BobAgent>() == null)
                {
                    Debug.LogError("SMOKE_FAIL: BobAgent not found in play mode");
                    Finish(1);
                }
            };
        }
    }

    private static void Finish(int exitCode)
    {
        if (!s_Running)
        {
            return;
        }

        s_Running = false;
        Application.logMessageReceived -= OnLogMessage;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }

        EditorApplication.delayCall += () => EditorApplication.Exit(exitCode);
    }
}
