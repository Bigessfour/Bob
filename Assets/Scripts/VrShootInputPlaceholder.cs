using UnityEngine;

/// <summary>
/// No-op VR stub until an XR rig is wired in.
/// </summary>
public class VrShootInputPlaceholder : MonoBehaviour, IShootInputProvider
{
    public bool TryGetShot(out Vector3 worldDirection, out float power)
    {
        worldDirection = Vector3.zero;
        power = 0f;
        return false;
    }
}
