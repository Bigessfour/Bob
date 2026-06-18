using UnityEngine;

/// <summary>
/// Trigger volume at the rim — awards a made basket when Bob enters while moving toward the hoop.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoopScoreZone : MonoBehaviour
{
    [Tooltip("Maximum upward speed (m/s) allowed for a made basket.")]
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

        bool swish = rb != null && rb.linearVelocity.magnitude <= ArcAcademyLayout.SwishSpeedThreshold;
        agent.RegisterMadeShot(swish);
    }
}
