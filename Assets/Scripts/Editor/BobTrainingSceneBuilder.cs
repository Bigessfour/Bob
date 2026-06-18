#if UNITY_EDITOR
using System.IO;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class BobTrainingSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

    private static readonly Color CourtOrange = new(0.92f, 0.45f, 0.12f);
    private static readonly Color CourtLine = new(0.95f, 0.95f, 0.95f);
    private static readonly Color KeyPaint = new(0.82f, 0.72f, 0.55f);
    private static readonly Color WarehouseConcrete = new(0.18f, 0.19f, 0.22f);
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

    [MenuItem("Bob/Create Training Scene")]
    public static void CreateTrainingSceneMenu()
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
        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var arena = new GameObject(ArcAcademyLayout.ArenaName);
        var manager = arena.AddComponent<ArcAcademyManager>();

        CreateCamera();
        CreateWarehouseShell(arena.transform);
        CreateCourt(arena.transform);
        CreateTrainingBays(arena.transform);
        CreateTrainingBaysBack(arena.transform);
        Transform spawnPad = CreateSpawnPad(arena.transform);
        CreateDecorativeHoops(arena.transform);
        (Transform rim, MovableHoop movableHoop) = CreateHoop(arena.transform);
        CreateBob(rim);
        CreateBoundaries(arena.transform);
        CreateLightingRig(arena.transform);
        CreateReflectionProbe(arena.transform);
        CreateTrajectoryVisuals(arena.transform, spawnPad, rim);

        manager.WireReferences(movableHoop, spawnPad);
        manager.SetupForTraining();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Debug.Log($"Arc Academy training scene created at {ScenePath}");
    }

    private static void CreateCamera()
    {
        var cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        cameraGo.AddComponent<Camera>();
        cameraGo.transform.position = ArcAcademyLayout.CameraPosition;
        cameraGo.transform.rotation = Quaternion.LookRotation(
            ArcAcademyLayout.CameraLookAt - ArcAcademyLayout.CameraPosition,
            Vector3.up);
        cameraGo.AddComponent<AudioListener>();
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
        ArcAcademyMaterialFactory.ApplyMaterial(
            floor,
            ArcAcademyMaterialFactory.CreateGlossyFloor(WarehouseConcrete, ArcAcademyLayout.FloorGlossiness));

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

        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(shell.transform);
        ceiling.transform.position = new Vector3(0f, ArcAcademyLayout.CeilingHeight, shellCenterZ);
        ceiling.transform.localScale = new Vector3(
            ArcAcademyLayout.ShellHalfWidth / 5f,
            1f,
            shellLength / 10f);
        ceiling.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        ArcAcademyMaterialFactory.ApplyMaterial(ceiling, ArcAcademyMaterialFactory.CreateStandard(WarehouseWall));
        Object.DestroyImmediate(ceiling.GetComponent<MeshCollider>());

        CreateMountainWindow(shell.transform, shellCenterZ);
    }

    private static GameObject CreateShellWall(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        ArcAcademyMaterialFactory.ApplyMaterial(wall, ArcAcademyMaterialFactory.CreateStandard(WarehouseWall));
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
                ArcAcademyMaterialFactory.CreateStandard(new Color(0.48f, 0.49f, 0.52f)));
            Object.DestroyImmediate(rib.GetComponent<BoxCollider>());
        }
    }

    private static void CreateMountainWindow(Transform shell, float shellCenterZ)
    {
        var windowRoot = new GameObject(ArcAcademyLayout.MountainWindowName);
        windowRoot.transform.SetParent(shell);
        windowRoot.transform.position = new Vector3(
            -ArcAcademyLayout.ShellHalfWidth + 0.05f,
            3.5f,
            shellCenterZ);

        var window = GameObject.CreatePrimitive(PrimitiveType.Quad);
        window.name = "WindowPane";
        window.transform.SetParent(windowRoot.transform, false);
        window.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        window.transform.localScale = new Vector3(14f, 4.5f, 1f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            window,
            ArcAcademyMaterialFactory.CreateMountainWindowMaterial());
        Object.DestroyImmediate(window.GetComponent<Collider>());
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
        ArcAcademyMaterialFactory.ApplyMaterial(floor, ArcAcademyMaterialFactory.CreateStandard(CourtOrange));

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
        ArcAcademyMaterialFactory.ApplyMaterial(keyFill, ArcAcademyMaterialFactory.CreateStandard(KeyPaint));
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
        var hoopX = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(200f, 340f, i / (float)segments) * Mathf.Deg2Rad;
            float x = hoopX + Mathf.Cos(angle) * radius;
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
        var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = ArcAcademyLayout.SpawnPadName;
        pad.transform.SetParent(parent);
        pad.transform.position = ArcAcademyLayout.SpawnPadPosition;
        pad.transform.localScale = ArcAcademyLayout.SpawnPadScale;
        ArcAcademyMaterialFactory.ApplyMaterial(pad, ArcAcademyMaterialFactory.CreateStandard(SpawnPadDark));
        Object.DestroyImmediate(pad.GetComponent<BoxCollider>());

        var glowTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glowTop.name = "SpawnPadGlow";
        glowTop.transform.SetParent(pad.transform);
        glowTop.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        glowTop.transform.localScale = new Vector3(0.92f, 0.08f, 0.88f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            glowTop,
            ArcAcademyMaterialFactory.CreateEmissive(AcademyPurple, ArcAcademyLayout.PlatformEmissiveIntensity));
        Object.DestroyImmediate(glowTop.GetComponent<BoxCollider>());

        CreateTextLabel(pad.transform, "Label_Bob", "Bob",
            new Vector3(0f, 1.35f, 0f), ArcAcademyLayout.LabelBobSize, Color.white);
        CreateTextLabel(pad.transform, "Label_ArcAcademy", "Arc Academy",
            new Vector3(0f, 0.15f, 0.52f), ArcAcademyLayout.LabelAcademySize, AcademyPurple);

        var rimLight = new GameObject("SpawnPadLight");
        rimLight.transform.SetParent(pad.transform);
        rimLight.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        var light = rimLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = AcademyPurple;
        light.intensity = 0.8f;
        light.range = 4f;

        return pad.transform;
    }

    private static void CreateTextLabel(
        Transform parent,
        string name,
        string text,
        Vector3 localPos,
        float charSize,
        Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        var textMesh = go.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = charSize * 0.08f;
        textMesh.fontSize = 64;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.fontStyle = FontStyle.Bold;
    }

    private static void CreateTrainingBays(Transform parent)
    {
        var baysRoot = new GameObject(ArcAcademyLayout.TrainingBaysName);
        baysRoot.transform.SetParent(parent);

        for (int i = 0; i < ArcAcademyLayout.TrainingBayCount; i++)
        {
            float bayZ = ArcAcademyLayout.TrainingBayStartZ + i * ArcAcademyLayout.TrainingBaySpacing;
            CreateTrainingBayShell(
                baysRoot.transform,
                $"Bay_{i + 1}",
                new Vector3(ArcAcademyLayout.TrainingBayX, 0f, bayZ));
        }
    }

    private static void CreateTrainingBaysBack(Transform parent)
    {
        var baysRoot = new GameObject(ArcAcademyLayout.TrainingBaysBackName);
        baysRoot.transform.SetParent(parent);

        float totalWidth = (ArcAcademyLayout.TrainingBayBackCount - 1) * ArcAcademyLayout.TrainingBayBackSpacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < ArcAcademyLayout.TrainingBayBackCount; i++)
        {
            float bayX = startX + i * ArcAcademyLayout.TrainingBayBackSpacing;
            CreateTrainingBayShell(
                baysRoot.transform,
                $"BackBay_{i + 1}",
                new Vector3(bayX, 0f, ArcAcademyLayout.TrainingBayBackZ),
                faceNegativeZ: true);
        }
    }

    private static void CreateTrainingBayShell(
        Transform parent,
        string name,
        Vector3 center,
        bool faceNegativeZ = false)
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
        ArcAcademyMaterialFactory.ApplyMaterial(floor, ArcAcademyMaterialFactory.CreateStandard(CourtOrange));
        Object.DestroyImmediate(floor.GetComponent<BoxCollider>());

        CreateBayWall(bay.transform, "BayWall_Back",
            new Vector3(0f, height * 0.5f, backZ),
            new Vector3(ArcAcademyLayout.TrainingBayWidth, height, 0.12f));
        CreateBayWall(bay.transform, "BayWall_Left",
            new Vector3(-halfW, height * 0.5f, depth * 0.5f),
            new Vector3(0.12f, height, depth));
        CreateBayWall(bay.transform, "BayWall_Right",
            new Vector3(halfW, height * 0.5f, depth * 0.5f),
            new Vector3(0.12f, height, depth));

        float rimZ = faceNegativeZ ? 0.25f : depth - 0.25f;
        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "BayBackboard";
        backboard.transform.SetParent(bay.transform);
        backboard.transform.localPosition = new Vector3(0f, 2.6f, rimZ + (faceNegativeZ ? 0.15f : -0.08f));
        backboard.transform.localScale = new Vector3(1.2f, 0.8f, 0.05f);
        ArcAcademyMaterialFactory.ApplyMaterial(backboard, ArcAcademyMaterialFactory.CreateStandard(BackboardWhite));
        Object.DestroyImmediate(backboard.GetComponent<BoxCollider>());

        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "BayRim";
        rim.transform.SetParent(bay.transform);
        rim.transform.localPosition = new Vector3(0f, 2.15f, rimZ);
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.transform.localScale = new Vector3(0.55f, 0.03f, 0.55f);
        ArcAcademyMaterialFactory.ApplyMaterial(rim, ArcAcademyMaterialFactory.CreateStandard(RimOrange));
        Object.DestroyImmediate(rim.GetComponent<Collider>());
    }

    private static void CreateBayWall(Transform parent, string name, Vector3 localPos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = scale;
        ArcAcademyMaterialFactory.ApplyMaterial(wall, ArcAcademyMaterialFactory.CreateStandard(BayWallWhite));

        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = $"{name}_Trim";
        trim.transform.SetParent(wall.transform);
        trim.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        trim.transform.localScale = new Vector3(1.02f, 0.08f, 1.02f);
        ArcAcademyMaterialFactory.ApplyMaterial(trim, ArcAcademyMaterialFactory.CreateStandard(BayTrim));
        Object.DestroyImmediate(trim.GetComponent<BoxCollider>());
        Object.DestroyImmediate(wall.GetComponent<BoxCollider>());
    }

    private static void CreateDecorativeHoops(Transform parent)
    {
        var root = new GameObject(ArcAcademyLayout.DecorativeHoopsName);
        root.transform.SetParent(parent);

        for (int i = 0; i < ArcAcademyLayout.DecorativeHoopRootPositions.Length; i++)
        {
            CreateDecorativeHoopAssembly(root.transform, $"DisplayHoop_{i + 1}",
                ArcAcademyLayout.DecorativeHoopRootPositions[i]);
        }
    }

    private static void CreateDecorativeHoopAssembly(Transform parent, string name, Vector3 position)
    {
        var hoopRoot = new GameObject(name);
        hoopRoot.transform.SetParent(parent);
        hoopRoot.transform.position = position;

        var baseJoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseJoint.name = "Base";
        baseJoint.transform.SetParent(hoopRoot.transform);
        baseJoint.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        baseJoint.transform.localScale = new Vector3(0.55f, 0.25f, 0.55f);
        ArcAcademyMaterialFactory.ApplyMaterial(baseJoint, ArcAcademyMaterialFactory.CreateStandard(JointWhite));
        Object.DestroyImmediate(baseJoint.GetComponent<Collider>());

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pole.name = "Pole";
        pole.transform.SetParent(hoopRoot.transform);
        pole.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        pole.transform.localScale = new Vector3(0.16f, 2.4f, 0.16f);
        ArcAcademyMaterialFactory.ApplyMaterial(pole, ArcAcademyMaterialFactory.CreateStandard(PoleGray));
        Object.DestroyImmediate(pole.GetComponent<Collider>());

        var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "Arm";
        arm.transform.SetParent(hoopRoot.transform);
        arm.transform.localPosition = new Vector3(0f, 2.45f, 0.05f);
        arm.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
        ArcAcademyMaterialFactory.ApplyMaterial(arm, ArcAcademyMaterialFactory.CreateStandard(JointWhite));
        Object.DestroyImmediate(arm.GetComponent<Collider>());

        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "Backboard";
        backboard.transform.SetParent(hoopRoot.transform);
        backboard.transform.localPosition = new Vector3(0f, 2.85f, 0.15f);
        backboard.transform.localScale = new Vector3(1.4f, 0.9f, 0.05f);
        ArcAcademyMaterialFactory.ApplyMaterial(backboard, ArcAcademyMaterialFactory.CreateStandard(BackboardWhite));
        Object.DestroyImmediate(backboard.GetComponent<Collider>());

        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "Rim";
        rim.transform.SetParent(hoopRoot.transform);
        rim.transform.localPosition = ArcAcademyLayout.RimLocalDefaultPosition;
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.transform.localScale = new Vector3(0.75f, 0.035f, 0.75f);
        ArcAcademyMaterialFactory.ApplyMaterial(rim, ArcAcademyMaterialFactory.CreateStandard(RimOrange));
        Object.DestroyImmediate(rim.GetComponent<Collider>());
    }

    private static (Transform rim, MovableHoop movableHoop) CreateHoop(Transform parent)
    {
        var hoopRoot = new GameObject(ArcAcademyLayout.HoopName);
        hoopRoot.transform.SetParent(parent);
        hoopRoot.transform.position = ArcAcademyLayout.HoopRootDefaultPosition;

        var movableHoop = hoopRoot.AddComponent<MovableHoop>();

        var baseJoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseJoint.name = "BaseJoint";
        baseJoint.transform.SetParent(hoopRoot.transform);
        baseJoint.transform.localPosition = new Vector3(0f, 0.25f, -0.35f);
        baseJoint.transform.localScale = new Vector3(0.5f, 0.35f, 0.5f);
        ArcAcademyMaterialFactory.ApplyMaterial(baseJoint, ArcAcademyMaterialFactory.CreateStandard(JointWhite));

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pole.name = "Pole";
        pole.transform.SetParent(hoopRoot.transform);
        pole.transform.localPosition = new Vector3(0f, 1.5f, -0.35f);
        pole.transform.localScale = new Vector3(0.18f, 3f, 0.18f);
        ArcAcademyMaterialFactory.ApplyMaterial(pole, ArcAcademyMaterialFactory.CreateStandard(PoleGray));

        var armJoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        armJoint.name = "ArmJoint";
        armJoint.transform.SetParent(hoopRoot.transform);
        armJoint.transform.localPosition = new Vector3(0f, 2.95f, -0.1f);
        armJoint.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
        ArcAcademyMaterialFactory.ApplyMaterial(armJoint, ArcAcademyMaterialFactory.CreateStandard(JointWhite));

        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "Backboard";
        backboard.transform.SetParent(hoopRoot.transform);
        backboard.transform.localPosition = new Vector3(0f, 3.05f, 0.08f);
        backboard.transform.localScale = new Vector3(1.8f, 1.05f, 0.06f);
        ArcAcademyMaterialFactory.ApplyMaterial(backboard, ArcAcademyMaterialFactory.CreateStandard(BackboardWhite));

        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = ArcAcademyLayout.RimName;
        rim.transform.SetParent(hoopRoot.transform);
        rim.transform.localPosition = ArcAcademyLayout.RimLocalDefaultPosition;
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.transform.localScale = new Vector3(0.9f, 0.04f, 0.9f);
        ArcAcademyMaterialFactory.ApplyMaterial(rim, ArcAcademyMaterialFactory.CreateStandard(RimOrange));

        movableHoop.SetRimTransform(rim.transform);

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
        return (rim.transform, movableHoop);
    }

    private static void CreateBob(Transform rim)
    {
        var bob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bob.name = "Bob";
        bob.transform.position = ArcAcademyLayout.BobSpawnPosition;
        bob.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);

        var bobMat = ArcAcademyMaterialFactory.CreateStandard(BobOrange);
        bobMat.EnableKeyword("_EMISSION");
        bobMat.SetColor("_EmissionColor", AcademyPurple * ArcAcademyLayout.BobGlowIntensity);
        ArcAcademyMaterialFactory.ApplyMaterial(bob, bobMat);

        var rb = bob.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = 0.6f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.8f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        var agent = bob.AddComponent<BobAgent>();
        agent.hoop = rim;

        var behavior = bob.GetComponent<BehaviorParameters>();
        behavior.BehaviorName = "Bob";
        behavior.BehaviorType = BehaviorType.Default;
        behavior.TeamId = 0;
        behavior.BrainParameters.VectorObservationSize = 8;
        behavior.BrainParameters.NumStackedVectorObservations = 1;
        behavior.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(3);

        bob.AddComponent<DecisionRequester>().DecisionPeriod = 1;
    }

    private static void CreateLightingRig(Transform parent)
    {
        var rig = new GameObject(ArcAcademyLayout.LightingRigName);
        rig.transform.SetParent(parent);

        var lightGo = new GameObject("Directional Light");
        lightGo.transform.SetParent(rig.transform);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

        var windowFill = new GameObject("WindowFill");
        windowFill.transform.SetParent(rig.transform);
        windowFill.transform.position = new Vector3(-8f, 5f, -2f);
        var fill = windowFill.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.45f;
        fill.color = new Color(0.75f, 0.85f, 1f);
        windowFill.transform.rotation = Quaternion.Euler(10f, 70f, 0f);

        float midZ = (ArcAcademyLayout.ShellNearZ + ArcAcademyLayout.ShellFarZ) * 0.5f;
        for (int row = 0; row < 3; row++)
        {
            for (int col = -2; col <= 2; col++)
            {
                float x = col * 3.5f;
                float z = midZ + (row - 1) * 6f;
                CreateCeilingStrip(rig.transform, $"CeilingStrip_{row}_{col}",
                    new Vector3(x, ArcAcademyLayout.CeilingHeight - 0.15f, z));
            }
        }

        CreatePointLight(rig.transform, "WarehouseLight_Center", new Vector3(0f, 7.5f, -4f), 1.6f);
    }

    private static void CreateCeilingStrip(Transform parent, string name, Vector3 position)
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = name;
        strip.transform.SetParent(parent);
        strip.transform.position = position;
        strip.transform.localScale = new Vector3(2.8f, 0.08f, 0.35f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            strip,
            ArcAcademyMaterialFactory.CreateEmissive(Color.white, 1.5f));
        Object.DestroyImmediate(strip.GetComponent<BoxCollider>());
    }

    private static void CreatePointLight(Transform parent, string name, Vector3 position, float intensity)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = intensity;
        light.range = 20f;
    }

    private static void CreateReflectionProbe(Transform parent)
    {
        var probeGo = new GameObject(ArcAcademyLayout.ReflectionProbeName);
        probeGo.transform.SetParent(parent);
        float midZ = (ArcAcademyLayout.CourtNearZ + ArcAcademyLayout.CourtFarZ) * 0.5f;
        probeGo.transform.position = new Vector3(0f, 4f, midZ);

        var probe = probeGo.AddComponent<ReflectionProbe>();
        probe.size = new Vector3(24f, 12f, 32f);
        probe.resolution = 128;
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
        probe.RenderProbe();
    }

    private static void CreateTrajectoryVisuals(Transform parent, Transform spawnPad, Transform mainRim)
    {
        var root = new GameObject(ArcAcademyLayout.TrajectoryVisualsName);
        root.transform.SetParent(parent);

        var visual = root.AddComponent<ArcTrajectoryVisual>();
        var start = spawnPad.position + ArcAcademyLayout.BobSpawnOffset + Vector3.up * 0.1f;
        var targets = new Vector3[ArcAcademyLayout.TrajectoryArcCount]
        {
            mainRim.position,
            ArcAcademyLayout.DecorativeRimWorldPosition(0),
            ArcAcademyLayout.DecorativeRimWorldPosition(1),
        };
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
        ArcAcademyMaterialFactory.ApplyMaterial(line, ArcAcademyMaterialFactory.CreateStandard(color));
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
