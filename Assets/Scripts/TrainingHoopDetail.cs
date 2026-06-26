using UnityEngine;

/// <summary>
/// Shared hoop upgrade: regulation-style rim colliders, backboard detail, and visual-only net.
/// Used by scene builder, arena builder, and play-mode fix for stable basketball physics.
/// </summary>
public static class TrainingHoopDetail
{
    public const int RimSegmentCount = 12;
    public const int NetStrandCount = 18;
    public const float RimOuterRadius = 0.43f;
    public const float RimTubeRadius = 0.018f;

    // Regulation 42×72 backboard proportions (local units on 1.8 × 1.05 glass panel).
    private const float GlassHalfWidth = 0.9f;
    private const float GlassHalfHeight = 0.525f;
    private const float GlassFaceZ = 0.045f;
    private const float FrameDepth = 0.035f;
    private const float MarkingLine = 0.022f;
    private const float MarkingInset = 0.07f;

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
        EnsureRimMaterial(rim);
        EnsureBackboardDetail(rim);
        EnsureVisualNet(rim);
        EnsureScoreTrigger(rim);
        EnsureHoopRenderersEnabled(rim);
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
            if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
        }

        foreach (var col in rimGo.GetComponents<MeshCollider>())
        {
            if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
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
        EnsureGymProBackboard(rim);
        EnsureBreakawayRimAssembly(rim);
        EnsureRimNetPigtails(rim);
    }

    private static void EnsureGymProBackboard(Transform rim)
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

        if (backboard.TryGetComponent(out Renderer backboardRenderer))
        {
            backboardRenderer.sharedMaterial = HoopVisualMaterials.CreateGymProGlassBackboard();
            backboardRenderer.enabled = true;
        }

        RemoveLegacyDetail(backboard, "TargetSquare_Outer");
        RemoveLegacyDetail(backboard, "TargetSquare_Inner");
        RemoveLegacyDetail(backboard, "BackboardPad_Bottom");

        EnsureAluminumFrame(backboard);
        EnsureRegulationGlassMarkings(backboard);
        EnsureSteelRimSupport(backboard);
        EnsureTuffGuardPadding(backboard);
    }

    private static void EnsureAluminumFrame(Transform backboard)
    {
        var frameMat = HoopVisualMaterials.CreateFrameAluminum();
        float frameZ = 0.02f;
        float frameThickness = 0.07f;

        EnsureDetailMesh(
            backboard,
            "BackboardFrame_Top",
            new Vector3(0f, GlassHalfHeight + frameThickness * 0.5f - 0.01f, frameZ),
            new Vector3(1.88f, frameThickness, FrameDepth),
            frameMat);
        EnsureDetailMesh(
            backboard,
            "BackboardFrame_Bottom",
            new Vector3(0f, -GlassHalfHeight - frameThickness * 0.5f + 0.01f, frameZ),
            new Vector3(1.88f, frameThickness, FrameDepth),
            frameMat);
        EnsureDetailMesh(
            backboard,
            "BackboardFrame_Left",
            new Vector3(-GlassHalfWidth - frameThickness * 0.5f + 0.01f, 0f, frameZ),
            new Vector3(frameThickness, 1.16f, FrameDepth),
            frameMat);
        EnsureDetailMesh(
            backboard,
            "BackboardFrame_Right",
            new Vector3(GlassHalfWidth + frameThickness * 0.5f - 0.01f, 0f, frameZ),
            new Vector3(frameThickness, 1.16f, FrameDepth),
            frameMat);
    }

    private static void EnsureRegulationGlassMarkings(Transform backboard)
    {
        var markingMat = HoopVisualMaterials.CreateRegulationMarking();
        float innerHalfW = GlassHalfWidth - MarkingInset;
        float innerHalfH = GlassHalfHeight - MarkingInset;

        EnsureOutlineRect(
            backboard,
            "GlassBorder",
            new Vector3(0f, 0f, GlassFaceZ),
            new Vector2(innerHalfW * 2f, innerHalfH * 2f),
            MarkingLine,
            markingMat);

        // 24×18" shooter's square on 72×42" board — centered above rim mount.
        EnsureOutlineRect(
            backboard,
            "TargetSquare",
            new Vector3(0f, -0.08f, GlassFaceZ + 0.002f),
            new Vector2(0.58f, 0.44f),
            MarkingLine,
            markingMat);
    }

    private static void EnsureSteelRimSupport(Transform backboard)
    {
        var steelMat = HoopVisualMaterials.CreateSteelSupport();
        EnsureDetailMesh(
            backboard,
            "RimSupportBar",
            new Vector3(0f, -0.02f, -0.015f),
            new Vector3(0.42f, 0.06f, 0.04f),
            steelMat);
        EnsureDetailMesh(
            backboard,
            "RimSupportPlate",
            new Vector3(0f, -0.02f, -0.04f),
            new Vector3(0.22f, 0.14f, 0.025f),
            steelMat);
    }

    private static void EnsureTuffGuardPadding(Transform backboard)
    {
        var padMat = HoopVisualMaterials.CreatePadVinyl();
        float padZ = 0.055f;
        float padDepth = 0.045f;

        // Bottom segment split for molded goal relief (rim clearance notch).
        EnsureDetailMesh(
            backboard,
            "BackboardPad_Bottom_L",
            new Vector3(-0.46f, -0.5f, padZ),
            new Vector3(0.72f, 0.13f, padDepth),
            padMat);
        EnsureDetailMesh(
            backboard,
            "BackboardPad_Bottom_R",
            new Vector3(0.46f, -0.5f, padZ),
            new Vector3(0.72f, 0.13f, padDepth),
            padMat);

        // Lower side wraps (PMCE / TuffGuard corner coverage).
        EnsureDetailMesh(
            backboard,
            "BackboardPad_Side_L",
            new Vector3(-0.88f, -0.28f, padZ),
            new Vector3(0.1f, 0.42f, padDepth),
            padMat);
        EnsureDetailMesh(
            backboard,
            "BackboardPad_Side_R",
            new Vector3(0.88f, -0.28f, padZ),
            new Vector3(0.1f, 0.42f, padDepth),
            padMat);
    }

    private static void EnsureBreakawayRimAssembly(Transform rim)
    {
        var rimMat = HoopVisualMaterials.CreateRimOrange();
        EnsureDetailMesh(
            rim,
            "RimBackplate",
            new Vector3(0f, 0.02f, -0.13f),
            new Vector3(0.32f, 0.18f, 0.16f),
            rimMat);
        EnsureDetailMesh(
            rim,
            "RimSpringCover",
            new Vector3(0f, -0.02f, -0.11f),
            new Vector3(0.24f, 0.08f, 0.1f),
            rimMat);
        EnsureDetailMesh(
            rim,
            "RimBracket_L",
            new Vector3(-0.13f, 0.04f, -0.15f),
            new Vector3(0.055f, 0.06f, 0.18f),
            rimMat);
        EnsureDetailMesh(
            rim,
            "RimBracket_R",
            new Vector3(0.13f, 0.04f, -0.15f),
            new Vector3(0.055f, 0.06f, 0.18f),
            rimMat);
    }

    private static void EnsureRimNetPigtails(Transform rim)
    {
        var pigtailMat = HoopVisualMaterials.CreateRimPigtail();
        const int pigtailCount = 12;
        const float pigtailRadius = 0.36f;

        for (int i = 0; i < pigtailCount; i++)
        {
            float angle = i / (float)pigtailCount * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            EnsureDetailMesh(
                rim,
                $"RimPigtail_{i}",
                new Vector3(cos * pigtailRadius, -0.03f, sin * pigtailRadius),
                new Vector3(0.018f, 0.018f, 0.018f),
                pigtailMat);
        }
    }

    private static void EnsureRimMaterial(Transform rim)
    {
        if (rim == null)
        {
            return;
        }

        var rimMaterial = HoopVisualMaterials.CreateRimOrange();
        if (rim.TryGetComponent(out Renderer rimRenderer))
        {
            rimRenderer.sharedMaterial = rimMaterial;
            rimRenderer.enabled = true;
        }
    }

    public static void EnsureScoreTrigger(Transform rim)
    {
        if (rim == null)
        {
            return;
        }

        var trigger = rim.Find(ArcAcademyLayout.HoopSuccessName);
        if (trigger == null)
        {
            var legacy = rim.Find("ScoreZone");
            if (legacy != null)
            {
                legacy.name = ArcAcademyLayout.HoopSuccessName;
                trigger = legacy;
            }
        }

        if (trigger == null)
        {
            var go = new GameObject(ArcAcademyLayout.HoopSuccessName);
            go.transform.SetParent(rim, false);
            trigger = go.transform;
        }

        trigger.localPosition = Vector3.zero;
        trigger.localRotation = Quaternion.identity;
        trigger.localScale = Vector3.one;

        foreach (var meshFilter in trigger.GetComponents<MeshFilter>())
        {
            if (Application.isPlaying) Object.Destroy(meshFilter); else Object.DestroyImmediate(meshFilter);
        }

        foreach (var meshRenderer in trigger.GetComponents<MeshRenderer>())
        {
            meshRenderer.enabled = false;
            if (Application.isPlaying) Object.Destroy(meshRenderer); else Object.DestroyImmediate(meshRenderer);
        }

        foreach (var sphere in trigger.GetComponents<SphereCollider>())
        {
            if (Application.isPlaying) Object.Destroy(sphere); else Object.DestroyImmediate(sphere);
        }

        if (!trigger.TryGetComponent(out CapsuleCollider capsule))
        {
            capsule = trigger.gameObject.AddComponent<CapsuleCollider>();
        }

        capsule.isTrigger = true;
        capsule.direction = 1;
        capsule.radius = ArcAcademyLayout.RimScoreRadius;
        capsule.height = ArcAcademyLayout.RimScoreHeight;
        capsule.center = Vector3.zero;

        if (!trigger.TryGetComponent(out HoopScoreZone _))
        {
            trigger.gameObject.AddComponent<HoopScoreZone>();
        }

        try
        {
            trigger.gameObject.tag = ArcAcademyLayout.HoopSuccessTag;
        }
        catch (UnityException)
        {
            Debug.LogWarning("HOOP_WARN: HoopSuccess tag missing from Tag Manager — add it in Project Settings.");
        }
    }

    private static void EnsureHoopRenderersEnabled(Transform rim)
    {
        if (rim == null)
        {
            return;
        }

        foreach (var renderer in rim.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.transform.name == ArcAcademyLayout.HoopSuccessName)
            {
                renderer.enabled = false;
                continue;
            }

            var collidersRoot = rim.Find("RimColliders");
            if (collidersRoot != null && renderer.transform.IsChildOf(collidersRoot))
            {
                continue;
            }

            renderer.enabled = true;
        }

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

        foreach (var renderer in backboard.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = true;
        }
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
            if (Application.isPlaying) Object.Destroy(netPhysics); else Object.DestroyImmediate(netPhysics);
        }

        StripNetPhysicsColliders(netRoot);
        ClearChildren(netRoot);

        for (int i = 0; i < NetStrandCount; i++)
        {
            float angle = i / (float)NetStrandCount * Mathf.PI * 2f;
            float topRadius = 0.36f;
            float bottomRadius = 0.18f;
            float midRadius = (topRadius + bottomRadius) * 0.5f;
            var strand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strand.name = $"NetStrand_{i}";
            strand.transform.SetParent(netRoot, false);
            strand.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * midRadius,
                -0.24f,
                Mathf.Sin(angle) * midRadius);
            strand.transform.localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
            strand.transform.localScale = new Vector3(0.011f, 0.24f, 0.011f);
            ApplyNetMaterial(strand.GetComponent<Renderer>());
            var strandCollider = strand.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(strandCollider); else Object.DestroyImmediate(strandCollider);
        }

        EnsureNetRing(netRoot, "NetRing_Upper", -0.1f, 0.68f);
        EnsureNetRing(netRoot, "NetRing_MidUpper", -0.16f, 0.58f);
        EnsureNetRing(netRoot, "NetRing_Mid", -0.22f, 0.48f);
        EnsureNetRing(netRoot, "NetRing_MidLower", -0.28f, 0.38f);
        EnsureNetRing(netRoot, "NetRing_Lower", -0.34f, 0.28f);
    }

    private static void EnsureNetRing(Transform netRoot, string name, float localY, float radius)
    {
        var ring = netRoot.Find(name);
        if (ring == null)
        {
            var torus = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torus.name = name;
            torus.transform.SetParent(netRoot, false);
            var torusCollider = torus.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(torusCollider); else Object.DestroyImmediate(torusCollider);
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
            if (Application.isPlaying) Object.Destroy(stray.gameObject); else Object.DestroyImmediate(stray.gameObject);
        }
    }

    private static void StripNetPhysicsColliders(Transform netRoot)
    {
        foreach (var col in netRoot.GetComponentsInChildren<Collider>(true))
        {
            if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
        }

        foreach (var rb in netRoot.GetComponentsInChildren<Rigidbody>(true))
        {
            if (Application.isPlaying) Object.Destroy(rb); else Object.DestroyImmediate(rb);
        }

        foreach (var joint in netRoot.GetComponentsInChildren<Joint>(true))
        {
            if (Application.isPlaying) Object.Destroy(joint); else Object.DestroyImmediate(joint);
        }
    }

    private static void RemoveLegacyDetail(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child == null)
        {
            return;
        }

        if (Application.isPlaying) Object.Destroy(child.gameObject); else Object.DestroyImmediate(child.gameObject);
    }

    private static void EnsureOutlineRect(
        Transform parent,
        string prefix,
        Vector3 center,
        Vector2 size,
        float lineWidth,
        Material material)
    {
        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;
        float depth = 0.012f;

        EnsureDetailMesh(
            parent,
            $"{prefix}_Top",
            center + new Vector3(0f, halfH - lineWidth * 0.5f, 0f),
            new Vector3(size.x, lineWidth, depth),
            material);
        EnsureDetailMesh(
            parent,
            $"{prefix}_Bottom",
            center + new Vector3(0f, -halfH + lineWidth * 0.5f, 0f),
            new Vector3(size.x, lineWidth, depth),
            material);
        EnsureDetailMesh(
            parent,
            $"{prefix}_Left",
            center + new Vector3(-halfW + lineWidth * 0.5f, 0f, 0f),
            new Vector3(lineWidth, size.y, depth),
            material);
        EnsureDetailMesh(
            parent,
            $"{prefix}_Right",
            center + new Vector3(halfW - lineWidth * 0.5f, 0f, 0f),
            new Vector3(lineWidth, size.y, depth),
            material);
    }

    private static void EnsureOutlineRect(
        Transform parent,
        string prefix,
        Vector3 center,
        Vector2 size,
        float lineWidth,
        Color color)
    {
        EnsureOutlineRect(parent, prefix, center, size, lineWidth, HoopVisualMaterials.CreateRegulationMarking());
    }

    private static void EnsureDetailMesh(
        Transform parent,
        string name,
        Vector3 localPos,
        Vector3 localScale,
        Material material)
    {
        var child = parent.Find(name);
        if (child == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            var collider = cube.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            child = cube.transform;
        }

        child.localPosition = localPos;
        child.localScale = localScale;
        child.gameObject.SetActive(true);

        if (child.TryGetComponent(out Renderer renderer) && material != null)
        {
            renderer.sharedMaterial = material;
            renderer.enabled = true;
        }
    }

    private static void EnsureDetailCube(
        Transform parent,
        string name,
        Vector3 localPos,
        Vector3 localScale,
        Color color,
        float alpha = 1f,
        float smoothness = 0.25f,
        float metallic = 0f)
    {
        var child = parent.Find(name);
        if (child == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            var collider = cube.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            child = cube.transform;
        }

        child.localPosition = localPos;
        child.localScale = localScale;
        child.gameObject.SetActive(true);

        var displayColor = color;
        displayColor.a = alpha;
        ApplyDetailMaterial(child.GetComponent<Renderer>(), displayColor, smoothness, metallic);
    }

    private static void ApplyDetailMaterial(Renderer renderer, Color color, float smoothness = 0.25f, float metallic = 0f)
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
            mat.SetFloat("_Smoothness", smoothness);
        }

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }

        renderer.sharedMaterial = mat;
    }

    private static void ApplyNetMaterial(Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = HoopVisualMaterials.CreateOpaqueNet();
        renderer.enabled = true;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;
            child.name = "DESTRUCT_PENDING";
            child.transform.SetParent(null);
            if (Application.isPlaying)
            {
                Object.Destroy(child);
            }
            else
            {
                Object.DestroyImmediate(child);
            }
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
