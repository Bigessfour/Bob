using System;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Per-shot launch + resolution log for offline run review (<c>summaries/bob_shots.csv</c>).
/// Captures actions, impulse, launch angle, solver match, and descending-near-rim flag.
/// </summary>
public static class BobShotActionLog
{
    public const string FileName = "bob_shots.csv";

    private static PendingShot s_Pending;

    private struct PendingShot
    {
        public bool Active;
        public int Iteration;
        public float Ax;
        public float Ay;
        public float Az;
        public float Fx;
        public float Fy;
        public float Fz;
        public float TowardHoopDot;
        public bool TrainingConnected;
        public string TimestampUtc;
        public float LaunchAngleDeg;
        public float SolverMatch;
        public bool DescendingNearRim;
    }

    public static string GetLogPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "summaries", FileName));
    }

    public static void RecordLaunch(
        int iteration,
        float ax,
        float ay,
        float az,
        Vector3 impulse,
        float towardHoopDot,
        bool trainingConnected,
        float launchAngleDeg = 0f,
        float solverMatch = 0f)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        s_Pending = new PendingShot
        {
            Active = true,
            Iteration = iteration,
            Ax = ax,
            Ay = ay,
            Az = az,
            Fx = impulse.x,
            Fy = impulse.y,
            Fz = impulse.z,
            TowardHoopDot = towardHoopDot,
            TrainingConnected = trainingConnected,
            TimestampUtc = DateTime.UtcNow.ToString("o"),
            LaunchAngleDeg = launchAngleDeg,
            SolverMatch = solverMatch,
            DescendingNearRim = false,
        };

        Debug.Log(
            $"BOB_SHOT: ep={iteration} a=({ax:F2},{ay:F2},{az:F2}) " +
            $"impulse=({impulse.x:F1},{impulse.y:F1},{impulse.z:F1}) " +
            $"angle={launchAngleDeg:F1}° match={solverMatch:F2} " +
            $"toward={towardHoopDot:F2} training={(trainingConnected ? 1 : 0)}");
    }

    /// <summary>Call while the ball is near the rim and descending (ideal entry kinematics).</summary>
    public static void NoteDescendingNearRim()
    {
        if (!s_Pending.Active)
        {
            return;
        }

        s_Pending.DescendingNearRim = true;
    }

    /// <summary>Flush the pending launch with episode outcome (call when the shot resolves).</summary>
    public static void RecordResolution(bool scored, float episodeNetReward, float peakArcQuality, string endReason)
    {
        if (!s_Pending.Active || !Application.isPlaying)
        {
            return;
        }

        try
        {
            string path = GetLogPath();
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            bool needsHeader = !File.Exists(path) || new FileInfo(path).Length == 0;
            using var writer = new StreamWriter(path, append: true);
            if (needsHeader)
            {
                writer.WriteLine(
                    "timestamp,iteration,training_connected,ax,ay,az,fx,fy,fz,toward_hoop_dot," +
                    "scored,episode_net_rl,peak_arc_pct,end_reason," +
                    "launch_angle_deg,solver_match,descending_near_rim");
            }

            var inv = CultureInfo.InvariantCulture;
            writer.WriteLine(string.Join(",",
                s_Pending.TimestampUtc,
                s_Pending.Iteration.ToString(inv),
                s_Pending.TrainingConnected ? "1" : "0",
                s_Pending.Ax.ToString("F4", inv),
                s_Pending.Ay.ToString("F4", inv),
                s_Pending.Az.ToString("F4", inv),
                s_Pending.Fx.ToString("F3", inv),
                s_Pending.Fy.ToString("F3", inv),
                s_Pending.Fz.ToString("F3", inv),
                s_Pending.TowardHoopDot.ToString("F3", inv),
                scored ? "1" : "0",
                episodeNetReward.ToString("F3", inv),
                (peakArcQuality * 100f).ToString("F2", inv),
                SanitizeCsv(endReason),
                s_Pending.LaunchAngleDeg.ToString("F1", inv),
                s_Pending.SolverMatch.ToString("F3", inv),
                s_Pending.DescendingNearRim ? "1" : "0"));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"BobShotActionLog: could not append shot row — {ex.Message}");
        }
        finally
        {
            s_Pending.Active = false;
        }
    }

    private static string SanitizeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "unknown";
        }

        return value.Replace(',', '_').Replace('\n', '_').Replace('\r', '_');
    }
}
