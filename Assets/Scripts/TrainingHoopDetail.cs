using UnityEngine;

/// <summary>
/// Shared hoop upgrade: regulation-style rim colliders, backboard detail, and visual-only net.
/// Used by scene builder, arena builder, and play-mode fix for stable basketball physics.
/// </summary>
public static class TrainingHoopDetail
{
    public const int RimSegmentCount = 12;
    public const int NetStrandCount = 12;
    public const float RimOuterRadius = 0.43f;
    public const float RimTubeRadius = 0.018f;

    private static readonly Color TargetRed = new(0.82f, 0.18f, 0.12f);
    private static readonly Color FrameDark = new(0.12f, 0.12f, 0.14f);
    private static readonly Color NetWhite = new(0.92f, 0.93f, 0.95f);

    private static readonly string[] DisableUnderHoop =
    {
        ArcAcademyLayout.PortableHoopStandName,
        "RoboticSwivelBase",
        "RoboticLauncher",
    };

    public static void UpgradeActiveHoop()
    {
        var hoopRoot = GameObject.Find(ArcAcademyLayout.HoopName);
        if (hoopRoot == null)
        {
            return;
        }

        UpgradeHoop(hoopRoot.transform);
    }

    public static void UpgradeHoop(Transform hoopRoot)
    {
        FreezeStationaryAssembly(hoopRoot);

        var rim = FindRim(hoopRoot);
        if (rim == null)
        {
            return;
        }

        AttachRimToBackboard(rim);
        ConfigureRimColliders(rim.gameObject);
        EnsureBackboardDetail(rim);
        EnsureVisualNet(rim);
    }

    /// <summary>
    /// Reparents HoopHead to the hoop root, disables the robotic arm, and freezes motion for training.
    /// </summary>
    public static void FreezeStationaryAssembly(Transform hoopRoot)
    {
        if (hoopRoot == null)
        {
            return;
        }

        DisableIdleAnimators(hoopRoot);

        var hoopHead = FindDeepChild(hoopRoot, "HoopHead");
        if (hoopHead == null)
        {
            return;
        }

        hoopHead.SetParent(hoopRoot, false);
        hoopHead.localPosition = ArcAcademyLayout.StationaryHoopHeadLocalPosition;
        hoopHead.localRotation = Quaternion.identity;

        foreach (var childName in DisableUnderHoop)
        {
            var child = hoopRoot.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        RemoveStrayDetailOnArm(hoopRoot);

        if (hoopRoot.TryGetComponent(out MovableHoop movableHoop))
        {
            movableHoop.SetStationaryForTraining(true);
            movableHoop.ApplyDefaultPose();
        }
    }

    public static Transform FindRim(Transform hoopRoot)
    {
        var rim = hoopRoot.Find($"{ArcAcademyLayout.RimName}");
        if (rim != null)
        {
            return rim;
        }

        rim = hoopRoot.Find($"HoopHead/{ArcAcademyLayout.RimName}");
        if (rim != null)
        {
            return rim;
        }

        return FindDeepChild(hoopRoot, ArcAcademyLayout.RimName);
    }

    public static void AttachRimToBackboard(Transform rim)
    {
        if (rim == null)
        {
            return;
        }

        var hoopHead = rim.parent;
        if (hoopHead == null || hoopHead.name != "HoopHead")
        {
            hoopHead = rim;
            while (hoopHead != null && hoopHead.name != "HoopHead")
            {
                hoopHead = hoopHead.parent;
            }

            if (hoopHead != null)
            {
                rim.SetParent(hoopHead, false);
            }
        }

        var backboard = hoopHead != null ? hoopHead.Find("Backboard") : null;
        if (backboard != null)
        {
            backboard.localPosition = ArcAcademyLayout.BackboardLocalOnHoopHead;
        }

        rim.localPosition = ArcAcademyLayout.RimLocalOnHoopHead;
        rim.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.localScale = new Vector3(0.9f, 0.04f, 0.9f);
    }

    public static void ConfigureRimColliders(GameObject rimGo)
    {
        foreach (var col in rimGo.GetComponents<CapsuleCollider>())
        {
            Object.Destroy(col);
        }

        foreach (var col in rimGo.GetComponents<MeshCollider>())
        {
            Object.Destroy(col);
        }

        var rb = rimGo.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = rimGo.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;

        if (!rimGo.TryGetComponent(out HoopRimContact _))
        {
            rimGo.AddComponent<HoopRimContact>();
        }

        var collidersRoot = rimGo.transform.Find("RimColliders");
        if (collidersRoot == null)
        {
            var root = new GameObject("RimColliders");
            root.transform.SetParent(rimGo.transform, false);
            collidersRoot = root.transform;
        }

        ClearChildren(collidersRoot);

        float segmentArc = (Mathf.PI * 2f * RimOuterRadius) / RimSegmentCount;
        for (int i = 0; i < RimSegmentCount; i++)
        {
            float angle = i / (float)RimSegmentCount * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            var seg = new GameObject($"RimSeg_{i}");
            seg.transform.SetParent(collidersRoot, false);
            seg.transform.localPosition = new Vector3(cos * RimOuterRadius, 0f, sin * RimOuterRadius);
            seg.transform.localRotation = Quaternion.Euler(90f, angle * Mathf.Rad2Deg + 90f, 0f);

            var cap = seg.AddComponent<CapsuleCollider>();
            cap.direction = 2;
            cap.radius = RimTubeRadius;
            cap.height = segmentArc * 1.05f;
            cap.material = HoopPhysicsMaterials.Rim;
        }
    }

    private static void EnsureBackboardDetail(Transform rim)
    {
        var hoopHead = rim.parent;
        if (hoopHead == null)
        {
            return;
        }

        var backboard = hoopHead.Find("Backboard");
        if (backboard == null)
        {
            return;
        }

        EnsureDetailCube(
            backboard,
            "BackboardFrame_Top",
            new Vector3(0f, 0.54f, 0.04f),
            new Vector3(1.82f, 0.06f, 0.02f),
            FrameDark);
        EnsureDetailCube(
            backboard,
            "BackboardFrame_Bottom",
            new Vector3(0f, -0.54f, 0.04f),
            new Vector3(1.82f, 0.06f, 0.02f),
            FrameDark);
        EnsureDetailCube(
            backboard,
            "BackboardFrame_Left",
            new Vector3(-0.9f, 0f, 0.04f),
            new Vector3(0.06f, 1.1f, 0.02f),
            FrameDark);
        EnsureDetailCube(
            backboard,
            "BackboardFrame_Right",
            new Vector3(0.9f, 0f, 0.04f),
            new Vector3(0.06f, 1.1f, 0.02f),
            FrameDark);

        EnsureDetailCube(
            backboard,
            "TargetSquare_Outer",
            new Vector3(0f, -0.08f, 0.045f),
            new Vector3(0.62f, 0.48f, 0.012f),
            TargetRed,
            alpha: 0.95f);
        EnsureDetailCube(
            backboard,
            "TargetSquare_Inner",
            new Vector3(0f, -0.08f, 0.048f),
            new Vector3(0.52f, 0.38f, 0.008f),
            Color.white,
            alpha: 0.35f);

        EnsureDetailCube(
            rim,
            "RimBracket_L",
            new Vector3(-0.12f, 0.04f, -0.14f),
            new Vector3(0.05f, 0.05f, 0.16f),
            FrameDark);
        EnsureDetailCube(
            rim,
            "RimBracket_R",
            new Vector3(0.12f, 0.04f, -0.14f),
            new Vector3(0.05f, 0.05f, 0.16f),
            FrameDark);
    }

    private static void EnsureVisualNet(Transform rim)
    {
        var netRoot = rim.Find("Net");
        if (netRoot == null)
        {
            var go = new GameObject("Net");
            go.transform.SetParent(rim, false);
            go.transform.localPosition = new Vector3(0f, -0.06f, 0f);
            netRoot = go.transform;
        }

        netRoot.localPosition = new Vector3(0f, -0.06f, 0f);

        if (!netRoot.TryGetComponent(out HoopSwishVfx _))
        {
            netRoot.gameObject.AddComponent<HoopSwishVfx>();
        }

        var netPhysics = netRoot.GetComponent<HoopNetPhysics>();
        if (netPhysics != null)
        {
            Object.Destroy(netPhysics);
        }

        StripNetPhysicsColliders(netRoot);
        ClearChildren(netRoot);

        for (int i = 0; i < NetStrandCount; i++)
        {
            float angle = i / (float)NetStrandCount * Mathf.PI * 2f;
            var strand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strand.name = $"NetStrand_{i}";
            strand.transform.SetParent(netRoot, false);
            strand.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.32f,
                -0.22f,
                Mathf.Sin(angle) * 0.32f);
            strand.transform.localScale = new Vector3(0.012f, 0.22f, 0.012f);
            ApplyNetMaterial(strand.GetComponent<Renderer>());
            Object.Destroy(strand.GetComponent<Collider>());
        }

        EnsureNetRing(netRoot, "NetRing_Upper", -0.12f, 0.52f);
        EnsureNetRing(netRoot, "NetRing_Lower", -0.28f, 0.34f);
    }

    private static void EnsureNetRing(Transform netRoot, string name, float localY, float radius)
    {
        var ring = netRoot.Find(name);
        if (ring == null)
        {
            var torus = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torus.name = name;
            torus.transform.SetParent(netRoot, false);
            Object.Destroy(torus.GetComponent<Collider>());
            ring = torus.transform;
        }

        ring.localPosition = new Vector3(0f, localY, 0f);
        ring.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ring.localScale = new Vector3(radius, 0.004f, radius);
        ApplyNetMaterial(ring.GetComponent<Renderer>());
    }

    private static void DisableIdleAnimators(Transform hoopRoot)
    {
        foreach (var launcher in hoopRoot.GetComponentsInChildren<RoboticLauncherVisual>(true))
        {
            launcher.enabled = false;
        }
    }

    private static void RemoveStrayDetailOnArm(Transform hoopRoot)
    {
        var stray = FindDeepChild(hoopRoot, "PolePadding");
        if (stray != null)
        {
            Object.Destroy(stray.gameObject);
        }
    }

    private static void StripNetPhysicsColliders(Transform netRoot)
    {
        foreach (var col in netRoot.GetComponentsInChildren<Collider>(true))
        {
            Object.Destroy(col);
        }

        foreach (var rb in netRoot.GetComponentsInChildren<Rigidbody>(true))
        {
            Object.Destroy(rb);
        }

        foreach (var joint in netRoot.GetComponentsInChildren<Joint>(true))
        {
            Object.Destroy(joint);
        }
    }

    private static void EnsureDetailCube(
        Transform parent,
        string name,
        Vector3 localPos,
        Vector3 localScale,
        Color color,
        float alpha = 1f)
    {
        var child = parent.Find(name);
        if (child == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            Object.Destroy(cube.GetComponent<Collider>());
            child = cube.transform;
        }

        child.localPosition = localPos;
        child.localScale = localScale;
        child.gameObject.SetActive(true);

        var displayColor = color;
        displayColor.a = alpha;
        ApplyDetailMaterial(child.GetComponent<Renderer>(), displayColor);
    }

    private static void ApplyDetailMaterial(Renderer renderer, Color color)
    {
        if (renderer == null)
        {
            return;
        }

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
            mat.SetFloat("_Smoothness", 0.25f);
        }

        renderer.sharedMaterial = mat;
    }

    private static void ApplyNetMaterial(Renderer renderer)
    {
        ApplyDetailMaterial(renderer, NetWhite);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
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
}
