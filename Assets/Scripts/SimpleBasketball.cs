using UnityEngine;

/// <summary>
/// Basketball projectile for minimal free-throw training. Scoring routes to the owning BobAgent.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SimpleBasketball : MonoBehaviour
{
    [SerializeField] private BobAgent owner;

    public BobAgent Owner => owner;

    public void Wire(BobAgent agent)
    {
        owner = agent;
    }

    public Rigidbody Body => GetComponent<Rigidbody>();
}
