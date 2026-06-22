using UnityEngine;

/// <summary>
/// Shared hoop upgrade: regulation-style rim colliders, backboard detail, and visual-only net.
/// Used by scene builder, arena builder, and play-mode fix for stable basketball physics.
/// </summary>
public static class TrainingHoopDetail
{
    public const int RimSegmentCount = 12;
    public const float RimOuterRadius = 0.43f;
    public const float RimTubeRadius = 0.018f;

    private static readonly Color TargetRed = new(0.82f, 0.18f, 0.12f);
    private static readonly Color FrameDark = new(0.12f, 0.12f, 0.14f);
    private static readonly Color NetWhite = new(0.92f, 0.93f, 0.95f);

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
        var rim = FindRim(hoopRoot);
        if (rim == null)
        {
            return;
        }

        ConfigureRimColliders(rim.gameObject);
        EnsureBackboardDetail(rim);
        EnsureVisualNet(rim);
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

        if (collidersRoot.childCount >= RimSegmentCount)
        {
            ApplyRimColliderMaterials(collidersRoot);
            return;
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

    private static void ApplyRimColliderMaterials(Transform collidersRoot)
    {
        foreach (var cap in collidersRoot.GetComponentsInChildren<CapsuleCollider>())
        {
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

        var pole = hoopHead.parent;
        if (pole != null)
        {
            EnsureDetailCube(
                pole,
                "PolePadding",
                new Vector3(0f, -0.35f, -0.08f),
                new Vector3(0.26f, 0.55f, 0.26f),
                FrameDark,
                alpha: 0.85f);
        }
    }

    private static void EnsureVisualNet(Transform rim)
    {
        var netRoot = rim.Find("Net");
        if (netRoot == null)
        {
            var go = new GameObject("Net");
            go.transform.SetParent(rim, false);
            go.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            netRoot = go.transform;
        }

        if (!netRoot.TryGetComponent(out HoopSwishVfx _))
        {
            netRoot.gameObject.AddComponent<HoopSwishVfx>();
        }

        var netPhysics = netRoot.GetComponent<HoopNetPhysics>();
        if (netPhysics != null)
        {
            netPhysics.RebuildVisualOnly(rim, NetWhite);
            return;
        }

        if (netRoot.childCount >= 8)
        {
            StripNetPhysicsColliders(netRoot);
            return;
        }

        ClearChildren(netRoot);
        for (int i = 0; i < 10; i++)
        {
            float angle = i / 10f * Mathf.PI * 2f;
            var strand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strand.name = $"NetStrand_{i}";
            strand.transform.SetParent(netRoot, false);
            strand.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.3f,
                -0.2f,
                Mathf.Sin(angle) * 0.3f);
            strand.transform.localScale = new Vector3(0.014f, 0.2f, 0.014f);
            ApplyNetMaterial(strand.GetComponent<Renderer>());
            Object.Destroy(strand.GetComponent<Collider>());
        }
    }

    private static void StripNetPhysicsColliders(Transform netRoot)
    {
        foreach (var col in netRoot.GetComponentsInChildren<Collider>())
        {
            Object.Destroy(col);
        }

        foreach (var rb in netRoot.GetComponentsInChildren<Rigidbody>())
        {
            Object.Destroy(rb);
        }

        foreach (var joint in netRoot.GetComponentsInChildren<Joint>())
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
