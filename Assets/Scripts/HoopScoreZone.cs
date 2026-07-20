using UnityEngine;

/// <summary>
/// Trigger volume at the rim — awards a made basket only when the ball (or Bob)
/// enters while falling top → bottom through the hoop cylinder.
/// Sideways skim, upward poke, or rim-out without full passage = no point.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoopScoreZone : MonoBehaviour
{
    [Tooltip("Minimum downward speed (m/s) required — ball must be falling through the hoop.")]
    public float minDownwardSpeed = 0.5f;

    [SerializeField] private HoopRimContact rimContact;
    [SerializeField] private HoopSwishVfx swishVfx;
    [SerializeField] private HoopNetPhysics netPhysics;

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

        if (netPhysics == null && transform.parent != null)
        {
            netPhysics = transform.parent.GetComponentInChildren<HoopNetPhysics>();
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

    /// <summary>
    /// True only when velocity is clearly downward (falling through the hoop toward the floor).
    /// </summary>
    private bool IsFallingThroughHoop(Rigidbody rb)
    {
        if (rb == null)
        {
            return false;
        }

        // World Y must be negative enough: top → bottom through the cylinder.
        return rb.linearVelocity.y <= -minDownwardSpeed;
    }

    private bool TryScoreBasketball(Collider other)
    {
        if (!other.TryGetComponent(out SimpleBasketball basketball) || basketball.Owner == null)
        {
            return false;
        }

        var rb = other.attachedRigidbody;
        // Consumed this collider either way so Bob path is not double-tried.
        if (!IsFallingThroughHoop(rb))
        {
            return true;
        }

        bool rimHit = rimContact != null && rimContact.HadRecentProjectileContact;
        bool swish = !rimHit
                     && rb.linearVelocity.magnitude <= ArcAcademyLayout.SwishSpeedThreshold;

        if (swish)
        {
            swishVfx?.PlaySwish();
            netPhysics?.PlaySwishFeedback();
            BobAudioFeedback.Instance?.PlaySwish();
        }

        RecordBasketballPointAndNotify(basketball.Owner, swish);
        BobAudioFeedback.Instance?.PlayScore();
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
        if (!IsFallingThroughHoop(rb))
        {
            return;
        }

        bool rimHit = rimContact != null && rimContact.HadRecentProjectileContact;
        bool swish = !rimHit
                     && rb.linearVelocity.magnitude <= ArcAcademyLayout.SwishSpeedThreshold;

        if (swish)
        {
            swishVfx?.PlaySwish();
            netPhysics?.PlaySwishFeedback();
            BobAudioFeedback.Instance?.PlaySwish();
        }

        RecordBasketballPointAndNotify(agent, swish);
        BobAudioFeedback.Instance?.PlayScore();
    }

    /// <summary>
    /// Records the canonical basketball point (for scoreboard, success rate, CSV) on every make,
    /// independent of whether the rich ArcAcademyManager feedback path is present.
    /// </summary>
    private void RecordBasketballPointAndNotify(BobAgent agent, bool swish)
    {
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
