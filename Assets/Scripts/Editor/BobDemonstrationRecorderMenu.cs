#if UNITY_EDITOR
using System.IO;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tier 2 BC tooling: attach/configure <see cref="DemonstrationRecorder"/> on Bob.
/// Record ~30–50 heuristic shots near the make island, then train with
/// <c>CONFIG=config/bob_free_throw_bc.yaml</c>.
/// </summary>
public static class BobDemonstrationRecorderMenu
{
    private const string DemoName = "bobfreethrow";
    private const string DemoRelDir = "Assets/Demos";

    [MenuItem("Bob/Demo/Enable Demonstration Recorder")]
    public static void EnableDemonstrationRecorder()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob == null)
        {
            EditorUtility.DisplayDialog("Bob demos", "BobAgent not found in the open scene.", "OK");
            return;
        }

        EnsureDemosFolder();

        var recorder = bob.GetComponent<DemonstrationRecorder>();
        if (recorder == null)
        {
            recorder = Undo.AddComponent<DemonstrationRecorder>(bob.gameObject);
        }

        string absDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", DemoRelDir));
        Directory.CreateDirectory(absDir);

        recorder.Record = true;
        recorder.DemonstrationName = DemoName;
        recorder.DemonstrationDirectory = absDir;
        // ~50 free-throw episodes × ~80 steps ≈ 4000; allow headroom for make hunting.
        recorder.NumStepsToRecord = 8000;

        var behavior = bob.GetComponent<BehaviorParameters>();
        if (behavior != null)
        {
            Undo.RecordObject(behavior, "Heuristic for demo recording");
            behavior.BehaviorType = BehaviorType.HeuristicOnly;
            EditorUtility.SetDirty(behavior);
        }

        EditorUtility.SetDirty(recorder);
        Debug.Log(
            "BOB_DEMO_RECORDER_OK: Record=true name="
            + DemoName
            + " dir="
            + absDir
            + " BehaviorType=HeuristicOnly. Auto-fires make-island while Record=true "
            + "(or HOLD Space for manual). Stop Play when done; expect "
            + Path.Combine(absDir, DemoName + ".demo"));
        EditorUtility.DisplayDialog(
            "Bob demos",
            "DemonstrationRecorder enabled (HeuristicOnly).\n\n"
                + "1. Press Play — shots auto-fire on the empirical make island\n"
                + "2. Optional: HOLD Space for manual control; E/Shift micro nudge\n"
                + "3. Prefer sessions with many MAKES (check HUD score)\n"
                + "4. Stop Play → Disable Demonstration Recorder\n"
                + "5. Confirm Assets/Demos/bobfreethrow.demo exists\n"
                + "6. CONFIG=config/bob_free_throw_probe_5k_residual_bc.yaml "
                + "RUN_ID=bob-v4.6-residual ./scripts/train.sh --force",
            "OK");
    }

    [MenuItem("Bob/Demo/Disable Demonstration Recorder")]
    public static void DisableDemonstrationRecorder()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob == null)
        {
            return;
        }

        var recorder = bob.GetComponent<DemonstrationRecorder>();
        if (recorder != null)
        {
            recorder.Record = false;
            EditorUtility.SetDirty(recorder);
        }

        var behavior = bob.GetComponent<BehaviorParameters>();
        if (behavior != null)
        {
            behavior.BehaviorType = BehaviorType.Default;
            EditorUtility.SetDirty(behavior);
        }

        Debug.Log("BOB_DEMO_RECORDER_OFF: Record=false BehaviorType=Default");
    }

    private static void EnsureDemosFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Demos"))
        {
            AssetDatabase.CreateFolder("Assets", "Demos");
        }

        string keepPath = Path.Combine(Application.dataPath, "Demos", ".gitkeep");
        if (!File.Exists(keepPath))
        {
            File.WriteAllText(keepPath, "");
            AssetDatabase.Refresh();
        }
    }
}
#endif
