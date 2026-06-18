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
        cameraGo.transform.position = new Vector3(0f, 6f, -12f);
        cameraGo.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        cameraGo.AddComponent<AudioListener>();

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "CourtFloor";
        floor.transform.localScale = new Vector3(2f, 1f, 2f);

        var hoopRoot = new GameObject("Hoop");
        hoopRoot.transform.position = new Vector3(0f, 4f, -10f);

        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        backboard.name = "Backboard";
        backboard.transform.SetParent(hoopRoot.transform);
        backboard.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        backboard.transform.localScale = new Vector3(1.5f, 0.1f, 1f);

        var rim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rim.name = "Rim";
        rim.transform.SetParent(hoopRoot.transform);
        rim.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        rim.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);

        var bob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bob.name = "Bob";
        bob.transform.position = new Vector3(0f, 1f, 0f);

        var bobRenderer = bob.GetComponent<Renderer>();
        var bobMaterial = new Material(Shader.Find("Standard"));
        bobMaterial.color = new Color(1f, 0.38f, 0f);
        bobRenderer.sharedMaterial = bobMaterial;

        var rb = bob.AddComponent<Rigidbody>();
        rb.useGravity = false;

        var agent = bob.AddComponent<BobAgent>();
        agent.hoop = rim.transform;

        var behavior = bob.GetComponent<BehaviorParameters>();
        behavior.BehaviorName = "Bob";
        behavior.TeamId = 0;
        behavior.BrainParameters.VectorObservationSize = 9;
        behavior.BrainParameters.NumStackedVectorObservations = 1;
        behavior.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(3);

        bob.AddComponent<DecisionRequester>().DecisionPeriod = 1;

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
}
