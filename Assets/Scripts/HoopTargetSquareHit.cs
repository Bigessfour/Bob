using UnityEngine;

/// <summary>
/// Shooter's-square hit zone on the backboard (regulation target box above the rim).
/// Awards a small curriculum RL reward once per episode when the ball enters the square —
/// less than a made free throw, enough to reinforce high-arc aim at the board target.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoopTargetSquareHit : MonoBehaviour
{
    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryNotify(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != null)
        {
            TryNotify(collision.collider);
        }
    }

    private static void TryNotify(Collider other)
    {
        BobAgent agent = null;
        if (other.TryGetComponent(out SimpleBasketball ball) && ball.Owner != null)
        {
            agent = ball.Owner;
        }
        else if (other.TryGetComponent(out BobAgent bob))
        {
            agent = bob;
        }

        agent?.NotifyBackboardSquareHit();
    }
}
