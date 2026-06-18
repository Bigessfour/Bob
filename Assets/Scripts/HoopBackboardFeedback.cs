using UnityEngine;

/// <summary>
/// Forwards backboard/rim clanks to ArcAcademyManager for optional feedback.
/// </summary>
public class HoopBackboardFeedback : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<BobAgent>() == null)
        {
            return;
        }

        ArcAcademyManager.Instance?.NotifyBackboardHit(collision.relativeVelocity.magnitude);
    }
}
