#if UNITY_EDITOR
using System.IO;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public static class BobTrainingSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

    private static readonly Color CourtOrange = new(0.96f, 0.42f, 0.08f);
    private static readonly Color CourtLine = new(0.98f, 0.98f, 1f);
    private static readonly Color KeyPaint = new(0.82f, 0.72f, 0.55f);
    private static readonly Color WarehouseConcrete = new(0.1f, 0.11f, 0.14f);
    private static readonly Color WarehouseWall = new(0.62f, 0.63f, 0.66f);
    private static readonly Color SpawnPadDark = new(0.08f, 0.08f, 0.1f);
    private static readonly Color BackboardWhite = new(0.92f, 0.92f, 0.88f);
    private static readonly Color RimOrange = new(1f, 0.45f, 0.1f);
    private static readonly Color PoleGray = new(0.35f, 0.35f, 0.38f);
    private static readonly Color JointWhite = new(0.78f, 0.78f, 0.82f);
    private static readonly Color BobOrange = new(1f, 0.38f, 0f);
    private static readonly Color AcademyPurple = new(0.55f, 0.2f, 0.95f);
    private static readonly Color BayTrim = new(0.08f, 0.08f, 0.1f);
    private static readonly Color BayWallWhite = new(0.92f, 0.93f, 0.95f);
    private static readonly Color BeamMetal = new(0.28f, 0.29f, 0.32f);
    private static readonly Color PlatformBlack = new(0.06f, 0.06f, 0.07f);
    private static readonly Color PartitionBlack = new(0.09f, 0.09f, 0.11f);

    [MenuItem("Bob/Create Training Scene")]
    public static void CreateTrainingSceneMenu()
    {
        CreateTrainingScene();
    }

    [MenuItem("Bob/Rebuild Arc Academy (HDRP)")]
    public static void RebuildArcAcademyHdrpMenu()
    {
        CreateTrainingScene();
    }

    public static void CreateTrainingSceneFromCli()
    {
        CreateTrainingScene();
        EditorApplication.Exit(0);
    }

    private static void CreateTrainingScene()
    {
        ArcAcademyHdrpSetup.EnsureHdrpPipeline();
        ArcAcademyShaderGraphSetup.EnsureMaterialLibrary();
        BobPhysicsLayerSetup.EnsureLayersAndCollisionMatrix();

        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var arena = new GameObject(ArcAcademyLayout.ArenaName);
        var manager = arena.AddComponent<ArcAcademyManager>();
        arena.AddComponent<BobTrainingStats>();
        arena.AddComponent<BobTrainingConnectionMonitor>();
        arena.AddComponent<ArcAcademyLabPlayFix>();

        CreateHdrpVolume(arena.transform);
        CreateAdaptiveProbeVolume(arena.transform);
        CreateHdrpSkyAndSun(arena.transform);
        CreateCamera();
        CreateWarehouseShell(arena.transform);
        CreateCourt(arena.transform);
        CreateTrainingBays(arena.transform);
        Transform spawnPad = CreateSpawnPad(arena.transform);
        (Transform rim, MovableHoop movableHoop) = CreateHoop(arena.transform);
        ArcAcademyScorePopup scorePopup = CreateScorePopup(arena.transform, rim);
        Transform ballSpawn = spawnPad.Find(ArcAcademyLayout.BallSpawnPointName);
        CreateBob(rim, ballSpawn);
        CreateBoundaries(arena.transform);
        CreateFloorDecals(arena.transform);
        CreateLightingRig(arena.transform);
        CreateReflectionProbes(arena.transform);
        CreateTrajectoryVisuals(arena.transform, spawnPad, rim);

        manager.WireReferences(movableHoop, spawnPad, ballSpawn, scorePopup);
        manager.SetupForTraining();
        ApplyTrainingPhysicsLayers(arena.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Debug.Log($"Arc Academy HDRP training scene created at {ScenePath}");
    }

    private static void CreateHdrpVolume(Transform parent)
    {
        var volumeGo = new GameObject(ArcAcademyLayout.HdrpVolumeName);
        volumeGo.transform.SetParent(parent);
        var volume = volumeGo.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;
        var profile = ArcAcademyHdrpSetup.LoadVolumeProfile();
        volume.sharedProfile = profile;
        EditorUtility.SetDirty(volume);
    }

    private static void CreateAdaptiveProbeVolume(Transform parent)
    {
        var apvGo = new GameObject(ArcAcademyLayout.AdaptiveProbeVolumeName);
        apvGo.transform.SetParent(parent);
        float midZ = (ArcAcademyLayout.ShellNearZ + ArcAcademyLayout.ShellFarZ) * 0.5f;
        apvGo.transform.position = new Vector3(0f, 4f, midZ);

        var apv = apvGo.AddComponent<ProbeVolume>();
        apv.mode = ProbeVolume.Mode.Local;
        apv.size = new Vector3(24f, 12f, 32f);
    }

    private static void CreateHdrpSkyAndSun(Transform parent)
    {
        var rig = new GameObject(ArcAcademyLayout.HdrpSkyRigName);
        rig.transform.SetParent(parent);

        var sunGo = new GameObject("Sun");
        sunGo.transform.SetParent(rig.transform);
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = ArcAcademyLabLighting.SunLux;
        sun.color = new Color(1f, 0.96f, 0.9f);
        sun.shadows = LightShadows.Soft;
        // Adjusted sun to better light the back mountain panorama (Example.jpg style)
        // Sun angled to send light in through the large back mountain windows (bright interior like Example.jpg)
        sunGo.transform.rotation = Quaternion.Euler(32f, 175f, 0f);
        sunGo.AddComponent<HDAdditionalLightData>();
    }

    private static void CreateCamera()
    {
        var cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        var camera = cameraGo.AddComponent<Camera>();
        var hdCamera = cameraGo.AddComponent<HDAdditionalCameraData>();
        hdCamera.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
        cameraGo.transform.position = ArcAcademyLayout.CameraPosition;
        cameraGo.transform.rotation = Quaternion.LookRotation(
            ArcAcademyLayout.CameraLookAt - ArcAcademyLayout.CameraPosition,
            Vector3.up);
        camera.fieldOfView = ArcAcademyLayout.CameraFieldOfView;
        cameraGo.AddComponent<AudioListener>();
        cameraGo.AddComponent<ArcAcademyDemoCamera>();
        cameraGo.AddComponent<ArcAcademyDemoUi>();
        cameraGo.AddComponent<BobTrainingScoreboard>();
        cameraGo.AddComponent<BobTrainingSuccessGraph>();
    }

    private static void CreateWarehouseShell(Transform parent)
    {
        var shell = new GameObject(ArcAcademyLayout.WarehouseShellName);
        shell.transform.SetParent(parent);

        float shellLength = ArcAcademyLayout.ShellNearZ - ArcAcademyLayout.ShellFarZ;
        float shellCenterZ = (ArcAcademyLayout.ShellNearZ + ArcAcademyLayout.ShellFarZ) * 0.5f;

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "WarehouseFloor";
        floor.transform.SetParent(shell.transform);
        floor.transform.position = new Vector3(0f, -0.02f, shellCenterZ);
        floor.transform.localScale = new Vector3(
            ArcAcademyLayout.ShellHalfWidth / 5f,
            1f,
            shellLength / 10f);
        // Dark glossy surrounding floor (Example.jpg) — orange court is separate overlay
        ArcAcademyMaterialFactory.ApplyMaterial(
            floor,
            ArcAcademyMaterialFactory.GetDarkGlossyFloor(WarehouseConcrete, ArcAcademyLayout.FloorGlossiness));

        var wallLeft = CreateShellWall(shell.transform, "Wall_Left",
            new Vector3(-ArcAcademyLayout.ShellHalfWidth, 4f, shellCenterZ),
            new Vector3(0.4f, 8f, shellLength));
        CreateShellWall(shell.transform, "Wall_Right",
            new Vector3(ArcAcademyLayout.ShellHalfWidth, 4f, shellCenterZ),
            new Vector3(0.4f, 8f, shellLength));
        var wallFar = CreateShellWall(shell.transform, "Wall_Far",
            new Vector3(0f, 4f, ArcAcademyLayout.ShellFarZ),
            new Vector3(ArcAcademyLayout.ShellHalfWidth * 2f, 8f, 0.4f));
        CreateShellWall(shell.transform, "Wall_Near",
            new Vector3(0f, 4f, ArcAcademyLayout.ShellNearZ),
            new Vector3(ArcAcademyLayout.ShellHalfWidth * 2f, 8f, 0.4f));

        AddCorrugatedRibs(wallLeft, shellLength, true);
        AddCorrugatedRibs(wallFar, ArcAcademyLayout.ShellHalfWidth * 2f, false);
        CreateCeilingTrusses(shell.transform, shellCenterZ, shellLength);

        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(shell.transform);
        ceiling.transform.position = new Vector3(0f, ArcAcademyLayout.CeilingHeight, shellCenterZ);
        ceiling.transform.localScale = new Vector3(
            ArcAcademyLayout.ShellHalfWidth / 5f,
            1f,
            shellLength / 10f);
        ceiling.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        ArcAcademyMaterialFactory.ApplyMaterial(ceiling, ArcAcademyMaterialFactory.GetMatteWall(WarehouseWall));
        Object.DestroyImmediate(ceiling.GetComponent<MeshCollider>());

        CreateMountainWindow(shell.transform, shellCenterZ);          // side accent window
        CreateBackMountainPanorama(shell.transform, shellCenterZ);   // dominant back-wall vista like Example.jpg

        // Hide the solid far wall renderer so the back panorama is visible (the panorama fills the opening)
        var farRenderer = wallFar.GetComponent<MeshRenderer>();
        if (farRenderer != null)
        {
            farRenderer.enabled = false;
        }
    }

    private static void CreateCeilingTrusses(Transform shell, float shellCenterZ, float shellLength)
    {
        var trussRoot = new GameObject("CeilingTrusses");
        trussRoot.transform.SetParent(shell);

        int trussCount = ArcAcademyLayout.CeilingTrussCount;
        float trussSpacing = (shellLength * 0.9f) / Mathf.Max(1, trussCount - 1);
        for (int i = 0; i < trussCount; i++)
        {
            float x = Mathf.Lerp(-4.5f, 4.5f, i / (float)(trussCount - 1));
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = $"TrussBeam_{i}";
            beam.transform.SetParent(trussRoot.transform);
            beam.transform.position = new Vector3(x, ArcAcademyLayout.CeilingHeight - 0.38f, shellCenterZ);
            beam.transform.localScale = new Vector3(0.22f, 0.32f, shellLength * 0.96f);
            ArcAcademyMaterialFactory.ApplyMaterial(beam, ArcAcademyMaterialFactory.GetMetal(BeamMetal));
            Object.DestroyImmediate(beam.GetComponent<BoxCollider>());
        }

        // Cross beams + dense rectangular lights for industrial warehouse ceiling (Example.jpg)
        int rows = ArcAcademyLayout.CeilingLightRows;
        int cols = ArcAcademyLayout.CeilingLightColsPerRow;
        for (int row = 0; row < rows; row++)
        {
            float z = Mathf.Lerp(shellCenterZ - shellLength * 0.38f, shellCenterZ + shellLength * 0.38f, row / (float)(rows - 1));
            var cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cross.name = $"TrussCross_{row}";
            cross.transform.SetParent(trussRoot.transform);
            cross.transform.position = new Vector3(0f, ArcAcademyLayout.CeilingHeight - 0.52f, z);
            cross.transform.localScale = new Vector3(ArcAcademyLayout.ShellHalfWidth * 1.65f, 0.16f, 0.20f);
            ArcAcademyMaterialFactory.ApplyMaterial(cross, ArcAcademyMaterialFactory.GetMetal(BeamMetal));
            Object.DestroyImmediate(cross.GetComponent<BoxCollider>());

            for (int c = 0; c < cols; c++)
            {
                float lx = Mathf.Lerp(-ArcAcademyLayout.ShellHalfWidth * 0.7f, ArcAcademyLayout.ShellHalfWidth * 0.7f, c / (float)(cols - 1));
                var housing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                housing.name = $"DownlightHousing_{row}_{c}";
                housing.transform.SetParent(trussRoot.transform);
                housing.transform.position = new Vector3(lx, ArcAcademyLayout.CeilingHeight - 0.72f, z);
                housing.transform.localScale = new Vector3(0.28f, 0.07f, 0.28f);
                ArcAcademyMaterialFactory.ApplyMaterial(housing, ArcAcademyMaterialFactory.GetMetal(PoleGray));
                Object.DestroyImmediate(housing.GetComponent<Collider>());

                var stripLightGo = new GameObject($"CeilingStripLight_{row}_{c}");
                stripLightGo.transform.SetParent(trussRoot.transform);
                stripLightGo.transform.position = new Vector3(lx, ArcAcademyLayout.CeilingHeight - 0.58f, z);
                stripLightGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                var strip = stripLightGo.AddComponent<Light>();
                strip.type = LightType.Rectangle;
                strip.areaSize = new Vector2(2.6f, 0.32f);
                strip.intensity = ArcAcademyLabLighting.CeilingStripLumen;
                strip.color = new Color(0.97f, 0.98f, 1f);
                stripLightGo.AddComponent<HDAdditionalLightData>();
            }
        }
    }

    private static GameObject CreateShellWall(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        ArcAcademyMaterialFactory.ApplyMaterial(wall, ArcAcademyMaterialFactory.GetMatteWall(WarehouseWall));
        Object.DestroyImmediate(wall.GetComponent<BoxCollider>());
        return wall;
    }

    private static void AddCorrugatedRibs(GameObject wall, float span, bool alongZ)
    {
        int ribCount = Mathf.Max(8, (int)(span / 1.2f));
        var ribs = new GameObject("CorrugatedRibs");
        ribs.transform.SetParent(wall.transform, false);

        for (int i = 0; i < ribCount; i++)
        {
            var rib = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rib.name = $"Rib_{i}";
            rib.transform.SetParent(ribs.transform, false);
            float t = (i + 0.5f) / ribCount - 0.5f;
            if (alongZ)
            {
                rib.transform.localPosition = new Vector3(0.52f, 0f, t * 0.95f);
                rib.transform.localScale = new Vector3(0.08f, 0.95f, 0.06f);
            }
            else
            {
                rib.transform.localPosition = new Vector3(t * 0.95f, 0f, 0.52f);
                rib.transform.localScale = new Vector3(0.06f, 0.95f, 0.08f);
            }

            ArcAcademyMaterialFactory.ApplyMaterial(
                rib,
                ArcAcademyMaterialFactory.GetMatteWall(new Color(0.48f, 0.49f, 0.52f)));
            Object.DestroyImmediate(rib.GetComponent<BoxCollider>());
        }
    }

    private static void CreateMountainWindow(Transform shell, float shellCenterZ)
    {
        // Large panoramic rear/side mountain windows (Example.jpg scale) using mountain_backdrop texture
        var windowRoot = new GameObject(ArcAcademyLayout.MountainWindowName);
        windowRoot.transform.SetParent(shell);
        windowRoot.transform.position = new Vector3(
            -ArcAcademyLayout.ShellHalfWidth + 0.08f,
            ArcAcademyLayout.MountainWindowY,
            shellCenterZ);

        float w = ArcAcademyLayout.MountainWindowWidth;
        float h = ArcAcademyLayout.MountainWindowHeight;

        var recess = GameObject.CreatePrimitive(PrimitiveType.Cube);
        recess.name = "WindowRecess";
        recess.transform.SetParent(windowRoot.transform, false);
        recess.transform.localPosition = new Vector3(-0.18f, 0f, 0f);
        recess.transform.localScale = new Vector3(0.42f, h + 0.4f, w + 0.6f);
        ArcAcademyMaterialFactory.ApplyMaterial(recess, ArcAcademyMaterialFactory.GetMatteWall(new Color(0.07f, 0.08f, 0.10f)));
        Object.DestroyImmediate(recess.GetComponent<BoxCollider>());

        var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backdrop.name = "MountainBackdrop";
        backdrop.transform.SetParent(windowRoot.transform, false);
        backdrop.transform.localPosition = new Vector3(-0.32f, 0f, 0f);
        backdrop.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        backdrop.transform.localScale = new Vector3(w * 0.98f, h * 0.92f, 1f);
        // Use Unlit for the mountain vista quad so it renders bright and full like the reference
        ArcAcademyMaterialFactory.ApplyMaterial(backdrop, ArcAcademyMaterialFactory.GetUnlitMountainBackdrop());
        Object.DestroyImmediate(backdrop.GetComponent<Collider>());

        var windowGlass = GameObject.CreatePrimitive(PrimitiveType.Quad);
        windowGlass.name = "WindowGlass";
        windowGlass.transform.SetParent(windowRoot.transform, false);
        windowGlass.transform.localPosition = new Vector3(0.05f, 0f, 0f);
        windowGlass.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        windowGlass.transform.localScale = new Vector3(w * 0.95f, h * 0.88f, 1f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            windowGlass,
            ArcAcademyMaterialFactory.GetGlass(new Color(0.90f, 0.94f, 1f)));
        Object.DestroyImmediate(windowGlass.GetComponent<Collider>());

        // Large frames
        float halfW = w * 0.5f;
        float halfH = h * 0.5f;
        CreateWindowFrame(windowRoot.transform, "WindowFrame_Top", new Vector3(0f, halfH + 0.05f, 0f), new Vector3(0.14f, w + 0.3f, 0.16f));
        CreateWindowFrame(windowRoot.transform, "WindowFrame_Bottom", new Vector3(0f, -halfH - 0.05f, 0f), new Vector3(0.14f, w + 0.3f, 0.16f));
        CreateWindowFrame(windowRoot.transform, "WindowFrame_Left", new Vector3(0f, 0f, -halfW - 0.05f), new Vector3(0.14f, h + 0.3f, 0.16f));
        CreateWindowFrame(windowRoot.transform, "WindowFrame_Right", new Vector3(0f, 0f, halfW + 0.05f), new Vector3(0.14f, h + 0.3f, 0.16f));
        CreateWindowMullions(windowRoot.transform, w, h);

        var windowLightGo = new GameObject("WindowFillLight");
        windowLightGo.transform.SetParent(windowRoot.transform, false);
        windowLightGo.transform.localPosition = new Vector3(-1.6f, 0.6f, 0f);
        windowLightGo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        var windowArea = windowLightGo.AddComponent<Light>();
        windowArea.type = LightType.Rectangle;
        windowArea.areaSize = new Vector2(w * 0.85f, h * 0.75f);
        windowArea.intensity = ArcAcademyLabLighting.WindowFillLumen;
        windowArea.color = new Color(0.86f, 0.92f, 1f);
        windowLightGo.AddComponent<HDAdditionalLightData>();
    }

    private static void CreateWindowMullions(Transform windowRoot, float width = 16f, float height = 5.5f)
    {
        var mullions = new GameObject("WindowMullions");
        mullions.transform.SetParent(windowRoot, false);

        int count = Mathf.Clamp((int)(width / 2.4f), 5, 9);
        for (int i = -count / 2; i <= count / 2; i++)
        {
            if (i == 0) continue;

            var mullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mullion.name = $"Mullion_{i}";
            mullion.transform.SetParent(mullions.transform, false);
            mullion.transform.localPosition = new Vector3(0.03f, 0f, i * (width / count));
            mullion.transform.localScale = new Vector3(0.07f, height * 0.92f, 0.09f);
            ArcAcademyMaterialFactory.ApplyMaterial(mullion, ArcAcademyMaterialFactory.GetMetal(BeamMetal));
            Object.DestroyImmediate(mullion.GetComponent<BoxCollider>());
        }
    }

    private static void CreateWindowFrame(Transform parent, string name, Vector3 localPos, Vector3 scale)
    {
        var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = name;
        frame.transform.SetParent(parent, false);
        frame.transform.localPosition = localPos;
        frame.transform.localScale = scale;
        ArcAcademyMaterialFactory.ApplyMaterial(frame, ArcAcademyMaterialFactory.GetMetal(BeamMetal));
        Object.DestroyImmediate(frame.GetComponent<BoxCollider>());
    }

    /// <summary>
    /// Large back-wall panoramic mountain vista (the main visual feature from Example.jpg).
    /// Placed on the far wall so hero camera down the court sees Colorado mountains behind the bays.
    /// </summary>
    private static void CreateBackMountainPanorama(Transform shell, float shellCenterZ)
    {
        var backRoot = new GameObject("BackMountainPanorama");
        backRoot.transform.SetParent(shell);
        // Place slightly inside the room (higher Z than far wall at ShellFarZ) so it is visible from camera
        backRoot.transform.position = new Vector3(0f, ArcAcademyLayout.MountainWindowY, ArcAcademyLayout.ShellFarZ + 0.35f);

        float w = ArcAcademyLayout.MountainWindowWidth * 1.05f;   // wider for back wall
        float h = ArcAcademyLayout.MountainWindowHeight * 1.05f;

        // Recess "into" the wall plane (negative local Z is toward exterior)
        var recess = GameObject.CreatePrimitive(PrimitiveType.Cube);
        recess.name = "BackWindowRecess";
        recess.transform.SetParent(backRoot.transform, false);
        recess.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        recess.transform.localScale = new Vector3(w + 0.8f, h + 0.5f, 0.35f);
        ArcAcademyMaterialFactory.ApplyMaterial(recess, ArcAcademyMaterialFactory.GetMatteWall(new Color(0.07f, 0.08f, 0.10f)));
        Object.DestroyImmediate(recess.GetComponent<BoxCollider>());

        // Mountain backdrop facing toward +Z (toward viewer inside the room)
        var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backdrop.name = "BackMountainBackdrop";
        backdrop.transform.SetParent(backRoot.transform, false);
        backdrop.transform.localPosition = new Vector3(0f, 0f, 0f);
        // Quad faces +Z by default
        backdrop.transform.localScale = new Vector3(w * 0.98f, h * 0.92f, 1f);
        // Use Unlit for the actual mountain vista quad so it renders bright and full like the reference windows
        ArcAcademyMaterialFactory.ApplyMaterial(backdrop, ArcAcademyMaterialFactory.GetUnlitMountainBackdrop());
        Object.DestroyImmediate(backdrop.GetComponent<Collider>());

        // Glass in front of backdrop
        var glass = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glass.name = "BackWindowGlass";
        glass.transform.SetParent(backRoot.transform, false);
        glass.transform.localPosition = new Vector3(0f, 0f, 0.08f);
        glass.transform.localScale = new Vector3(w * 0.95f, h * 0.88f, 1f);
        var glassMat = ArcAcademyMaterialFactory.GetGlass(new Color(0.90f, 0.94f, 1f));
        // More transparent glass for clear mountain vista through the big back windows
        if (glassMat.HasProperty("_BaseColor"))
        {
            var c = glassMat.GetColor("_BaseColor");
            glassMat.SetColor("_BaseColor", new Color(c.r, c.g, c.b, 0.15f));
        }
        ArcAcademyMaterialFactory.ApplyMaterial(glass, glassMat);
        Object.DestroyImmediate(glass.GetComponent<Collider>());

        // Frames for the big back window
        float halfW = w * 0.5f;
        float halfH = h * 0.5f;
        CreateWindowFrame(backRoot.transform, "BackWindowFrame_Top",    new Vector3(0f,  halfH + 0.06f, 0f), new Vector3(w + 0.4f, 0.15f, 0.14f));
        CreateWindowFrame(backRoot.transform, "BackWindowFrame_Bottom", new Vector3(0f, -halfH - 0.06f, 0f), new Vector3(w + 0.4f, 0.15f, 0.14f));
        CreateWindowFrame(backRoot.transform, "BackWindowFrame_Left",   new Vector3(-halfW - 0.06f, 0f, 0f), new Vector3(0.15f, h + 0.4f, 0.14f));
        CreateWindowFrame(backRoot.transform, "BackWindowFrame_Right",  new Vector3( halfW + 0.06f, 0f, 0f), new Vector3(0.15f, h + 0.4f, 0.14f));

        // Vertical mullions
        var mullions = new GameObject("BackWindowMullions");
        mullions.transform.SetParent(backRoot.transform, false);
        int count = Mathf.Clamp((int)(w / 2.6f), 6, 10);
        for (int i = -count / 2; i <= count / 2; i++)
        {
            if (i == 0) continue;
            var m = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m.name = $"BackMullion_{i}";
            m.transform.SetParent(mullions.transform, false);
            m.transform.localPosition = new Vector3(i * (w / count), 0f, 0f);
            m.transform.localScale = new Vector3(0.08f, h * 0.9f, 0.10f);
            ArcAcademyMaterialFactory.ApplyMaterial(m, ArcAcademyMaterialFactory.GetMetal(BeamMetal));
            Object.DestroyImmediate(m.GetComponent<BoxCollider>());
        }

        // Make backdrop and glass not receive shadows and not cast (pure emissive vista)
        foreach (var goName in new[] { "BackMountainBackdrop", "BackWindowGlass" })
        {
            var go = backRoot.transform.Find(goName);
            if (go != null)
            {
                var rend = go.GetComponent<MeshRenderer>();
                if (rend != null)
                {
                    rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    rend.receiveShadows = false;
                }
            }
        }

        // Strong fill light from the mountains (inside the room)
        var fill = new GameObject("BackMountainFill");
        fill.transform.SetParent(backRoot.transform, false);
        fill.transform.localPosition = new Vector3(0f, 0f, 2.5f);
        var area = fill.AddComponent<Light>();
        area.type = LightType.Rectangle;
        area.areaSize = new Vector2(w * 0.9f, h * 0.8f);
        area.intensity = ArcAcademyLabLighting.WindowFillLumen;
        area.color = new Color(0.85f, 0.91f, 1f);
        fill.AddComponent<HDAdditionalLightData>();
    }

    private static void CreateCourt(Transform parent)
    {
        float courtLength = ArcAcademyLayout.CourtNearZ - ArcAcademyLayout.CourtFarZ;
        float courtCenterZ = (ArcAcademyLayout.CourtNearZ + ArcAcademyLayout.CourtFarZ) * 0.5f;

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = ArcAcademyLayout.CourtFloorName;
        floor.transform.SetParent(parent);
        floor.transform.position = new Vector3(0f, 0.01f, courtCenterZ);
        floor.transform.localScale = new Vector3(
            ArcAcademyLayout.CourtHalfWidth / 5f,
            1f,
            courtLength / 10f);
        ArcAcademyMaterialFactory.ApplyMaterial(floor, ArcAcademyMaterialFactory.GetGlossyFloor(CourtOrange));

        var markings = new GameObject(ArcAcademyLayout.CourtMarkingsName);
        markings.transform.SetParent(parent);

        float baselineZ = ArcAcademyLayout.HoopRootDefaultPosition.z;
        CreateLineMark(markings.transform, "Baseline", new Vector3(0f, 0.03f, baselineZ),
            new Vector3(ArcAcademyLayout.CourtHalfWidth * 2f, 0.04f, 0.08f), CourtLine);
        CreateLineMark(markings.transform, "FreeThrowLine", new Vector3(0f, 0.03f, ArcAcademyLayout.FreeThrowLineZ),
            new Vector3(ArcAcademyLayout.KeyHalfWidth * 2f, 0.04f, 0.08f), CourtLine);

        float keyFrontZ = baselineZ + ArcAcademyLayout.KeyDepthFromBaseline;
        CreateLineMark(markings.transform, "KeyLeft",
            new Vector3(-ArcAcademyLayout.KeyHalfWidth, 0.03f, (baselineZ + keyFrontZ) * 0.5f),
            new Vector3(0.08f, 0.04f, ArcAcademyLayout.KeyDepthFromBaseline), CourtLine);
        CreateLineMark(markings.transform, "KeyRight",
            new Vector3(ArcAcademyLayout.KeyHalfWidth, 0.03f, (baselineZ + keyFrontZ) * 0.5f),
            new Vector3(0.08f, 0.04f, ArcAcademyLayout.KeyDepthFromBaseline), CourtLine);
        CreateLineMark(markings.transform, "KeyFront", new Vector3(0f, 0.03f, keyFrontZ),
            new Vector3(ArcAcademyLayout.KeyHalfWidth * 2f, 0.04f, 0.08f), CourtLine);

        var keyFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyFill.name = "KeyPaint";
        keyFill.transform.SetParent(markings.transform);
        keyFill.transform.position = new Vector3(0f, 0.025f, (baselineZ + keyFrontZ) * 0.5f);
        keyFill.transform.localScale = new Vector3(
            ArcAcademyLayout.KeyHalfWidth * 2f,
            0.03f,
            ArcAcademyLayout.KeyDepthFromBaseline);
        ArcAcademyMaterialFactory.ApplyMaterial(keyFill, ArcAcademyMaterialFactory.GetMatteWall(KeyPaint));
        Object.DestroyImmediate(keyFill.GetComponent<BoxCollider>());

        CreateThreePointArc(markings.transform, baselineZ);
        CreateCenterCircle(markings.transform, courtCenterZ);

        var distanceMarks = new GameObject(ArcAcademyLayout.DistanceMarkingsName);
        distanceMarks.transform.SetParent(parent);
        for (int i = 0; i < ArcAcademyLayout.DistanceMarkOffsetsFromBaseline.Length; i++)
        {
            float offset = ArcAcademyLayout.DistanceMarkOffsetsFromBaseline[i];
            float markZ = baselineZ + offset;
            CreateLineMark(distanceMarks.transform, $"DistanceMark_{offset:0}m",
                new Vector3(0f, 0.035f, markZ),
                new Vector3(ArcAcademyLayout.CourtHalfWidth * 2f, 0.04f, 0.06f), CourtLine);
        }
    }

    private static void CreateThreePointArc(Transform parent, float baselineZ)
    {
        var arcRoot = new GameObject("ThreePointArc");
        arcRoot.transform.SetParent(parent);
        int segments = 36;
        float radius = ArcAcademyLayout.ThreePointArcRadius;

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(200f, 340f, i / (float)segments) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = baselineZ + Mathf.Sin(angle) * radius;
            CreateLineMark(arcRoot.transform, $"ThreePt_{i}",
                new Vector3(x, 0.032f, z),
                new Vector3(0.12f, 0.04f, 0.12f), CourtLine);
        }
    }

    private static void CreateCenterCircle(Transform parent, float centerZ)
    {
        var circleRoot = new GameObject("CenterCircle");
        circleRoot.transform.SetParent(parent);
        int segments = 24;
        float radius = ArcAcademyLayout.CenterCircleRadius;

        for (int i = 0; i < segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = centerZ + Mathf.Sin(angle) * radius;
            CreateLineMark(circleRoot.transform, $"Center_{i}",
                new Vector3(x, 0.032f, z),
                new Vector3(0.1f, 0.04f, 0.1f), CourtLine);
        }
    }

    private static Transform CreateSpawnPad(Transform parent)
    {
        // Central black elevated "Bob" platform — dominant in Example.jpg with strong purple glow + branding.
        var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = ArcAcademyLayout.SpawnPadName;
        pad.transform.SetParent(parent);
        pad.transform.position = ArcAcademyLayout.SpawnPadPosition;
        pad.transform.localScale = ArcAcademyLayout.SpawnPadScale;
        ArcAcademyMaterialFactory.ApplyMaterial(pad, ArcAcademyMaterialFactory.GetCentralPlatform(PlatformBlack));
        Object.DestroyImmediate(pad.GetComponent<BoxCollider>());

        // Strong purple under-glow + edge accents (black platform + purple VFX)
        AddSpawnPadEdgeGlow(pad.transform, AcademyPurple);

        var glowTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glowTop.name = "SpawnPadGlow";
        glowTop.transform.SetParent(pad.transform);
        glowTop.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        glowTop.transform.localScale = new Vector3(0.98f, 0.025f, 0.92f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            glowTop,
            ArcAcademyMaterialFactory.CreateEmissive(AcademyPurple, ArcAcademyLayout.PlatformEmissiveIntensity * 1.15f));
        Object.DestroyImmediate(glowTop.GetComponent<Collider>());

        // Extra low purple emissive ring on the base for "strong purple glow"
        var baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseRing.name = "PlatformBaseRing";
        baseRing.transform.SetParent(pad.transform);
        baseRing.transform.localPosition = new Vector3(0f, -0.12f, 0f);
        baseRing.transform.localScale = new Vector3(1.02f, 0.035f, 1.02f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            baseRing,
            ArcAcademyMaterialFactory.CreateEmissive(AcademyPurple, 0.7f));
        Object.DestroyImmediate(baseRing.GetComponent<Collider>());

        var spawnPoint = new GameObject(ArcAcademyLayout.BallSpawnPointName);
        spawnPoint.transform.SetParent(pad.transform);
        spawnPoint.transform.localPosition = ArcAcademyLayout.BobSpawnOffset;

        pad.AddComponent<SpawnPadPulse>();

        var branding = new GameObject(ArcAcademyLayout.SpawnPadBrandingName);
        branding.transform.SetParent(pad.transform);
        branding.transform.localPosition = Vector3.zero;

        // Large "Bob" + "Arc Academy" placed to sit on/above the black platform top (flat + billboard)
        CreateTextLabel(branding.transform, "Label_Bob", "Bob",
            new Vector3(0f, 1.05f, 0f), ArcAcademyLayout.LabelBobSize * 1.35f, Color.white, true);
        CreateTextLabel(branding.transform, "Label_ArcAcademy", "Arc Academy",
            new Vector3(0f, 0.38f, 0.72f), ArcAcademyLayout.LabelAcademySize * 1.35f,
            AcademyPurple, true);

        var rimLight = new GameObject("SpawnPadLight");
        rimLight.transform.SetParent(pad.transform);
        rimLight.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        var light = rimLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = AcademyPurple;
        light.intensity = ArcAcademyLabLighting.SpawnPadPointLumen;
        light.range = 7.5f;
        rimLight.AddComponent<HDAdditionalLightData>();

        var particles = new GameObject("SpawnPadParticles");
        particles.transform.SetParent(pad.transform);
        particles.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        var ps = particles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(AcademyPurple);
        main.startSize = 0.09f;
        main.startLifetime = 1.6f;
        main.maxParticles = 70;
        main.loop = true;
        var emission = ps.emission;
        emission.rateOverTime = 22f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1.05f;

        return pad.transform;
    }

    private static void AddSpawnPadEdgeGlow(Transform pad, Color glowColor)
    {
        float halfX = ArcAcademyLayout.SpawnPadScale.x * 0.5f;
        float halfZ = ArcAcademyLayout.SpawnPadScale.z * 0.5f;
        float y = 0.02f;

        CreateEdgeStrip(pad, "EdgeGlow_Front", new Vector3(0f, y, halfZ), new Vector3(halfX * 2f, 0.04f, 0.06f), glowColor);
        CreateEdgeStrip(pad, "EdgeGlow_Back", new Vector3(0f, y, -halfZ), new Vector3(halfX * 2f, 0.04f, 0.06f), glowColor);
        CreateEdgeStrip(pad, "EdgeGlow_Left", new Vector3(-halfX, y, 0f), new Vector3(0.06f, 0.04f, halfZ * 2f), glowColor);
        CreateEdgeStrip(pad, "EdgeGlow_Right", new Vector3(halfX, y, 0f), new Vector3(0.06f, 0.04f, halfZ * 2f), glowColor);
    }

    private static void CreateEdgeStrip(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = name;
        strip.transform.SetParent(parent, false);
        strip.transform.localPosition = localPos;
        strip.transform.localScale = scale;
        ArcAcademyMaterialFactory.ApplyMaterial(
            strip,
            ArcAcademyMaterialFactory.CreateEmissive(color, ArcAcademyLayout.PlatformEmissiveIntensity * 0.85f));
        Object.DestroyImmediate(strip.GetComponent<BoxCollider>());
    }

    private static void CreateTextLabel(
        Transform parent,
        string name,
        string text,
        Vector3 localPos,
        float charSize,
        Color color,
        bool faceCamera = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = faceCamera ? Quaternion.identity : Quaternion.Euler(90f, 0f, 0f);

        var textMesh = go.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = charSize * 0.08f;
        textMesh.fontSize = 64;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.fontStyle = FontStyle.Bold;

        // Make text pop on dark platform (emissive tint via color intensity)
        if (color == Color.white || color.r > 0.9f)
        {
            // Bob label brighter
            textMesh.color = new Color(1f, 1f, 1f, 1f);
        }

        if (faceCamera)
        {
            go.AddComponent<CameraFacingBillboard>();
        }
    }

    private static void CreateTrainingBays(Transform parent)
    {
        var baysRoot = new GameObject(ArcAcademyLayout.TrainingBaysName);
        baysRoot.transform.SetParent(parent);

        for (int i = 0; i < ArcAcademyLayout.TrainingBayCount; i++)
        {
            CreateTrainingBayShell(
                baysRoot.transform,
                $"Bay_{i + 1}",
                ArcAcademyLayout.TrainingBayPositions[i],
                ArcAcademyLayout.TrainingBayFaceNegativeZ[i],
                i);
        }
    }

    private static void CreateTrainingBayShell(
        Transform parent,
        string name,
        Vector3 center,
        bool faceNegativeZ,
        int bayIndex)
    {
        var bay = new GameObject(name);
        bay.transform.SetParent(parent);
        bay.transform.position = center;

        float halfW = ArcAcademyLayout.TrainingBayWidth * 0.5f;
        float depth = ArcAcademyLayout.TrainingBayDepth;
        float height = ArcAcademyLayout.TrainingBayWallHeight;
        float backZ = faceNegativeZ ? 0f : depth;

        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "BayFloor";
        floor.transform.SetParent(bay.transform);
        floor.transform.localPosition = new Vector3(0f, 0.02f, depth * 0.5f);
        floor.transform.localScale = new Vector3(ArcAcademyLayout.TrainingBayWidth, 0.04f, depth);
        ArcAcademyMaterialFactory.ApplyMaterial(floor, ArcAcademyMaterialFactory.GetGlossyFloor(CourtOrange));
        Object.DestroyImmediate(floor.GetComponent<BoxCollider>());

        CreateBayWall(bay.transform, "BayWall_Back",
            new Vector3(0f, height * 0.5f, backZ),
            new Vector3(ArcAcademyLayout.TrainingBayWidth, height, 0.12f));
        var leftWall = CreateBayWall(bay.transform, "BayWall_Left",
            new Vector3(-halfW, height * 0.5f, depth * 0.5f),
            new Vector3(0.12f, height, depth));
        CreateBayWall(bay.transform, "BayWall_Right",
            new Vector3(halfW, height * 0.5f, depth * 0.5f),
            new Vector3(0.12f, height, depth));

        float rimZ = faceNegativeZ ? 0.25f : depth - 0.25f;
        var hoopRoot = new GameObject("BayHoop");
        hoopRoot.transform.SetParent(bay.transform);
        hoopRoot.transform.localPosition = new Vector3(0f, 0f, rimZ);
        hoopRoot.AddComponent<DecorativeHoopMarker>();

        ArcAcademyPortableHoopBuilder.CreateStand(
            hoopRoot.transform,
            Vector3.zero,
            faceNegativeZ ? 180f : 0f,
            0.82f,
            faceNegativeZ,
            includeBackboardAndRim: true,
            includeRoboticLauncher: true,
            useSolidBackboard: true);  // solid for ref match on decorative bays

        if (bayIndex > 0)
        {
            CreateBayDivider(parent, center, ArcAcademyLayout.TrainingBayPositions[bayIndex - 1]);
        }
        // Low partition trim note: divider height controlled in CreateBayDivider + layout const
    }

    private static void CreateBayDivider(Transform baysRoot, Vector3 currentCenter, Vector3 previousCenter)
    {
        Vector3 midpoint = (currentCenter + previousCenter) * 0.5f;
        Vector3 delta = currentCenter - previousCenter;
        if (delta.sqrMagnitude < 0.01f)
        {
            return;
        }

        var divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
        divider.name = "BayDivider";
        divider.transform.SetParent(baysRoot);
        divider.transform.position = midpoint + Vector3.up * (ArcAcademyLayout.BayPartitionHeight * 0.5f);
        divider.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        divider.transform.localScale = new Vector3(0.09f, ArcAcademyLayout.BayPartitionHeight, 1.55f);
        ArcAcademyMaterialFactory.ApplyMaterial(divider, ArcAcademyMaterialFactory.GetBayPanel(BayWallWhite));
        Object.DestroyImmediate(divider.GetComponent<BoxCollider>());
    }

    private static Transform CreateBayWall(Transform parent, string name, Vector3 localPos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = scale;
        ArcAcademyMaterialFactory.ApplyMaterial(wall, ArcAcademyMaterialFactory.GetBayPanel(BayWallWhite));

        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = $"{name}_Trim";
        trim.transform.SetParent(wall.transform);
        trim.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        trim.transform.localScale = new Vector3(1.02f, 0.08f, 1.02f);
        ArcAcademyMaterialFactory.ApplyMaterial(trim, ArcAcademyMaterialFactory.GetBayPanel(BayTrim));
        Object.DestroyImmediate(trim.GetComponent<BoxCollider>());
        Object.DestroyImmediate(wall.GetComponent<BoxCollider>());
        return wall.transform;
    }

    private static (Transform rim, MovableHoop movableHoop) CreateHoop(Transform parent)
    {
        var hoopRoot = new GameObject(ArcAcademyLayout.HoopName);
        hoopRoot.transform.SetParent(parent);
        hoopRoot.transform.position = ArcAcademyLayout.HoopRootDefaultPosition;

        var movableHoop = hoopRoot.AddComponent<MovableHoop>();

        var swivelBaseGo = new GameObject("RoboticSwivelBase");
        swivelBaseGo.transform.SetParent(hoopRoot.transform);
        swivelBaseGo.transform.localPosition = Vector3.zero;
        var swivelBaseBody = swivelBaseGo.AddComponent<ArticulationBody>();
        swivelBaseBody.immovable = true;

        var basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlate.name = "SwivelBasePlate";
        basePlate.transform.SetParent(swivelBaseGo.transform);
        basePlate.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        basePlate.transform.localScale = new Vector3(0.85f, 0.08f, 0.85f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            basePlate,
            ArcAcademyMaterialFactory.GetMetal(JointWhite));
        Object.DestroyImmediate(basePlate.GetComponent<Collider>());

        var swivelLinkGo = new GameObject("SwivelLink");
        swivelLinkGo.transform.SetParent(swivelBaseGo.transform);
        swivelLinkGo.transform.localPosition = new Vector3(0f, 0.22f, 0f);
        var swivelBody = swivelLinkGo.AddComponent<ArticulationBody>();
        ConfigureRevoluteJoint(swivelBody, new Vector3(0f, 0f, 90f));

        var swivelColumn = GameObject.CreatePrimitive(PrimitiveType.Cube);
        swivelColumn.name = "SwivelColumn";
        swivelColumn.transform.SetParent(swivelLinkGo.transform);
        swivelColumn.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        swivelColumn.transform.localScale = new Vector3(0.22f, 0.7f, 0.22f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            swivelColumn,
            ArcAcademyMaterialFactory.GetMetal(PoleGray));
        Object.DestroyImmediate(swivelColumn.GetComponent<Collider>());

        var armLinkGo = new GameObject("ArmLink");
        armLinkGo.transform.SetParent(swivelLinkGo.transform);
        armLinkGo.transform.localPosition = new Vector3(0f, 0.75f, 0.05f);
        var armBody = armLinkGo.AddComponent<ArticulationBody>();
        ConfigureRevoluteJoint(armBody, Quaternion.identity);

        var armMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        armMesh.name = "ArmSegment";
        armMesh.transform.SetParent(armLinkGo.transform);
        armMesh.transform.localPosition = new Vector3(0f, 0.15f, 0.18f);
        armMesh.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
        armMesh.transform.localScale = new Vector3(0.2f, 0.14f, 0.55f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            armMesh,
            ArcAcademyMaterialFactory.GetMetal(JointWhite));
        Object.DestroyImmediate(armMesh.GetComponent<Collider>());

        var hoopHead = new GameObject("HoopHead");
        hoopHead.transform.SetParent(armLinkGo.transform);
        hoopHead.transform.localPosition = new Vector3(0f, 0.25f, 0.42f);

        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "Backboard";
        backboard.transform.SetParent(hoopHead.transform);
        backboard.transform.localPosition = new Vector3(0f, 0.55f, 0.08f);
        backboard.transform.localScale = new Vector3(1.8f, 1.05f, 0.06f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            backboard,
            ArcAcademyMaterialFactory.GetActiveBackboardGlass(BackboardWhite));
        backboard.GetComponent<BoxCollider>().material = HoopPhysicsMaterials.Backboard;
        backboard.AddComponent<HoopBackboardFeedback>();

        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = ArcAcademyLayout.RimName;
        rim.transform.SetParent(hoopHead.transform);
        rim.transform.localPosition = ArcAcademyLayout.RimLocalDefaultPosition;
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.transform.localScale = new Vector3(0.9f, 0.04f, 0.9f);
        ArcAcademyMaterialFactory.ApplyMaterial(rim, ArcAcademyMaterialFactory.GetRubber(RimOrange));
        Object.DestroyImmediate(rim.GetComponent<CapsuleCollider>());
        var rimRb = rim.AddComponent<Rigidbody>();
        rimRb.isKinematic = true;
        rimRb.useGravity = false;
        rim.AddComponent<HoopRimContact>();
        TrainingHoopDetail.ConfigureRimColliders(rim);

        var netRoot = new GameObject("Net");
        netRoot.transform.SetParent(rim.transform);
        netRoot.transform.localPosition = new Vector3(0f, -0.08f, 0f);
        var netPhysics = netRoot.AddComponent<HoopNetPhysics>();
        var netMaterial = ArcAcademyMaterialFactory.GetMatteWall(Color.white);
        netPhysics.BuildNet(rim.transform, netMaterial, HoopPhysicsMaterials.NetStrand, physicsColliders: false);
        netRoot.AddComponent<HoopSwishVfx>();

        movableHoop.SetRimTransform(rim.transform);
        movableHoop.WireArticulation(swivelLinkGo.transform, armLinkGo.transform, swivelBody, armBody);

        var scoreZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        scoreZone.name = ArcAcademyLayout.ScoreZoneName;
        scoreZone.transform.SetParent(rim.transform);
        scoreZone.transform.localPosition = Vector3.zero;
        scoreZone.transform.localScale = Vector3.one * (ArcAcademyLayout.RimScoreRadius * 2f / 0.9f);
        var scoreRenderer = scoreZone.GetComponent<Renderer>();
        if (scoreRenderer != null)
        {
            scoreRenderer.enabled = false;
        }

        var scoreCollider = scoreZone.GetComponent<SphereCollider>();
        scoreCollider.isTrigger = true;
        scoreZone.AddComponent<HoopScoreZone>();

        movableHoop.ApplyDefaultPose();
        TrainingHoopDetail.UpgradeHoop(hoopRoot.transform);
        ArcAcademyPortableHoopBuilder.AddActiveHoopShell(hoopRoot.transform);
        return (rim.transform, movableHoop);
    }

    private static void ConfigureRevoluteJoint(ArticulationBody body, Vector3 anchorEuler)
    {
        ConfigureRevoluteJoint(body, Quaternion.Euler(anchorEuler));
    }

    private static void ConfigureRevoluteJoint(ArticulationBody body, Quaternion anchorRotation)
    {
        body.jointType = ArticulationJointType.RevoluteJoint;
        body.anchorRotation = anchorRotation;
        body.twistLock = ArticulationDofLock.LimitedMotion;
        body.swingYLock = ArticulationDofLock.LockedMotion;
        body.swingZLock = ArticulationDofLock.LockedMotion;
    }

    private static ArcAcademyScorePopup CreateScorePopup(Transform parent, Transform rim)
    {
        var popupRoot = new GameObject(ArcAcademyLayout.ScorePopupName);
        popupRoot.transform.SetParent(parent);
        popupRoot.transform.position = rim.position + Vector3.up * 0.65f;

        var textGo = new GameObject("ScoreText");
        textGo.transform.SetParent(popupRoot.transform);
        textGo.transform.localPosition = Vector3.zero;
        var textMesh = textGo.AddComponent<TextMesh>();
        textMesh.characterSize = 0.12f;
        textMesh.fontSize = 64;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontStyle = FontStyle.Bold;
        textGo.AddComponent<CameraFacingBillboard>();

        var popup = popupRoot.AddComponent<ArcAcademyScorePopup>();
        return popup;
    }

    private static void CreateSimpleNet(Transform rim)
    {
        var netRoot = new GameObject("Net");
        netRoot.transform.SetParent(rim);
        netRoot.transform.localPosition = new Vector3(0f, -0.08f, 0f);

        for (int i = 0; i < 8; i++)
        {
            float angle = i / 8f * Mathf.PI * 2f;
            var strand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strand.name = $"NetStrand_{i}";
            strand.transform.SetParent(netRoot.transform);
            strand.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.28f, -0.18f, Mathf.Sin(angle) * 0.28f);
            strand.transform.localScale = new Vector3(0.02f, 0.18f, 0.02f);
            ArcAcademyMaterialFactory.ApplyMaterial(
                strand,
                ArcAcademyMaterialFactory.GetMatteWall(Color.white));
            Object.DestroyImmediate(strand.GetComponent<Collider>());
        }
    }

    private static void CreateBob(Transform rim, Transform ballSpawn)
    {
        var bob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bob.name = "Bob";
        bob.transform.position = ballSpawn != null ? ballSpawn.position : ArcAcademyLayout.BobSpawnPosition;
        bob.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);

        var bobMat = ArcAcademyMaterialFactory.CreateEmissive(BobOrange, ArcAcademyLayout.BobGlowIntensity);
        ArcAcademyMaterialFactory.ApplyMaterial(bob, bobMat);

        var rb = bob.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = 0.6f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.15f;
        rb.constraints = RigidbodyConstraints.None;

        var agent = bob.AddComponent<BobAgent>();
        agent.hoop = rim;

        bob.AddComponent<BobEntranceController>();
        var idle = bob.AddComponent<BobIdleAnimation>();
        idle.Wire(ballSpawn);

        bob.AddComponent<VrShootInputPlaceholder>();
        var shooting = bob.AddComponent<BobShootingInput>();
        shooting.Wire(rim, ballSpawn);

        var behavior = bob.GetComponent<BehaviorParameters>();
        behavior.BehaviorName = "Bob";
        behavior.BehaviorType = BehaviorType.Default;
        behavior.TeamId = 0;
        behavior.BrainParameters.VectorObservationSize = 8;
        behavior.BrainParameters.NumStackedVectorObservations = 1;
        behavior.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(3);

        bob.AddComponent<DecisionRequester>().DecisionPeriod = 1;

        if (BobPhysicsLayers.LayersConfigured)
        {
            BobPhysicsLayers.SetLayerRecursively(bob, BobPhysicsLayers.BobLayer);
        }
    }

    private static void ApplyTrainingPhysicsLayers(Transform arena)
    {
        if (!BobPhysicsLayers.LayersConfigured)
        {
            return;
        }

        int decor = BobPhysicsLayers.DecorationLayer;
        int training = BobPhysicsLayers.TrainingArenaLayer;

        var shell = arena.Find(ArcAcademyLayout.WarehouseShellName);
        if (shell != null)
        {
            BobPhysicsLayers.SetLayerRecursively(shell.gameObject, decor);
        }

        var bays = arena.Find(ArcAcademyLayout.TrainingBaysName);
        if (bays != null)
        {
            BobPhysicsLayers.SetLayerRecursively(bays.gameObject, decor);
        }

        var trajectory = arena.Find(ArcAcademyLayout.TrajectoryVisualsName);
        if (trajectory != null)
        {
            BobPhysicsLayers.SetLayerRecursively(trajectory.gameObject, decor);
        }

        var decals = arena.Find(ArcAcademyLayout.FloorDecalsName);
        if (decals != null)
        {
            BobPhysicsLayers.SetLayerRecursively(decals.gameObject, decor);
        }

        var lighting = arena.Find(ArcAcademyLayout.LightingRigName);
        if (lighting != null)
        {
            BobPhysicsLayers.SetLayerRecursively(lighting.gameObject, decor);
        }

        var courtFloor = arena.Find(ArcAcademyLayout.CourtFloorName);
        if (courtFloor != null)
        {
            courtFloor.gameObject.layer = training;
        }

        var boundaries = arena.Find("Boundaries");
        if (boundaries != null)
        {
            BobPhysicsLayers.SetLayerRecursively(boundaries.gameObject, training);
        }

        var hoop = arena.Find(ArcAcademyLayout.HoopName);
        if (hoop != null)
        {
            BobPhysicsLayers.SetLayerRecursively(hoop.gameObject, training);
        }
    }

    private static void CreateLightingRig(Transform parent)
    {
        var rig = new GameObject(ArcAcademyLayout.LightingRigName);
        rig.transform.SetParent(parent);

        var keyFill = new GameObject("LabKeyFill");
        keyFill.transform.SetParent(rig.transform);
        keyFill.transform.position = new Vector3(2f, 6f, 6f);
        var fill = keyFill.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = ArcAcademyLabLighting.FillDirectionalLux;
        fill.color = new Color(0.92f, 0.94f, 1f);
        fill.shadows = LightShadows.None;
        keyFill.transform.rotation = Quaternion.Euler(38f, -25f, 0f);
        keyFill.AddComponent<HDAdditionalLightData>();

        CreatePointLight(
            rig.transform,
            "WarehouseLight_Center",
            new Vector3(0f, 7.5f, -4f),
            ArcAcademyLabLighting.CenterPointLumen);

        var bobRim = new GameObject("BobRimLight");
        bobRim.transform.SetParent(rig.transform);
        bobRim.transform.position = ArcAcademyLayout.SpawnPadPosition + new Vector3(0f, 1.2f, 0f);
        var bobSpot = bobRim.AddComponent<Light>();
        bobSpot.type = LightType.Spot;
        bobSpot.color = AcademyPurple;
        bobSpot.intensity = ArcAcademyLabLighting.BobSpotLumen;
        bobSpot.range = 8f;
        bobSpot.spotAngle = 55f;
        bobSpot.innerSpotAngle = 25f;
        bobRim.transform.rotation = Quaternion.Euler(55f, 180f, 0f);
        bobRim.AddComponent<HDAdditionalLightData>();
    }

    private static void CreateCeilingAreaLight(Transform parent, string name, Vector3 position)
    {
        var strip = new GameObject(name);
        strip.transform.SetParent(parent);
        strip.transform.position = position;
        var light = strip.AddComponent<Light>();
        light.type = LightType.Rectangle;
        light.areaSize = new Vector2(2.8f, 0.35f);
        light.intensity = ArcAcademyLabLighting.CeilingStripLumen;
        light.color = new Color(0.95f, 0.97f, 1f);
        strip.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        strip.AddComponent<HDAdditionalLightData>();

        var emissive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emissive.name = $"{name}_Mesh";
        emissive.transform.SetParent(strip.transform);
        emissive.transform.localPosition = Vector3.zero;
        emissive.transform.localScale = new Vector3(2.8f, 0.08f, 0.35f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            emissive,
            ArcAcademyMaterialFactory.CreateEmissive(Color.white, 0.5f));
        Object.DestroyImmediate(emissive.GetComponent<BoxCollider>());
    }

    private static void CreatePointLight(Transform parent, string name, Vector3 position, float intensity)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = intensity;
        light.range = 22f;
        go.AddComponent<HDAdditionalLightData>();
    }

    private static void CreateReflectionProbes(Transform parent)
    {
        float midZ = (ArcAcademyLayout.CourtNearZ + ArcAcademyLayout.CourtFarZ) * 0.5f;

        var courtProbeGo = new GameObject(ArcAcademyLayout.ReflectionProbeName);
        courtProbeGo.transform.SetParent(parent);
        courtProbeGo.transform.position = new Vector3(0f, 4f, midZ);
        ConfigureReflectionProbe(courtProbeGo.AddComponent<ReflectionProbe>(), new Vector3(24f, 12f, 32f), 256);

        var windowProbeGo = new GameObject(ArcAcademyLayout.ReflectionProbeWindowName);
        windowProbeGo.transform.SetParent(parent);
        windowProbeGo.transform.position = new Vector3(-7.5f, 4f, midZ);
        ConfigureReflectionProbe(windowProbeGo.AddComponent<ReflectionProbe>(), new Vector3(8f, 6f, 4f), 512);
    }

    private static void ConfigureReflectionProbe(ReflectionProbe probe, Vector3 size, int resolution)
    {
        probe.size = size;
        probe.resolution = resolution;
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
        probe.RenderProbe();
    }

    private static void CreateFloorDecals(Transform parent)
    {
        var root = new GameObject(ArcAcademyLayout.FloorDecalsName);
        root.transform.SetParent(parent);

        float courtCenterZ = (ArcAcademyLayout.CourtNearZ + ArcAcademyLayout.CourtFarZ) * 0.5f;
        var logoRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        logoRing.name = "Decal_CourtLogoRing";
        logoRing.transform.SetParent(root.transform);
        logoRing.transform.position = new Vector3(0f, 0.04f, courtCenterZ);
        logoRing.transform.localScale = new Vector3(1.4f, 0.01f, 1.4f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            logoRing,
            ArcAcademyMaterialFactory.CreateEmissive(AcademyPurple, 0.45f));
        Object.DestroyImmediate(logoRing.GetComponent<Collider>());
    }

    private static void CreateTrajectoryVisuals(Transform parent, Transform spawnPad, Transform mainRim)
    {
        var root = new GameObject(ArcAcademyLayout.TrajectoryVisualsName);
        root.transform.SetParent(parent);

        var visual = root.AddComponent<ArcTrajectoryVisual>();
        var spawnPoint = spawnPad.Find(ArcAcademyLayout.BallSpawnPointName);
        var start = spawnPoint != null
            ? spawnPoint.position + Vector3.up * 0.1f
            : spawnPad.position + ArcAcademyLayout.BobSpawnOffset + Vector3.up * 0.1f;
        var targets = ArcAcademyLayout.BuildTrajectoryArcTargets(mainRim.position);
        visual.ConfigureStaticArcs(start, targets);
    }

    private static void CreateBoundaries(Transform parent)
    {
        var boundsParent = new GameObject("Boundaries");
        boundsParent.transform.SetParent(parent);

        float midZ = (ArcAcademyLayout.ShellNearZ + ArcAcademyLayout.ShellFarZ) * 0.5f;
        float length = ArcAcademyLayout.ShellNearZ - ArcAcademyLayout.ShellFarZ + 2f;
        float halfWidth = ArcAcademyLayout.ShellHalfWidth + 0.5f;

        CreateWall(boundsParent, "Wall_Left", new Vector3(-halfWidth, 2f, midZ), new Vector3(0.5f, 4f, length));
        CreateWall(boundsParent, "Wall_Right", new Vector3(halfWidth, 2f, midZ), new Vector3(0.5f, 4f, length));
        CreateWall(boundsParent, "Wall_Near", new Vector3(0f, 2f, ArcAcademyLayout.ShellNearZ + 0.5f), new Vector3(halfWidth * 2f, 4f, 0.5f));
        CreateWall(boundsParent, "Wall_Far", new Vector3(0f, 2f, ArcAcademyLayout.ShellFarZ - 0.5f), new Vector3(halfWidth * 2f, 4f, 0.5f));
        CreateWall(boundsParent, "Ceiling", new Vector3(0f, ArcAcademyLayout.CeilingHeight, midZ), new Vector3(halfWidth * 2f, 0.5f, length));
    }

    private static void CreateLineMark(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
    {
        var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = name;
        line.transform.SetParent(parent);
        line.transform.position = pos;
        line.transform.localScale = scale;
        ArcAcademyMaterialFactory.ApplyMaterial(line, ArcAcademyMaterialFactory.GetMatteWall(color));
        Object.DestroyImmediate(line.GetComponent<BoxCollider>());
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var entry in scenes)
        {
            if (entry.path == scenePath)
            {
                return;
            }
        }

        var updated = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(updated, 0);
        updated[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = updated;
    }

    private static void CreateWall(GameObject parent, string name, Vector3 pos, Vector3 scale)
    {
        var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = name;
        w.transform.SetParent(parent.transform);
        w.transform.position = pos;
        w.transform.localScale = scale;
        var rend = w.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.enabled = false;
        }
    }
}
#endif
