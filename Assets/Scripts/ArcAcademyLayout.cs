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
    public const string SpawnPadName = "SpawnPad";
    public const string BallSpawnPointName = "BallSpawnPoint";
    public const string ScorePopupName = "ScorePopup";
    public const string TrainingScoreboardName = "TrainingScoreboard";
    public const string HoopName = "Hoop";
    public const string RimName = "Rim";
    public const string ScoreZoneName = "ScoreZone";
    public const string MountainWindowName = "MountainWindow";
    public const string TrajectoryVisualsName = "TrajectoryVisuals";
    public const string DecorativeHoopsName = "DecorativeHoops";
    public const string SpawnPadBrandingName = "SpawnPadBranding";
    public const string LightingRigName = "LightingRig";
    public const string ReflectionProbeName = "ReflectionProbe";
    public const string ReflectionProbeWindowName = "ReflectionProbe_Window";
    public const string SignageArcAcademyName = "Signage_ArcAcademy";
    public const string FloorDecalsName = "FloorDecals";
    public const string HdrpVolumeName = "HdrpVolume";
    public const string AdaptiveProbeVolumeName = "AdaptiveProbeVolume";
    public const string HdrpSkyRigName = "HdrpSkyRig";
    public const string RoboticLauncherPrefix = "RoboticLauncher";
    public const string PortableHoopStandName = "PortableHoopStand";

    /// <summary>Bay indices sampled for portfolio trajectory arcs (Example.jpg).</summary>
    public static readonly int[] TrajectoryBaySampleIndices = { 0, 2, 4, 6 };

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

    public static readonly Vector3 HoopRootDefaultPosition = new(0f, 0f, -5.5f);
    /// <summary>World-space rim height offset when estimating positions from hoop root (legacy helpers).</summary>
    public static readonly Vector3 RimLocalDefaultPosition = new(0f, 3.05f, 0.2f);

    /// <summary>Rim attached to backboard on HoopHead — must stay parented under Backboard/HoopHead.</summary>
    public static readonly Vector3 RimLocalOnHoopHead = new(0f, 0.05f, 0.35f);

    /// <summary>Backboard center on HoopHead (matches scene builder).</summary>
    public static readonly Vector3 BackboardLocalOnHoopHead = new(0f, 0.55f, 0.08f);

    /// <summary>Fixed HoopHead pose on Hoop root when robotic arm is disabled for training.</summary>
    public static readonly Vector3 StationaryHoopHeadLocalPosition = new(0f, 3f, 0.2f);

    /// <summary>
    /// Central elevated black "Bob" platform (Example.jpg hero element).
    /// Repositioned toward visual center of orange court for panoramic composition.
    /// SpawnPad* names kept for full compatibility with manager, pulse, trajectory, agent.
    /// </summary>
    public static readonly Vector3 SpawnPadPosition = new(0f, 0.28f, -1.8f);
    public static readonly Vector3 SpawnPadScale = new(4.0f, 0.55f, 2.8f);
    public static readonly Vector3 BobSpawnOffset = new(0f, 0.58f, 0f);

    public const int TrainingBayCount = 8;
    public const float TrainingBayWidth = 2.6f;
    public const float TrainingBayDepth = 3.4f;
    public const float TrainingBayWallHeight = 3.2f;

    /// <summary>Low white/black partitions between bays (matches Example.jpg style, not full walls).</summary>
    public const float BayPartitionHeight = 1.9f;

    /// <summary>Large panoramic mountain windows (rear/side) using mountain_backdrop texture.</summary>
    public const float MountainWindowWidth = 17.5f;
    public const float MountainWindowHeight = 6.2f;
    public const float MountainWindowY = 4.2f;

    /// <summary>Ceiling density — lab-readable (2×3), not full photoreal warehouse grid.</summary>
    public const int CeilingTrussCount = 5;
    public const int CeilingLightRows = 2;
    public const int CeilingLightColsPerRow = 3;

    /// <summary>Eight modular shooting bays around back and right perimeter (Example.jpg). Low partitions separate bays.</summary>
    public static readonly Vector3[] TrainingBayPositions =
    {
        new(-7f, 0f, -13f),
        new(-4.2f, 0f, -14.5f),
        new(-1.4f, 0f, -14.5f),
        new(1.4f, 0f, -14.5f),
        new(4.2f, 0f, -14.5f),
        new(7f, 0f, -13f),
        new(7.2f, 0f, -8f),
        new(7.2f, 0f, -4f),
    };

    public static readonly bool[] TrainingBayFaceNegativeZ =
    {
        true, true, true, true, true, true, false, false,
    };

    /// <summary>Extra mid-court display stations (bays provide the primary 8 hoops).</summary>
    public static readonly Vector3[] DecorativeHoopRootPositions = { };

    public const int TrajectoryArcCount = 3;
    public const int TrajectoryArcSegments = 32;
    public const float TrajectoryArcHeight = 4.5f;

    public static Vector3 BobSpawnPosition => SpawnPadPosition + BobSpawnOffset;

    public static Vector3 MainRimWorldPosition =>
        RimWorldPosition(HoopRootDefaultPosition, RimLocalDefaultPosition);

    /// <summary>Court-level hero shot — Bob at the line, backboard + rim centered ahead.</summary>
    public static readonly Vector3 CameraPosition = new(2.0f, 1.55f, 0.6f);
    public static readonly Vector3 CameraLookAt = new(0f, 2.85f, -5.2f);
    public const float CameraFieldOfView = 48f;

    public static readonly Vector3 FloorDecalEntrancePosition = new(0f, 0.04f, 8f);
    public static readonly Vector3 EntranceCameraPosition = new(1.6f, 2.0f, 3.2f);
    public static readonly Vector3 EntranceCameraLookAt = new(0f, 2.2f, -3.5f);

    public const float FloorGlossiness = 0.42f;
    public const float PlatformEmissiveIntensity = 0.65f;
    public const float ArcLineEmissiveIntensity = 0.45f;
    public const float BobGlowIntensity = 0.55f;
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
        return root + new Vector3(0f, 2.2f, RimLocalDefaultPosition.z);
    }

    public static Vector3 BayRimWorldPosition(int bayIndex)
    {
        if (bayIndex < 0 || bayIndex >= TrainingBayCount)
        {
            return MainRimWorldPosition;
        }

        var center = TrainingBayPositions[bayIndex];
        bool faceNegativeZ = TrainingBayFaceNegativeZ[bayIndex];
        float depth = TrainingBayDepth;
        float rimZ = faceNegativeZ ? 0.25f : depth - 0.25f;
        return center + new Vector3(0f, 2.2f, rimZ);
    }

    public static Vector3[] BuildTrajectoryArcTargets(Vector3 mainRimWorld)
    {
        int decorativeCount = DecorativeHoopRootPositions.Length;
        int baySampleCount = TrajectoryBaySampleIndices.Length;
        var targets = new Vector3[1 + decorativeCount + baySampleCount];
        int index = 0;
        targets[index++] = mainRimWorld;

        for (int i = 0; i < decorativeCount; i++)
        {
            targets[index++] = DecorativeRimWorldPosition(i);
        }

        for (int i = 0; i < baySampleCount; i++)
        {
            targets[index++] = BayRimWorldPosition(TrajectoryBaySampleIndices[i]);
        }

        return targets;
    }

    public static int ExpectedPortableHoopStandCount =>
        TrainingBayCount + DecorativeHoopRootPositions.Length + 1;

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
