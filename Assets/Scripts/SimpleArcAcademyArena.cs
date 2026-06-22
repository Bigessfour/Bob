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

    public const string SpawnPointName = "SpawnPoint";
    public const string FloorName = "Floor";
    public const string WallNorthName = "Wall_North";
    public const string WallSouthName = "Wall_South";
    public const string WallEastName = "Wall_East";
    public const string WallWestName = "Wall_West";

    public const string GoalBudgetSurplusName = "Goal_BudgetSurplus";
    public const string ZoneTaxRevenueName = "Zone_TaxRevenue";
    public const string ObstaclePrefix = "Obstacle_OverSpend_";

    public static readonly Color FloorColor = new(0.333f, 0.333f, 0.333f);
    public static readonly Color WallColor = new(0f, 0.667f, 1f);
    public static readonly Color TargetRed = new(1f, 0.15f, 0.15f);
    public static readonly Color TargetYellow = new(1f, 0.92f, 0.1f);
    public static readonly Color TargetGreen = new(0.2f, 0.95f, 0.35f);

    public static readonly Vector3 FloorPosition = Vector3.zero;
    public static readonly Vector3 FloorScale = new(20f, 1f, 20f);

    /// <summary>Free-throw spawn on simple arena floor (≈ legacy BobSpawnPosition).</summary>
    public static readonly Vector3 BobSpawnLocalPosition = new(0f, 0.02f, -2f);

    public static bool HasArenaFloor()
    {
        var arena = GameObject.Find(RootName);
        return arena != null && arena.transform.Find(FloorName) != null;
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
