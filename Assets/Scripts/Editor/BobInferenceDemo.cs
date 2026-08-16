#if UNITY_EDITOR
using Unity.InferenceEngine;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Portfolio inference helper. Classmate showcase must use InferenceOnly + ONNX —
/// HeuristicOnly is the analytic solver and must never be labeled as learned policy.
/// </summary>
public static class BobInferenceDemo
{
    public const string DefaultModelPath = "Assets/Models/Bob.onnx";
    private const string ModelPrefsKey = "Bob.InferenceModelPath";

    [MenuItem("Bob/Demo/Prepare Classmate Showcase (Inference ONNX)")]
    public static void PrepareClassmateShowcase()
    {
        if (!TryConfigureInference(forceFilePickerOnMiss: true, out string message))
        {
            EditorUtility.DisplayDialog("Bob showcase", message, "OK");
            return;
        }

        EditorUtility.DisplayDialog(
            "Bob showcase — InferenceOnly",
            "Ready. Confirm the HUD chip says INFERENCE (green), not SOLVER.\n\n"
                + "1. Press Play once\n"
                + "2. Console must contain BOB_INFERENCE_OK\n"
                + "3. Launch actions on the HUD should NOT stay at c=0 every shot\n"
                + "4. Expect ~30–35% makes (v4.8) — not 100%\n\n"
                + message,
            "OK");
    }

    [MenuItem("Bob/Demo/Prepare Solver Wow (Heuristic c≈0)")]
    public static void PrepareSolverWow()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob == null)
        {
            EditorUtility.DisplayDialog("Bob solver demo", "BobAgent not found in the open scene.", "OK");
            return;
        }

        DisableRecorder(bob);

        var behavior = bob.GetComponent<BehaviorParameters>();
        if (behavior == null)
        {
            EditorUtility.DisplayDialog("Bob solver demo", "BehaviorParameters missing on Bob.", "OK");
            return;
        }

        behavior.BehaviorType = BehaviorType.HeuristicOnly;
        EditorUtility.SetDirty(behavior);
        Debug.Log("BOB_SOLVER_DEMO_OK: BehaviorType=HeuristicOnly — analytic prior, not ONNX.");
        EditorUtility.DisplayDialog(
            "Bob solver wow",
            "HeuristicOnly. This is the physics/solver demo (near-100% swish).\n\n"
                + "Do NOT call this the trained policy. After 3–4 makes, switch to\n"
                + "Bob → Demo → Prepare Classmate Showcase.",
            "OK");
    }

    [MenuItem("Bob/Demo/Enable Inference Only")]
    public static void EnableInferenceOnly()
    {
        if (!TryConfigureInference(forceFilePickerOnMiss: true, out string message))
        {
            EditorUtility.DisplayDialog("Bob inference", message, "OK");
            return;
        }

        Debug.Log(message);
    }

    [MenuItem("Bob/Demo/Restore Training Behavior (Default)")]
    public static void RestoreTrainingBehavior()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob == null)
        {
            return;
        }

        DisableRecorder(bob);

        var behavior = bob.GetComponent<BehaviorParameters>();
        if (behavior == null)
        {
            return;
        }

        behavior.BehaviorType = BehaviorType.Default;
        EditorUtility.SetDirty(behavior);
        Debug.Log("BOB_TRAINING_MODE_OK: BehaviorType=Default (Python trainer handshake)");
    }

    private static bool TryConfigureInference(bool forceFilePickerOnMiss, out string message)
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob == null)
        {
            message = "BobAgent not found in the open scene.";
            return false;
        }

        var behavior = bob.GetComponent<BehaviorParameters>();
        if (behavior == null)
        {
            message = "BehaviorParameters missing on Bob.";
            return false;
        }

        DisableRecorder(bob);

        string path = EditorPrefs.GetString(ModelPrefsKey, DefaultModelPath);
        var model = AssetDatabase.LoadAssetAtPath<ModelAsset>(path);
        if (model == null)
        {
            model = AssetDatabase.LoadAssetAtPath<ModelAsset>(DefaultModelPath);
            if (model != null)
            {
                path = DefaultModelPath;
                EditorPrefs.SetString(ModelPrefsKey, path);
            }
        }

        if (model == null && forceFilePickerOnMiss)
        {
            path = EditorUtility.OpenFilePanel("Select Bob ONNX / NNModel", "Assets/Models", "onnx");
            if (string.IsNullOrEmpty(path))
            {
                message = "No model selected.";
                return false;
            }

            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            EditorPrefs.SetString(ModelPrefsKey, path);
            model = AssetDatabase.LoadAssetAtPath<ModelAsset>(path);
        }

        if (model == null)
        {
            message =
                "No model asset found. Copy results/bob-v4.8-tight-prior/Bob.onnx to "
                + DefaultModelPath
                + " and retry.";
            return false;
        }

        behavior.Model = model;
        behavior.BehaviorType = BehaviorType.InferenceOnly;
        EditorUtility.SetDirty(behavior);
        message = $"BOB_INFERENCE_OK: BehaviorType=InferenceOnly model={path}";
        Debug.Log(message);
        return true;
    }

    private static void DisableRecorder(BobAgent bob)
    {
        var recorder = bob.GetComponent<DemonstrationRecorder>();
        if (recorder == null)
        {
            return;
        }

        recorder.Record = false;
        EditorUtility.SetDirty(recorder);
    }
}
#endif
