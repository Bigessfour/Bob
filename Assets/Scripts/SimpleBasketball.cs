using UnityEngine;

/// <summary>
/// Basketball projectile for minimal free-throw training. Scoring routes to the owning BobAgent.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SimpleBasketball : MonoBehaviour
{
    [SerializeField] private BobAgent owner;
    [SerializeField] private float maxSpeed = 14f;

    public BobAgent Owner => owner;

    public void Wire(BobAgent agent)
    {
        owner = agent;
    }

    public Rigidbody Body => GetComponent<Rigidbody>();

    private void FixedUpdate()
    {
        var body = Body;
        if (body == null || body.isKinematic)
        {
            return;
        }

        if (body.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
        }

        BasketballProjectileSetup.UpdateTrailEmit(body);
    }
}
