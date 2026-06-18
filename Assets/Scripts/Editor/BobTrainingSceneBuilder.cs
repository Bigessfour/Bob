#if UNITY_EDITOR
using System.IO;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BobTrainingSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

    private static readonly Color CourtOrange = new(0.92f, 0.45f, 0.12f);
    private static readonly Color CourtLine = new(0.95f, 0.95f, 0.95f);
    private static readonly Color KeyPaint = new(0.82f, 0.72f, 0.55f);
    private static readonly Color WarehouseConcrete = new(0.28f, 0.28f, 0.3f);
    private static readonly Color WarehouseWall = new(0.55f, 0.56f, 0.58f);
    private static readonly Color SpawnPadDark = new(0.12f, 0.12f, 0.14f);
    private static readonly Color BackboardWhite = new(0.92f, 0.92f, 0.88f);
    private static readonly Color RimOrange = new(1f, 0.45f, 0.1f);
    private static readonly Color PoleGray = new(0.35f, 0.35f, 0.38f);
    private static readonly Color JointWhite = new(0.78f, 0.78f, 0.82f);
    private static readonly Color BobOrange = new(1f, 0.38f, 0f);

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

        CreateLighting();
        CreateCamera();
        CreateWarehouseShell(arena.transform);
        CreateCourt(arena.transform);
        Transform spawnPad = CreateSpawnPad(arena.transform);
        (Transform rim, MovableHoop movableHoop) = CreateHoop(arena.transform);
        CreateBob(rim);
        CreateBoundaries(arena.transform);

        manager.WireReferences(movableHoop, spawnPad);
        manager.SetupForTraining();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Debug.Log($"Arc Academy training scene created at {ScenePath}");
    }

    private static void CreateLighting()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.05f;
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(52f, -28f, 0f);

        CreatePointLight("WarehouseLight_Left", new Vector3(-6f, 7.5f, -2f), 1.2f);
        CreatePointLight("WarehouseLight_Center", new Vector3(0f, 7.8f, -4f), 1.4f);
        CreatePointLight("WarehouseLight_Right", new Vector3(6f, 7.5f, -2f), 1.2f);
    }

    private static void CreatePointLight(string name, Vector3 position, float intensity)
    {
        var go = new GameObject(name);
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = intensity;
        light.range = 18f;
        go.transform.position = position;
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
        ApplyColor(floor, WarehouseConcrete);

        CreateShellWall(shell.transform, "Wall_Left",
            new Vector3(-ArcAcademyLayout.ShellHalfWidth, 4f, shellCenterZ),
            new Vector3(0.4f, 8f, shellLength));
        CreateShellWall(shell.transform, "Wall_Right",
            new Vector3(ArcAcademyLayout.ShellHalfWidth, 4f, shellCenterZ),
            new Vector3(0.4f, 8f, shellLength));
        CreateShellWall(shell.transform, "Wall_Near",
            new Vector3(0f, 4f, ArcAcademyLayout.ShellNearZ),
            new Vector3(ArcAcademyLayout.ShellHalfWidth * 2f, 8f, 0.4f));
        CreateShellWall(shell.transform, "Wall_Far",
            new Vector3(0f, 4f, ArcAcademyLayout.ShellFarZ),
            new Vector3(ArcAcademyLayout.ShellHalfWidth * 2f, 8f, 0.4f));

        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(shell.transform);
        ceiling.transform.position = new Vector3(0f, ArcAcademyLayout.CeilingHeight, shellCenterZ);
        ceiling.transform.localScale = new Vector3(
            ArcAcademyLayout.ShellHalfWidth / 5f,
            1f,
            shellLength / 10f);
        ceiling.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        ApplyColor(ceiling, WarehouseWall);
        Object.DestroyImmediate(ceiling.GetComponent<MeshCollider>());
    }

    private static void CreateShellWall(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        ApplyColor(wall, WarehouseWall);
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
        ApplyColor(floor, CourtOrange);

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
        ApplyColor(keyFill, KeyPaint);
        Object.DestroyImmediate(keyFill.GetComponent<BoxCollider>());

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

    private static Transform CreateSpawnPad(Transform parent)
    {
        var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = ArcAcademyLayout.SpawnPadName;
        pad.transform.SetParent(parent);
        pad.transform.position = ArcAcademyLayout.SpawnPadPosition;
        pad.transform.localScale = ArcAcademyLayout.SpawnPadScale;
        ApplyColor(pad, SpawnPadDark);
        Object.DestroyImmediate(pad.GetComponent<BoxCollider>());
        return pad.transform;
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
        ApplyColor(baseJoint, JointWhite);

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pole.name = "Pole";
        pole.transform.SetParent(hoopRoot.transform);
        pole.transform.localPosition = new Vector3(0f, 1.5f, -0.35f);
        pole.transform.localScale = new Vector3(0.18f, 3f, 0.18f);
        ApplyColor(pole, PoleGray);

        var armJoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        armJoint.name = "ArmJoint";
        armJoint.transform.SetParent(hoopRoot.transform);
        armJoint.transform.localPosition = new Vector3(0f, 2.95f, -0.1f);
        armJoint.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
        ApplyColor(armJoint, JointWhite);

        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "Backboard";
        backboard.transform.SetParent(hoopRoot.transform);
        backboard.transform.localPosition = new Vector3(0f, 3.05f, 0.08f);
        backboard.transform.localScale = new Vector3(1.8f, 1.05f, 0.06f);
        ApplyColor(backboard, BackboardWhite);

        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = ArcAcademyLayout.RimName;
        rim.transform.SetParent(hoopRoot.transform);
        rim.transform.localPosition = ArcAcademyLayout.RimLocalDefaultPosition;
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.transform.localScale = new Vector3(0.9f, 0.04f, 0.9f);
        ApplyColor(rim, RimOrange);

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
        ApplyColor(bob, BobOrange);

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
        ApplyColor(line, color);
        Object.DestroyImmediate(line.GetComponent<BoxCollider>());
    }

    private static void ApplyColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        renderer.sharedMaterial = mat;
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
