using UnityEngine;

/// <summary>
/// Layout constants for the AI Warehouse–style simple training arena (primitives only).
/// Built by <see cref="SimpleArcAcademyArenaBuilder"/> in the Editor.
/// </summary>
public static class SimpleArcAcademyArena
{
    public const string RootName = "SimpleArcAcademyArena";
    public const string PrefabPath = "Assets/Prefabs/Prefab_SimpleArena.prefab";
    public const string BobPrefabPath = "Assets/Prefabs/Prefab_Bob.prefab";
    public const string BasketballPrefabPath = "Assets/Prefabs/Prefab_Basketball.prefab";

    public const string SpawnPointName = "SpawnPoint";
    public const string FloorName = "Floor";
    public const string WallNorthName = "Wall_North";
    public const string WallSouthName = "Wall_South";
    public const string WallEastName = "Wall_East";
    public const string WallWestName = "Wall_West";

    public const string GoalBudgetSurplusName = "Goal_BudgetSurplus";
    public const string ZoneTaxRevenueName = "Zone_TaxRevenue";
    public const string ObstaclePrefix = "Obstacle_OverSpend_";

    /// <summary>Budget-flavor props are hidden in AI Warehouse lab polish mode.</summary>
    public static readonly bool ShowBudgetFlavorProps = false;

    public static readonly Color FloorColor = new(0.82f, 0.70f, 0.54f);
    public static readonly Color WallColor = new(0.92f, 0.92f, 0.92f);
    public static readonly Color TargetRed = new(1f, 0.15f, 0.15f);
    public static readonly Color TargetYellow = new(1f, 0.92f, 0.1f);
    public static readonly Color TargetGreen = new(0.2f, 0.95f, 0.35f);

    public static readonly Vector3 FloorPosition = Vector3.zero;
    public static readonly Vector3 FloorScale = new(20f, 1f, 20f);

    /// <summary>Free-throw line spawn — Bob stands on the hardwood (no pedestal).</summary>
    public static readonly Vector3 BobSpawnLocalPosition =
        new(0f, 0f, ArcAcademyLayout.FreeThrowLineWorldZ);

    /// <summary>Lift Bob so the cube sits on the floor (scale 0.42 → half-height ≈ 0.21 m).</summary>
    public static readonly Vector3 BobFloorSpawnOffset = new(0f, 0.21f, 0f);

    /// <summary>Ball release offset from Bob spawn (matches BasketballProjectileSetup.ReleaseOffset).</summary>
    public static readonly Vector3 BallReleaseLocalOffset = BasketballProjectileSetup.ReleaseOffset;

    public const string LabHudWallName = WallSouthName;
    public const string LabHudRootName = BobWallTrainingHud.RootName;
    public const string PowerPathPulseName = "PowerPathPulse";

    /// <summary>World-space HUD on Wall_South inner face — lower-left of hoop (back wall).</summary>
    public const float LabHudWallInsetZ = 0.12f;
    public const float LabHudWorldX = -2.8f;
    public const float LabHudWorldY = 2.35f;
    public const float LabHudWorldZ = -9.88f;

    /// <summary>Inner padding (px) between the black panel edge and HUD text.</summary>
    public const float LabHudPanelPadding = 14f;

    /// <summary>Canvas px — sized for full headline + RL detail strings (~0.84 × 0.92 m at scale).</summary>
    public static readonly Vector2 LabHudCanvasSize = new(420f, 460f);
    public static readonly Vector3 LabHudCanvasScale = new(0.002f, 0.002f, 0.002f);

    /// <summary>East sideline camera — level side view across spawn → hoop (AI Warehouse readability).</summary>
    public static readonly Vector3 LabCameraPosition = new(13f, 3.2f, -3.5f);
    public static readonly Vector3 LabCameraLookAt = new(0f, 2f, -4.5f);
    public const float LabCameraFieldOfView = 52f;

    /// <summary>Behind Bob at the line — level down-court view toward hoop (Hero alternate in lab).</summary>
    public static readonly Vector3 LabBehindBobCameraPosition = new(0.5f, 2.35f, 1.2f);
    public static readonly Vector3 LabBehindBobCameraLookAt = new(0f, 2.3f, -5.5f);
    public const float LabBehindBobCameraFieldOfView = 50f;

    public static Vector3 GetHeroCameraPosition =>
        HasArenaFloor() ? LabBehindBobCameraPosition : ArcAcademyLayout.CameraPosition;

    public static Vector3 GetHeroCameraLookAt =>
        HasArenaFloor() ? LabBehindBobCameraLookAt : ArcAcademyLayout.CameraLookAt;

    public static float GetHeroCameraFieldOfView =>
        HasArenaFloor() ? LabBehindBobCameraFieldOfView : ArcAcademyLayout.CameraFieldOfView;

    public static bool IsLabViewActive => HasArenaFloor();

    public static bool HasArenaFloor()
    {
        var arena = GameObject.Find(RootName);
        return arena != null && arena.transform.Find(FloorName) != null;
    }

    public static Vector3 GetLabBobSpawnPosition(Transform spawnPoint)
    {
        return spawnPoint != null
            ? spawnPoint.position + BobFloorSpawnOffset
            : new Vector3(0f, BobFloorSpawnOffset.y, ArcAcademyLayout.FreeThrowLineWorldZ);
    }

    public static Quaternion GetSpawnFacingRotation(Vector3 spawnPosition, Transform hoopTransform)
    {
        if (hoopTransform == null)
        {
            return Quaternion.identity;
        }

        Vector3 toHoop = hoopTransform.position - spawnPosition;
        toHoop.y = 0f;
        if (toHoop.sqrMagnitude < 0.001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(toHoop.normalized, Vector3.up);
    }

    public static readonly Vector3 WallNorthScale = new(22f, 4f, 1f);
    public static readonly Vector3 WallNorthPosition = new(0f, 2f, 10.5f);

    public static readonly Vector3 WallSouthScale = new(22f, 4f, 1f);
    public static readonly Vector3 WallSouthPosition = new(0f, 2f, -10.5f);

    public static readonly Vector3 WallEastScale = new(1f, 4f, 21f);
    public static readonly Vector3 WallEastPosition = new(10.5f, 2f, 0f);

    public static readonly Vector3 WallWestScale = new(1f, 4f, 21f);
    public static readonly Vector3 WallWestPosition = new(-10.5f, 2f, 0f);

    public static readonly Vector3 GoalPosition = new(0f, 1.5f, 8f);
    public static readonly Vector3 GoalScale = new(1.5f, 1.5f, 1.5f);

    public static readonly Vector3 ZonePosition = new(0f, 0.02f, -6f);
    public static readonly Vector3 ZoneScale = new(3f, 1f, 2f);

    public static readonly (Vector3 position, Vector3 scale)[] ObstaclePlacements =
    {
        (new Vector3(4f, 0.5f, 3f), Vector3.one),
        (new Vector3(-3f, 0.5f, 2f), Vector3.one),
        (new Vector3(2f, 0.5f, -2f), Vector3.one),
        (new Vector3(-5f, 0.5f, -4f), Vector3.one),
    };
}
