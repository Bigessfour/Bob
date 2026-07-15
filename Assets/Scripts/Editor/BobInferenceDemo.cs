#if UNITY_EDITOR
using Unity.InferenceEngine;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Portfolio inference helper: switch Bob to InferenceOnly when a Sentis/NNModel asset is assigned.
/// Menu: Bob → Demo → Enable Inference (requires model) / Restore Training Behavior.
/// </summary>
public static class BobInferenceDemo
{
    private const string ModelPrefsKey = "Bob.InferenceModelPath";

    [MenuItem("Bob/Demo/Enable Inference Only")]
    public static void EnableInferenceOnly()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob == null)
        {
            EditorUtility.DisplayDialog("Bob inference", "BobAgent not found in the open scene.", "OK");
            return;
        }

        var behavior = bob.GetComponent<BehaviorParameters>();
        if (behavior == null)
        {
            EditorUtility.DisplayDialog("Bob inference", "BehaviorParameters missing on Bob.", "OK");
            return;
        }

        string path = EditorPrefs.GetString(ModelPrefsKey, "Assets/Models/Bob.onnx");
        var model = AssetDatabase.LoadAssetAtPath<ModelAsset>(path);
        if (model == null)
        {
            // ML-Agents 4.x may use NNModel from Inference package alias.
            path = EditorUtility.OpenFilePanel("Select Bob ONNX / NNModel", "Assets", "onnx");
            if (string.IsNullOrEmpty(path))
            {
                return;
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
            EditorUtility.DisplayDialog(
                "Bob inference",
                "No model asset found. Export from results/<run>/Bob after training "
                    + "(mlagents-learn writes .onnx under the run folder), copy into Assets/Models/Bob.onnx, then retry.",
                "OK");
            return;
        }

        behavior.Model = model;
        behavior.BehaviorType = BehaviorType.InferenceOnly;
        EditorUtility.SetDirty(behavior);
        Debug.Log($"BOB_INFERENCE_OK: BehaviorType=InferenceOnly model={path}");
    }

    [MenuItem("Bob/Demo/Restore Training Behavior (Default)")]
    public static void RestoreTrainingBehavior()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob == null)
        {
            return;
        }

        var behavior = bob.GetComponent<BehaviorParameters>();
        if (behavior == null)
        {
            return;
        }

        behavior.BehaviorType = BehaviorType.Default;
        EditorUtility.SetDirty(behavior);
        Debug.Log("BOB_INFERENCE_OK: BehaviorType=Default (training / heuristic handshake)");
    }
}
#endif
