#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// Editor-only guardrails: warn when script compiles during Play (kills ML-Agents handshake)
/// and log a clear message when Play exits while training was active.
/// </summary>
[InitializeOnLoad]
public static class BobTrainingPlayModeGuard
{
    static BobTrainingPlayModeGuard()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        CompilationPipeline.compilationStarted += OnCompilationStarted;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                BobTrainingSessionFlags.ResetPlaySession();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                if (BobTrainingSessionFlags.WasTrainingConnectedThisPlaySession)
                {
                    Debug.LogWarning(
                        "BOB_TRAINING_END: Play mode stopping while trainer was connected. " +
                        "The Python trainer will log 'Communicator has exited' and may crash after repeated restarts. " +
                        "Stop Play only when done training; press Play again only after the terminal shows 'Listening on port 5004'.");
                }

                BobTrainingSessionFlags.ResetPlaySession();
                break;
        }
    }

    private static void OnCompilationStarted(object _)
    {
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        Debug.LogError(
            "BOB_TRAINING_COMPILE_DURING_PLAY: C# scripts are recompiling while Play is active. " +
            "Unity will exit Play and disconnect the trainer. Save script edits only when Play is stopped, " +
            "wait for compile to finish, then start ./scripts/train.sh and press Play once.");
    }
}
#endif
