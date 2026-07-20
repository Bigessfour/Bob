using Unity.MLAgents;
using UnityEngine;

/// <summary>
/// Reads ML-Agents <c>distance_scale</c> and applies regulation-distance curriculum:
/// Bob stays on the free-throw line; the hoop moves closer on Z (never spawn offset like v4.5).
/// </summary>
public static class BobCurriculum
{
    public const string DistanceScaleParameter = "distance_scale";

    /// <summary>1 = regulation; 0.65 = first lesson (~35% shorter horizontal shot).</summary>
    public static float CurrentDistanceScale { get; private set; } = 1f;

    /// <summary>Call at episode prepare — before spawn and before the first observation.</summary>
    public static void RefreshFromEnvironment(MovableHoop hoop)
    {
        CurrentDistanceScale = 1f;
        if (Academy.IsInitialized)
        {
            CurrentDistanceScale = Academy.Instance.EnvironmentParameters.GetWithDefault(
                DistanceScaleParameter, 1f);
        }

        CurrentDistanceScale = Mathf.Clamp(CurrentDistanceScale, 0.5f, 1f);
        hoop?.ApplyCurriculumDistance(CurrentDistanceScale);
    }
}
