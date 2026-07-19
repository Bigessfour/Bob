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
        // ~40 free-throw episodes × ~75 steps ≈ 3000; allow headroom for recording session.
        recorder.NumStepsToRecord = 4000;

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
            + " BehaviorType=HeuristicOnly. HOLD Space/Fire1 to shoot (make-island arc). "
            + "E/Shift=micro elevation nudge, A/D=gentle lateral. Stop Play when done; "
            + "expect "
            + Path.Combine(absDir, DemoName + ".demo"));
        EditorUtility.DisplayDialog(
            "Bob demos",
            "DemonstrationRecorder enabled (HeuristicOnly).\n\n"
                + "1. Press Play\n"
                + "2. HOLD Space (or left mouse) to shoot — waits until you press (won't auto-fire)\n"
                + "3. Default = make-island arc; E/Shift = tiny up/down nudge; A/D = slight lateral\n"
                + "4. Record ~30–50 MAKES (not just attempts)\n"
                + "5. Stop Play → Disable Demonstration Recorder\n"
                + "6. Confirm Assets/Demos/bobfreethrow.demo exists\n"
                + "7. CONFIG=config/bob_free_throw_bc.yaml RUN_ID=bob-v4.3 ./scripts/train.sh --force",
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
