using UnityEngine;

/// <summary>
/// Tracks recent Bob collisions with the rim for swish detection.
/// </summary>
public class HoopRimContact : MonoBehaviour
{
    [SerializeField] private float contactWindowSeconds = 0.25f;

    private float lastBobContactTime = -999f;
    private HoopSwishVfx swishVfx;

    public bool HadRecentBobContact => Time.time - lastBobContactTime <= contactWindowSeconds;

    private void Awake()
    {
        swishVfx = GetComponentInChildren<HoopSwishVfx>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<BobAgent>() == null)
        {
            return;
        }

        lastBobContactTime = Time.time;
        swishVfx?.PlayRimContact();
    }
}
