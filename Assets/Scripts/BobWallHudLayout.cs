using UnityEngine;

/// <summary>
/// Positions the world-space lab HUD on Wall_South (behind the hoop), lower-left of the rim.
/// Strips stray HUD roots from other walls and keeps Wall_North a plain solid surface.
/// </summary>
public static class BobWallHudLayout
{
    public const float HudWorldPositionTolerance = 1.5f;
    public const float MinCameraFacingDot = 0.5f;

    private static readonly string[] StripHudWallNames =
    {
        SimpleArcAcademyArena.WallNorthName,
        SimpleArcAcademyArena.WallWestName,
        SimpleArcAcademyArena.WallEastName,
    };

    public static void ApplyActiveArenaLayout()
    {
        var arena = GameObject.Find(SimpleArcAcademyArena.RootName);
        if (arena == null)
        {
            return;
        }

        ApplyLabHudLayout(arena.transform);
    }

    public static void ApplyLabHudLayout(Transform arenaRoot)
    {
        if (arenaRoot == null)
        {
            return;
        }

        StripStrayHudRoots(arenaRoot);
        CleanNorthWall(arenaRoot);

        var wallSouth = arenaRoot.Find(SimpleArcAcademyArena.LabHudWallName);
        if (wallSouth == null)
        {
            return;
        }

        var hud = wallSouth.Find(BobWallTrainingHud.RootName);
        if (hud == null)
        {
            return;
        }

        if (hud.parent != wallSouth)
        {
            hud.SetParent(wallSouth, false);
        }

        PlaceHudOnSouthWall(wallSouth, hud);
    }

    public static Vector3 GetHudWorldPosition(Transform wallSouth)
    {
        var innerFaceWorldZ = wallSouth.position.z + (SimpleArcAcademyArena.WallSouthScale.z * 0.5f);
        return new Vector3(
            SimpleArcAcademyArena.LabHudWorldX,
            SimpleArcAcademyArena.LabHudWorldY,
            innerFaceWorldZ + SimpleArcAcademyArena.LabHudWallInsetZ);
    }

    public static Vector3 GetBlendedCameraPosition()
    {
        return (SimpleArcAcademyArena.LabCameraPosition + SimpleArcAcademyArena.GetHeroCameraPosition) * 0.5f;
    }

    public static bool IsFacingCamera(Transform hudTransform, Vector3 cameraPosition)
    {
        Vector3 toCamera = cameraPosition - hudTransform.position;
        if (toCamera.sqrMagnitude < 0.001f)
        {
            return false;
        }

        Vector3 faceNormal = -hudTransform.forward;
        return Vector3.Dot(faceNormal.normalized, toCamera.normalized) >= MinCameraFacingDot;
    }

    private static void PlaceHudOnSouthWall(Transform wallSouth, Transform hud)
    {
        Vector3 worldTarget = GetHudWorldPosition(wallSouth);
        Vector3 localPos = wallSouth.InverseTransformPoint(worldTarget);
        hud.localPosition = localPos;
        hud.localRotation = ComputeHudRotation(worldTarget);
        hud.localScale = Vector3.one;
    }

    private static Quaternion ComputeHudRotation(Vector3 hudWorldPosition)
    {
        Vector3 toCamera = GetBlendedCameraPosition() - hudWorldPosition;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude < 0.001f)
        {
            toCamera = Vector3.forward;
        }

        var rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);

        if (!IsFacingCameraWithRotation(hudWorldPosition, rotation, SimpleArcAcademyArena.LabCameraPosition)
            || !IsFacingCameraWithRotation(hudWorldPosition, rotation, SimpleArcAcademyArena.GetHeroCameraPosition))
        {
            rotation *= Quaternion.Euler(0f, 180f, 0f);
        }

        return rotation;
    }

    private static bool IsFacingCameraWithRotation(
        Vector3 hudWorldPosition,
        Quaternion rotation,
        Vector3 cameraPosition)
    {
        Vector3 toCamera = cameraPosition - hudWorldPosition;
        if (toCamera.sqrMagnitude < 0.001f)
        {
            return false;
        }

        Vector3 faceNormal = -(rotation * Vector3.forward);
        return Vector3.Dot(faceNormal.normalized, toCamera.normalized) >= MinCameraFacingDot;
    }

    private static void StripStrayHudRoots(Transform arenaRoot)
    {
        foreach (var wallName in StripHudWallNames)
        {
            var wall = arenaRoot.Find(wallName);
            if (wall == null)
            {
                continue;
            }

            var stray = wall.Find(BobWallTrainingHud.RootName);
            if (stray != null)
            {
                Object.Destroy(stray.gameObject);
            }
        }
    }

    private static void CleanNorthWall(Transform arenaRoot)
    {
        var northWall = arenaRoot.Find(SimpleArcAcademyArena.WallNorthName);
        if (northWall == null)
        {
            return;
        }

        for (int i = northWall.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(northWall.GetChild(i).gameObject);
        }

        var referenceWall = arenaRoot.Find(SimpleArcAcademyArena.WallSouthName);
        if (referenceWall != null
            && referenceWall.TryGetComponent(out Renderer referenceRenderer)
            && referenceRenderer.sharedMaterial != null
            && northWall.TryGetComponent(out Renderer northRenderer))
        {
            northRenderer.sharedMaterial = referenceRenderer.sharedMaterial;
        }
    }
}
