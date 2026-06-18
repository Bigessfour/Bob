using UnityEngine;

/// <summary>
/// Backward-compatible aliases for Arc Academy layout constants used by older references.
/// </summary>
public static class BobCourtLayout
{
    public const string ArenaName = ArcAcademyLayout.ArenaName;
    public const string CourtFloorName = ArcAcademyLayout.CourtFloorName;
    public const string CourtMarkingsName = ArcAcademyLayout.CourtMarkingsName;
    public const string HoopName = ArcAcademyLayout.HoopName;
    public const string RimName = ArcAcademyLayout.RimName;
    public const string ScoreZoneName = ArcAcademyLayout.ScoreZoneName;

    public const float CourtHalfWidth = ArcAcademyLayout.CourtHalfWidth;
    public const float CourtNearZ = ArcAcademyLayout.CourtNearZ;
    public const float CourtFarZ = ArcAcademyLayout.CourtFarZ;
    public const float FreeThrowLineZ = ArcAcademyLayout.FreeThrowLineZ;
    public const float KeyHalfWidth = ArcAcademyLayout.KeyHalfWidth;
    public const float KeyDepthFromBaseline = ArcAcademyLayout.KeyDepthFromBaseline;

    public static Vector3 HoopRootPosition => ArcAcademyLayout.HoopRootDefaultPosition;
    public static Vector3 RimLocalPosition => ArcAcademyLayout.RimLocalDefaultPosition;
    public static Vector3 RimWorldPosition =>
        ArcAcademyLayout.RimWorldPosition(HoopRootPosition, RimLocalPosition);

    public static Vector3 BobSpawnPosition => ArcAcademyLayout.BobSpawnPosition;
    public static Vector3 CameraPosition => ArcAcademyLayout.CameraPosition;
    public static Vector3 CameraLookAt => ArcAcademyLayout.CameraLookAt;

    public const float RimScoreRadius = ArcAcademyLayout.RimScoreRadius;
    public const float SpawnLateralJitter = ArcAcademyLayout.SpawnLateralJitter;

    public static bool IsOutOfBounds(Vector3 worldPosition) =>
        ArcAcademyLayout.IsOutOfBounds(worldPosition);
}
