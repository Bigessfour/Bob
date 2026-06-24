using UnityEngine;

/// <summary>
/// Trigger volume at the rim — awards a made basket when Bob or the basketball enters while moving downward.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoopScoreZone : MonoBehaviour
{
    [Tooltip("Maximum upward speed (m/s) allowed for a made basket.")]
    public float minDownwardSpeed = 0.5f;

    [SerializeField] private HoopRimContact rimContact;
    [SerializeField] private HoopSwishVfx swishVfx;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (rimContact == null)
        {
            rimContact = GetComponentInParent<HoopRimContact>();
        }

        if (swishVfx == null && transform.parent != null)
        {
            swishVfx = transform.parent.GetComponentInChildren<HoopSwishVfx>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TryScoreBasketball(other))
        {
            return;
        }

        TryScoreBob(other);
    }

    private bool TryScoreBasketball(Collider other)
    {
        if (!other.TryGetComponent(out SimpleBasketball basketball) || basketball.Owner == null)
        {
            return false;
        }

        var rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.y > minDownwardSpeed)
        {
            return true;
        }

        bool rimHit = rimContact != null && rimContact.HadRecentProjectileContact;
        bool swish = !rimHit
                     && rb != null
                     && rb.linearVelocity.magnitude <= ArcAcademyLayout.SwishSpeedThreshold;

        if (swish)
        {
            swishVfx?.PlaySwish();
        }

        var agent = basketball.Owner;
        RecordBasketballPointAndNotify(agent, swish);

        return true;
    }

    private void TryScoreBob(Collider other)
    {
        var agent = other.GetComponent<BobAgent>();
        if (agent == null || agent.ProjectileBody != null)
        {
            return;
        }

        var rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.y > minDownwardSpeed)
        {
            return;
        }

        bool rimHit = rimContact != null && rimContact.HadRecentProjectileContact;
        bool swish = !rimHit
                     && rb != null
                     && rb.linearVelocity.magnitude <= ArcAcademyLayout.SwishSpeedThreshold;

        if (swish)
        {
            swishVfx?.PlaySwish();
        }

        RecordBasketballPointAndNotify(agent, swish);
    }

    /// <summary>
    /// Records the canonical basketball point (for scoreboard, success rate, CSV) on every make,
    /// independent of whether the rich ArcAcademyManager feedback path is present.
    /// This makes scoring robust for the minimal free-throw trainer path and any fallback.
    /// </summary>
    private void RecordBasketballPointAndNotify(BobAgent agent, bool swish)
    {
        // Authoritative make detection site: always increment the basketball score metric.
        // Success rate, HUDs, and session log derive from this (see BobTrainingStats + what-finished-looks-like.md).
        BobTrainingStats.Instance?.RecordBasketballPoint();

        if (ArcAcademyManager.Instance != null)
        {
            ArcAcademyManager.Instance.NotifyMadeBasket(agent, swish);
        }
        else
        {
            agent.RegisterMadeShot(swish);
        }
    }
}
