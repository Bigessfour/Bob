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
    [SerializeField] private float minImpulse = 5.5f;
    [SerializeField] private float maxImpulse = 16f;
    [SerializeField] private float maxHoldSeconds = 1f;
    [SerializeField] private float groundedVelocityThreshold = 0.35f;
    [SerializeField] private float spawnProximityRadius = 1.5f;
    [SerializeField] private float spinTorqueScale = 0.35f;
    [SerializeField] private IShootInputProvider vrInput;

    private Rigidbody rb;
    private Camera mainCamera;
    private BobEntranceController entrance;
    private ArcTrajectoryVisual trajectoryVisual;
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
        entrance = GetComponent<BobEntranceController>();
        behaviorParameters = GetComponent<BehaviorParameters>();
        trajectoryVisual = Object.FindAnyObjectByType<ArcTrajectoryVisual>();
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
            trajectoryVisual?.ClearPreview();
            return;
        }

        if (vrInput != null && vrInput.TryGetShot(out Vector3 vrDir, out float vrPower))
        {
            FireImpulse(vrDir.normalized * Mathf.Lerp(minImpulse, maxImpulse, vrPower));
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireTowardHoop(maxImpulse * 0.88f);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            holdingShot = true;
            holdStartTime = Time.time;
        }

        if (holdingShot && Input.GetMouseButton(0))
        {
            UpdateAimPreview(Mathf.Clamp01((Time.time - holdStartTime) / maxHoldSeconds));
        }

        if (holdingShot && Input.GetMouseButtonUp(0))
        {
            float hold = Mathf.Clamp01((Time.time - holdStartTime) / maxHoldSeconds);
            float power = Mathf.Lerp(minImpulse, maxImpulse, hold);
            FireFromMouseRay(power);
            holdingShot = false;
            trajectoryVisual?.ClearPreview();
        }
    }

    private void UpdateAimPreview(float hold)
    {
        if (trajectoryVisual == null || hoopTarget == null)
        {
            return;
        }

        Vector3 start = transform.position;
        Vector3 end = hoopTarget.position + Vector3.up * 0.15f;
        if (mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 dir = ray.direction;
            dir.y = Mathf.Max(dir.y, 0.2f);
            end = start + dir.normalized * Mathf.Lerp(6f, 12f, hold);
        }

        trajectoryVisual.PreviewArc(start, end);
    }

    private bool CanAcceptManualInput()
    {
        if (entrance != null && entrance.IsActive)
        {
            return false;
        }

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
        BobPhysicsUtility.ClearVelocitiesIfDynamic(rb);
        rb.AddForce(impulse, ForceMode.Impulse);

        Vector3 spinAxis = Vector3.Cross(Vector3.up, impulse.normalized);
        if (spinAxis.sqrMagnitude > 0.001f)
        {
            rb.AddTorque(spinAxis.normalized * impulse.magnitude * spinTorqueScale, ForceMode.Impulse);
        }
    }
}
