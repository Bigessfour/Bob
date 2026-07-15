using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode: made basket increments BasketballPoints (canonical scoreboard metric).
/// Requires com.unity.test-framework; runs via Window → General → Test Runner.
/// </summary>
public class BobScoreIncrementPlayModeTest
{
    [UnityTest]
    public IEnumerator MadeBasket_IncrementsBasketballPoints()
    {
        var existing = Object.FindAnyObjectByType<BobTrainingStats>();
        BobTrainingStats stats = existing;
        if (stats == null)
        {
            var go = new GameObject("BobTrainingStats_Test");
            stats = go.AddComponent<BobTrainingStats>();
        }

        yield return null;

        int before = stats.BasketballPoints;
        stats.RecordBasketballPoint();
        Assert.AreEqual(before + 1, stats.BasketballPoints,
            "RecordBasketballPoint must increment BasketballPoints for HUD/CSV.");

        if (existing == null)
        {
            Object.Destroy(stats.gameObject);
        }
    }
}
