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
    public const string TrainingBaysName = "TrainingBays";
    public const string TrainingBaysBackName = "TrainingBaysBack";
    public const string SpawnPadName = "SpawnPad";
    public const string HoopName = "Hoop";
    public const string RimName = "Rim";
    public const string ScoreZoneName = "ScoreZone";
    public const string MountainWindowName = "MountainWindow";
    public const string TrajectoryVisualsName = "TrajectoryVisuals";
    public const string DecorativeHoopsName = "DecorativeHoops";
    public const string LightingRigName = "LightingRig";
    public const string ReflectionProbeName = "ReflectionProbe";

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
    public const float ThreePointArcRadius = 6.75f;
    public const float CenterCircleRadius = 1.8f;

    /// <summary>Distance hash marks measured from baseline toward spawn (meters).</summary>
    public static readonly float[] DistanceMarkOffsetsFromBaseline = { 3f, 6f, 9f };

    public static readonly Vector3 HoopRootDefaultPosition = new(0f, 0f, -9f);
    public static readonly Vector3 RimLocalDefaultPosition = new(0f, 3.05f, 0.2f);

    public static readonly Vector3 SpawnPadPosition = new(0f, 0.15f, FreeThrowLineZ);
    public static readonly Vector3 SpawnPadScale = new(2.8f, 0.4f, 2f);
    public static readonly Vector3 BobSpawnOffset = new(0f, 0.55f, 0f);

    public const int TrainingBayCount = 6;
    public const float TrainingBayWidth = 2.6f;
    public const float TrainingBayDepth = 3.4f;
    public const float TrainingBayWallHeight = 3.2f;
    public const float TrainingBayStartZ = -12f;
    public const float TrainingBaySpacing = 3.6f;
    public const float TrainingBayX = -6.2f;

    public const int TrainingBayBackCount = 8;
    public const float TrainingBayBackZ = -14.5f;
    public const float TrainingBayBackSpacing = 2.1f;

    public static readonly Vector3[] DecorativeHoopRootPositions =
    {
        new(-4.5f, 0f, -5.5f),
        new(4.8f, 0f, -6f),
        new(1.5f, 0f, -4f),
    };

    public const int TrajectoryArcCount = 3;
    public const int TrajectoryArcSegments = 32;
    public const float TrajectoryArcHeight = 4.5f;

    public static Vector3 BobSpawnPosition => SpawnPadPosition + BobSpawnOffset;

    public static Vector3 MainRimWorldPosition =>
        RimWorldPosition(HoopRootDefaultPosition, RimLocalDefaultPosition);

    /// <summary>Hero camera angle aligned with portfolio reference.</summary>
    public static readonly Vector3 CameraPosition = new(9f, 6.8f, 11f);
    public static readonly Vector3 CameraLookAt = new(0f, 2.8f, -3f);

    public const float FloorGlossiness = 0.75f;
    public const float PlatformEmissiveIntensity = 1.4f;
    public const float ArcLineEmissiveIntensity = 1.2f;
    public const float BobGlowIntensity = 0.85f;
    public const float LabelBobSize = 1.2f;
    public const float LabelAcademySize = 0.45f;

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

    public static Vector3 DecorativeRimWorldPosition(int index)
    {
        if (index < 0 || index >= DecorativeHoopRootPositions.Length)
        {
            return MainRimWorldPosition;
        }

        var root = DecorativeHoopRootPositions[index];
        return root + new Vector3(0f, RimLocalDefaultPosition.y, RimLocalDefaultPosition.z);
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
