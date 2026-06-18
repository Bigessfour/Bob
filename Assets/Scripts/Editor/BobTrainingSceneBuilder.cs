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

    private static readonly Color CourtWood = new(0.76f, 0.55f, 0.28f);
    private static readonly Color CourtLine = new(0.95f, 0.95f, 0.95f);
    private static readonly Color KeyPaint = new(0.82f, 0.72f, 0.55f);
    private static readonly Color BackboardWhite = new(0.92f, 0.92f, 0.88f);
    private static readonly Color RimOrange = new(1f, 0.45f, 0.1f);
    private static readonly Color PoleGray = new(0.35f, 0.35f, 0.38f);
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

        var arena = new GameObject(BobCourtLayout.ArenaName);

        CreateLighting();
        CreateCamera();
        CreateCourt(arena.transform);
        Transform rim = CreateHoop(arena.transform);
        CreateBob(rim);
        CreateBoundaries(arena.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Debug.Log($"Bob training scene created at {ScenePath}");
    }

    private static void CreateLighting()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
    }

    private static void CreateCamera()
    {
        var cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        cameraGo.AddComponent<Camera>();
        cameraGo.transform.position = BobCourtLayout.CameraPosition;
        cameraGo.transform.rotation = Quaternion.LookRotation(
            BobCourtLayout.CameraLookAt - BobCourtLayout.CameraPosition,
            Vector3.up);
        cameraGo.AddComponent<AudioListener>();
    }

    private static void CreateCourt(Transform parent)
    {
        float courtLength = BobCourtLayout.CourtNearZ - BobCourtLayout.CourtFarZ;
        float courtCenterZ = (BobCourtLayout.CourtNearZ + BobCourtLayout.CourtFarZ) * 0.5f;

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = BobCourtLayout.CourtFloorName;
        floor.transform.SetParent(parent);
        floor.transform.position = new Vector3(0f, 0f, courtCenterZ);
        floor.transform.localScale = new Vector3(
            BobCourtLayout.CourtHalfWidth / 5f,
            1f,
            courtLength / 10f);
        ApplyColor(floor, CourtWood);

        var markings = new GameObject(BobCourtLayout.CourtMarkingsName);
        markings.transform.SetParent(parent);

        float baselineZ = BobCourtLayout.HoopRootPosition.z;
        CreateLineMark(markings.transform, "Baseline", new Vector3(0f, 0.02f, baselineZ),
            new Vector3(BobCourtLayout.CourtHalfWidth * 2f, 0.04f, 0.08f), CourtLine);
        CreateLineMark(markings.transform, "FreeThrowLine", new Vector3(0f, 0.02f, BobCourtLayout.FreeThrowLineZ),
            new Vector3(BobCourtLayout.KeyHalfWidth * 2f, 0.04f, 0.08f), CourtLine);

        float keyFrontZ = baselineZ + BobCourtLayout.KeyDepthFromBaseline;
        CreateLineMark(markings.transform, "KeyLeft", new Vector3(-BobCourtLayout.KeyHalfWidth, 0.02f, (baselineZ + keyFrontZ) * 0.5f),
            new Vector3(0.08f, 0.04f, BobCourtLayout.KeyDepthFromBaseline), CourtLine);
        CreateLineMark(markings.transform, "KeyRight", new Vector3(BobCourtLayout.KeyHalfWidth, 0.02f, (baselineZ + keyFrontZ) * 0.5f),
            new Vector3(0.08f, 0.04f, BobCourtLayout.KeyDepthFromBaseline), CourtLine);
        CreateLineMark(markings.transform, "KeyFront", new Vector3(0f, 0.02f, keyFrontZ),
            new Vector3(BobCourtLayout.KeyHalfWidth * 2f, 0.04f, 0.08f), CourtLine);

        var keyFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        keyFill.name = "KeyPaint";
        keyFill.transform.SetParent(markings.transform);
        keyFill.transform.position = new Vector3(0f, 0.015f, (baselineZ + keyFrontZ) * 0.5f);
        keyFill.transform.localScale = new Vector3(
            BobCourtLayout.KeyHalfWidth * 2f,
            0.03f,
            BobCourtLayout.KeyDepthFromBaseline);
        ApplyColor(keyFill, KeyPaint);
        Object.DestroyImmediate(keyFill.GetComponent<BoxCollider>());
    }

    private static Transform CreateHoop(Transform parent)
    {
        var hoopRoot = new GameObject(BobCourtLayout.HoopName);
        hoopRoot.transform.SetParent(parent);
        hoopRoot.transform.position = BobCourtLayout.HoopRootPosition;

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pole.name = "Pole";
        pole.transform.SetParent(hoopRoot.transform);
        pole.transform.localPosition = new Vector3(0f, 1.5f, -0.35f);
        pole.transform.localScale = new Vector3(0.18f, 3f, 0.18f);
        ApplyColor(pole, PoleGray);

        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "Backboard";
        backboard.transform.SetParent(hoopRoot.transform);
        backboard.transform.localPosition = new Vector3(0f, 3.05f, 0.08f);
        backboard.transform.localScale = new Vector3(1.8f, 1.05f, 0.06f);
        ApplyColor(backboard, BackboardWhite);

        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = BobCourtLayout.RimName;
        rim.transform.SetParent(hoopRoot.transform);
        rim.transform.localPosition = BobCourtLayout.RimLocalPosition;
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.transform.localScale = new Vector3(0.9f, 0.04f, 0.9f);
        ApplyColor(rim, RimOrange);

        var scoreZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        scoreZone.name = BobCourtLayout.ScoreZoneName;
        scoreZone.transform.SetParent(rim.transform);
        scoreZone.transform.localPosition = Vector3.zero;
        scoreZone.transform.localScale = Vector3.one * (BobCourtLayout.RimScoreRadius * 2f / 0.9f);
        ApplyColor(scoreZone, new Color(0f, 1f, 0f, 0.15f));
        var scoreRenderer = scoreZone.GetComponent<Renderer>();
        if (scoreRenderer != null)
        {
            scoreRenderer.enabled = false;
        }

        var scoreCollider = scoreZone.GetComponent<SphereCollider>();
        scoreCollider.isTrigger = true;
        scoreZone.AddComponent<HoopScoreZone>();

        return rim.transform;
    }

    private static void CreateBob(Transform rim)
    {
        var bob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bob.name = "Bob";
        bob.transform.position = BobCourtLayout.BobSpawnPosition;
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

        float midZ = (BobCourtLayout.CourtNearZ + BobCourtLayout.CourtFarZ) * 0.5f;
        float length = BobCourtLayout.CourtNearZ - BobCourtLayout.CourtFarZ + 2f;
        float halfWidth = BobCourtLayout.CourtHalfWidth + 0.5f;

        CreateWall(boundsParent, "Wall_Left", new Vector3(-halfWidth, 2f, midZ), new Vector3(0.5f, 4f, length));
        CreateWall(boundsParent, "Wall_Right", new Vector3(halfWidth, 2f, midZ), new Vector3(0.5f, 4f, length));
        CreateWall(boundsParent, "Wall_Near", new Vector3(0f, 2f, BobCourtLayout.CourtNearZ + 0.5f), new Vector3(halfWidth * 2f, 4f, 0.5f));
        CreateWall(boundsParent, "Wall_Far", new Vector3(0f, 2f, BobCourtLayout.CourtFarZ - 0.5f), new Vector3(halfWidth * 2f, 4f, 0.5f));
        CreateWall(boundsParent, "Ceiling", new Vector3(0f, 8f, midZ), new Vector3(halfWidth * 2f, 0.5f, length));
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
