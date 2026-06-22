using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Appends per-iteration session metrics to summaries/bob_session.csv for offline plotting.
/// </summary>
public static class BobTrainingSessionLog
{
    public const string FileName = "bob_session.csv";

    public static string GetLogPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "summaries", FileName));
    }

    public static void Append(BobTrainingStats stats, bool previousEpisodeScored)
    {
        if (stats == null || !Application.isPlaying)
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
                    "timestamp,iteration,scored,basketball_points,session_success_pct,rolling_success_pct,rolling_arc_quality,net_rl");
            }

            writer.WriteLine(string.Join(",",
                DateTime.UtcNow.ToString("o"),
                stats.TotalIterations,
                previousEpisodeScored ? 1 : 0,
                stats.BasketballPoints,
                (stats.SessionSuccessRate * 100f).ToString("F2"),
                (stats.RollingSuccessRate * 100f).ToString("F2"),
                (stats.RollingAverageArcQuality * 100f).ToString("F2"),
                stats.NetSessionReward.ToString("F3")));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"BobTrainingSessionLog: could not append session row — {ex.Message}");
        }
    }
}
