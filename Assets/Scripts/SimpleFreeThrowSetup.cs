using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// One-click minimal free-throw trainer setup for BobTraining.unity.
///
/// Press Play after setup — Bob (launcher) receives PPO actions and launches the basketball.
/// Behavior name Bob, 8 observations, 3 continuous actions (unchanged YAML contract).
///
/// Editor test steps:
/// 1. TrainingArena → SimpleFreeThrowSetup → "Setup Minimal Trainer" (or Bob → Setup → Simple Free Throw Trainer)
/// 2. Press Play → check Game tab for orange court + visible hoop + ball launches
/// 3. ./scripts/train.sh → wait for port 5004 → Play → scoreboard shows Training (PPO)
/// </summary>
public class SimpleFreeThrowSetup : MonoBehaviour
{
    public const string SetupObjectName = "SimpleFreeThrowSetup";
    public const string CourtName = "Court";
    public const string KeyMarkingsName = "KeyMarkings";
    public const string BasketballName = BasketballProjectileSetup.BasketballName;
    public const string SimpleHoopName = "SimpleHoop";

    private static readonly string[] DisableArenaRoots =
    {
        ArcAcademyLayout.WarehouseShellName,
        ArcAcademyLayout.TrainingBaysName,
        ArcAcademyLayout.MountainWindowName,
        "BackMountainPanorama",
        ArcAcademyLayout.TrajectoryVisualsName,
        ArcAcademyLayout.FloorDecalsName,
        ArcAcademyLayout.DistanceMarkingsName,
        ArcAcademyLayout.AdaptiveProbeVolumeName,
        ArcAcademyLayout.ReflectionProbeWindowName,
        ArcAcademyLayout.LightingRigName,
        ArcAcademyLayout.ReflectionProbeName,
    };

    private static readonly string[] DisableSpawnPadChildren =
    {
        "EdgeGlow_Front", "EdgeGlow_Back", "EdgeGlow_Left", "EdgeGlow_Right",
        "SpawnPadGlow", "PlatformBaseRing", ArcAcademyLayout.SpawnPadBrandingName,
        "SpawnPadLight", "SpawnPadParticles",
    };

    private static readonly string[] DisableHoopVisuals =
    {
        ArcAcademyLayout.PortableHoopStandName,
        "RoboticSwivelBase",
    };

    [SerializeField] private bool setupOnPlay;

    public bool IsConfigured { get; private set; }

    private void Awake()
    {
        if (setupOnPlay && !IsConfigured)
        {
            ApplyAll();
        }
    }

    public static SimpleFreeThrowSetup EnsureOnArena(Transform arena)
    {
        var existing = arena.Find(SetupObjectName);
        if (existing != null && existing.TryGetComponent(out SimpleFreeThrowSetup setup))
        {
            return setup;
        }

        var go = new GameObject(SetupObjectName);
        go.transform.SetParent(arena, false);
        return go.AddComponent<SimpleFreeThrowSetup>();
    }

    /// <summary>Idempotent full setup — safe to run multiple times.</summary>
    [ContextMenu("Setup Minimal Trainer")]
    public void ApplyAll()
    {
        var arena = GameObject.Find(ArcAcademyLayout.ArenaName)?.transform;
        if (arena == null)
        {
            Debug.LogError("SIMPLE_FREE_THROW_FAIL: TrainingArena not found.");
            return;
        }

        int disabled = Step1_DisableDecorations(arena);
        int lights = Step3_ApplyLightingAndVolume();
        int built = Step2_EnsureCourtHoopAndBall(arena);
        bool wired = Step4_WireBobLauncher();

        IsConfigured = true;
        Debug.Log(
            $"✅ SIMPLE FREE THROW SETUP COMPLETE — disabled {disabled} roots, " +
            $"configured {lights} sun, built/verified {built} primitives, bob wired={wired}.");
    }

    private static int Step1_DisableDecorations(Transform arena)
    {
        int count = 0;

        foreach (var rootName in DisableArenaRoots)
        {
            count += SetActiveIfFound(arena, rootName, false);
        }

        var spawnPad = arena.Find(ArcAcademyLayout.SpawnPadName);
        if (spawnPad != null)
        {
            foreach (var childName in DisableSpawnPadChildren)
            {
                count += SetActiveIfFound(spawnPad, childName, false);
            }
        }

        var hoop = arena.Find(ArcAcademyLayout.HoopName);
        if (hoop != null)
        {
            foreach (var visualName in DisableHoopVisuals)
            {
                count += SetActiveIfFound(hoop, visualName, false);
            }
        }

        foreach (var light in Object.FindObjectsByType<Light>())
        {
            if (light == null)
            {
                continue;
            }

            if (light.gameObject.name == "Sun")
            {
                light.shadows = LightShadows.None;
                continue;
            }

            light.shadows = LightShadows.None;
            if (SetActive(light.gameObject, false))
            {
                count++;
            }
        }

        return count;
    }

    private static int Step3_ApplyLightingAndVolume()
    {
        EnsureSun();
        ArcAcademyLabRenderPreset.ApplyMinimalTrainerVolumeInScene();
        ConfigureMainCamera();
        return 1;
    }

    private static int Step2_EnsureCourtHoopAndBall(Transform arena)
    {
        int count = 0;
        count += EnsureCourt(arena) ? 1 : 0;
        count += EnsureKeyMarkings(arena) ? 1 : 0;
        count += EnsureHoop(arena) ? 1 : 0;
        count += EnsureBasketball(arena) ? 1 : 0;
        return count;
    }

    private static bool Step4_WireBobLauncher()
    {
        var bob = GameObject.Find("Bob");
        if (bob == null || !bob.TryGetComponent(out BobAgent agent))
        {
            Debug.LogWarning("SIMPLE_FREE_THROW_WARN: Bob or BobAgent not found for wiring.");
            return false;
        }

        var rim = FindRimTransform();
        if (rim == null)
        {
            Debug.LogWarning("SIMPLE_FREE_THROW_WARN: Rim not found for BobAgent.hoop.");
            return false;
        }

        agent.hoop = rim;

        var basketball = GameObject.Find(BasketballName);
        if (basketball != null && basketball.TryGetComponent(out Rigidbody ballRb))
        {
            BasketballProjectileSetup.WireLauncher(agent, ballRb);
        }
        else
        {
            Debug.LogWarning("SIMPLE_FREE_THROW_WARN: Basketball missing — Bob will self-launch.");
        }

        return true;
    }

    private static bool EnsureCourt(Transform arena)
    {
        var court = arena.Find(CourtName);
        if (court == null)
        {
            var legacy = arena.Find(ArcAcademyLayout.CourtFloorName);
            if (legacy != null)
            {
                legacy.name = CourtName;
                court = legacy;
            }
        }

        if (court == null)
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = CourtName;
            plane.transform.SetParent(arena, false);
            plane.transform.position = new Vector3(0f, 0.01f, -2f);
            plane.transform.localScale = new Vector3(1.4f, 1f, 2.4f);
            court = plane.transform;
        }

        court.gameObject.SetActive(true);
        ApplyCourtMaterial(court.GetComponent<Renderer>());
        SetLayerRecursively(court.gameObject, BobPhysicsLayers.TrainingArenaLayer);
        return true;
    }

    private static bool EnsureKeyMarkings(Transform arena)
    {
        var court = arena.Find(CourtName);
        if (court == null)
        {
            return false;
        }

        var keyRoot = court.Find(KeyMarkingsName);
        if (keyRoot == null)
        {
            var go = new GameObject(KeyMarkingsName);
            go.transform.SetParent(court, false);
            keyRoot = go.transform;
        }

        keyRoot.gameObject.SetActive(true);
        EnsureKeyLine(keyRoot, "KeyLeft", new Vector3(-1.83f, 0.02f, -1.2f), new Vector3(0.06f, 0.01f, 3.6f));
        EnsureKeyLine(keyRoot, "KeyRight", new Vector3(1.83f, 0.02f, -1.2f), new Vector3(0.06f, 0.01f, 3.6f));
        EnsureKeyLine(keyRoot, "KeyFront", new Vector3(0f, 0.02f, -3f), new Vector3(3.66f, 0.01f, 0.06f));
        EnsureKeyLine(keyRoot, "KeyPaint", new Vector3(0f, 0.02f, -2f), new Vector3(3.5f, 0.01f, 2.8f), 0.92f);
        return true;
    }

    private static void EnsureKeyLine(Transform parent, string name, Vector3 localPos, Vector3 scale, float alpha = 1f)
    {
        var line = parent.Find(name);
        if (line == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            line = cube.transform;
            Object.Destroy(cube.GetComponent<Collider>());
        }

        line.localPosition = localPos;
        line.localScale = scale;
        line.gameObject.SetActive(true);
        ApplyLineMaterial(line.GetComponent<Renderer>(), alpha);
    }

    private static bool EnsureHoop(Transform arena)
    {
        var hoopRoot = arena.Find(ArcAcademyLayout.HoopName);
        if (hoopRoot != null)
        {
            hoopRoot.gameObject.SetActive(true);
            var rim = FindDeepChild(hoopRoot, ArcAcademyLayout.RimName);
            if (rim != null)
            {
                EnsureScoreZone(rim);
                return true;
            }
        }

        var simple = arena.Find(SimpleHoopName);
        if (simple == null)
        {
            var go = new GameObject(SimpleHoopName);
            go.transform.SetParent(arena, false);
            go.transform.position = ArcAcademyLayout.HoopRootDefaultPosition;
            simple = go.transform;
        }

        simple.gameObject.SetActive(true);

        var backboard = EnsureChildPrimitive(simple, "Backboard", PrimitiveType.Cube,
            new Vector3(0f, 3.05f, 0.05f), new Vector3(1.8f, 1.2f, 0.06f));
        ApplyWhiteMaterial(backboard.GetComponent<Renderer>());

        var rimGo = EnsureChildPrimitive(simple, ArcAcademyLayout.RimName, PrimitiveType.Cylinder,
            new Vector3(0f, 3.05f, 0.35f), new Vector3(0.9f, 0.03f, 0.9f));
        rimGo.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ApplyMetalMaterial(rimGo.GetComponent<Renderer>());

        if (!rimGo.TryGetComponent(out CapsuleCollider rimCol))
        {
            rimCol = rimGo.gameObject.AddComponent<CapsuleCollider>();
        }

        rimCol.isTrigger = false;
        rimCol.radius = 0.23f;
        rimCol.height = 0.9f;
        rimCol.direction = 2;

        if (!rimGo.TryGetComponent(out Rigidbody rimRb))
        {
            rimRb = rimGo.gameObject.AddComponent<Rigidbody>();
        }

        rimRb.isKinematic = true;
        rimRb.useGravity = false;

        if (!rimGo.TryGetComponent(out HoopRimContact _))
        {
            rimGo.gameObject.AddComponent<HoopRimContact>();
        }

        EnsureScoreZone(rimGo);
        return true;
    }

    private static void EnsureScoreZone(Transform rim)
    {
        var zone = rim.Find(ArcAcademyLayout.ScoreZoneName);
        if (zone == null)
        {
            var go = new GameObject(ArcAcademyLayout.ScoreZoneName);
            go.transform.SetParent(rim, false);
            go.transform.localPosition = Vector3.zero;
            zone = go.transform;
        }

        if (!zone.TryGetComponent(out SphereCollider sphere))
        {
            sphere = zone.gameObject.AddComponent<SphereCollider>();
        }

        sphere.isTrigger = true;
        sphere.radius = ArcAcademyLayout.RimScoreRadius;

        if (!zone.TryGetComponent(out HoopScoreZone _))
        {
            zone.gameObject.AddComponent<HoopScoreZone>();
        }
    }

    private static bool EnsureBasketball(Transform arena)
    {
        var release = BasketballProjectileSetup.GetReleasePosition(
            ArcAcademyLayout.BobSpawnPosition);
        BasketballProjectileSetup.EnsureBasketball(arena, release);
        return true;
    }

    private static void EnsureSun()
    {
        Light sun = null;

        foreach (var light in Object.FindObjectsByType<Light>())
        {
            if (light == null || light.gameObject.name != "Sun")
            {
                continue;
            }

            if (sun == null)
            {
                sun = light;
            }
            else
            {
                light.shadows = LightShadows.None;
                light.enabled = false;
            }
        }

        if (sun == null)
        {
            var rig = GameObject.Find(ArcAcademyLayout.HdrpSkyRigName)?.transform;
            var parent = rig != null ? rig : GameObject.Find(ArcAcademyLayout.ArenaName)?.transform;
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            if (!sun.TryGetComponent(out HDAdditionalLightData _))
            {
                sunGo.AddComponent<HDAdditionalLightData>();
            }
        }

        sun.gameObject.SetActive(true);
        sun.enabled = true;
        sun.type = LightType.Directional;
        sun.lightUnit = LightUnit.Lux;
        sun.intensity = 6000f;
        sun.shadows = LightShadows.Soft;
        sun.color = new Color(1f, 0.98f, 0.94f);
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        if (sun.TryGetComponent(out HDAdditionalLightData hd))
        {
            hd.SetLightDimmer(1f, 0f);
            hd.UpdateAllLightValues();
        }

        ArcAcademyLabRenderPreset.EnforceSingleDirectionalShadow();
    }

    private static void ConfigureMainCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.transform.position = ArcAcademyLayout.CameraPosition;
        cam.transform.rotation = Quaternion.LookRotation(
            ArcAcademyLayout.CameraLookAt - ArcAcademyLayout.CameraPosition,
            Vector3.up);
        cam.fieldOfView = ArcAcademyLayout.CameraFieldOfView;

        if (!cam.TryGetComponent(out HDAdditionalCameraData hd))
        {
            hd = cam.gameObject.AddComponent<HDAdditionalCameraData>();
        }

        hd.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
    }

    private static Transform FindRimTransform()
    {
        var hoop = GameObject.Find(ArcAcademyLayout.HoopName);
        if (hoop != null)
        {
            var rim = FindDeepChild(hoop.transform, ArcAcademyLayout.RimName);
            if (rim != null)
            {
                return rim;
            }
        }

        var simple = GameObject.Find(SimpleHoopName);
        if (simple != null)
        {
            var rim = simple.transform.Find(ArcAcademyLayout.RimName);
            if (rim != null)
            {
                return rim;
            }
        }

        return null;
    }

    private static Transform EnsureChildPrimitive(
        Transform parent,
        string name,
        PrimitiveType primitive,
        Vector3 localPos,
        Vector3 localScale)
    {
        var child = parent.Find(name);
        if (child == null)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.SetParent(parent, false);
            child = go.transform;
        }

        child.localPosition = localPos;
        child.localScale = localScale;
        child.gameObject.SetActive(true);
        return child;
    }

    private static int SetActiveIfFound(Transform parent, string childName, bool active)
    {
        var child = parent.Find(childName);
        if (child == null)
        {
            return 0;
        }

        return SetActive(child.gameObject, active) ? 1 : 0;
    }

    private static bool SetActive(GameObject go, bool active)
    {
        if (go.activeSelf == active)
        {
            return false;
        }

        go.SetActive(active);
        return true;
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

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        if (layer < 0)
        {
            return;
        }

        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private static void ApplyCourtMaterial(Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = CreateLitMaterial(new Color(0.96f, 0.42f, 0.08f), 0.35f, 0f);
    }

    private static void ApplyLineMaterial(Renderer renderer, float alpha)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = CreateLitMaterial(new Color(1f, 1f, 1f, alpha), 0.1f, 0f);
    }

    private static void ApplyWhiteMaterial(Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = CreateLitMaterial(Color.white, 0.2f, 0f);
    }

    private static void ApplyMetalMaterial(Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = CreateLitMaterial(new Color(0.75f, 0.75f, 0.78f), 0.7f, 0.85f);
    }

    private static Material CreateLitMaterial(Color color, float smoothness, float metallic)
    {
        var shader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        else
        {
            mat.color = color;
        }

        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", smoothness);
        }

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }

        return mat;
    }
}
