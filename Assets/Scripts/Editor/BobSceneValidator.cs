#if UNITY_EDITOR
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BobSceneValidator
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

    public static void VerifyFromCli()
    {
        EditorSceneManager.OpenScene(ScenePath);

        if (GameObject.Find(ArcAcademyLayout.ArenaName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: TrainingArena root missing");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<ArcAcademyManager>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: ArcAcademyManager missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.CourtFloorName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: CourtFloor missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.SpawnPadName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: SpawnPad missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.DistanceMarkingsName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: DistanceMarkings missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.TrainingBaysName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: TrainingBays missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.TrainingBaysBackName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: TrainingBaysBack missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.MountainWindowName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: MountainWindow missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.DecorativeHoopsName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: DecorativeHoops missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var decorative = GameObject.Find(ArcAcademyLayout.DecorativeHoopsName);
        if (decorative.transform.childCount < 2)
        {
            Debug.LogError("VALIDATE_FAIL: DecorativeHoops must contain at least 2 display hoops");
            EditorApplication.Exit(1);
            return;
        }

        var trajectoryRoot = GameObject.Find(ArcAcademyLayout.TrajectoryVisualsName);
        if (trajectoryRoot == null)
        {
            Debug.LogError("VALIDATE_FAIL: TrajectoryVisuals missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (trajectoryRoot.GetComponentsInChildren<LineRenderer>().Length < 3)
        {
            Debug.LogError("VALIDATE_FAIL: TrajectoryVisuals must contain at least 3 arc LineRenderers");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<ArcTrajectoryVisual>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: ArcTrajectoryVisual component missing");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.ReflectionProbeName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: ReflectionProbe missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var hoop = GameObject.Find(ArcAcademyLayout.HoopName);
        if (hoop == null)
        {
            Debug.LogError("VALIDATE_FAIL: Hoop assembly missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (hoop.GetComponent<MovableHoop>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: MovableHoop component missing on Hoop");
            EditorApplication.Exit(1);
            return;
        }

        var rim = hoop.transform.Find(ArcAcademyLayout.RimName);
        if (rim == null)
        {
            Debug.LogError("VALIDATE_FAIL: Rim missing under Hoop");
            EditorApplication.Exit(1);
            return;
        }

        if (rim.Find(ArcAcademyLayout.ScoreZoneName)?.GetComponent<HoopScoreZone>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: ScoreZone trigger missing on Rim");
            EditorApplication.Exit(1);
            return;
        }

        var agent = Object.FindAnyObjectByType<BobAgent>();
        if (agent == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobAgent missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var behavior = agent.GetComponent<BehaviorParameters>();
        if (behavior == null)
        {
            Debug.LogError("VALIDATE_FAIL: BehaviorParameters missing on Bob");
            EditorApplication.Exit(1);
            return;
        }

        if (behavior.BehaviorName != "Bob")
        {
            Debug.LogError($"VALIDATE_FAIL: Expected behavior Bob, got {behavior.BehaviorName}");
            EditorApplication.Exit(1);
            return;
        }

        if (behavior.BehaviorType != BehaviorType.Default)
        {
            Debug.LogError($"VALIDATE_FAIL: Expected BehaviorType Default, got {behavior.BehaviorType}");
            EditorApplication.Exit(1);
            return;
        }

        if (behavior.BrainParameters.VectorObservationSize != 8)
        {
            Debug.LogError("VALIDATE_FAIL: Expected 8 vector observations");
            EditorApplication.Exit(1);
            return;
        }

        if (behavior.BrainParameters.ActionSpec.NumContinuousActions != 3)
        {
            Debug.LogError("VALIDATE_FAIL: Expected 3 continuous actions");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.hoop == null || agent.hoop.name != ArcAcademyLayout.RimName)
        {
            Debug.LogError("VALIDATE_FAIL: Hoop reference not wired to Rim on BobAgent");
            EditorApplication.Exit(1);
            return;
        }

        var rb = agent.GetComponent<Rigidbody>();
        if (rb == null || !rb.useGravity)
        {
            Debug.LogError("VALIDATE_FAIL: Bob Rigidbody must use gravity for arc shots");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("VALIDATE_PASS: Bob training scene is ready for Play mode and training");
        EditorApplication.Exit(0);
    }
}
#endif
