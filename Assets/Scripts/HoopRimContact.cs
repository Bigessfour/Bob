using UnityEngine;

/// <summary>
/// Tracks recent Bob or basketball collisions with the rim for swish detection.
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
        if (collision.collider.GetComponent<BobAgent>() == null
            && collision.collider.GetComponent<SimpleBasketball>() == null)
        {
            return;
        }

        lastProjectileContactTime = Time.time;
        swishVfx?.PlayRimContact();
    }
}
