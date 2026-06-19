using UnityEngine;

/// <summary>
/// Safe Rigidbody helpers — Unity rejects velocity writes on kinematic bodies.
/// </summary>
public static class BobPhysicsUtility
{
    public static void ClearVelocitiesIfDynamic(Rigidbody body)
    {
        if (body == null || body.isKinematic)
        {
            return;
        }

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
