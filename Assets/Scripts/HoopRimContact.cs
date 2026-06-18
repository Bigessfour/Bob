using UnityEngine;

/// <summary>
/// Tracks recent Bob collisions with the rim for swish detection.
/// </summary>
public class HoopRimContact : MonoBehaviour
{
    [SerializeField] private float contactWindowSeconds = 0.25f;

    private float lastBobContactTime = -999f;

    public bool HadRecentBobContact => Time.time - lastBobContactTime <= contactWindowSeconds;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<BobAgent>() != null)
        {
            lastBobContactTime = Time.time;
        }
    }
}
