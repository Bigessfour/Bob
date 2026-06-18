using UnityEngine;

/// <summary>
/// Shared free-throw half-court layout for BobTraining scene and BobAgent resets.
/// Unity units ≈ meters. Used by scene builder, validator, and agent bounds.
/// </summary>
public static class BobCourtLayout
{
    public const string ArenaName = "TrainingArena";
    public const string CourtFloorName = "CourtFloor";
    public const string CourtMarkingsName = "CourtMarkings";
    public const string HoopName = "Hoop";
    public const string RimName = "Rim";
    public const string ScoreZoneName = "ScoreZone";

    public const float CourtHalfWidth = 7f;
    public const float CourtNearZ = 8f;
    public const float CourtFarZ = -14f;

    public const float FreeThrowLineZ = 2f;
    public const float KeyHalfWidth = 1.83f;
    public const float KeyDepthFromBaseline = 5.8f;

    /// <summary>Hoop assembly anchor on the court baseline.</summary>
    public static readonly Vector3 HoopRootPosition = new(0f, 0f, -9f);

    /// <summary>Rim center relative to hoop root (regulation ~3.05 m height).</summary>
    public static readonly Vector3 RimLocalPosition = new(0f, 3.05f, 0.2f);

    public static Vector3 RimWorldPosition => HoopRootPosition + RimLocalPosition;

    public static readonly Vector3 BobSpawnPosition = new(0f, 1.2f, FreeThrowLineZ);
    public static readonly Vector3 CameraPosition = new(0f, 4.8f, 6f);
    public static readonly Vector3 CameraLookAt = new(0f, 3.2f, -5f);

    public const float RimScoreRadius = 0.45f;
    public const float SpawnLateralJitter = 0.25f;

    public static bool IsOutOfBounds(Vector3 worldPosition)
    {
        return worldPosition.y < 0.35f
               || Mathf.Abs(worldPosition.x) > CourtHalfWidth + 0.5f
               || worldPosition.z > CourtNearZ + 0.5f
               || worldPosition.z < CourtFarZ - 0.5f;
    }
}
