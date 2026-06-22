using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Session training metrics for scoreboards and wall HUD (iterations, RL, basketball points, arc quality).
/// </summary>
public class BobTrainingStats : MonoBehaviour
{
    public const int DefaultRollingWindow = 24;

    public static BobTrainingStats Instance { get; private set; }

    public int TotalIterations { get; private set; }

    public float TotalRewards { get; private set; }

    /// <summary>Sum of negative RL reward magnitudes (shown as a positive "penalties" total).</summary>
    public float TotalPenalties { get; private set; }

    /// <summary>Made free throws — one basketball point each (separate from RL reward).</summary>
    public int BasketballPoints { get; private set; }

    public float LastEpisodeNetReward { get; private set; }

    public float CurrentEpisodeNetReward { get; private set; }

    public float LastEpisodePeakArcQuality { get; private set; }

    public float NetSessionReward => TotalRewards - TotalPenalties;

    public float SessionSuccessRate =>
        TotalIterations > 0 ? BasketballPoints / (float)TotalIterations : 0f;

    public float RollingSuccessRate => ComputeRollingRate(DefaultRollingWindow);

    public float RollingAverageArcQuality => ComputeRollingArcQuality(DefaultRollingWindow);

    private readonly Queue<float> m_RecentOutcomes = new();
    private readonly Queue<float> m_RecentArcQuality = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        BobPhysicsLayers.EnsureCollisionMatrix();
        ResetSession();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ResetSession()
    {
        TotalIterations = 0;
        TotalRewards = 0f;
        TotalPenalties = 0f;
        BasketballPoints = 0;
        LastEpisodeNetReward = 0f;
        CurrentEpisodeNetReward = 0f;
        LastEpisodePeakArcQuality = 0f;
        m_RecentOutcomes.Clear();
        m_RecentArcQuality.Clear();
    }

    /// <param name="previousEpisodeScored">Whether the episode that just ended scored a basket.</param>
    public void BeginIteration(bool previousEpisodeScored)
    {
        if (TotalIterations > 0)
        {
            RecordOutcome(previousEpisodeScored);
        }

        LastEpisodeNetReward = CurrentEpisodeNetReward;
        CurrentEpisodeNetReward = 0f;
        TotalIterations++;
        BobTrainingSessionLog.Append(this, previousEpisodeScored);
    }

    public void RecordReward(float amount)
    {
        if (amount >= 0f)
        {
            TotalRewards += amount;
        }
        else
        {
            TotalPenalties += -amount;
        }

        CurrentEpisodeNetReward += amount;
    }

    public void RecordBasketballPoint()
    {
        BasketballPoints++;
    }

    /// <summary>Call at episode boundary with peak arc quality (0–1) for the shot that just finished.</summary>
    public void FlushEpisodeArcQuality(float peakQuality)
    {
        LastEpisodePeakArcQuality = Mathf.Clamp01(peakQuality);
        m_RecentArcQuality.Enqueue(LastEpisodePeakArcQuality);
        while (m_RecentArcQuality.Count > DefaultRollingWindow * 4)
        {
            m_RecentArcQuality.Dequeue();
        }
    }

    public IReadOnlyList<float> GetRecentOutcomes(int maxCount)
    {
        return BuildRollingSeries(m_RecentOutcomes, maxCount);
    }

    public IReadOnlyList<float> GetRecentArcQuality(int maxCount)
    {
        return BuildRollingSeries(m_RecentArcQuality, maxCount);
    }

    public float ComputeRollingRate(int window)
    {
        if (m_RecentOutcomes.Count == 0)
        {
            return 0f;
        }

        int count = Mathf.Min(window, m_RecentOutcomes.Count);
        float sum = 0f;
        int skip = m_RecentOutcomes.Count - count;
        int index = 0;
        foreach (float outcome in m_RecentOutcomes)
        {
            if (index >= skip)
            {
                sum += outcome;
            }

            index++;
        }

        return sum / count;
    }

    public float ComputeRollingArcQuality(int window)
    {
        if (m_RecentArcQuality.Count == 0)
        {
            return 0f;
        }

        int count = Mathf.Min(window, m_RecentArcQuality.Count);
        float sum = 0f;
        int skip = m_RecentArcQuality.Count - count;
        int index = 0;
        foreach (float quality in m_RecentArcQuality)
        {
            if (index >= skip)
            {
                sum += quality;
            }

            index++;
        }

        return sum / count;
    }

    private static IReadOnlyList<float> BuildRollingSeries(Queue<float> source, int maxCount)
    {
        if (source.Count == 0)
        {
            return System.Array.Empty<float>();
        }

        var list = new List<float>(source);
        if (list.Count > maxCount)
        {
            list = list.GetRange(list.Count - maxCount, maxCount);
        }

        float rolling = 0f;
        var rates = new List<float>(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            rolling += list[i];
            rates.Add(rolling / (i + 1));
        }

        return rates;
    }

    private void RecordOutcome(bool scored)
    {
        m_RecentOutcomes.Enqueue(scored ? 1f : 0f);
        while (m_RecentOutcomes.Count > DefaultRollingWindow * 4)
        {
            m_RecentOutcomes.Dequeue();
        }
    }
}
