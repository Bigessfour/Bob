using UnityEngine;

/// <summary>
/// Arc Academy warehouse training court layout — shared by scene builder, manager, and agent.
/// Unity units ≈ meters. Scene file remains Assets/Scenes/BobTraining.unity for CLI compatibility.
/// </summary>
public static class ArcAcademyLayout
{
    public const string ArenaName = "TrainingArena";
    public const string WarehouseShellName = "WarehouseShell";
    public const string CourtFloorName = "CourtFloor";
    public const string CourtMarkingsName = "CourtMarkings";
    public const string DistanceMarkingsName = "DistanceMarkings";
    public const string SpawnPadName = "SpawnPad";
    public const string HoopName = "Hoop";
    public const string RimName = "Rim";
    public const string ScoreZoneName = "ScoreZone";

    public const float CourtHalfWidth = 7f;
    public const float CourtNearZ = 10f;
    public const float CourtFarZ = -14f;
    public const float ShellHalfWidth = 9f;
    public const float ShellNearZ = 12f;
    public const float ShellFarZ = -16f;
    public const float CeilingHeight = 9f;

    public const float FreeThrowLineZ = 2f;
    public const float KeyHalfWidth = 1.83f;
    public const float KeyDepthFromBaseline = 5.8f;

    /// <summary>Distance hash marks measured from baseline toward spawn (meters).</summary>
    public static readonly float[] DistanceMarkOffsetsFromBaseline = { 3f, 6f, 9f };

    public static readonly Vector3 HoopRootDefaultPosition = new(0f, 0f, -9f);
    public static readonly Vector3 RimLocalDefaultPosition = new(0f, 3.05f, 0.2f);

    public static readonly Vector3 SpawnPadPosition = new(0f, 0.15f, FreeThrowLineZ);
    public static readonly Vector3 SpawnPadScale = new(1.8f, 0.3f, 1.2f);
    public static readonly Vector3 BobSpawnOffset = new(0f, 0.55f, 0f);

    public static Vector3 BobSpawnPosition => SpawnPadPosition + BobSpawnOffset;

    public static readonly Vector3 CameraPosition = new(2.5f, 5.5f, 7f);
    public static readonly Vector3 CameraLookAt = new(0f, 3.2f, -6f);

    public const float RimScoreRadius = 0.45f;
    public const float SpawnLateralJitter = 0.35f;
    public const float SwishSpeedThreshold = 2.5f;

    public const float MaxHoopOffsetX = 1.2f;
    public const float MaxHoopOffsetZ = 0.8f;
    public const float MinRimHeight = 2.85f;
    public const float MaxRimHeight = 3.25f;

    public const float IdealArcApexRatio = 0.55f;
    public const float ArcQualityRewardScale = 0.1f;

    public static Vector3 RimWorldPosition(Vector3 hoopRootPosition, Vector3 rimLocalPosition)
    {
        return hoopRootPosition + rimLocalPosition;
    }

    public static bool IsOutOfBounds(Vector3 worldPosition)
    {
        return worldPosition.y < 0.35f
               || Mathf.Abs(worldPosition.x) > ShellHalfWidth + 0.5f
               || worldPosition.z > ShellNearZ + 0.5f
               || worldPosition.z < ShellFarZ - 0.5f;
    }

    public static float GetStageHoopOffsetScale(int stage)
    {
        return Mathf.Clamp(stage / 5f, 0.2f, 1f);
    }
}
