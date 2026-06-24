#if UNITY_EDITOR
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
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

        if (rim.Find(ArcAcademyLayout.HoopSuccessName)?.GetComponent<HoopScoreZone>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: HoopSuccess trigger missing on Rim");
            EditorApplication.Exit(1);
            return;
        }

        if (!VerifyHoopSuccessTrigger(rim))
        {
            EditorApplication.Exit(1);
            return;
        }

        if (!VerifyRimVisuals(rim))
        {
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
        bool hasOrbit = mainCamera != null && (mainCamera.GetComponentInParent<CameraOrbit>() != null || mainCamera.GetComponent<CameraOrbit>() != null);
        bool hasDemo = mainCamera != null && mainCamera.GetComponent<ArcAcademyDemoCamera>() != null;
        if (mainCamera == null || (!hasOrbit && !hasDemo))
        {
            Debug.LogError("VALIDATE_FAIL: CameraOrbit (preferred) or ArcAcademyDemoCamera missing on Main Camera / rig");
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

        VerifyHalfCourtMarkings(arenaRoot.transform);

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

        if (agent.transform.Find(BobFaceLayout.LeftEyeName) == null
            || agent.transform.Find(BobFaceLayout.RightEyeName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: Bob must have LeftEye and RightEye — rerun arena builder");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.transform.Find("Eye_Left") != null || agent.transform.Find("Eye_Right") != null)
        {
            Debug.LogError("VALIDATE_FAIL: Legacy Eye_Left/Eye_Right quads must be removed from Bob");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.GetComponent<BobEyeFollow>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobEyeFollow missing on Bob");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.GetComponent<BobVisualApplier>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: BobVisualApplier missing on Bob — rerun arena builder");
            EditorApplication.Exit(1);
            return;
        }

        if (!agent.TryGetComponent(out Renderer bobRenderer)
            || BobVisualProfile.IsLikelyMissingBodyMaterial(bobRenderer.sharedMaterial))
        {
            Debug.LogError("VALIDATE_FAIL: Bob body material must be orange lab material — rerun arena builder");
            EditorApplication.Exit(1);
            return;
        }

        if (agent.transform.Find(BobFaceLayout.MouthName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: Bob must have HappyMouth line renderer");
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

        if (!VerifyHoopSuccessTrigger(agent.hoop))
        {
            EditorApplication.Exit(1);
            return;
        }

        if (!VerifyRimVisuals(agent.hoop))
        {
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

        // Enforce no background decorative hoops taking room on the clean court (Prompt 6 / visual north star)
        var mainHoop = GameObject.Find(ArcAcademyLayout.HoopName);
        var extraDecorative = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
            .Count(t => t.name == ArcAcademyLayout.PortableHoopStandName
                        && t.gameObject.activeSelf
                        && (mainHoop == null || !t.IsChildOf(mainHoop.transform)));
        if (extraDecorative > 0)
        {
            Debug.LogError($"VALIDATE_FAIL: {extraDecorative} background decorative hoop stands still active and taking up room on the court. Run Bob → Polish → Fix Training View or simple arena apply.");
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

        var labHud = Object.FindAnyObjectByType<BobWallTrainingHud>();
        if (labHud == null || labHud.transform.parent == null)
        {
            Debug.LogError("VALIDATE_FAIL: LabTrainingHud must be parented under Wall_South");
            EditorApplication.Exit(1);
            return;
        }

        if (labHud.transform.parent.name != SimpleArcAcademyArena.LabHudWallName)
        {
            Debug.LogError("VALIDATE_FAIL: LabTrainingHud must be parented under Wall_South");
            EditorApplication.Exit(1);
            return;
        }

        float hudWorldX = labHud.transform.position.x;
        if (Mathf.Abs(hudWorldX - SimpleArcAcademyArena.LabHudWorldX) > BobWallHudLayout.HudWorldPositionTolerance)
        {
            Debug.LogError(
                $"VALIDATE_FAIL: LabTrainingHud world X must be near back-wall anchor ({SimpleArcAcademyArena.LabHudWorldX}), got {hudWorldX:F2}");
            EditorApplication.Exit(1);
            return;
        }

        float hudWorldZ = labHud.transform.position.z;
        if (Mathf.Abs(hudWorldZ - SimpleArcAcademyArena.LabHudWorldZ) > BobWallHudLayout.HudWorldPositionTolerance)
        {
            Debug.LogError(
                $"VALIDATE_FAIL: LabTrainingHud world Z must be near back wall ({SimpleArcAcademyArena.LabHudWorldZ}), got {hudWorldZ:F2}");
            EditorApplication.Exit(1);
            return;
        }

        var westWall = arenaRoot.transform.Find(SimpleArcAcademyArena.WallWestName);
        if (westWall != null && westWall.Find(BobWallTrainingHud.RootName) != null)
        {
            Debug.LogError("VALIDATE_FAIL: Wall_West must not contain LabTrainingHud");
            EditorApplication.Exit(1);
            return;
        }

        var northWall = arenaRoot.transform.Find(SimpleArcAcademyArena.WallNorthName);
        if (northWall != null && northWall.Find(BobWallTrainingHud.RootName) != null)
        {
            Debug.LogError("VALIDATE_FAIL: Wall_North must not contain LabTrainingHud");
            EditorApplication.Exit(1);
            return;
        }

        if (northWall != null && northWall.childCount > 0)
        {
            Debug.LogError("VALIDATE_FAIL: Wall_North must be a solid wall with no HUD children");
            EditorApplication.Exit(1);
            return;
        }

        if (!BobWallHudLayout.IsFacingCamera(labHud.transform, SimpleArcAcademyArena.LabCameraPosition)
            || !BobWallHudLayout.IsFacingCamera(labHud.transform, SimpleArcAcademyArena.GetHeroCameraPosition))
        {
            Debug.LogError("VALIDATE_FAIL: LabTrainingHud must face both LabHero and Hero cameras");
            EditorApplication.Exit(1);
            return;
        }

        var hudCanvas = labHud.GetComponentInChildren<Canvas>();
        if (hudCanvas == null || hudCanvas.GetComponent<CanvasScaler>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: LabTrainingHud canvas must have CanvasScaler");
            EditorApplication.Exit(1);
            return;
        }

        var episodesHeadline = labHud.transform.Find("Canvas/Panel/EpisodesText")?.GetComponent<Text>();
        if (episodesHeadline == null || episodesHeadline.GetComponent<Outline>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: LabTrainingHud headline text must use Outline for readability");
            EditorApplication.Exit(1);
            return;
        }

        if (labHud.GetComponent<CameraFacingBillboard>() == null)
        {
            Debug.LogError("VALIDATE_FAIL: LabTrainingHud must use CameraFacingBillboard for orbit readability");
            EditorApplication.Exit(1);
            return;
        }

        var arenaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimpleArcAcademyArena.PrefabPath);
        if (arenaPrefab == null)
        {
            Debug.LogError("VALIDATE_FAIL: Prefab_SimpleArena missing at " + SimpleArcAcademyArena.PrefabPath);
            EditorApplication.Exit(1);
            return;
        }

        var prefabSouthWall = arenaPrefab.transform.Find(SimpleArcAcademyArena.LabHudWallName);
        if (prefabSouthWall == null || prefabSouthWall.Find(BobWallTrainingHud.RootName) == null)
        {
            Debug.LogError("VALIDATE_FAIL: Prefab_SimpleArena must bake LabTrainingHud under Wall_South");
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

        bool hasOrbit = mainCamera.GetComponentInParent<CameraOrbit>() != null || mainCamera.GetComponent<CameraOrbit>() != null;
        bool hasDemo = mainCamera.GetComponent<ArcAcademyDemoCamera>() != null;
        if (!hasOrbit && !hasDemo)
        {
            Debug.LogError("VALIDATE_FAIL: CameraOrbit (on rig) or ArcAcademyDemoCamera missing on Main Camera");
            EditorApplication.Exit(1);
            return;
        }

        // Permanent guard against cascade shadow spam (HDRP: only 1 directional may cast shadows).
        var shadowCasters = Object.FindObjectsByType<Light>()
            .Count(l => l != null && l.type == LightType.Directional && l.shadows != LightShadows.None && l.enabled && l.gameObject.activeInHierarchy);
        if (shadowCasters > 1)
        {
            Debug.LogError($"VALIDATE_FAIL: {shadowCasters} directional lights have shadows enabled (only Sun should). Re-apply lab preset or fix light creation.");
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

    private static bool VerifyHoopSuccessTrigger(Transform rim)
    {
        var trigger = rim.Find(ArcAcademyLayout.HoopSuccessName);
        if (trigger == null)
        {
            Debug.LogError("VALIDATE_FAIL: HoopSuccess child missing under active Rim");
            return false;
        }

        if (!trigger.CompareTag(ArcAcademyLayout.HoopSuccessTag))
        {
            Debug.LogError("VALIDATE_FAIL: HoopSuccess trigger must use HoopSuccess tag");
            return false;
        }

        if (trigger.GetComponent<SphereCollider>() != null)
        {
            Debug.LogError("VALIDATE_FAIL: HoopSuccess must use CapsuleCollider, not SphereCollider");
            return false;
        }

        if (!trigger.TryGetComponent(out CapsuleCollider capsule) || !capsule.isTrigger)
        {
            Debug.LogError("VALIDATE_FAIL: HoopSuccess must have trigger CapsuleCollider");
            return false;
        }

        return true;
    }

    private static bool VerifyRimVisuals(Transform rim)
    {
        if (!rim.TryGetComponent(out Renderer rimRenderer) || !rimRenderer.enabled)
        {
            Debug.LogError("VALIDATE_FAIL: Active Rim MeshRenderer must be enabled");
            return false;
        }

        var shaderName = rimRenderer.sharedMaterial != null ? rimRenderer.sharedMaterial.shader.name : string.Empty;
        if (string.IsNullOrEmpty(shaderName) || (!shaderName.Contains("HDRP/Lit") && !shaderName.Contains("Standard")))
        {
            Debug.LogError("VALIDATE_FAIL: Active Rim must use HDRP/Lit material");
            return false;
        }

        return true;
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

    private static void VerifyHalfCourtMarkings(Transform arenaRoot)
    {
        const float tolerance = 0.15f;
        var markings = arenaRoot.Find(SimpleArcCourtMarkingsBuilder.CourtMarkingsName);
        if (markings == null || !markings.gameObject.activeInHierarchy)
        {
            Debug.LogError("VALIDATE_FAIL: CourtMarkings missing or inactive on SimpleArcAcademyArena");
            EditorApplication.Exit(1);
            return;
        }

        if (markings.parent != arenaRoot)
        {
            Debug.LogError("VALIDATE_FAIL: CourtMarkings must be parented to SimpleArcAcademyArena root");
            EditorApplication.Exit(1);
            return;
        }

        var floor = arenaRoot.Find(SimpleArcAcademyArena.FloorName);
        if (floor != null && floor.Find(SimpleArcCourtMarkingsBuilder.CourtMarkingsName) != null)
        {
            Debug.LogError("VALIDATE_FAIL: CourtMarkings nested under scaled Floor");
            EditorApplication.Exit(1);
            return;
        }

        if (markings.Find("KeyPaint") == null || markings.Find("ThreePointArc") == null)
        {
            Debug.LogError("VALIDATE_FAIL: Half-court hero markings incomplete (KeyPaint / ThreePointArc)");
            EditorApplication.Exit(1);
            return;
        }

        var ftLine = markings.Find("FreeThrowLine");
        if (ftLine == null)
        {
            Debug.LogError("VALIDATE_FAIL: FreeThrowLine missing from court markings");
            EditorApplication.Exit(1);
            return;
        }

        float ftWorldZ = arenaRoot.TransformPoint(ftLine.localPosition).z;
        if (Mathf.Abs(ftWorldZ - ArcAcademyLayout.FreeThrowLineWorldZ) > tolerance)
        {
            Debug.LogError(
                $"VALIDATE_FAIL: FreeThrowLine world Z expected {ArcAcademyLayout.FreeThrowLineWorldZ:F2}, got {ftWorldZ:F2}");
            EditorApplication.Exit(1);
        }
    }
}
#endif
