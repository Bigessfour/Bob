#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// Half-court hero zone markings on Simple Arc Academy hardwood — regulation lane, royal blue key,
/// free-throw semicircle, three-point arc, sidelines, and half-court line.
/// Parented to arena root (never under scaled Floor).
/// </summary>
public static class SimpleArcCourtMarkingsBuilder
{
    public const string CourtMarkingsName = "CourtMarkings";

    private const float LineY = 0.012f;
    private const float KeyPaintY = 0.008f;
    private const float LineThickness = 0.03f;
    private const float ArcSegmentSize = 0.06f;

    private static readonly Color CourtLine = new(0.98f, 0.98f, 1f);
    private static readonly Color KeyPaintFill = HoopVisualMaterials.BackboardPadBlue;

    public static void EnsureCourtMarkings(Transform arenaRoot)
    {
        if (arenaRoot == null)
        {
            return;
        }

        DestroyMisplacedMarkings(arenaRoot);

        var markings = arenaRoot.Find(CourtMarkingsName);
        if (markings == null)
        {
            var go = new GameObject(CourtMarkingsName);
            go.transform.SetParent(arenaRoot, false);
            markings = go.transform;
        }
        else if (markings.parent != arenaRoot)
        {
            markings.SetParent(arenaRoot, false);
        }

        var floor = arenaRoot.Find(SimpleArcAcademyArena.FloorName);
        if (floor != null)
        {
            var legacy = floor.Find(CourtMarkingsName);
            if (legacy != null)
            {
                Object.DestroyImmediate(legacy.gameObject);
            }
        }

        markings.localPosition = Vector3.zero;
        markings.localRotation = Quaternion.identity;
        markings.localScale = Vector3.one;
        markings.gameObject.SetActive(true);

        float baselineZ = ArcAcademyLayout.BaselineWorldZ;
        float keyFrontZ = ArcAcademyLayout.KeyFrontWorldZ;
        float keyCenterZ = ArcAcademyLayout.KeyCenterWorldZ;
        float ftZ = ArcAcademyLayout.FreeThrowLineWorldZ;
        float halfCourtZ = ArcAcademyLayout.HalfCourtLineWorldZ;
        float courtHalfW = ArcAcademyLayout.CourtHalfWidth;
        float keyHalfW = ArcAcademyLayout.KeyHalfWidth;
        float keyDepth = ArcAcademyLayout.KeyDepthFromBaseline;
        float sidelineLength = halfCourtZ - baselineZ;

        CreateLineMark(markings, "Baseline",
            new Vector3(0f, LineY, baselineZ),
            new Vector3(courtHalfW * 2f, LineThickness, LineThickness));

        CreateLineMark(markings, "FreeThrowLine",
            new Vector3(0f, LineY, ftZ),
            new Vector3(keyHalfW * 2f, LineThickness, LineThickness));

        CreateLineMark(markings, "KeyLeft",
            new Vector3(-keyHalfW, LineY, keyCenterZ),
            new Vector3(LineThickness, LineThickness, keyDepth));

        CreateLineMark(markings, "KeyRight",
            new Vector3(keyHalfW, LineY, keyCenterZ),
            new Vector3(LineThickness, LineThickness, keyDepth));

        EnsureKeyPaint(markings, keyCenterZ, keyHalfW, keyDepth);
        EnsureFreeThrowSemicircle(markings, ftZ);
        EnsureThreePointArc(markings, baselineZ);
        EnsureRestrictedAreaArc(markings, baselineZ);
        EnsureSidelines(markings, baselineZ, halfCourtZ, courtHalfW, sidelineLength);
        EnsureHalfCourtLine(markings, halfCourtZ, courtHalfW);
    }

    private static void EnsureKeyPaint(Transform parent, float keyCenterZ, float keyHalfW, float keyDepth)
    {
        var keyFill = parent.Find("KeyPaint");
        if (keyFill == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "KeyPaint";
            cube.transform.SetParent(parent, false);
            keyFill = cube.transform;
            Object.DestroyImmediate(cube.GetComponent<Collider>());
        }

        keyFill.localPosition = new Vector3(0f, KeyPaintY, keyCenterZ);
        keyFill.localScale = new Vector3(keyHalfW * 2f, 0.008f, keyDepth);
        keyFill.gameObject.SetActive(true);
        ApplyKeyPaintMaterial(keyFill.GetComponent<Renderer>());
        BobPhysicsLayers.SetLayerRecursively(keyFill.gameObject, BobPhysicsLayers.DecorationLayer);

        RemoveLegacyChild(parent, "KeyPaintBorderLeft");
        RemoveLegacyChild(parent, "KeyPaintBorderRight");
        RemoveLegacyChild(parent, "KeyFront");
    }

    private static void EnsureFreeThrowSemicircle(Transform parent, float ftZ)
    {
        var arcRoot = parent.Find("FreeThrowArc");
        if (arcRoot == null)
        {
            var go = new GameObject("FreeThrowArc");
            go.transform.SetParent(parent, false);
            arcRoot = go.transform;
        }

        float radius = ArcAcademyLayout.FreeThrowCircleRadius;
        int segments = 24;

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(180f, 0f, i / (float)segments) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = ftZ + Mathf.Sin(angle) * radius;
            CreateLineMark(arcRoot, $"FtArc_{i}",
                new Vector3(x, LineY, z),
                new Vector3(ArcSegmentSize, LineThickness, ArcSegmentSize));
        }
    }

    private static void EnsureThreePointArc(Transform parent, float baselineZ)
    {
        var arcRoot = parent.Find("ThreePointArc");
        if (arcRoot == null)
        {
            var go = new GameObject("ThreePointArc");
            go.transform.SetParent(parent, false);
            arcRoot = go.transform;
        }

        int segments = 36;
        float radius = ArcAcademyLayout.ThreePointArcRadius;

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(200f, 340f, i / (float)segments) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = baselineZ + Mathf.Sin(angle) * radius;
            CreateLineMark(arcRoot, $"ThreePt_{i}",
                new Vector3(x, LineY, z),
                new Vector3(ArcSegmentSize, LineThickness, ArcSegmentSize));
        }
    }

    private static void EnsureRestrictedAreaArc(Transform parent, float baselineZ)
    {
        var arcRoot = parent.Find("RestrictedAreaArc");
        if (arcRoot == null)
        {
            var go = new GameObject("RestrictedAreaArc");
            go.transform.SetParent(parent, false);
            arcRoot = go.transform;
        }

        float radius = ArcAcademyLayout.RestrictedAreaRadius;
        float rimZ = baselineZ + 1.22f;
        int segments = 14;

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(180f, 0f, i / (float)segments) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = rimZ + Mathf.Sin(angle) * radius;
            CreateLineMark(arcRoot, $"Restricted_{i}",
                new Vector3(x, LineY, z),
                new Vector3(ArcSegmentSize * 0.7f, LineThickness, ArcSegmentSize * 0.7f));
        }
    }

    private static void EnsureSidelines(
        Transform parent,
        float baselineZ,
        float halfCourtZ,
        float courtHalfW,
        float sidelineLength)
    {
        float sidelineCenterZ = (baselineZ + halfCourtZ) * 0.5f;
        CreateLineMark(parent, "SidelineLeft",
            new Vector3(-courtHalfW, LineY, sidelineCenterZ),
            new Vector3(LineThickness, LineThickness, sidelineLength));
        CreateLineMark(parent, "SidelineRight",
            new Vector3(courtHalfW, LineY, sidelineCenterZ),
            new Vector3(LineThickness, LineThickness, sidelineLength));
    }

    private static void EnsureHalfCourtLine(Transform parent, float halfCourtZ, float courtHalfW)
    {
        CreateLineMark(parent, "HalfCourtLine",
            new Vector3(0f, LineY, halfCourtZ),
            new Vector3(courtHalfW * 2f, LineThickness, LineThickness));
    }

    private static void CreateLineMark(Transform parent, string name, Vector3 localPos, Vector3 scale)
    {
        var line = parent.Find(name);
        if (line == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            line = cube.transform;
            Object.DestroyImmediate(cube.GetComponent<Collider>());
        }

        line.localPosition = localPos;
        line.localScale = scale;
        line.gameObject.SetActive(true);

        ApplyLineMaterial(line.GetComponent<Renderer>());
        BobPhysicsLayers.SetLayerRecursively(line.gameObject, BobPhysicsLayers.DecorationLayer);
    }

    private static void ApplyLineMaterial(Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = ArcAcademyMaterialFactory.CreateCourtLineMaterial(CourtLine);
    }

    private static void ApplyKeyPaintMaterial(Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = ArcAcademyMaterialFactory.CreateKeyPaintMaterial(KeyPaintFill);
    }

    private static void RemoveLegacyChild(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child != null)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void DestroyMisplacedMarkings(Transform arenaRoot)
    {
        var transforms = arenaRoot.GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
        {
            if (t == null || t.name != CourtMarkingsName || t.parent == arenaRoot)
            {
                continue;
            }

            Object.DestroyImmediate(t.gameObject);
        }
    }
}
#endif
