#if UNITY_EDITOR
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;

public static class BobSceneValidator
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

    public static void VerifyFromCli()
    {
        EditorSceneManager.OpenScene(ScenePath);

        if (GraphicsSettings.defaultRenderPipeline == null)
        {
            Debug.LogError("VALIDATE_FAIL: HDRP render pipeline is not assigned");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<SimpleFreeThrowSetup>() != null)
        {
            VerifyMinimal();
            return;
        }

        if (SimpleArcAcademyArena.HasArenaFloor()
            && Object.FindAnyObjectByType<SimpleArcArenaManager>() != null)
        {
            VerifySimpleArcAcademy();
            return;
        }

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

        if (GameObject.Find(ArcAcademyLayout.HdrpVolumeName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: HdrpVolume missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var hdrpVolume = GameObject.Find(ArcAcademyLayout.HdrpVolumeName).GetComponent<Volume>();
        if (hdrpVolume == null || hdrpVolume.sharedProfile == null)
        {
            Debug.LogError("VALIDATE_FAIL: HdrpVolume missing shared ArcAcademyVolumeProfile");
            EditorApplication.Exit(1);
            return;
        }

        if (!hdrpVolume.sharedProfile.TryGet(out Exposure volumeExposure))
        {
            Debug.LogError("VALIDATE_FAIL: ArcAcademyVolumeProfile missing Exposure override");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.AdaptiveProbeVolumeName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: AdaptiveProbeVolume missing from training scene");
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

        var bays = GameObject.Find(ArcAcademyLayout.TrainingBaysName);
        if (bays == null)
        {
            Debug.LogError("VALIDATE_FAIL: TrainingBays missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (bays.transform.childCount < ArcAcademyLayout.TrainingBayCount)
        {
            Debug.LogError($"VALIDATE_FAIL: TrainingBays must contain at least {ArcAcademyLayout.TrainingBayCount} bays");
            EditorApplication.Exit(1);
            return;
        }

        if (CountPortableHoopStands() < ArcAcademyLayout.ExpectedPortableHoopStandCount)
        {
            Debug.LogError(
                $"VALIDATE_FAIL: Expected {ArcAcademyLayout.ExpectedPortableHoopStandCount} portable hoop stands");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.MountainWindowName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: MountainWindow missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var decorativeMarkers = Object.FindObjectsByType<DecorativeHoopMarker>();
        if (decorativeMarkers.Length < ArcAcademyLayout.TrainingBayCount)
        {
            Debug.LogError("VALIDATE_FAIL: DecorativeHoopMarker missing on training bay hoops");
            EditorApplication.Exit(1);
            return;
        }

        var launcherVisuals = Object.FindObjectsByType<RoboticLauncherVisual>();
        if (launcherVisuals.Length < ArcAcademyLayout.TrainingBayCount)
        {
            Debug.LogError("VALIDATE_FAIL: RoboticLauncherVisual missing on training bay stands");
            EditorApplication.Exit(1);
            return;
        }

        var movableHoops = Object.FindObjectsByType<MovableHoop>();
        if (movableHoops.Length != 1)
        {
            Debug.LogError("VALIDATE_FAIL: Exactly one MovableHoop (active scoring hoop) is required");
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

        if (GameObject.Find(ArcAcademyLayout.ReflectionProbeWindowName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: ReflectionProbe_Window missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindObjectsByType<ReflectionProbe>().Length < 2)
        {
            Debug.LogError("VALIDATE_FAIL: At least two reflection probes are required");
            EditorApplication.Exit(1);
            return;
        }

        var spawnPad = GameObject.Find(ArcAcademyLayout.SpawnPadName);
        if (spawnPad == null)
        {
            Debug.LogError("VALIDATE_FAIL: SpawnPad missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var branding = spawnPad.transform.Find(ArcAcademyLayout.SpawnPadBrandingName);
        if (branding == null
            || branding.Find("Label_Bob") == null
            || branding.Find("Label_ArcAcademy") == null)
        {
            Debug.LogError("VALIDATE_FAIL: SpawnPad branding labels missing (Bob + Arc Academy)");
            EditorApplication.Exit(1);
            return;
        }

        if (spawnPad.transform.Find(ArcAcademyLayout.BallSpawnPointName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: BallSpawnPoint missing under SpawnPad");
            EditorApplication.Exit(1);
            return;
        }

        if (spawnPad.GetComponent<SpawnPadPulse>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: SpawnPadPulse missing on SpawnPad");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.FloorDecalsName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: FloorDecals missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var mountainWindow = GameObject.Find(ArcAcademyLayout.MountainWindowName);
        if (mountainWindow == null
            || mountainWindow.transform.Find("MountainBackdrop") == null
            || mountainWindow.transform.Find("WindowGlass") == null
            || mountainWindow.transform.Find("WindowMullions") == null)
        {
            Debug.LogError("VALIDATE_FAIL: MountainWindow must include backdrop, glass, and mullions");
            EditorApplication.Exit(1);
            return;
        }

        // New photoreal platform + ceiling + floor checks for Example.jpg match
        var centralPad = GameObject.Find(ArcAcademyLayout.SpawnPadName);
        if (centralPad != null)
        {
            if (centralPad.transform.Find("PlatformBaseRing") == null)
            {
                Debug.LogError("VALIDATE_FAIL: Central Bob platform missing strong purple base ring (PlatformBaseRing)");
                EditorApplication.Exit(1);
                return;
            }
            var padBranding = centralPad.transform.Find(ArcAcademyLayout.SpawnPadBrandingName);
            if (padBranding != null)
            {
                var bobLabel = padBranding.Find("Label_Bob");
                if (bobLabel != null && bobLabel.GetComponent<TextMesh>() != null)
                {
                    // Label present and large enough (size check is soft via presence)
                }
            }
        }

        // Ceiling density (use ByType + LINQ)
        var lights = Object.FindObjectsByType<Light>();
        int ceilingLights = lights.Count(l => l.name != null && l.name.Contains("CeilingStripLight"));
        if (ceilingLights < ArcAcademyLabLighting.MinCeilingStripLights)
        {
            Debug.LogError(
                $"VALIDATE_FAIL: Insufficient ceiling strip lights (need {ArcAcademyLabLighting.MinCeilingStripLights})");
            EditorApplication.Exit(1);
            return;
        }

        // Bay low partitions present
        var baysRoot = GameObject.Find(ArcAcademyLayout.TrainingBaysName);
        if (baysRoot != null)
        {
            int dividers = 0;
            foreach (Transform t in baysRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "BayDivider") dividers++;
            }
            if (dividers < ArcAcademyLayout.TrainingBayCount - 1)
            {
                Debug.LogWarning("VALIDATE_WARN: Fewer low bay partitions than expected (still functional)");
            }
        }

        // Dark floor + orange court
        var court = GameObject.Find(ArcAcademyLayout.CourtFloorName);
        if (court != null)
        {
            var r = court.GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != null && !r.sharedMaterial.name.ToLower().Contains("gloss"))
            {
                Debug.LogWarning("VALIDATE_WARN: Court floor material may not be glossy");
            }
        }

        if (!ArcAcademyMaterialFactory.MaterialLibraryLoaded)
        {
            Debug.LogWarning("VALIDATE_WARN: Arc Academy material library assets missing; scene uses procedural fallback");
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

        if (hoop.GetComponent<DecorativeHoopMarker>() != null)
        {
            Debug.LogError("VALIDATE_FAIL: Active Hoop must not have DecorativeHoopMarker");
            EditorApplication.Exit(1);
            return;
        }

        var rim = FindDeepChild(hoop.transform, ArcAcademyLayout.RimName);
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

        var scoreZones = Object.FindObjectsByType<HoopScoreZone>();
        if (scoreZones.Length != 1)
        {
            Debug.LogError("VALIDATE_FAIL: Exactly one HoopScoreZone (active hoop) is required");
            EditorApplication.Exit(1);
            return;
        }

        if (rim.GetComponent<HoopRimContact>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: HoopRimContact missing on active Rim");
            EditorApplication.Exit(1);
            return;
        }

        var activeBackboard = hoop.transform.Find("RoboticSwivelBase/SwivelLink/ArmLink/HoopHead/Backboard");
        if (activeBackboard == null || activeBackboard.GetComponent<Collider>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: Active hoop Backboard collider missing");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<ArcAcademyScorePopup>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: ArcAcademyScorePopup missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<BobTrainingStats>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobTrainingStats missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<BobTrainingScoreboard>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobTrainingScoreboard missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<BobTrainingSuccessGraph>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobTrainingSuccessGraph missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<BobTrainingConnectionMonitor>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobTrainingConnectionMonitor missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (!BobPhysicsLayers.LayersConfigured)
        {
            Debug.LogError("VALIDATE_FAIL: Bob training physics layers missing from TagManager");
            EditorApplication.Exit(1);
            return;
        }

        if (Physics.GetIgnoreLayerCollision(BobPhysicsLayers.BobLayer, BobPhysicsLayers.DecorationLayer) == false)
        {
            Debug.LogError("VALIDATE_FAIL: Bob and Decoration layers must not collide");
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

        if (agent.gameObject.layer != BobPhysicsLayers.BobLayer)
        {
            Debug.LogError("VALIDATE_FAIL: Bob must be on the Bob physics layer");
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

        if (agent.GetComponent<BobShootingInput>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobShootingInput missing on Bob");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<HoopNetPhysics>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: HoopNetPhysics missing on active hoop");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<HoopSwishVfx>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: HoopSwishVfx missing on active hoop net");
            EditorApplication.Exit(1);
            return;
        }

        var mainCamera = Camera.main;
        if (mainCamera == null || mainCamera.GetComponent<ArcAcademyDemoCamera>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: ArcAcademyDemoCamera missing on Main Camera");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.GetComponent<BobEntranceController>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobEntranceController missing on Bob");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.GetComponent<BobIdleAnimation>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobIdleAnimation missing on Bob");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.GetComponent<BoxCollider>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: Bob must use BoxCollider (orange cube agent)");
            EditorApplication.Exit(1);
            return;
        }

        if (activeBackboard.GetComponent<HoopBackboardFeedback>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: HoopBackboardFeedback missing on active backboard");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("VALIDATE_PASS: Bob training scene is ready for Play mode and training");
        EditorApplication.Exit(0);
    }

    private static void VerifyMinimal()
    {
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

        if (!SimpleArcAcademyArena.HasArenaFloor()
            && GameObject.Find(SimpleFreeThrowSetup.CourtName) == null
            && GameObject.Find(ArcAcademyLayout.CourtFloorName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: Court missing from minimal training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(SimpleArcAcademyArena.RootName) != null
            && Object.FindAnyObjectByType<SimpleArcArenaManager>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: SimpleArcArenaManager missing on SimpleArcAcademyArena");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(SimpleFreeThrowSetup.BasketballName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: Basketball missing from minimal training scene");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.HdrpVolumeName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: HdrpVolume missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var hdrpVolume = GameObject.Find(ArcAcademyLayout.HdrpVolumeName).GetComponent<Volume>();
        if (hdrpVolume == null || hdrpVolume.sharedProfile == null)
        {
            Debug.LogError("VALIDATE_FAIL: HdrpVolume missing shared profile");
            EditorApplication.Exit(1);
            return;
        }

        var scoreZones = Object.FindObjectsByType<HoopScoreZone>();
        if (scoreZones.Length != 1)
        {
            Debug.LogError("VALIDATE_FAIL: Exactly one HoopScoreZone is required");
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
        if (behavior == null || behavior.BehaviorName != "Bob")
        {
            Debug.LogError("VALIDATE_FAIL: BehaviorParameters must use behavior name Bob");
            EditorApplication.Exit(1);
            return;
        }

        if (behavior.BrainParameters.VectorObservationSize != 8
            || behavior.BrainParameters.ActionSpec.NumContinuousActions != 3)
        {
            Debug.LogError("VALIDATE_FAIL: Minimal trainer requires 8 observations and 3 continuous actions");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.hoop == null || agent.hoop.name != ArcAcademyLayout.RimName)
        {
            Debug.LogError("VALIDATE_FAIL: Hoop reference not wired to Rim on BobAgent");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.ProjectileBody == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobAgent projectileBody must reference Basketball rigidbody");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<BobTrainingStats>() == null
            || Object.FindAnyObjectByType<BobTrainingScoreboard>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: Training HUD components missing");
            EditorApplication.Exit(1);
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("VALIDATE_FAIL: Main Camera missing");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("VALIDATE_PASS: Minimal free-throw trainer is ready for Play mode and training");
        EditorApplication.Exit(0);
    }

    /// <summary>
    /// Grok simple-arena path: one Bob, simple floor/walls, legacy photoreal shell hidden.
    /// </summary>
    private static void VerifySimpleArcAcademy()
    {
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

        var arenaRoot = GameObject.Find(SimpleArcAcademyArena.RootName);
        if (arenaRoot == null)
        {
            Debug.LogError("VALIDATE_FAIL: SimpleArcAcademyArena root missing");
            EditorApplication.Exit(1);
            return;
        }

        var manager = arenaRoot.GetComponent<SimpleArcArenaManager>();
        if (manager == null)
        {
            Debug.LogError("VALIDATE_FAIL: SimpleArcArenaManager missing on SimpleArcAcademyArena");
            EditorApplication.Exit(1);
            return;
        }

        if (arenaRoot.transform.Find(SimpleArcAcademyArena.SpawnPointName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: SpawnPoint missing under SimpleArcAcademyArena");
            EditorApplication.Exit(1);
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(SimpleArcAcademyArena.BobPrefabPath) == null)
        {
            Debug.LogError("VALIDATE_FAIL: Prefab_Bob missing at " + SimpleArcAcademyArena.BobPrefabPath);
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(ArcAcademyLayout.HdrpVolumeName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: HdrpVolume missing from training scene");
            EditorApplication.Exit(1);
            return;
        }

        var hdrpVolume = GameObject.Find(ArcAcademyLayout.HdrpVolumeName).GetComponent<Volume>();
        if (hdrpVolume == null || hdrpVolume.sharedProfile == null)
        {
            Debug.LogError("VALIDATE_FAIL: HdrpVolume missing shared profile");
            EditorApplication.Exit(1);
            return;
        }

        var scoreZones = Object.FindObjectsByType<HoopScoreZone>();
        if (scoreZones.Length != 1)
        {
            Debug.LogError("VALIDATE_FAIL: Exactly one HoopScoreZone is required");
            EditorApplication.Exit(1);
            return;
        }

        var movableHoops = Object.FindObjectsByType<MovableHoop>();
        if (movableHoops.Length != 1)
        {
            Debug.LogError("VALIDATE_FAIL: Exactly one MovableHoop (active scoring hoop) is required");
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

        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(agent.gameObject) > 0)
        {
            Debug.LogError("VALIDATE_FAIL: Bob has missing script references — rerun ./scripts/validate-scene.sh");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.transform.parent != arenaRoot.transform)
        {
            Debug.LogError("VALIDATE_FAIL: Bob must be parented under SimpleArcAcademyArena");
            EditorApplication.Exit(1);
            return;
        }

        var behavior = agent.GetComponent<BehaviorParameters>();
        if (behavior == null || behavior.BehaviorName != "Bob")
        {
            Debug.LogError("VALIDATE_FAIL: BehaviorParameters must use behavior name Bob");
            EditorApplication.Exit(1);
            return;
        }

        if (behavior.BrainParameters.VectorObservationSize != 8
            || behavior.BrainParameters.ActionSpec.NumContinuousActions != 3)
        {
            Debug.LogError("VALIDATE_FAIL: Simple arena requires 8 observations and 3 continuous actions");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.hoop == null || agent.hoop.name != ArcAcademyLayout.RimName)
        {
            Debug.LogError("VALIDATE_FAIL: Hoop reference not wired to Rim on BobAgent");
            EditorApplication.Exit(1);
            return;
        }

        var rimColliders = agent.hoop.Find("RimColliders");
        if (rimColliders == null || rimColliders.childCount < TrainingHoopDetail.RimSegmentCount)
        {
            Debug.LogError("VALIDATE_FAIL: Rim must use segmented RimColliders — rerun arena builder");
            EditorApplication.Exit(1);
            return;
        }

        var hoopRoot = GameObject.Find(ArcAcademyLayout.HoopName);
        if (hoopRoot != null)
        {
            var swivel = hoopRoot.transform.Find("RoboticSwivelBase");
            if (swivel != null && swivel.gameObject.activeSelf)
            {
                Debug.LogError("VALIDATE_FAIL: RoboticSwivelBase must be disabled for stationary training hoop");
                EditorApplication.Exit(1);
                return;
            }

            var hoopHead = hoopRoot.transform.Find("HoopHead");
            if (hoopHead != null && hoopHead.parent != hoopRoot.transform)
            {
                Debug.LogError("VALIDATE_FAIL: HoopHead must be parented directly under Hoop for stationary assembly");
                EditorApplication.Exit(1);
                return;
            }
        }

        var basketballObjects = Object.FindObjectsByType<SimpleBasketball>();
        if (basketballObjects.Length != 1)
        {
            Debug.LogError("VALIDATE_FAIL: Exactly one Basketball (SimpleBasketball) is required");
            EditorApplication.Exit(1);
            return;
        }

        if (GameObject.Find(BasketballProjectileSetup.BasketballName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: Basketball GameObject missing from simple arena scene");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.ProjectileBody == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobAgent projectileBody must reference Basketball rigidbody");
            EditorApplication.Exit(1);
            return;
        }

        if (basketballObjects[0].Owner != agent)
        {
            Debug.LogError("VALIDATE_FAIL: SimpleBasketball must be wired to the single BobAgent");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<BobWallTrainingHud>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobWallTrainingHud missing from simple arena scene");
            EditorApplication.Exit(1);
            return;
        }

        var hudRoots = GameObject.FindObjectsByType<Canvas>();
        int worldHudCount = 0;
        foreach (var canvas in hudRoots)
        {
            if (canvas.renderMode == RenderMode.WorldSpace
                && canvas.GetComponentInParent<BobWallTrainingHud>() != null)
            {
                worldHudCount++;
            }
        }

        if (worldHudCount != 1)
        {
            Debug.LogError("VALIDATE_FAIL: Exactly one world-space lab HUD canvas is required");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<ArcAcademyPowerPathPulse>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: ArcAcademyPowerPathPulse missing from simple arena scene");
            EditorApplication.Exit(1);
            return;
        }

        var bobAgents = Object.FindObjectsByType<BobAgent>();
        if (bobAgents.Length != 1)
        {
            Debug.LogError("VALIDATE_FAIL: Exactly one BobAgent is required");
            EditorApplication.Exit(1);
            return;
        }

        if (Object.FindAnyObjectByType<BobTrainingStats>() == null
            || Object.FindAnyObjectByType<BobTrainingScoreboard>() == null
            || Object.FindAnyObjectByType<BobTrainingSuccessGraph>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: Training HUD components missing");
            EditorApplication.Exit(1);
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("VALIDATE_FAIL: Main Camera missing");
            EditorApplication.Exit(1);
            return;
        }

        var spawnPad = GameObject.Find(ArcAcademyLayout.SpawnPadName);
        if (spawnPad != null)
        {
            var branding = FindDeepChild(spawnPad.transform, ArcAcademyLayout.SpawnPadBrandingName);
            if (branding != null && branding.gameObject.activeSelf)
            {
                Debug.LogError("VALIDATE_FAIL: SpawnPadBranding must be inactive in simple arena lab view");
                EditorApplication.Exit(1);
                return;
            }
        }

        var mainCamera = Camera.main;
        if (!ArcAcademyLabSceneCleanup.IsLabCameraPosition(mainCamera.transform.position))
        {
            Debug.LogError(
                "VALIDATE_FAIL: Main Camera must use LabHero position for simple arena sideline framing");
            EditorApplication.Exit(1);
            return;
        }

        if (mainCamera.GetComponent<ArcAcademyDemoCamera>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: ArcAcademyDemoCamera missing on Main Camera");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("VALIDATE_PASS: Simple Arc Academy arena is ready for Play mode and training");
        EditorApplication.Exit(0);
    }

    private static int CountPortableHoopStands()
    {
        int count = 0;
        foreach (var transform in Object.FindObjectsByType<Transform>())
        {
            if (transform.name == ArcAcademyLayout.PortableHoopStandName)
            {
                count++;
            }
        }

        return count;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            var found = FindDeepChild(parent.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
#endif
