#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Collections.Generic;
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

    public static void ApplyAll()
    {
        BobPhysicsLayerSetup.EnsureLayersAndCollisionMatrix();

        EnsureMaterialAssets();
        var materials = LoadMaterials();

        var arenaRoot = FindOrCreateArenaRoot();
        arenaRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        BuildArenaHierarchy(arenaRoot.transform, materials);
        SimpleArcLabLightingBuilder.EnsureLabGymFillLights(arenaRoot.transform);
        SimpleArcCourtMarkingsBuilder.EnsureCourtMarkings(arenaRoot.transform);
        EnsureSpawnAndManager(arenaRoot);
        BobWallHudBuilder.EnsureWallTrainingHud(arenaRoot.transform);
        SavePrefabFromInstance(arenaRoot);
        WireBobToArena(arenaRoot);
        EnsureSingleBasketball(arenaRoot);
        TrainingHoopDetail.UpgradeActiveHoop();

        HideLegacyCourtVisuals();
        ApplyLabScenePreset();
        BobWallHudLayout.ApplyLabHudLayout(arenaRoot.transform);
        EnsurePowerPathPulse(arenaRoot.transform);
        StripBackgroundHoopDecorations();
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

        var floor = root.Find(SimpleArcAcademyArena.FloorName);
        if (floor != null)
        {
            var legacyMarkings = floor.Find(SimpleArcCourtMarkingsBuilder.CourtMarkingsName);
            if (legacyMarkings != null)
            {
                Object.DestroyImmediate(legacyMarkings.gameObject);
            }
        }

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
        manager.ConfigureLabFloorSpawn(SimpleArcAcademyArena.BobFloorSpawnOffset);
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

        if (PrefabUtility.IsPartOfPrefabInstance(bobGo))
        {
            PrefabUtility.UnpackPrefabInstance(
                bobGo,
                PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
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

        EnsureBobVisual(bobGo);
        EnsureBobFace(bobGo);
        NormalizeBobTransform(bobGo, bobAgent);
        SaveBobPrefabAsset(bobGo);

        bobGo.transform.SetParent(arenaRoot.transform, true);
        NormalizeBobTransform(bobGo, bobAgent);

        var manager = arenaRoot.GetComponent<SimpleArcArenaManager>();
        if (manager != null)
        {
            manager.Wire(bobAgent, spawn, AssetDatabase.LoadAssetAtPath<GameObject>(SimpleArcAcademyArena.BobPrefabPath));
            manager.ConfigureLabFloorSpawn(SimpleArcAcademyArena.BobFloorSpawnOffset);
            bobAgent.ApplySpawn(manager.GetBobSpawnPosition(), manager.GetBobSpawnRotation());
            EditorUtility.SetDirty(bobAgent);
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

        var previews = bobGo.GetComponents<BobShotArcPreview>();
        for (int i = 1; i < previews.Length; i++)
        {
            Object.DestroyImmediate(previews[i]);
        }
        if (previews.Length == 0)
        {
            bobGo.AddComponent<BobShotArcPreview>();
        }

        var manager = arenaRoot.GetComponent<SimpleArcArenaManager>();
        var spawn = arenaRoot.transform.Find(SimpleArcAcademyArena.SpawnPointName);
        Vector3 bobSpawn = manager != null
            ? manager.GetBobSpawnPosition()
            : (spawn != null
                ? SimpleArcAcademyArena.GetLabBobSpawnPosition(spawn)
                : new Vector3(0f, SimpleArcAcademyArena.BobFloorSpawnOffset.y, ArcAcademyLayout.FreeThrowLineWorldZ));

        Quaternion bobRotation = manager != null
            ? manager.GetBobSpawnRotation()
            : SimpleArcAcademyArena.GetSpawnFacingRotation(bobSpawn, bobAgent.hoop);

        var releasePos = BasketballProjectileSetup.GetReleasePosition(bobSpawn, bobRotation);
        var ball = BasketballProjectileSetup.EnsureBasketball(arenaRoot.transform, releasePos);

        if (ball.TryGetComponent(out Rigidbody ballRb))
        {
            BasketballProjectileSetup.WireLauncher(bobAgent, ballRb);
            EditorUtility.SetDirty(bobAgent);
        }

        SaveBasketballPrefab(ball);
        NormalizeBobTransform(bobGo, bobAgent);
        SaveBobPrefabAsset(bobGo);
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

    private static void NormalizeBobTransform(GameObject bobGo, BobAgent bobAgent)
    {
        float scale = BobVisualProfile.AgentCubeScale;
        bobGo.transform.localScale = new Vector3(scale, scale, scale);

        Vector3 spawn = bobGo.transform.position;
        bobGo.transform.rotation = SimpleArcAcademyArena.GetSpawnFacingRotation(
            spawn,
            bobAgent != null ? bobAgent.hoop : null);
        EditorUtility.SetDirty(bobGo);
    }

    private static void SaveBobPrefabAsset(GameObject bob)
    {
        RepairVrShootInputReference(bob);

        float scale = BobVisualProfile.AgentCubeScale;
        bob.transform.localScale = new Vector3(scale, scale, scale);
        bob.name = "Bob";

        Directory.CreateDirectory(PrefabsFolder);
        var path = SimpleArcAcademyArena.BobPrefabPath;

        GameObject source = bob;
        GameObject temp = null;
        if (PrefabUtility.IsPartOfPrefabInstance(bob))
        {
            temp = Object.Instantiate(bob);
            temp.name = bob.name;
            temp.transform.localScale = new Vector3(scale, scale, scale);
            source = temp;
        }

        try
        {
            PrefabUtility.SaveAsPrefabAsset(source, path);
            AssetDatabase.SaveAssets();
        }
        finally
        {
            if (temp != null)
            {
                Object.DestroyImmediate(temp);
            }
        }

        bob.transform.localScale = new Vector3(scale, scale, scale);
        EditorUtility.SetDirty(bob);
    }

    /// <summary>
    /// Scene rebuilds can leave VrShootInputPlaceholder on a stale embedded MonoScript fileID;
    /// PrefabUtility rejects that in batchmode. Recreate the component when the script asset is missing.
    /// </summary>
    private static void RepairVrShootInputReference(GameObject bob)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(bob);

        foreach (var mb in bob.GetComponents<MonoBehaviour>())
        {
            if (mb == null)
            {
                continue;
            }

            if (MonoScript.FromMonoBehaviour(mb) == null)
            {
                Object.DestroyImmediate(mb);
            }
        }

        foreach (var placeholder in bob.GetComponents<VrShootInputPlaceholder>())
        {
            Object.DestroyImmediate(placeholder);
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
        DestroyIfFound($"{arena}/Boundaries");
        DestroyIfFound($"{arena}/{ArcAcademyLayout.WarehouseShellName}");
        DestroyIfFound($"{arena}/{ArcAcademyLayout.TrainingBaysName}");
        DestroyIfFound($"{arena}/{ArcAcademyLayout.MountainWindowName}");
        DestroyIfFound($"{arena}/{ArcAcademyLayout.DecorativeHoopsName}");
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.DistanceMarkingsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.FloorDecalsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.SpawnPadBrandingName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.CourtMarkingsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.TrajectoryVisualsName}", false);
        SetActiveIfFound($"{arena}/{ArcAcademyLayout.SignageArcAcademyName}", false);
        DestroyIfFound("ComplexRenderGroup");

        // Aggressively remove any reintroduced background hoop decorations that take up room on the court
        // (the 8 bay portable stands etc. should not be visible in simple single-hoop training mode)
        HideExtraDecorativeHoops();

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
        ArcAcademyLabSceneCleanup.HideLegacyClutter();

        var spawnPad = GameObject.Find($"{arena}/{ArcAcademyLayout.SpawnPadName}");
        if (spawnPad != null)
        {
            spawnPad.SetActive(false);
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

        // Target CameraRig (parent at exact requested pose) + child Main Camera.
        // Write pose to rig so it "is at (13, 3.2, -3.5) looking at..."; keep child local zero.
        GameObject rig = GameObject.Find("CameraRig");
        Camera cam = null;
        Transform camXform = null;

        if (rig != null)
        {
            camXform = rig.transform;
            cam = rig.GetComponentInChildren<Camera>();
            if (cam == null && Camera.main != null)
            {
                cam = Camera.main;
            }
        }
        else
        {
            cam = Camera.main;
            camXform = cam != null ? cam.transform : null;
        }

        if (camXform == null)
        {
            return;
        }

        camXform.position = SimpleArcAcademyArena.LabCameraPosition;
        camXform.rotation = Quaternion.LookRotation(
            SimpleArcAcademyArena.LabCameraLookAt - SimpleArcAcademyArena.LabCameraPosition,
            Vector3.up);

        if (cam != null)
        {
            cam.fieldOfView = SimpleArcAcademyArena.LabCameraFieldOfView;

            // Force child local identity (if the camera component lives on a child of rig)
            if (cam.transform.parent != null && cam.transform.parent.name == "CameraRig")
            {
                cam.transform.localPosition = Vector3.zero;
                cam.transform.localRotation = Quaternion.identity;
            }
        }

        // Call orbit reset on rig (preferred) so internal yaw/pitch/distance are correct
        if (rig != null)
        {
            if (rig.TryGetComponent(out CameraOrbit orbit))
            {
                orbit.ResetToDefault();
            }
        }

        EditorUtility.SetDirty(rig != null ? rig : (cam != null ? cam.gameObject : null));
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

    private static void EnsureBobVisual(GameObject bob)
    {
        if (!bob.TryGetComponent(out Renderer renderer))
        {
            return;
        }

        var bodyMat = EnsureBobBodyMaterialAsset();
        renderer.sharedMaterial = bodyMat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        if (!bob.TryGetComponent(out BobVisualApplier applier))
        {
            applier = bob.AddComponent<BobVisualApplier>();
        }

        applier.SetBodyMaterialAsset(bodyMat);
        EditorUtility.SetDirty(bob);
    }

    private static Material EnsureBobBodyMaterialAsset()
    {
        const string path = BobVisualProfile.BodyMaterialAssetPath;
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null)
        {
            SyncBobBodyMaterial(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        var folder = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        mat = BuildBobBodyMaterial();
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
    }

    private static Material BuildBobBodyMaterial()
    {
        var mat = ArcAcademyMaterialFactory.CreateEmissive(
            BobVisualProfile.BodyOrange,
            BobVisualProfile.BodyGlowIntensity);
        SyncBobBodyMaterial(mat);
        return mat;
    }

    private static void SyncBobBodyMaterial(Material mat)
    {
        if (mat == null)
        {
            return;
        }

        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", BobVisualProfile.BodySmoothness);
        }

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", BobVisualProfile.BodyMetallic);
        }
    }

    private static void EnsureBobFace(GameObject bob)
    {
        // Sphere eyes + smile on -Z face (toward hoop); persisted via SaveBobPrefabAsset.
        RemoveLegacyEye(bob.transform, "Eye_Left");
        RemoveLegacyEye(bob.transform, "Eye_Right");

        EnsureBobEyeSphere(bob.transform, BobFaceLayout.LeftEyeName, BobFaceLayout.LeftEyeLocalPosition);
        EnsureBobEyeSphere(bob.transform, BobFaceLayout.RightEyeName, BobFaceLayout.RightEyeLocalPosition);
        EnsureHappyMouth(bob.transform);

        // Cleanup any duplicate BobSpeechBubble child GameObjects
        var speechBubbles = new List<Transform>();
        for (int i = 0; i < bob.transform.childCount; i++)
        {
            var child = bob.transform.GetChild(i);
            if (child.name == "BobSpeechBubble")
            {
                speechBubbles.Add(child);
            }
        }
        for (int i = 1; i < speechBubbles.Count; i++)
        {
            Object.DestroyImmediate(speechBubbles[i].gameObject);
        }

        // Cleanup any duplicate components
        var animators = bob.GetComponents<BobProceduralAnimator>();
        for (int i = 1; i < animators.Length; i++) Object.DestroyImmediate(animators[i]);
        if (animators.Length == 0) bob.AddComponent<BobProceduralAnimator>();

        var expressions = bob.GetComponents<BobFaceExpression>();
        for (int i = 1; i < expressions.Length; i++) Object.DestroyImmediate(expressions[i]);
        if (expressions.Length == 0) bob.AddComponent<BobFaceExpression>();

        var eyeFollows = bob.GetComponents<BobEyeFollow>();
        for (int i = 1; i < eyeFollows.Length; i++) Object.DestroyImmediate(eyeFollows[i]);
        if (eyeFollows.Length == 0) bob.AddComponent<BobEyeFollow>();

        WireBobEyeFollow(bob);
        EnsureSpeechBubble(bob);
    }

    private static void EnsureSpeechBubble(GameObject bob)
    {
        var bubbleRoot = bob.transform.Find("BobSpeechBubble");
        if (bubbleRoot == null)
        {
            bubbleRoot = new GameObject("BobSpeechBubble").transform;
            bubbleRoot.SetParent(bob.transform, false);
        }

        bubbleRoot.localPosition = new Vector3(0f, 1.15f, 0f);
        bubbleRoot.localRotation = Quaternion.identity;
        bubbleRoot.localScale = Vector3.one;

        var background = EnsureSpeechBubbleBackground(bubbleRoot);
        var textMesh = EnsureSpeechBubbleText(bubbleRoot);

        if (bubbleRoot.GetComponent<CameraFacingBillboard>() == null)
        {
            bubbleRoot.gameObject.AddComponent<CameraFacingBillboard>();
        }

        if (!bubbleRoot.TryGetComponent(out BobSpeechBubble bubble))
        {
            bubble = bubbleRoot.gameObject.AddComponent<BobSpeechBubble>();
        }

        var so = new SerializedObject(bubble);
        so.FindProperty("textMesh").objectReferenceValue = textMesh;
        so.FindProperty("background").objectReferenceValue = background;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(bubbleRoot.gameObject);
    }

    private static Transform EnsureSpeechBubbleBackground(Transform bubbleRoot)
    {
        var existing = bubbleRoot.Find("BubbleBackground");
        Transform background;
        if (existing != null)
        {
            background = existing;
        }
        else
        {
            var bgGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bgGo.name = "BubbleBackground";
            bgGo.transform.SetParent(bubbleRoot, false);
            Object.DestroyImmediate(bgGo.GetComponent<Collider>());
            background = bgGo.transform;
        }

        background.localPosition = new Vector3(0f, 0f, 0.015f);
        background.localRotation = Quaternion.identity;
        background.localScale = new Vector3(2.35f, 0.68f, 1f);
        BobPhysicsLayers.SetLayerRecursively(background.gameObject, BobPhysicsLayers.DecorationLayer);

        var renderer = background.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = ArcAcademyMaterialFactory.CreateHdrpLit(
                new Color(0.97f, 0.98f, 1f, 1f),
                0.04f,
                0f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        return background;
    }

    private static TextMesh EnsureSpeechBubbleText(Transform bubbleRoot)
    {
        var existing = bubbleRoot.Find("BubbleText");
        GameObject textGo;
        if (existing != null)
        {
            textGo = existing.gameObject;
        }
        else
        {
            textGo = new GameObject("BubbleText");
            textGo.transform.SetParent(bubbleRoot, false);
        }

        textGo.transform.localPosition = Vector3.zero;
        textGo.transform.localRotation = Quaternion.identity;
        textGo.transform.localScale = Vector3.one;
        BobPhysicsLayers.SetLayerRecursively(textGo, BobPhysicsLayers.DecorationLayer);

        // Defensive: batchmode / prior bad saves can leave missing-script TextMesh refs (similar to VR placeholder).
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(textGo);
        foreach (var mb in textGo.GetComponents<MonoBehaviour>())
        {
            if (mb != null && MonoScript.FromMonoBehaviour(mb) == null)
            {
                Object.DestroyImmediate(mb);
            }
        }

        var textMesh = textGo.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = textGo.AddComponent<TextMesh>();
        }

        // Ensure a font (prefab may serialize 0/default; batch creation needs explicit to avoid set_text issues).
        if (textMesh.font == null)
        {
            textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        textMesh.text = BobVisualProfile.FormatPraise("Great job, Bob!");
        textMesh.characterSize = 0.075f;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        textMesh.richText = true;
        return textMesh;
    }

    private static void WireBobEyeFollow(GameObject bob)
    {
        if (!bob.TryGetComponent(out BobEyeFollow eyeFollow))
        {
            return;
        }

        var so = new SerializedObject(eyeFollow);
        so.FindProperty("leftEye").objectReferenceValue = bob.transform.Find(BobFaceLayout.LeftEyeName);
        so.FindProperty("rightEye").objectReferenceValue = bob.transform.Find(BobFaceLayout.RightEyeName);
        var mouthTransform = bob.transform.Find(BobFaceLayout.MouthName);
        so.FindProperty("mouth").objectReferenceValue = mouthTransform != null
            ? mouthTransform.GetComponent<LineRenderer>()
            : null;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(eyeFollow);
    }

    private static void RemoveLegacyEye(Transform bob, string legacyName)
    {
        var legacy = bob.Find(legacyName);
        if (legacy != null)
        {
            Object.DestroyImmediate(legacy.gameObject);
        }
    }

    private static void EnsureBobEyeSphere(Transform bob, string name, Vector3 localPosition)
    {
        var existing = bob.Find(name);
        Transform eyeRoot;

        if (existing != null)
        {
            eyeRoot = existing;
        }
        else
        {
            var pivot = new GameObject(name);
            pivot.transform.SetParent(bob, false);
            eyeRoot = pivot.transform;
        }

        eyeRoot.localPosition = localPosition;
        eyeRoot.localRotation = Quaternion.identity;
        eyeRoot.localScale = Vector3.one;
        BobPhysicsLayers.SetLayerRecursively(eyeRoot.gameObject, BobPhysicsLayers.DecorationLayer);

        EnsureEyePart(
            eyeRoot,
            BobFaceLayout.ScleraName,
            PrimitiveType.Sphere,
            BobFaceLayout.ScleraLocalScale,
            Vector3.zero,
            BobFaceLayout.ScleraColor);

        EnsureEyePart(
            eyeRoot,
            BobFaceLayout.PupilName,
            PrimitiveType.Sphere,
            BobFaceLayout.PupilLocalScale,
            BobFaceLayout.PupilLocalOffset,
            BobFaceLayout.PupilColor);
    }

    private static void EnsureEyePart(
        Transform eyeRoot,
        string partName,
        PrimitiveType primitiveType,
        Vector3 localScale,
        Vector3 localPosition,
        Color color)
    {
        var existing = eyeRoot.Find(partName);
        GameObject partGo;

        if (existing != null)
        {
            partGo = existing.gameObject;
        }
        else
        {
            partGo = GameObject.CreatePrimitive(primitiveType);
            partGo.name = partName;
            partGo.transform.SetParent(eyeRoot, false);

            var collider = partGo.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        partGo.transform.localPosition = localPosition;
        partGo.transform.localRotation = Quaternion.identity;
        partGo.transform.localScale = localScale;
        BobPhysicsLayers.SetLayerRecursively(partGo, BobPhysicsLayers.DecorationLayer);

        var renderer = partGo.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = ArcAcademyMaterialFactory.CreateHdrpLit(color, 0.1f, 0f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static void EnsureHappyMouth(Transform bob)
    {
        var existing = bob.Find(BobFaceLayout.MouthName);
        GameObject mouthGo;

        if (existing != null)
        {
            mouthGo = existing.gameObject;
        }
        else
        {
            mouthGo = new GameObject(BobFaceLayout.MouthName);
            mouthGo.transform.SetParent(bob, false);
        }

        mouthGo.transform.localPosition = BobFaceLayout.MouthLocalPosition;
        mouthGo.transform.localRotation = Quaternion.identity;
        mouthGo.transform.localScale = Vector3.one;
        BobPhysicsLayers.SetLayerRecursively(mouthGo, BobPhysicsLayers.DecorationLayer);

        var line = mouthGo.GetComponent<LineRenderer>();
        if (line == null)
        {
            line = mouthGo.AddComponent<LineRenderer>();
        }

        line.useWorldSpace = false;
        line.loop = false;
        line.widthMultiplier = BobFaceLayout.MouthLineWidth;
        line.positionCount = BobFaceLayout.MouthSmileLocalPoints.Length;
        for (int i = 0; i < BobFaceLayout.MouthSmileLocalPoints.Length; i++)
        {
            line.SetPosition(i, BobFaceLayout.MouthSmileLocalPoints[i]);
        }

        line.material = ArcAcademyMaterialFactory.CreateHdrpLit(BobFaceLayout.MouthColor, 0.1f, 0f);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.numCapVertices = 4;
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

        var child = FindDeepChild(parent.transform, parts[1]);
        if (child != null)
        {
            child.gameObject.SetActive(active);
        }
    }

    private static void DestroyIfFound(string path)
    {
        var parts = path.Split('/');
        Transform target = null;
        if (parts.Length == 2)
        {
            var parent = GameObject.Find(parts[0]);
            if (parent != null)
            {
                target = FindDeepChild(parent.transform, parts[1]);
            }
        }
        else
        {
            var go = GameObject.Find(path);
            if (go != null)
            {
                target = go.transform;
            }
        }

        if (target != null)
        {
            Undo.DestroyObjectImmediate(target.gameObject);
        }
    }

    private static void HideExtraDecorativeHoops()
    {
        var activeHoop = GameObject.Find(ArcAcademyLayout.HoopName);
        int hidden = 0;

        // Deactivate all PortableHoopStand that are not children of the active scoring hoop
        var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (var t in allTransforms)
        {
            if (t.name != ArcAcademyLayout.PortableHoopStandName)
                continue;

            bool isPartOfActive = activeHoop != null && t.IsChildOf(activeHoop.transform);
            if (!isPartOfActive)
            {
                if (t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(false);
                    hidden++;
                }
            }
        }

        // Also deactivate any remaining objects with DecorativeHoopMarker that aren't for the active
        var markers = Object.FindObjectsByType<DecorativeHoopMarker>(FindObjectsInactive.Include);
        foreach (var marker in markers)
        {
            bool isActiveScoring = activeHoop != null && marker.transform.IsChildOf(activeHoop.transform);
            if (!isActiveScoring && marker.gameObject.activeSelf)
            {
                marker.gameObject.SetActive(false);
                hidden++;
            }
        }

        if (hidden > 0)
        {
            Debug.Log($"[SimpleArc] Hid {hidden} background decorative hoop stands/markers for clean single-hoop court (no clutter taking room).");
        }
    }

    /// <summary>
    /// Completely remove (destroy) background decorative hoop objects from the scene
    /// so they don't take up room in the clean single-hoop training court (for BobTraining.unity lab focus).
    /// The full complex view with bays is in the _Backup scene.
    /// </summary>
    private static void StripBackgroundHoopDecorations()
    {
        // In simple lab, ensure background decorative hoops are deactivated so they don't take up room or appear on the clean court.
        // (DestroyImmediate during builder can cause access-after-destroy in some build steps; deactivate is sufficient and safe.)
        int hidden = 0;

        var stands = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        for (int i = 0; i < stands.Length; i++)
        {
            if (stands[i].name == ArcAcademyLayout.PortableHoopStandName && stands[i].gameObject.activeSelf)
            {
                stands[i].gameObject.SetActive(false);
                hidden++;
            }
        }

        var markers = Object.FindObjectsByType<DecorativeHoopMarker>(FindObjectsInactive.Include);
        for (int i = 0; i < markers.Length; i++)
        {
            if (markers[i].gameObject.activeSelf)
            {
                markers[i].gameObject.SetActive(false);
                hidden++;
            }
        }

        if (hidden > 0)
        {
            Debug.Log($"[SimpleArc] Deactivated {hidden} background decorative hoop objects for clean single-hoop court.");
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
            new Color(0.18f, 0.18f, 0.20f),
            floorTex,
            smoothness: 0.18f,
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
        ArcAcademyMaterialFactory.RefreshHoopMaterialAssets();
    }

    private static void EnsureLitMaterialWithTexture(
        string assetPath,
        string assetName,
        Color baseColor,
        Texture2D texture,
        Texture2D normalMap,
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

        if (normalMap != null && mat.HasProperty("_NormalMap"))
        {
            mat.SetTexture("_NormalMap", normalMap);
            mat.EnableKeyword("_NORMALMAP");
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

    private static void EnsureLitMaterialWithTexture(
        string assetPath,
        string assetName,
        Color baseColor,
        Texture2D texture,
        float smoothness,
        float metallic,
        Vector2 textureScale)
    {
        EnsureLitMaterialWithTexture(
            assetPath,
            assetName,
            baseColor,
            texture,
            null,
            smoothness,
            metallic,
            textureScale);
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
