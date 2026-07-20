using UnityEngine;

/// <summary>
/// Analytic free-throw launch: high-arc world impulse so the ball peaks above the rim
/// and descends through the hoop (swish), not a flat backboard push.
/// </summary>
public static class BobSwishLaunchSolver
{
    /// <summary>Realistic free-throw lob (~58°) — apex above rim, descending entry.</summary>
    public const float PreferredLaunchAngleDegrees = 58f;

    /// <summary>E / Shift nudge on launch angle (degrees).</summary>
    public const float AngleNudgeDegrees = 4f;

    /// <summary>
    /// Aim above rim center so the ball's geometric center clears the front lip
    /// (ball radius ≈ 0.12 m). Vacuum aim-at-center clips the iron after damping.
    /// </summary>
    public const float AimAboveRimMeters = 0.18f;

    /// <summary>
    /// Extra depth past rim center toward the backboard — short vacuum solutions
    /// become front-rim misses once linearDamping bleeds speed.
    /// </summary>
    public const float AimPastRimMeters = 0.10f;

    /// <summary>Speed boost to offset basketball linearDamping on the way to the rim.</summary>
    public const float DampingCompensation = 1.18f;

    /// <summary>
    /// Solves launch velocity for a projectile under <see cref="Physics.gravity"/> that passes
    /// through the rim at <paramref name="launchAngleDegrees"/> (high-arc root).
    /// Returns world-space impulse for <see cref="ForceMode.Impulse"/> (= mass × Δv).
    /// </summary>
    public static bool TryComputeWorldImpulse(
        Vector3 launchPos,
        Vector3 rimPos,
        float mass,
        float launchAngleDegrees,
        out Vector3 worldImpulse)
    {
        worldImpulse = Vector3.zero;

        Vector3 toRimFlat = new Vector3(rimPos.x - launchPos.x, 0f, rimPos.z - launchPos.z);
        if (toRimFlat.sqrMagnitude < 0.0025f)
        {
            return false;
        }

        Vector3 horizDir = toRimFlat.normalized;
        Vector3 target = rimPos
            + Vector3.up * AimAboveRimMeters
            + horizDir * AimPastRimMeters;
        Vector3 toTarget = target - launchPos;
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
        float dx = flat.magnitude;
        float dy = toTarget.y;
        if (dx < 0.05f)
        {
            return false;
        }

        float g = Mathf.Abs(Physics.gravity.y);
        if (g < 0.01f)
        {
            return false;
        }

        float angle = launchAngleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        float tan = Mathf.Tan(angle);

        // v² = g·dx² / (2·cos²·(dx·tanθ − dy)) — requires dx·tanθ > dy (angle steep enough).
        float riseTerm = dx * tan - dy;
        float denom = 2f * cos * cos * riseTerm;
        if (denom <= 0.05f)
        {
            return false;
        }

        float speedSq = g * dx * dx / denom;
        if (speedSq <= 0f || float.IsNaN(speedSq) || float.IsInfinity(speedSq))
        {
            return false;
        }

        float speed = Mathf.Sqrt(speedSq) * DampingCompensation;
        Vector3 flightDir = flat / dx;
        Vector3 velocity = flightDir * (speed * cos) + Vector3.up * (speed * sin);
        worldImpulse = velocity * Mathf.Max(mass, 0.01f);
        return true;
    }

    /// <summary>Elevation angle (degrees) of a world impulse in the launch vertical plane.</summary>
    public static float LaunchAngleDegreesFromImpulse(Vector3 worldImpulse)
    {
        Vector3 flat = new Vector3(worldImpulse.x, 0f, worldImpulse.z);
        float horiz = flat.magnitude;
        if (horiz < 0.01f)
        {
            return worldImpulse.y >= 0f ? 90f : -90f;
        }

        return Mathf.Atan2(worldImpulse.y, horiz) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Maps a world impulse back into Bob's continuous actions
    /// (local impulse via spawn facing, then undo fx/fy/fz scales + biases).
    /// </summary>
    public static void WorldImpulseToActions(
        Vector3 worldImpulse,
        Quaternion spawnRotation,
        float lateralForceScale,
        float verticalForceScale,
        float verticalBias,
        float forwardForceScale,
        float forwardBias,
        out float ax,
        out float ay,
        out float az)
    {
        Vector3 local = Quaternion.Inverse(spawnRotation) * worldImpulse;
        ax = Mathf.Clamp(local.x / Mathf.Max(lateralForceScale, 0.01f), -1f, 1f);
        ay = Mathf.Clamp(
            (local.y - verticalBias) / Mathf.Max(verticalForceScale, 0.01f), -1f, 1f);
        az = Mathf.Clamp(
            (local.z - forwardBias) / Mathf.Max(forwardForceScale, 0.01f), -1f, 1f);
    }

    /// <summary>
    /// Maps a world-space residual impulse back into Bob's continuous actions
    /// (local residual via spawn facing, divided by residual scales).
    /// </summary>
    public static void WorldResidualToActions(
        Vector3 residualWorld,
        Quaternion spawnRotation,
        float lateralScale,
        float verticalScale,
        float forwardScale,
        out float ax,
        out float ay,
        out float az)
    {
        Vector3 local = Quaternion.Inverse(spawnRotation) * residualWorld;
        ax = Mathf.Clamp(local.x / Mathf.Max(lateralScale, 0.01f), -1f, 1f);
        ay = Mathf.Clamp(local.y / Mathf.Max(verticalScale, 0.01f), -1f, 1f);
        az = Mathf.Clamp(local.z / Mathf.Max(forwardScale, 0.01f), -1f, 1f);
    }
}
