#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the AI Warehouse–style simple arena from primitives, saves a prefab, and replaces
/// the visible photoreal court shell in BobTraining.unity.
///
/// Unity APIs (6.0): GameObject.CreatePrimitive, AssetDatabase.CreateAsset,
/// PrefabUtility.SaveAsPrefabAssetAndConnect — see Unity Scripting API.
/// </summary>
public static class SimpleArcAcademyArenaBuilder
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";
    private const string MaterialsFolder = "Assets/Materials/SimpleArena";
    private const string PrefabsFolder = "Assets/Prefabs";

    private const string FloorMatPath = MaterialsFolder + "/Mat_Floor_Grid.mat";
    private const string WallMatPath = MaterialsFolder + "/Mat_Wall_Tile_White.mat";
    private const string LegacyWallMatPath = MaterialsFolder + "/Mat_Wall_Blue.mat";
    private const string TargetRedMatPath = MaterialsFolder + "/Mat_Target_Red.mat";
    private const string TargetYellowMatPath = MaterialsFolder + "/Mat_Target_Yellow.mat";
    private const string TargetGreenMatPath = MaterialsFolder + "/Mat_Target_Green.mat";

    [MenuItem("Bob/Setup/Simple Arc Academy Arena")]
    public static void MenuApply()
    {
        if (!EnsureScene())
        {
            return;
        }

        ApplyAll();
        EditorUtility.DisplayDialog(
            "Simple Arc Academy Arena",
            "Arena built, prefab saved, and legacy court visuals hidden.\nPress Play to preview.",
            "OK");
    }

    [MenuItem("Tools/Bob/Setup Simple Arc Academy Arena")]
    public static void MenuApplySilent()
    {
        ApplySilently();
    }

    public static void ApplySilently()
    {
        if (!EnsureScene())
        {
            return;
        }

        ApplyAll();
        EditorSceneManager.SaveOpenScenes();
    }

    public static void ApplyFromCli()
    {
        if (EditorApplication.isCompiling)
        {
            EditorApplication.delayCall += ApplyFromCli;
            return;
        }

        if (!EnsureScene())
        {
            EditorApplication.Exit(1);
            return;
        }

        ApplyAll();
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("SIMPLE_ARENA_OK: Simple Arc Academy arena applied via CLI.");
        EditorApplication.Exit(0);
    }

    private static bool EnsureScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath);
        }

        if (GameObject.Find(ArcAcademyLayout.ArenaName) == null)
        {
            Debug.LogError("SIMPLE_ARENA_FAIL: TrainingArena not found — open BobTraining.unity.");
            return false;
        }

        return true;
    }

    private static void ApplyAll()
    {
        BobPhysicsLayerSetup.EnsureLayersAndCollisionMatrix();

        EnsureMaterialAssets();
        var materials = LoadMaterials();

        var arenaRoot = FindOrCreateArenaRoot();
        arenaRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        BuildArenaHierarchy(arenaRoot.transform, materials);
        EnsureSpawnAndManager(arenaRoot);
        SavePrefabFromInstance(arenaRoot);
        WireBobToArena(arenaRoot);
        EnsureSingleBasketball(arenaRoot);

        HideLegacyCourtVisuals();
        ApplyLabScenePreset();
        BobWallHudBuilder.EnsureWallTrainingHud(arenaRoot.transform);
        EnsurePowerPathPulse(arenaRoot.transform);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"✅ SIMPLE ARC ACADEMY ARENA — built at scene root, prefab at {SimpleArcAcademyArena.PrefabPath}");
    }

    private static GameObject FindOrCreateArenaRoot()
    {
        var existing = GameObject.Find(SimpleArcAcademyArena.RootName);
        if (existing != null)
        {
            return existing;
        }

        return new GameObject(SimpleArcAcademyArena.RootName);
    }

    private static void BuildArenaHierarchy(Transform root, ArenaMaterials materials)
    {
        EnsurePrimitive(
            root,
            SimpleArcAcademyArena.FloorName,
            PrimitiveType.Plane,
            SimpleArcAcademyArena.FloorPosition,
            SimpleArcAcademyArena.FloorScale,
            materials.Floor,
            BobPhysicsLayers.TrainingArenaLayer,
            keepCollider: true);

        EnsurePrimitive(
            root,
            SimpleArcAcademyArena.WallNorthName,
            PrimitiveType.Cube,
            SimpleArcAcademyArena.WallNorthPosition,
            SimpleArcAcademyArena.WallNorthScale,
            materials.Wall,
            BobPhysicsLayers.TrainingArenaLayer,
            keepCollider: true);

        EnsurePrimitive(
            root,
            SimpleArcAcademyArena.WallSouthName,
            PrimitiveType.Cube,
            SimpleArcAcademyArena.WallSouthPosition,
            SimpleArcAcademyArena.WallSouthScale,
            materials.Wall,
            BobPhysicsLayers.TrainingArenaLayer,
            keepCollider: true);

        EnsurePrimitive(
            root,
            SimpleArcAcademyArena.WallEastName,
            PrimitiveType.Cube,
            SimpleArcAcademyArena.WallEastPosition,
            SimpleArcAcademyArena.WallEastScale,
            materials.Wall,
            BobPhysicsLayers.TrainingArenaLayer,
            keepCollider: true);

        EnsurePrimitive(
            root,
            SimpleArcAcademyArena.WallWestName,
            PrimitiveType.Cube,
            SimpleArcAcademyArena.WallWestPosition,
            SimpleArcAcademyArena.WallWestScale,
            materials.Wall,
            BobPhysicsLayers.TrainingArenaLayer,
            keepCollider: true);

        if (!SimpleArcAcademyArena.ShowBudgetFlavorProps)
        {
            return;
        }

        EnsurePrimitive(
            root,
            SimpleArcAcademyArena.GoalBudgetSurplusName,
            PrimitiveType.Sphere,
            SimpleArcAcademyArena.GoalPosition,
            SimpleArcAcademyArena.GoalScale,
            materials.TargetRed,
            BobPhysicsLayers.DecorationLayer,
            keepCollider: false);

        EnsurePrimitive(
            root,
            SimpleArcAcademyArena.ZoneTaxRevenueName,
            PrimitiveType.Plane,
            SimpleArcAcademyArena.ZonePosition,
            SimpleArcAcademyArena.ZoneScale,
            materials.TargetGreen,
            BobPhysicsLayers.DecorationLayer,
            keepCollider: false);

        for (int i = 0; i < SimpleArcAcademyArena.ObstaclePlacements.Length; i++)
        {
            var placement = SimpleArcAcademyArena.ObstaclePlacements[i];
            EnsurePrimitive(
                root,
                $"{SimpleArcAcademyArena.ObstaclePrefix}{i + 1}",
                PrimitiveType.Cube,
                placement.position,
                placement.scale,
                materials.TargetYellow,
                BobPhysicsLayers.DecorationLayer,
                keepCollider: false);
        }
    }

    private static void EnsurePrimitive(
        Transform parent,
        string name,
        PrimitiveType primitiveType,
        Vector3 localPosition,
        Vector3 localScale,
        Material material,
        int layer,
        bool keepCollider)
    {
        var child = parent.Find(name);
        GameObject go;

        if (child == null)
        {
            go = GameObject.CreatePrimitive(primitiveType);
            go.name = name;
            go.transform.SetParent(parent, false);
        }
        else
        {
            go = child.gameObject;
        }

        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;
        go.SetActive(true);

        BobPhysicsLayers.SetLayerRecursively(go, layer);

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        if (!keepCollider)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }
    }

    private static void EnsureSpawnAndManager(GameObject arenaRoot)
    {
        var spawn = EnsureSpawnPoint(arenaRoot.transform);
        var manager = arenaRoot.GetComponent<SimpleArcArenaManager>();
        if (manager == null)
        {
            manager = arenaRoot.AddComponent<SimpleArcArenaManager>();
        }

        manager.Wire(null, spawn, null);
        EditorUtility.SetDirty(manager);
    }

    private static void WireBobToArena(GameObject arenaRoot)
    {
        if (PrefabUtility.IsPartOfPrefabInstance(arenaRoot))
        {
            PrefabUtility.UnpackPrefabInstance(
                arenaRoot,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
        }

        var spawn = arenaRoot.transform.Find(SimpleArcAcademyArena.SpawnPointName);
        var bobGo = GameObject.Find("Bob");
        if (bobGo == null)
        {
            Debug.LogWarning("SIMPLE_ARENA_WARN: Bob not found — skip agent wiring.");
            return;
        }

        if (!bobGo.TryGetComponent(out BobAgent bobAgent))
        {
            Debug.LogWarning("SIMPLE_ARENA_WARN: BobAgent missing on Bob.");
            return;
        }

        var rim = FindRimTransform();
        if (rim != null)
        {
            bobAgent.hoop = rim;
            EditorUtility.SetDirty(bobAgent);
        }
        else
        {
            Debug.LogWarning("SIMPLE_ARENA_WARN: Rim not found for BobAgent.hoop.");
        }

        EnsureBobFace(bobGo);
        var bobPrefab = SaveBobPrefab(bobGo);

        bobGo.transform.SetParent(arenaRoot.transform, true);

        var manager = arenaRoot.GetComponent<SimpleArcArenaManager>();
        if (manager != null)
        {
            manager.Wire(bobAgent, spawn, bobPrefab);
            EditorUtility.SetDirty(manager);
        }
    }

    private static void EnsureSingleBasketball(GameObject arenaRoot)
    {
        EnforceSingleBobAndBallInstances(arenaRoot);

        var bobGo = GameObject.Find("Bob");
        if (bobGo == null || !bobGo.TryGetComponent(out BobAgent bobAgent))
        {
            Debug.LogWarning("SIMPLE_ARENA_WARN: Bob not found — skip basketball wiring.");
            return;
        }

        if (bobGo.GetComponent<BobShotArcPreview>() == null)
        {
            bobGo.AddComponent<BobShotArcPreview>();
        }

        var manager = arenaRoot.GetComponent<SimpleArcArenaManager>();
        var spawn = arenaRoot.transform.Find(SimpleArcAcademyArena.SpawnPointName);
        Vector3 bobSpawn = manager != null
            ? manager.GetBobSpawnPosition()
            : (spawn != null
                ? spawn.position + ArcAcademyLayout.BobSpawnOffset
                : ArcAcademyLayout.BobSpawnPosition);

        var releasePos = BasketballProjectileSetup.GetReleasePosition(bobSpawn);
        var ball = BasketballProjectileSetup.EnsureBasketball(arenaRoot.transform, releasePos);

        if (ball.TryGetComponent(out Rigidbody ballRb))
        {
            BasketballProjectileSetup.WireLauncher(bobAgent, ballRb);
            EditorUtility.SetDirty(bobAgent);
        }

        SaveBasketballPrefab(ball);
        SaveBobPrefab(bobGo);
    }

    private static void EnforceSingleBobAndBallInstances(GameObject arenaRoot)
    {
        BobAgent primaryBob = null;
        foreach (var agent in Object.FindObjectsByType<BobAgent>())
        {
            if (primaryBob == null)
            {
                primaryBob = agent;
                continue;
            }

            Object.DestroyImmediate(agent.gameObject);
        }

        SimpleBasketball primaryMarker = null;
        foreach (var marker in Object.FindObjectsByType<SimpleBasketball>())
        {
            if (primaryMarker == null)
            {
                primaryMarker = marker;
                continue;
            }

            Object.DestroyImmediate(marker.gameObject);
        }

        var namedBall = GameObject.Find(BasketballProjectileSetup.BasketballName);
        if (namedBall != null && namedBall.transform.parent != arenaRoot.transform)
        {
            namedBall.transform.SetParent(arenaRoot.transform, true);
        }
    }

    private static GameObject SaveBasketballPrefab(GameObject ball)
    {
        Directory.CreateDirectory(PrefabsFolder);
        var path = SimpleArcAcademyArena.BasketballPrefabPath;

        if (PrefabUtility.IsPartOfPrefabInstance(ball))
        {
            return PrefabUtility.SaveAsPrefabAssetAndConnect(
                ball,
                path,
                InteractionMode.AutomatedAction);
        }

        return PrefabUtility.SaveAsPrefabAssetAndConnect(
            ball,
            path,
            InteractionMode.AutomatedAction);
    }

    private static Transform EnsureSpawnPoint(Transform arenaRoot)
    {
        var spawn = arenaRoot.Find(SimpleArcAcademyArena.SpawnPointName);
        if (spawn == null)
        {
            var go = new GameObject(SimpleArcAcademyArena.SpawnPointName);
            go.transform.SetParent(arenaRoot, false);
            spawn = go.transform;
        }

        spawn.localPosition = SimpleArcAcademyArena.BobSpawnLocalPosition;
        spawn.localRotation = Quaternion.identity;
        spawn.localScale = Vector3.one;
        return spawn;
    }

    private static GameObject SaveBobPrefab(GameObject bob)
    {
        RepairVrShootInputReference(bob);

        Directory.CreateDirectory(PrefabsFolder);
        var path = SimpleArcAcademyArena.BobPrefabPath;

        if (PrefabUtility.IsPartOfPrefabInstance(bob))
        {
            var duplicate = Object.Instantiate(bob);
            duplicate.name = bob.name;
            try
            {
                return PrefabUtility.SaveAsPrefabAssetAndConnect(
                    duplicate,
                    path,
                    InteractionMode.AutomatedAction);
            }
            finally
            {
                Object.DestroyImmediate(duplicate);
            }
        }

        return PrefabUtility.SaveAsPrefabAssetAndConnect(
            bob,
            path,
            InteractionMode.AutomatedAction);
    }

    /// <summary>
    /// Scene rebuilds can leave VrShootInputPlaceholder on a stale embedded MonoScript fileID;
    /// PrefabUtility rejects that in batchmode. Recreate the component when the script asset is missing.
    /// </summary>
    private static void RepairVrShootInputReference(GameObject bob)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(bob);

        if (bob.GetComponent<VrShootInputPlaceholder>() == null)
        {
            bob.AddComponent<VrShootInputPlaceholder>();
        }
    }

    private static Transform FindRimTransform()
    {
        var hoop = GameObject.Find(ArcAcademyLayout.HoopName);
        if (hoop == null)
        {
            return null;
        }

        return FindDeepChild(hoop.transform, ArcAcademyLayout.RimName);
    }

    private static Transform FindDeepChild(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindDeepChild(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static void SavePrefabFromInstance(GameObject arenaRoot)
    {
        Directory.CreateDirectory(PrefabsFolder);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimpleArcAcademyArena.PrefabPath);
        if (prefab == null)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(
                arenaRoot,
                SimpleArcAcademyArena.PrefabPath,
                InteractionMode.AutomatedAction);
            return;
        }

        PrefabUtility.SaveAsPrefabAssetAndConnect(
            arenaRoot,
            SimpleArcAcademyArena.PrefabPath,
            InteractionMode.AutomatedAction);
    }

    private static void HideLegacyCourtVisuals()
    {
        var arena = ArcAcademyLayout.ArenaName;

        SetActiveIfFound($"{arena}/{ArcAcademyLayout.CourtFloorName}", false);
        SetActiveIfFound($"{arena}/Boundaries", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.WarehouseShellName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.TrainingBaysName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.MountainWindowName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.DecorativeHoopsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.DistanceMarkingsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.FloorDecalsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.SpawnPadBrandingName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.CourtMarkingsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.TrajectoryVisualsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.SignageArcAcademyName}", false);
        SetActiveIfFound($"{arena}/ComplexRenderGroup", false);

        var lightingRig = GameObject.Find($"{arena}/{ArcAcademyLayout.LightingRigName}");
        if (lightingRig != null)
        {
            for (int i = 0; i < lightingRig.transform.childCount; i++)
            {
                var child = lightingRig.transform.GetChild(i);
                if (child.name != "Sun" && !child.name.StartsWith("Hdrp"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        HideSimpleArenaBudgetProps();

        var spawnPad = GameObject.Find($"{arena}/{ArcAcademyLayout.SpawnPadName}");
        if (spawnPad != null)
        {
            if (spawnPad.TryGetComponent(out Renderer renderer))
            {
                renderer.enabled = false;
            }

            spawnPad.SetActive(true);
            var ballSpawn = spawnPad.transform.Find(ArcAcademyLayout.BallSpawnPointName);
            if (ballSpawn != null)
            {
                ballSpawn.gameObject.SetActive(true);
            }
        }
    }

    private static void HideSimpleArenaBudgetProps()
    {
        var arena = GameObject.Find(SimpleArcAcademyArena.RootName);
        if (arena == null)
        {
            return;
        }

        SetChildActive(arena.transform, SimpleArcAcademyArena.GoalBudgetSurplusName, false);
        SetChildActive(arena.transform, SimpleArcAcademyArena.ZoneTaxRevenueName, false);

        for (int i = 0; i < SimpleArcAcademyArena.ObstaclePlacements.Length; i++)
        {
            SetChildActive(arena.transform, $"{SimpleArcAcademyArena.ObstaclePrefix}{i + 1}", false);
        }
    }

    private static void SetChildActive(Transform parent, string childName, bool active)
    {
        var child = parent.Find(childName);
        if (child != null)
        {
            child.gameObject.SetActive(active);
        }
    }

    private static void ApplyLabScenePreset()
    {
        ArcAcademyLabRenderPreset.ApplyLabViewPreset();

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        camera.transform.position = SimpleArcAcademyArena.LabCameraPosition;
        camera.transform.rotation = Quaternion.LookRotation(
            SimpleArcAcademyArena.LabCameraLookAt - SimpleArcAcademyArena.LabCameraPosition,
            Vector3.up);
        camera.fieldOfView = SimpleArcAcademyArena.LabCameraFieldOfView;

        if (camera.TryGetComponent(out ArcAcademyDemoCamera demoCamera))
        {
            demoCamera.ResetToLabHero();
            EditorUtility.SetDirty(demoCamera);
        }

        EditorUtility.SetDirty(camera);
    }

    private static void EnsurePowerPathPulse(Transform arenaRoot)
    {
        var floor = arenaRoot.Find(SimpleArcAcademyArena.FloorName);
        if (floor == null)
        {
            return;
        }

        var existing = floor.Find(SimpleArcAcademyArena.PowerPathPulseName);
        GameObject pulseGo;
        if (existing == null)
        {
            pulseGo = new GameObject(SimpleArcAcademyArena.PowerPathPulseName);
            pulseGo.transform.SetParent(floor, false);
        }
        else
        {
            pulseGo = existing.gameObject;
        }

        pulseGo.transform.localPosition = Vector3.zero;
        if (pulseGo.GetComponent<ArcAcademyPowerPathPulse>() == null)
        {
            pulseGo.AddComponent<ArcAcademyPowerPathPulse>();
        }
    }

    private static void EnsureBobFace(GameObject bob)
    {
        EnsureBobEye(bob.transform, "Eye_Left", new Vector3(-0.18f, 0.12f, 0.51f));
        EnsureBobEye(bob.transform, "Eye_Right", new Vector3(0.18f, 0.12f, 0.51f));

        if (bob.GetComponent<BobProceduralAnimator>() == null)
        {
            bob.AddComponent<BobProceduralAnimator>();
        }

        if (bob.GetComponent<BobFaceExpression>() == null)
        {
            bob.AddComponent<BobFaceExpression>();
        }

        if (bob.GetComponent<BobSpeechBubble>() == null)
        {
            var bubbleRoot = new GameObject("BobSpeechBubble");
            bubbleRoot.transform.SetParent(bob.transform, false);
            bubbleRoot.transform.localPosition = new Vector3(0f, 1.1f, 0f);

            var textGo = new GameObject("BubbleText");
            textGo.transform.SetParent(bubbleRoot.transform, false);
            textGo.transform.localPosition = Vector3.zero;
            textGo.transform.localRotation = Quaternion.identity;
            textGo.transform.localScale = Vector3.one;

            var textMesh = textGo.AddComponent<TextMesh>();
            textMesh.text = "Great job, Bob!";
            textMesh.characterSize = 0.08f;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            var bubble = bubbleRoot.AddComponent<BobSpeechBubble>();
            var so = new SerializedObject(bubble);
            so.FindProperty("textMesh").objectReferenceValue = textMesh;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void EnsureBobEye(Transform bob, string name, Vector3 localPosition)
    {
        var existing = bob.Find(name);
        GameObject eyeGo;

        if (existing != null)
        {
            eyeGo = existing.gameObject;
        }
        else
        {
            eyeGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            eyeGo.name = name;
            eyeGo.transform.SetParent(bob, false);

            var collider = eyeGo.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        eyeGo.transform.localPosition = localPosition;
        eyeGo.transform.localRotation = Quaternion.identity;
        eyeGo.transform.localScale = new Vector3(0.14f, 0.2f, 1f);
        BobPhysicsLayers.SetLayerRecursively(eyeGo, BobPhysicsLayers.DecorationLayer);

        var renderer = eyeGo.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = ArcAcademyMaterialFactory.CreateHdrpLit(new Color(0.08f, 0.08f, 0.1f), 0.1f, 0f);
        }
    }

    private static void SetActiveIfFound(string path, bool active)
    {
        var parts = path.Split('/');
        if (parts.Length != 2)
        {
            return;
        }

        var parent = GameObject.Find(parts[0]);
        if (parent == null)
        {
            return;
        }

        var child = parent.transform.Find(parts[1]);
        if (child != null)
        {
            child.gameObject.SetActive(active);
        }
    }

    private static void EnsureMaterialAssets()
    {
        Directory.CreateDirectory(MaterialsFolder);

        var floorTex = SimpleArenaTextureFactory.EnsureFloorGridTexture();
        var wallTex = SimpleArenaTextureFactory.EnsureWallTileTexture();

        EnsureLitMaterialWithTexture(
            FloorMatPath,
            "Mat_Floor_Grid",
            SimpleArcAcademyArena.FloorColor,
            floorTex,
            smoothness: 0.25f,
            metallic: 0f,
            textureScale: new Vector2(20f, 20f));

        EnsureLitMaterialWithTexture(
            WallMatPath,
            "Mat_Wall_Tile_White",
            SimpleArcAcademyArena.WallColor,
            wallTex,
            smoothness: 0.15f,
            metallic: 0f,
            textureScale: new Vector2(8f, 2f));

        EnsureUnlitMaterial(TargetRedMatPath, "Mat_Target_Red", SimpleArcAcademyArena.TargetRed);
        EnsureUnlitMaterial(TargetYellowMatPath, "Mat_Target_Yellow", SimpleArcAcademyArena.TargetYellow);
        EnsureUnlitMaterial(TargetGreenMatPath, "Mat_Target_Green", SimpleArcAcademyArena.TargetGreen);
    }

    private static void EnsureLitMaterialWithTexture(
        string assetPath,
        string assetName,
        Color baseColor,
        Texture2D texture,
        float smoothness,
        float metallic,
        Vector2 textureScale)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (mat == null)
        {
            mat = ArcAcademyMaterialFactory.CreateHdrpLit(baseColor, smoothness, metallic);
            mat.name = assetName;
            AssetDatabase.CreateAsset(mat, assetPath);
        }

        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", baseColor);
        }

        if (texture != null && mat.HasProperty("_BaseColorMap"))
        {
            mat.SetTexture("_BaseColorMap", texture);
            mat.SetTextureScale("_BaseColorMap", textureScale);
        }

        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", smoothness);
        }

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }

        EditorUtility.SetDirty(mat);
    }

    private static void EnsureUnlitMaterial(string assetPath, string assetName, Color color)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (existing != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateArcLineMaterial(color, 1f);
        mat.name = assetName;
        AssetDatabase.CreateAsset(mat, assetPath);
    }

    private static ArenaMaterials LoadMaterials()
    {
        var wall = AssetDatabase.LoadAssetAtPath<Material>(WallMatPath)
            ?? AssetDatabase.LoadAssetAtPath<Material>(LegacyWallMatPath);

        return new ArenaMaterials
        {
            Floor = AssetDatabase.LoadAssetAtPath<Material>(FloorMatPath),
            Wall = wall,
            TargetRed = AssetDatabase.LoadAssetAtPath<Material>(TargetRedMatPath),
            TargetYellow = AssetDatabase.LoadAssetAtPath<Material>(TargetYellowMatPath),
            TargetGreen = AssetDatabase.LoadAssetAtPath<Material>(TargetGreenMatPath),
        };
    }

    private sealed class ArenaMaterials
    {
        public Material Floor;
        public Material Wall;
        public Material TargetRed;
        public Material TargetYellow;
        public Material TargetGreen;
    }
}
#endif
