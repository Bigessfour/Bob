using UnityEngine;

/// <summary>
/// Tracks recent Bob or basketball collisions with the rim for swish detection,
/// and notifies the owning agent once so PPO can learn to avoid rim contact.
/// </summary>
public class HoopRimContact : MonoBehaviour
{
    [SerializeField] private float contactWindowSeconds = 0.25f;

    private float lastProjectileContactTime = -999f;
    private HoopSwishVfx swishVfx;

    public bool HadRecentBobContact => HadRecentProjectileContact;

    public bool HadRecentProjectileContact => Time.time - lastProjectileContactTime <= contactWindowSeconds;

    private void Awake()
    {
        swishVfx = GetComponentInChildren<HoopSwishVfx>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        BobAgent agent = null;
        if (collision.collider.TryGetComponent(out SimpleBasketball ball) && ball.Owner != null)
        {
            agent = ball.Owner;
        }
        else if (collision.collider.TryGetComponent(out BobAgent bob))
        {
            agent = bob;
        }

        if (agent == null)
        {
            return;
        }

        lastProjectileContactTime = Time.time;
        swishVfx?.PlayRimContact();
        agent.NotifyRimContact();
    }
}
