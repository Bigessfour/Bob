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
    private const string WallMatPath = MaterialsFolder + "/Mat_Wall_Blue.mat";
    private const string TargetRedMatPath = MaterialsFolder + "/Mat_Target_Red.mat";
    private const string TargetYellowMatPath = MaterialsFolder + "/Mat_Target_Yellow.mat";
    private const string TargetGreenMatPath = MaterialsFolder + "/Mat_Target_Green.mat";

    [MenuItem("Bob/Setup/Simple Arc Academy Arena")]
    [MenuItem("Tools/Bob/Setup Simple Arc Academy Arena")]
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
        SavePrefabFromInstance(arenaRoot);

        HideLegacyCourtVisuals();
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
        SetActiveIfFound($"{ArcAcademyLayout.ArenaName}/{ArcAcademyLayout.CourtFloorName}", false);
        SetActiveIfFound($"{ArcAcademyLayout.ArenaName}/Boundaries", false);

        var spawnPad = GameObject.Find($"{ArcAcademyLayout.ArenaName}/{ArcAcademyLayout.SpawnPadName}");
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

        EnsureLitMaterial(
            FloorMatPath,
            "Mat_Floor_Grid",
            SimpleArcAcademyArena.FloorColor,
            smoothness: 0.35f,
            metallic: 0f);

        EnsureLitMaterial(
            WallMatPath,
            "Mat_Wall_Blue",
            SimpleArcAcademyArena.WallColor,
            smoothness: 0.2f,
            metallic: 0f);

        EnsureUnlitMaterial(TargetRedMatPath, "Mat_Target_Red", SimpleArcAcademyArena.TargetRed);
        EnsureUnlitMaterial(TargetYellowMatPath, "Mat_Target_Yellow", SimpleArcAcademyArena.TargetYellow);
        EnsureUnlitMaterial(TargetGreenMatPath, "Mat_Target_Green", SimpleArcAcademyArena.TargetGreen);
    }

    private static void EnsureLitMaterial(
        string assetPath,
        string assetName,
        Color baseColor,
        float smoothness,
        float metallic)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (existing != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateHdrpLit(baseColor, smoothness, metallic);
        mat.name = assetName;
        AssetDatabase.CreateAsset(mat, assetPath);
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
        return new ArenaMaterials
        {
            Floor = AssetDatabase.LoadAssetAtPath<Material>(FloorMatPath),
            Wall = AssetDatabase.LoadAssetAtPath<Material>(WallMatPath),
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
