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
        if (ArcAcademyManager.Instance != null)
        {
            ArcAcademyManager.Instance.NotifyMadeBasket(agent, swish);
        }
        else
        {
            agent.RegisterMadeShot(swish);
        }

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
