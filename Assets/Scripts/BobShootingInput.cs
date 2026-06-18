using Unity.MLAgents.Policies;
using UnityEngine;

/// <summary>
/// VR-ready shooting input placeholder. Future XR modules can implement this interface.
/// </summary>
public interface IShootInputProvider
{
    bool TryGetShot(out Vector3 worldDirection, out float power);
}

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

/// <summary>
/// Manual shooting for Play mode — Space or mouse release toward the hoop.
/// Disabled during ML-Agents Default training to avoid conflicting with PPO actions.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BobShootingInput : MonoBehaviour
{
    [SerializeField] private Transform hoopTarget;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float minImpulse = 4f;
    [SerializeField] private float maxImpulse = 14f;
    [SerializeField] private float maxHoldSeconds = 1.2f;
    [SerializeField] private float groundedVelocityThreshold = 0.35f;
    [SerializeField] private float spawnProximityRadius = 1.5f;
    [SerializeField] private IShootInputProvider vrInput;

    private Rigidbody rb;
    private Camera mainCamera;
    private bool holdingShot;
    private float holdStartTime;
    private BehaviorParameters behaviorParameters;

    public void Wire(Transform hoop, Transform spawn)
    {
        hoopTarget = hoop;
        spawnPoint = spawn;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        behaviorParameters = GetComponent<BehaviorParameters>();
        if (vrInput == null)
        {
            vrInput = GetComponent<VrShootInputPlaceholder>();
        }
    }

    private void Update()
    {
        if (!CanAcceptManualInput())
        {
            holdingShot = false;
            return;
        }

        if (vrInput != null && vrInput.TryGetShot(out Vector3 vrDir, out float vrPower))
        {
            FireImpulse(vrDir.normalized * Mathf.Lerp(minImpulse, maxImpulse, vrPower));
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireTowardHoop(maxImpulse * 0.85f);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            holdingShot = true;
            holdStartTime = Time.time;
        }

        if (holdingShot && Input.GetMouseButtonUp(0))
        {
            float hold = Mathf.Clamp01((Time.time - holdStartTime) / maxHoldSeconds);
            float power = Mathf.Lerp(minImpulse, maxImpulse, hold);
            FireFromMouseRay(power);
            holdingShot = false;
        }
    }

    private bool CanAcceptManualInput()
    {
        if (rb == null)
        {
            return false;
        }

        if (behaviorParameters != null && behaviorParameters.BehaviorType == BehaviorType.Default)
        {
            return false;
        }

        if (rb.linearVelocity.magnitude > groundedVelocityThreshold)
        {
            return false;
        }

        if (spawnPoint != null)
        {
            Vector3 flatDelta = transform.position - spawnPoint.position;
            flatDelta.y = 0f;
            if (flatDelta.magnitude > spawnProximityRadius)
            {
                return false;
            }
        }

        return true;
    }

    private void FireFromMouseRay(float power)
    {
        if (mainCamera == null)
        {
            FireTowardHoop(power);
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 direction = ray.direction;
        direction.y = Mathf.Max(direction.y, 0.25f);
        FireImpulse(direction.normalized * power);
    }

    private void FireTowardHoop(float power)
    {
        if (hoopTarget == null)
        {
            FireImpulse(Vector3.forward * power + Vector3.up * power * 0.6f);
            return;
        }

        Vector3 toHoop = hoopTarget.position - transform.position;
        toHoop.y += 0.4f;
        FireImpulse(toHoop.normalized * power);
    }

    private void FireImpulse(Vector3 impulse)
    {
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(impulse, ForceMode.Impulse);
    }
}
