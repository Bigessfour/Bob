using UnityEngine;

/// <summary>
/// Trigger volume at the rim — awards a made basket when Bob enters while moving toward the hoop.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoopScoreZone : MonoBehaviour
{
    [Tooltip("Minimum downward speed (m/s) to count as a shot through the hoop, not a fly-by from below.")]
    public float minDownwardSpeed = 0.5f;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var agent = other.GetComponent<BobAgent>();
        if (agent == null)
        {
            return;
        }

        var rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.y > minDownwardSpeed)
        {
            return;
        }

        agent.RegisterMadeShot();
    }
}
