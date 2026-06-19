#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// White wheeled portable hoop stands with robotic launcher arms (Example.jpg bays).
/// Visual-only for training bays; shell-only for active MovableHoop scoring head.
/// </summary>
public static class ArcAcademyPortableHoopBuilder
{
    private static readonly Color StandWhite = new(0.94f, 0.95f, 0.97f);
    private static readonly Color StandBlack = new(0.08f, 0.08f, 0.1f);
    private static readonly Color BackboardWhite = new(0.92f, 0.92f, 0.88f);
    private static readonly Color RimOrange = new(1f, 0.45f, 0.1f);
    private static readonly Color ArmMetal = new(0.72f, 0.74f, 0.78f);
    private static readonly Color ArmAccent = new(0.28f, 0.29f, 0.32f);

    /// <summary>Builds a portable stand under parent; returns the rim transform for arc wiring.</summary>
    public static Transform CreateStand(
        Transform parent,
        Vector3 localPosition,
        float yawDegrees,
        float scale,
        bool faceNegativeZ,
        bool includeBackboardAndRim = true,
        bool includeRoboticLauncher = true,
        bool useSolidBackboard = false)
    {
        var stand = new GameObject(ArcAcademyLayout.PortableHoopStandName);
        stand.transform.SetParent(parent);
        stand.transform.localPosition = localPosition;
        stand.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);

        float s = scale;
        BuildStandBase(stand.transform, s);

        if (includeRoboticLauncher)
        {
            BuildRoboticLauncher(stand.transform, s, faceNegativeZ);
        }

        if (!includeBackboardAndRim)
        {
            return stand.transform;
        }

        float boardZ = faceNegativeZ ? 0.14f * s : -0.14f * s;
        var backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "Backboard";
        backboard.transform.SetParent(stand.transform, false);
        backboard.transform.localPosition = new Vector3(0f, 2.75f * s, boardZ);
        backboard.transform.localScale = new Vector3(1.35f * s, 0.88f * s, 0.05f * s);
        var bbMat = useSolidBackboard
            ? ArcAcademyMaterialFactory.GetSolidBackboard(BackboardWhite)
            : ArcAcademyMaterialFactory.GetGlass(BackboardWhite);
        ArcAcademyMaterialFactory.ApplyMaterial(backboard, bbMat);
        Object.DestroyImmediate(backboard.GetComponent<BoxCollider>());

        var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.name = "Rim";
        rim.transform.SetParent(stand.transform, false);
        rim.transform.localPosition = new Vector3(0f, 2.2f * s, 0f);
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        rim.transform.localScale = new Vector3(0.62f * s, 0.028f * s, 0.62f * s);
        ArcAcademyMaterialFactory.ApplyMaterial(
            rim,
            ArcAcademyMaterialFactory.GetRubber(RimOrange));
        Object.DestroyImmediate(rim.GetComponent<Collider>());

        AddSimpleNet(rim.transform, s);

        return rim.transform;
    }

    /// <summary>Visual base shell for the active MovableHoop (articulation + scoring head unchanged).</summary>
    public static void AddActiveHoopShell(Transform hoopRoot)
    {
        CreateStand(
            hoopRoot,
            Vector3.zero,
            0f,
            1f,
            faceNegativeZ: true,
            includeBackboardAndRim: false,
            includeRoboticLauncher: true,
            useSolidBackboard: false);

        foreach (var renderer in hoopRoot.GetComponentsInChildren<Renderer>())
        {
            if (renderer.gameObject.name is "SwivelBasePlate" or "SwivelColumn" or "ArmSegment")
            {
                renderer.enabled = false;
            }
        }
    }

    private static void BuildRoboticLauncher(Transform stand, float scale, bool faceNegativeZ)
    {
        var launcherRoot = new GameObject("RoboticLauncher");
        launcherRoot.transform.SetParent(stand, false);
        launcherRoot.transform.localPosition = new Vector3(0f, 1.55f * scale, faceNegativeZ ? 0.08f * scale : -0.08f * scale);

        var shoulder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shoulder.name = "LauncherShoulder";
        shoulder.transform.SetParent(launcherRoot.transform, false);
        shoulder.transform.localPosition = Vector3.zero;
        shoulder.transform.localScale = new Vector3(0.22f * scale, 0.1f * scale, 0.22f * scale);
        ArcAcademyMaterialFactory.ApplyMaterial(shoulder, ArcAcademyMaterialFactory.GetMetal(ArmAccent));
        Object.DestroyImmediate(shoulder.GetComponent<Collider>());

        var arm = new GameObject("LauncherArm");
        arm.transform.SetParent(launcherRoot.transform, false);
        arm.transform.localPosition = new Vector3(0f, 0.08f * scale, 0f);

        var upper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        upper.name = "ArmUpper";
        upper.transform.SetParent(arm.transform, false);
        upper.transform.localPosition = new Vector3(0f, 0.12f * scale, 0.18f * scale);
        upper.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
        upper.transform.localScale = new Vector3(0.12f * scale, 0.1f * scale, 0.42f * scale);
        ArcAcademyMaterialFactory.ApplyMaterial(upper, ArcAcademyMaterialFactory.GetMetal(ArmMetal));
        Object.DestroyImmediate(upper.GetComponent<Collider>());

        var elbow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        elbow.name = "ArmElbow";
        elbow.transform.SetParent(arm.transform, false);
        elbow.transform.localPosition = new Vector3(0f, 0.05f * scale, 0.38f * scale);
        elbow.transform.localScale = Vector3.one * 0.1f * scale;
        ArcAcademyMaterialFactory.ApplyMaterial(elbow, ArcAcademyMaterialFactory.GetMetal(ArmAccent));
        Object.DestroyImmediate(elbow.GetComponent<Collider>());

        var fore = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fore.name = "ArmFore";
        fore.transform.SetParent(arm.transform, false);
        fore.transform.localPosition = new Vector3(0f, -0.02f * scale, 0.52f * scale);
        fore.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
        fore.transform.localScale = new Vector3(0.09f * scale, 0.08f * scale, 0.28f * scale);
        ArcAcademyMaterialFactory.ApplyMaterial(fore, ArcAcademyMaterialFactory.GetMetal(ArmMetal));
        Object.DestroyImmediate(fore.GetComponent<Collider>());

        var nozzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        nozzle.name = "LauncherNozzle";
        nozzle.transform.SetParent(arm.transform, false);
        nozzle.transform.localPosition = new Vector3(0f, -0.04f * scale, 0.68f * scale);
        nozzle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        nozzle.transform.localScale = new Vector3(0.08f * scale, 0.05f * scale, 0.08f * scale);
        ArcAcademyMaterialFactory.ApplyMaterial(nozzle, ArcAcademyMaterialFactory.GetMetal(StandBlack));
        Object.DestroyImmediate(nozzle.GetComponent<Collider>());

        var visual = launcherRoot.AddComponent<RoboticLauncherVisual>();
        var spawnPad = GameObject.Find(ArcAcademyLayout.SpawnPadName);
        if (spawnPad != null)
        {
            visual.SetAimTarget(spawnPad.transform);
        }
    }

    private static void BuildStandBase(Transform stand, float s)
    {
        var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseGo.name = "StandBase";
        baseGo.transform.SetParent(stand, false);
        baseGo.transform.localPosition = new Vector3(0f, 0.32f * s, 0f);
        baseGo.transform.localScale = new Vector3(0.95f * s, 0.64f * s, 0.72f * s);
        ArcAcademyMaterialFactory.ApplyMaterial(
            baseGo,
            ArcAcademyMaterialFactory.GetMatteWall(StandWhite));
        Object.DestroyImmediate(baseGo.GetComponent<BoxCollider>());

        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = "StandBaseTrim";
        trim.transform.SetParent(baseGo.transform, false);
        trim.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        trim.transform.localScale = new Vector3(1.02f, 0.12f, 1.02f);
        ArcAcademyMaterialFactory.ApplyMaterial(
            trim,
            ArcAcademyMaterialFactory.GetMatteWall(StandBlack));
        Object.DestroyImmediate(trim.GetComponent<BoxCollider>());

        AddWheel(stand, new Vector3(-0.34f * s, 0.12f * s, 0.28f * s), s);
        AddWheel(stand, new Vector3(0.34f * s, 0.12f * s, 0.28f * s), s);
        AddWheel(stand, new Vector3(-0.34f * s, 0.12f * s, -0.28f * s), s);
        AddWheel(stand, new Vector3(0.34f * s, 0.12f * s, -0.28f * s), s);

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pole.name = "StandPole";
        pole.transform.SetParent(stand, false);
        pole.transform.localPosition = new Vector3(0f, 1.45f * s, 0f);
        pole.transform.localScale = new Vector3(0.1f * s, 2.2f * s, 0.1f * s);
        ArcAcademyMaterialFactory.ApplyMaterial(
            pole,
            ArcAcademyMaterialFactory.GetMatteWall(StandWhite));
        Object.DestroyImmediate(pole.GetComponent<Collider>());
    }

    private static void AddWheel(Transform parent, Vector3 localPos, float scale)
    {
        var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "Wheel";
        wheel.transform.SetParent(parent, false);
        wheel.transform.localPosition = localPos;
        wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        wheel.transform.localScale = new Vector3(0.14f * scale, 0.04f * scale, 0.14f * scale);
        ArcAcademyMaterialFactory.ApplyMaterial(
            wheel,
            ArcAcademyMaterialFactory.GetMatteWall(StandBlack));
        Object.DestroyImmediate(wheel.GetComponent<Collider>());
    }

    private static void AddSimpleNet(Transform rim, float scale)
    {
        var netRoot = new GameObject("Net");
        netRoot.transform.SetParent(rim, false);
        netRoot.transform.localPosition = new Vector3(0f, -0.06f * scale, 0f);

        for (int i = 0; i < 6; i++)
        {
            float angle = i / 6f * Mathf.PI * 2f;
            var strand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            strand.name = $"NetStrand_{i}";
            strand.transform.SetParent(netRoot.transform, false);
            strand.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.22f * scale, -0.12f * scale, Mathf.Sin(angle) * 0.22f * scale);
            strand.transform.localScale = new Vector3(0.015f * scale, 0.14f * scale, 0.015f * scale);
            ArcAcademyMaterialFactory.ApplyMaterial(
                strand,
                ArcAcademyMaterialFactory.GetMatteWall(Color.white));
            Object.DestroyImmediate(strand.GetComponent<Collider>());
        }
    }
}
#endif
