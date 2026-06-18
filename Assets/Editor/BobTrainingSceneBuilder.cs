using System.IO;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BobTrainingSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

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

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        var camera = cameraGo.AddComponent<Camera>();
        cameraGo.transform.position = new Vector3(0f, 7f, -18f);
        cameraGo.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
        cameraGo.AddComponent<AudioListener>();

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "CourtFloor";
        floor.transform.localScale = new Vector3(25f, 1f, 25f);

        // Simple light gray / wood-ish court material
        var floorRenderer = floor.GetComponent<Renderer>();
        var floorMat = new Material(Shader.Find("Standard"));
        floorMat.color = new Color(0.82f, 0.82f, 0.78f);
        floorRenderer.sharedMaterial = floorMat;

        var hoopRoot = new GameObject("Hoop");
        hoopRoot.transform.position = new Vector3(0f, 4.5f, -13f);

        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        backboard.name = "Backboard";
        backboard.transform.SetParent(hoopRoot.transform);
        backboard.transform.localPosition = new Vector3(0f, 0.9f, 0.3f);
        backboard.transform.localScale = new Vector3(2.2f, 0.12f, 1.1f);

        // Thin horizontal cylinder as rim (ring plane). No built-in Torus primitive.
        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "Rim";
        rim.transform.SetParent(hoopRoot.transform);
        rim.transform.localPosition = new Vector3(0f, 0f, 0f);
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.transform.localScale = new Vector3(1.35f, 0.06f, 1.35f);

        var bob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bob.name = "Bob";
        bob.transform.position = new Vector3(0f, 1.5f, 0f);
        bob.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        var bobRenderer = bob.GetComponent<Renderer>();
        var bobMaterial = new Material(Shader.Find("Standard"));
        bobMaterial.color = new Color(1f, 0.38f, 0f);
        bobRenderer.sharedMaterial = bobMaterial;

        var rb = bob.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        var agent = bob.AddComponent<BobAgent>();
        agent.hoop = rim.transform;

        var behavior = bob.GetComponent<BehaviorParameters>();
        behavior.BehaviorName = "Bob";
        behavior.TeamId = 0;
        behavior.BrainParameters.VectorObservationSize = 8;
        behavior.BrainParameters.NumStackedVectorObservations = 1;
        behavior.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(3);

        bob.AddComponent<DecisionRequester>().DecisionPeriod = 1;

        // Low/invisible boundary walls so Bob doesn't fly to infinity
        var boundsParent = new GameObject("Boundaries");
        CreateWall(boundsParent, "Wall_Left",  new Vector3(-13f, 2f, -6.5f), new Vector3(1f, 4f, 26f));
        CreateWall(boundsParent, "Wall_Right", new Vector3( 13f, 2f, -6.5f), new Vector3(1f, 4f, 26f));
        CreateWall(boundsParent, "Wall_Back",  new Vector3(0f, 2f,  12f),   new Vector3(26f, 4f, 1f));
        CreateWall(boundsParent, "Wall_Far",   new Vector3(0f, 2f, -25f),   new Vector3(26f, 4f, 1f));

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Debug.Log($"Bob training scene created at {ScenePath}");
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
        // Invisible but solid walls (colliders still work). Comment out to visualize.
        var rend = w.GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;
    }
}
