using UnityEngine;

/// <summary>
/// Shared hoop upgrade: regulation-style rim colliders, backboard detail, and visual-only net.
/// Used by scene builder, arena builder, and play-mode fix for stable basketball physics.
/// </summary>
public static class TrainingHoopDetail
{
    public const int RimSegmentCount = 12;
    public const int NetStrandCount = 26;
    public const float RimOuterRadius = 0.43f;
    public const float RimTubeRadius = 0.018f;

    // Regulation 42×72 backboard proportions (local units on 1.8 × 1.05 glass panel).
    // Depths kept flush so the target reads as one product-style piece (not stacked layers).
    private const float GlassHalfWidth = 0.9f;
    private const float GlassHalfHeight = 0.525f;
    private const float GlassFaceZ = 0.018f;
    private const float FrameDepth = 0.022f;
    private const float MarkingLine = 0.022f;
    private const float MarkingInset = 0.06f;
    private const float MarkingDepth = 0.008f;

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
        EnsureHoopReflectionProbeCoverage();
    }

    /// <summary>
    /// Repositions the court ReflectionProbe onto the free-throw lane so Thin glass
    /// gets accurate probe fallback (Unity HDRP refractive material guidance).
    /// </summary>
    public static void EnsureHoopReflectionProbeCoverage()
    {
        var probeGo = GameObject.Find(ArcAcademyLayout.ReflectionProbeName);
        if (probeGo == null || !probeGo.TryGetComponent(out ReflectionProbe probe))
        {
            return;
        }

        Vector3 rim = ArcAcademyLayout.MainRimWorldPosition;
        probeGo.transform.position = new Vector3(
            rim.x,
            Mathf.Max(rim.y, 2.8f),
            (rim.z + ArcAcademyLayout.FreeThrowLineWorldZ) * 0.5f);
        probe.size = new Vector3(16f, 10f, 22f);
        // Refresh after reposition; safe for Realtime/Baked (avoids ReflectionProbeMode
        // enum resolution issues in the Bob runtime asmdef).
        probe.RenderProbe();
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
        // FIBA/NCAA: ring top edge horizontal and parallel to the floor (not a vertical target).
        // Torus + RimColliders lie in XZ with Y through the hole — identity rotation.
        rim.localRotation = Quaternion.identity;
        rim.localScale = Vector3.one;
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
            // Horizontal ring in XZ; capsule tangent follows the circle.
            seg.transform.localPosition = new Vector3(cos * RimOuterRadius, 0f, sin * RimOuterRadius);
            seg.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);

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
            backboardRenderer.sharedMaterial = HoopVisualMaterials.CreateProductBackboard();
            backboardRenderer.enabled = true;
        }

        RemoveLegacyDetail(backboard, "TargetSquare_Outer");
        RemoveLegacyDetail(backboard, "TargetSquare_Inner");
        RemoveLegacyDetail(backboard, "BackboardPad_Bottom");

        EnsureAluminumFrame(backboard);
        EnsureRegulationGlassMarkings(backboard);
        EnsureSteelRimSupport(backboard);
        EnsureTuffGuardPadding(backboard);
        DeactivateLayeredBackboardExtras(backboard);
    }

    private static void EnsureAluminumFrame(Transform backboard)
    {
        var frameMat = HoopVisualMaterials.CreateFrameAluminum();
        float frameZ = 0.012f;
        float frameThickness = 0.05f;

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
        var markingMat = HoopVisualMaterials.CreateProductMarking();

        // Single readable backboard: shooter's square only (no nested GlassBorder frame
        // that reads as a second/third board from the lab camera).
        EnsureOutlineRect(
            backboard,
            "TargetSquare",
            new Vector3(0f, -0.08f, GlassFaceZ + 0.001f),
            new Vector2(0.64f, 0.48f),
            MarkingLine,
            markingMat);

        // Hide any legacy GlassBorder strips left from older upgrades.
        for (int i = 0; i < backboard.childCount; i++)
        {
            var ch = backboard.GetChild(i);
            if (ch.name.StartsWith("GlassBorder") && ch.gameObject.activeSelf)
            {
                ch.gameObject.SetActive(false);
            }
        }

        EnsureTargetSquareHitZone(backboard);
    }

    /// <summary>
    /// Thin trigger covering the shooter's square so PPO can earn a small curriculum
    /// reward for high-arc hits on the orange target box (≪ make reward).
    /// Parent is HoopHead (not Backboard) so Backboard lossy scale does not distort the zone.
    /// </summary>
    public static void EnsureTargetSquareHitZone(Transform backboard)
    {
        if (backboard == null)
        {
            return;
        }

        var hoopHead = backboard.parent;
        if (hoopHead == null)
        {
            hoopHead = backboard;
        }

        const string zoneName = "TargetSquareHitZone";
        var zone = hoopHead.Find(zoneName);
        if (zone == null)
        {
            zone = backboard.Find(zoneName);
        }

        if (zone == null)
        {
            var go = new GameObject(zoneName);
            go.transform.SetParent(hoopHead, false);
            zone = go.transform;
        }
        else if (zone.parent != hoopHead)
        {
            zone.SetParent(hoopHead, true);
        }

        // World pose: shooter's square center, slightly court-side of the flatter glass face.
        zone.position = backboard.TransformPoint(new Vector3(0f, -0.08f, GlassFaceZ + 0.02f));
        zone.rotation = backboard.rotation;
        zone.localScale = Vector3.one;

        if (!zone.TryGetComponent(out BoxCollider box))
        {
            box = zone.gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = true;
        box.size = new Vector3(0.64f, 0.48f, 0.12f);
        box.center = Vector3.zero;

        if (!zone.TryGetComponent(out HoopTargetSquareHit _))
        {
            zone.gameObject.AddComponent<HoopTargetSquareHit>();
        }

        BobPhysicsLayers.SetLayerRecursively(zone.gameObject, BobPhysicsLayers.TrainingArenaLayer);
    }

    private static void EnsureSteelRimSupport(Transform backboard)
    {
        var steelMat = HoopVisualMaterials.CreateSteelSupport();
        EnsureDetailMesh(
            backboard,
            "RimSupportBar",
            new Vector3(0f, -0.035f, -0.015f),
            new Vector3(0.46f, 0.07f, 0.048f),
            steelMat);
        EnsureDetailMesh(
            backboard,
            "RimSupportPlate",
            new Vector3(0f, -0.035f, -0.04f),
            new Vector3(0.24f, 0.15f, 0.028f),
            steelMat);
        EnsureDetailMesh(
            backboard,
            "RimSupportGusset_L",
            new Vector3(-0.14f, -0.06f, -0.03f),
            new Vector3(0.04f, 0.08f, 0.035f),
            steelMat);
        EnsureDetailMesh(
            backboard,
            "RimSupportGusset_R",
            new Vector3(0.14f, -0.06f, -0.03f),
            new Vector3(0.04f, 0.08f, 0.035f),
            steelMat);
    }

    /// <summary>
    /// Product-style target skips TuffGuard pads (they read as a third pane from the lab camera).
    /// Deactivates any leftover pad meshes from older upgrades.
    /// </summary>
    private static void EnsureTuffGuardPadding(Transform backboard)
    {
        DeactivateNamedChildren(
            backboard,
            "BackboardPad_Bottom_L",
            "BackboardPad_Bottom_R",
            "BackboardPad_Side_L",
            "BackboardPad_Side_R",
            "BackboardPad_Corner_L",
            "BackboardPad_Corner_R");
    }

    /// <summary>
    /// Hide extruded lips / pads that stacked depth in front of the main glass collider.
    /// </summary>
    private static void DeactivateLayeredBackboardExtras(Transform backboard)
    {
        DeactivateNamedChildren(
            backboard,
            "BackboardFrame_Top_Lip",
            "BackboardFrame_Bottom_Lip",
            "BackboardFrame_Left_Lip",
            "BackboardFrame_Right_Lip",
            "BackboardPad_Side_L",
            "BackboardPad_Side_R",
            "BackboardPad_Corner_L",
            "BackboardPad_Corner_R",
            "BackboardPad_Bottom_L",
            "BackboardPad_Bottom_R");

        // Chamfer face strips from the old dual-depth outline (TargetSquare_*_Face).
        for (int i = 0; i < backboard.childCount; i++)
        {
            var ch = backboard.GetChild(i);
            if (ch.name.Contains("_Face") && ch.gameObject.activeSelf)
            {
                ch.gameObject.SetActive(false);
            }
        }
    }

    private static void DeactivateNamedChildren(Transform parent, params string[] names)
    {
        foreach (var name in names)
        {
            var child = parent.Find(name);
            if (child != null && child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private static void EnsureBreakawayRimAssembly(Transform rim)
    {
        var rimMat = HoopVisualMaterials.CreateRimOrange();
        // Slightly thicker brackets/backplate so the breakaway reads as one solid unit with the tube.
        EnsureDetailMesh(
            rim,
            "RimBackplate",
            new Vector3(0f, 0.02f, -0.14f),
            new Vector3(0.36f, 0.20f, 0.18f),
            rimMat);
        EnsureDetailMesh(
            rim,
            "RimSpringCover",
            new Vector3(0f, -0.015f, -0.12f),
            new Vector3(0.26f, 0.09f, 0.11f),
            rimMat);
        EnsureDetailMesh(
            rim,
            "RimBracket_L",
            new Vector3(-0.14f, 0.035f, -0.15f),
            new Vector3(0.065f, 0.07f, 0.20f),
            rimMat);
        EnsureDetailMesh(
            rim,
            "RimBracket_R",
            new Vector3(0.14f, 0.035f, -0.15f),
            new Vector3(0.065f, 0.07f, 0.20f),
            rimMat);

        // Small weld beads where brackets meet the rim tube.
        EnsureDetailCylinder(
            rim,
            "RimWeld_L",
            new Vector3(-0.14f, 0.01f, -0.05f),
            new Vector3(0.026f, 0.014f, 0.026f),
            Quaternion.Euler(0f, 0f, 90f),
            rimMat);
        EnsureDetailCylinder(
            rim,
            "RimWeld_R",
            new Vector3(0.14f, 0.01f, -0.05f),
            new Vector3(0.026f, 0.014f, 0.026f),
            Quaternion.Euler(0f, 0f, 90f),
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
            // Short radial cylinders read as welded hanger loops, not floating boxes.
            EnsureDetailCylinder(
                rim,
                $"RimPigtail_{i}",
                new Vector3(cos * pigtailRadius, -0.028f, sin * pigtailRadius),
                new Vector3(0.012f, 0.014f, 0.012f),
                Quaternion.Euler(90f, angle * Mathf.Rad2Deg, 0f),
                pigtailMat);
        }
    }

    private static void EnsureRimMaterial(Transform rim)
    {
        if (rim == null)
        {
            return;
        }

        EnsureRimTubeVisual(rim);
    }

    /// <summary>
    /// Replaces the flat scaled-cylinder rim look with a procedural torus tube.
    /// Physics stay on <c>RimColliders</c> capsules — this is visual-only.
    /// </summary>
    private static void EnsureRimTubeVisual(Transform rim)
    {
        var rimMaterial = HoopVisualMaterials.CreateRimOrange();

        // Hide the legacy flat cylinder on the rim root (keeps transform for physics children).
        if (rim.TryGetComponent(out MeshRenderer rootRenderer))
        {
            rootRenderer.enabled = false;
        }

        var visual = rim.Find("RimTubeVisual");
        if (visual == null)
        {
            var go = new GameObject("RimTubeVisual");
            go.transform.SetParent(rim, false);
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            visual = go.transform;
        }

        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        visual.localScale = Vector3.one;

        if (!visual.TryGetComponent(out MeshFilter meshFilter))
        {
            meshFilter = visual.gameObject.AddComponent<MeshFilter>();
        }

        meshFilter.sharedMesh = GenerateTorus(
            RimOuterRadius * 0.95f,
            RimTubeRadius * 2.0f,
            majorSegments: 36,
            minorSegments: 14);

        if (!visual.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRenderer = visual.gameObject.AddComponent<MeshRenderer>();
        }

        meshRenderer.sharedMaterial = rimMaterial;
        meshRenderer.enabled = true;
    }

    /// <summary>
    /// Lightweight procedural torus (XZ plane, Y through the hole) for rim tube / net rings.
    /// </summary>
    private static Mesh GenerateTorus(
        float majorRadius,
        float minorRadius,
        int majorSegments = 32,
        int minorSegments = 12)
    {
        var mesh = new Mesh { name = "ProceduralTorus" };
        int vertCount = (majorSegments + 1) * (minorSegments + 1);
        var vertices = new Vector3[vertCount];
        var normals = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];
        var triangles = new int[majorSegments * minorSegments * 6];

        for (int i = 0; i <= majorSegments; i++)
        {
            float majorAngle = i * Mathf.PI * 2f / majorSegments;
            var majorCenter = new Vector3(
                Mathf.Cos(majorAngle) * majorRadius,
                0f,
                Mathf.Sin(majorAngle) * majorRadius);

            for (int j = 0; j <= minorSegments; j++)
            {
                float minorAngle = j * Mathf.PI * 2f / minorSegments;
                var normal = new Vector3(
                    Mathf.Cos(majorAngle) * Mathf.Cos(minorAngle),
                    Mathf.Sin(minorAngle),
                    Mathf.Sin(majorAngle) * Mathf.Cos(minorAngle));

                int idx = i * (minorSegments + 1) + j;
                vertices[idx] = majorCenter + normal * minorRadius;
                normals[idx] = normal;
                uvs[idx] = new Vector2(i / (float)majorSegments, j / (float)minorSegments);
            }
        }

        int tri = 0;
        for (int i = 0; i < majorSegments; i++)
        {
            for (int j = 0; j < minorSegments; j++)
            {
                int current = i * (minorSegments + 1) + j;
                int next = (i + 1) * (minorSegments + 1) + j;

                triangles[tri++] = current;
                triangles[tri++] = current + 1;
                triangles[tri++] = next;

                triangles[tri++] = current + 1;
                triangles[tri++] = next + 1;
                triangles[tri++] = next;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
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

        var tubeVisual = rim.Find("RimTubeVisual");
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

            // Prefer procedural torus — keep the legacy flat cylinder mesh hidden.
            if (tubeVisual != null && renderer.transform == rim)
            {
                renderer.enabled = false;
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
        var hoopHead = rim.parent != null ? rim.parent : rim;
        var netRoot = hoopHead.Find("Net");
        if (netRoot == null)
        {
            netRoot = rim.Find("Net");
        }

        if (netRoot == null)
        {
            var go = new GameObject("Net");
            go.transform.SetParent(hoopHead, false);
            netRoot = go.transform;
        }
        else if (netRoot.parent != hoopHead)
        {
            netRoot.SetParent(hoopHead, true);
        }

        // World-aligned under HoopHead. Horizontal rim (FIBA): net hangs down along -Y.
        netRoot.position = rim.position;
        netRoot.rotation = Quaternion.identity;
        netRoot.localScale = Vector3.one;

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

        var netMat = HoopVisualMaterials.CreateOpaqueNet();
        const float topRadius = 0.36f;
        const float bottomRadius = 0.16f;
        float midRadius = (topRadius + bottomRadius) * 0.5f;

        for (int i = 0; i < NetStrandCount; i++)
        {
            float angle = i / (float)NetStrandCount * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // Horizontal rim: circle in XZ, strands hang along -Y.
            CreateNetStrand(
                netRoot,
                $"NetStrand_{i}_Top",
                new Vector3(cos * ((topRadius + midRadius) * 0.5f), -0.13f, sin * ((topRadius + midRadius) * 0.5f)),
                new Vector3(0.014f, 0.12f, 0.014f),
                netMat);
            CreateNetStrand(
                netRoot,
                $"NetStrand_{i}_Bot",
                new Vector3(cos * ((midRadius + bottomRadius) * 0.5f), -0.35f, sin * ((midRadius + bottomRadius) * 0.5f)),
                new Vector3(0.007f, 0.125f, 0.007f),
                netMat);

            if (i % 2 == 0)
            {
                float nextAngle = (i + 1) / (float)NetStrandCount * Mathf.PI * 2f;
                float midAngle = (angle + nextAngle) * 0.5f;
                float span = Mathf.Abs(Mathf.DeltaAngle(angle * Mathf.Rad2Deg, nextAngle * Mathf.Rad2Deg))
                    * Mathf.Deg2Rad * midRadius;
                EnsureDetailMesh(
                    netRoot,
                    $"NetCross_{i}",
                    new Vector3(Mathf.Cos(midAngle) * midRadius, -0.24f, Mathf.Sin(midAngle) * midRadius),
                    new Vector3(span * 0.95f, 0.006f, 0.006f),
                    netMat);
                var cross = netRoot.Find($"NetCross_{i}");
                if (cross != null)
                {
                    cross.localRotation = Quaternion.Euler(0f, -midAngle * Mathf.Rad2Deg, 0f);
                }
            }

            if (i % 4 == 0)
            {
                EnsureDetailMesh(
                    netRoot,
                    $"NetKnot_{i}",
                    new Vector3(cos * midRadius, -0.24f, sin * midRadius),
                    new Vector3(0.014f, 0.014f, 0.014f),
                    netMat);
            }
        }

        EnsureNetRing(netRoot, "NetRing_Upper", -0.08f, topRadius, netMat);
        EnsureNetRing(netRoot, "NetRing_MidUpper", -0.16f, Mathf.Lerp(topRadius, midRadius, 0.5f), netMat);
        EnsureNetRing(netRoot, "NetRing_Mid", -0.24f, midRadius, netMat);
        EnsureNetRing(netRoot, "NetRing_MidLower", -0.32f, Mathf.Lerp(midRadius, bottomRadius, 0.5f), netMat);
        EnsureNetRing(netRoot, "NetRing_Lower", -0.40f, bottomRadius, netMat);
    }

    private static void CreateNetStrand(
        Transform netRoot,
        string name,
        Vector3 localPos,
        Vector3 localScale,
        Material netMat)
    {
        var strand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        strand.name = name;
        strand.transform.SetParent(netRoot, false);
        strand.transform.localPosition = localPos;
        strand.transform.localRotation = Quaternion.identity;
        strand.transform.localScale = localScale;
        ApplyNetMaterial(strand.GetComponent<Renderer>(), netMat);
        var strandCollider = strand.GetComponent<Collider>();
        if (Application.isPlaying) Object.Destroy(strandCollider); else Object.DestroyImmediate(strandCollider);
    }

    private static void EnsureNetRing(
        Transform netRoot,
        string name,
        float localY,
        float majorRadius,
        Material netMat)
    {
        var ring = netRoot.Find(name);
        if (ring == null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(netRoot, false);
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            ring = go.transform;
        }

        if (!ring.TryGetComponent(out MeshFilter meshFilter))
        {
            meshFilter = ring.gameObject.AddComponent<MeshFilter>();
        }

        if (!ring.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRenderer = ring.gameObject.AddComponent<MeshRenderer>();
        }

        var legacyCollider = ring.GetComponent<Collider>();
        if (legacyCollider != null)
        {
            if (Application.isPlaying) Object.Destroy(legacyCollider); else Object.DestroyImmediate(legacyCollider);
        }

        // Torus in XZ (Y through hole) — parallel to the horizontal rim.
        ring.localPosition = new Vector3(0f, localY, 0f);
        ring.localRotation = Quaternion.identity;
        ring.localScale = Vector3.one;
        meshFilter.sharedMesh = GenerateTorus(majorRadius, 0.0055f, 28, 8);
        ApplyNetMaterial(meshRenderer, netMat);
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
        float depth = MarkingDepth;

        // Single flush strip per edge — no dual-depth face chamfer (reads as stacked layers).
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
        EnsureOutlineRect(parent, prefix, center, size, lineWidth, HoopVisualMaterials.CreateProductMarking());
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
        child.localRotation = Quaternion.identity;
        child.localScale = localScale;
        child.gameObject.SetActive(true);

        if (child.TryGetComponent(out Renderer renderer) && material != null)
        {
            renderer.sharedMaterial = material;
            renderer.enabled = true;
        }
    }

    private static void EnsureDetailCylinder(
        Transform parent,
        string name,
        Vector3 localPos,
        Vector3 localScale,
        Quaternion localRot,
        Material material)
    {
        var child = parent.Find(name);
        if (child == null)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            var collider = cylinder.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            child = cylinder.transform;
        }
        else if (child.TryGetComponent(out MeshFilter filter)
                 && (filter.sharedMesh == null || filter.sharedMesh.name.IndexOf("Cube", System.StringComparison.Ordinal) >= 0))
        {
            // Upgrade legacy cube pigtails to cylinders without leaving kitbash boxes.
            if (Application.isPlaying) Object.Destroy(child.gameObject); else Object.DestroyImmediate(child.gameObject);
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            var collider = cylinder.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            child = cylinder.transform;
        }

        child.localPosition = localPos;
        child.localRotation = localRot;
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

    private static void ApplyNetMaterial(Renderer renderer, Material netMat = null)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = netMat != null ? netMat : HoopVisualMaterials.CreateOpaqueNet();
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
