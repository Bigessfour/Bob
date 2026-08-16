using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// PlayMode: a descending basketball through <see cref="HoopScoreZone"/> increments
/// <see cref="BobTrainingStats.BasketballPoints"/>. This is the real scoring path,
/// not <c>RecordBasketballPoint()</c> called in isolation.
/// </summary>
public class BobScoreZonePhysicsPlayModeTest
{
    [UnityTest]
    public IEnumerator DescendingBall_ThroughScoreZone_IncrementsBasketballPoints()
    {
        var statsGo = new GameObject("BobTrainingStats_PhysicsTest");
        var stats = statsGo.AddComponent<BobTrainingStats>();
        yield return null;

        var bobGo = new GameObject("Bob_PhysicsTest");
        var bobRb = bobGo.AddComponent<Rigidbody>();
        bobRb.isKinematic = true;
        bobRb.useGravity = false;
        var agent = bobGo.AddComponent<BobAgent>();
        agent.SuppressEpisodeEnd = true;

        var hoopGo = new GameObject("HoopScoreZone_PhysicsTest");
        hoopGo.transform.position = new Vector3(0f, 3f, 0f);
        var capsule = hoopGo.AddComponent<CapsuleCollider>();
        capsule.isTrigger = true;
        capsule.direction = 1;
        capsule.radius = 0.45f;
        capsule.height = 0.6f;
        var zone = hoopGo.AddComponent<HoopScoreZone>();
        zone.minDownwardSpeed = 0.5f;

        var ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballGo.name = "Basketball_PhysicsTest";
        ballGo.transform.position = new Vector3(0f, 3.35f, 0f);
        ballGo.transform.localScale = Vector3.one * 0.24f;
        var ballRb = ballGo.GetComponent<Rigidbody>();
        ballRb.useGravity = false;
        ballRb.isKinematic = false;
        ballRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        var marker = ballGo.AddComponent<SimpleBasketball>();
        marker.Wire(agent);

        int before = stats.BasketballPoints;
        ballRb.linearVelocity = new Vector3(0f, -4f, 0f);

        float timeout = Time.time + 2f;
        while (Time.time < timeout && stats.BasketballPoints == before)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.AreEqual(
            before + 1,
            stats.BasketballPoints,
            "HoopScoreZone must record +1 when a wired basketball falls through the cylinder.");

        Object.Destroy(ballGo);
        Object.Destroy(hoopGo);
        Object.Destroy(bobGo);
        Object.Destroy(statsGo);
    }
}
