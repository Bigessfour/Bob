using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// BobAgent - the cheerful orange cube that learns to sink free throws.
///
/// Bob uses continuous 3D force control (no gravity) to propel himself toward the hoop.
/// Trained with PPO via ML-Agents. Behavior name must be "Bob" to match config/bob_free_throw.yaml.
///
/// Observations (8):
///   - Bob position (x,y,z)
///   - Relative vector to hoop (dx,dy,dz)
///   - Horizontal velocity (vx, vz)
///
/// Actions (3 continuous):
///   - X force, Y force (with lift bias), Z force (forward throw bias)
///
/// Rewards:
///   - Small per-step penalty (encourage efficient shots)
///   - Proximity shaping
///   - +2.0 (or +1) when a "made shot" is detected (passes through hoop volume)
///
/// Portfolio note: This is a minimal but complete Agent subclass demonstrating
/// Initialize/OnEpisodeBegin/CollectObservations/OnActionReceived/Heuristic.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BobAgent : Agent
{
    [Header("Environment References")]
    [Tooltip("Drag the Rim (or target center) child of the Hoop GameObject here")]
    public Transform hoop;

    [Header("Force Tuning (for Heuristic + initial training)")]
    public float lateralForceScale = 12f;
    public float verticalForceScale = 14f;
    public float verticalBias = 6f;     // helps give initial lift
    public float forwardForceScale = 18f;
    public float forwardBias = -4f;     // slight forward toward negative Z (hoop)

    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BobAgent: Rigidbody missing! Add it via Inspector.");
        }

        // Cheerful portfolio personality
        Debug.Log("🏀 Bob the Free Throw Champion has entered the arena! " +
                  "Ready to learn the perfect arc through PPO trial-and-error.");
    }

    public override void OnEpisodeBegin()
    {
        // Reset to starting free-throw position (per user spec)
        transform.localPosition = new Vector3(0f, 1.5f, 0f);

        // Stop all motion
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Small random lateral variation helps robustness (optional but nice)
        float xJitter = Random.Range(-0.3f, 0.3f);
        transform.localPosition = new Vector3(xJitter, 1.5f, 0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Exactly 8 observations to match Behavior Parameters > Vector Observation Space Size = 8
        // 1-3: where Bob is
        sensor.AddObservation(transform.localPosition);

        // 4-6: where the hoop is relative to Bob (most important signal)
        if (hoop != null)
        {
            sensor.AddObservation(hoop.position - transform.position);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        // 7-8: how fast Bob is already moving (horizontal only; vy largely action-driven)
        if (rb != null)
        {
            Vector3 v = rb.linearVelocity;
            sensor.AddObservation(v.x);
            sensor.AddObservation(v.z);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (rb == null || hoop == null) return;

        var c = actions.ContinuousActions;

        // Map [-1,1] policy outputs to world forces with useful ranges and bias for throwing forward/up
        float fx = c[0] * lateralForceScale;
        float fy = c[1] * verticalForceScale + verticalBias;
        float fz = c[2] * forwardForceScale + forwardBias;

        rb.AddForce(new Vector3(fx, fy, fz), ForceMode.Impulse);

        // === Reward shaping ===
        // Living penalty: encourages finishing the shot quickly
        AddReward(-0.005f);

        // Proximity shaping: small dense reward for getting closer on xz plane
        Vector3 toHoop = hoop.position - transform.position;
        float xzDist = new Vector2(toHoop.x, toHoop.z).magnitude;
        AddReward(-0.002f * xzDist);   // closer is better

        // === Made-shot detection (simple but effective for MVP) ===
        // Detect when Bob (our "ball") is roughly in the vertical band of the rim
        // and within the hoop's horizontal radius while moving downward-ish.
        float hoopY = hoop.position.y;
        bool heightOk = transform.position.y > (hoopY - 0.25f) &&
                        transform.position.y < (hoopY + 0.9f);
        bool xzOk = xzDist < 0.75f;
        bool falling = rb.linearVelocity.y <= 0.5f;   // not rocketing upward through it

        if (heightOk && xzOk && falling)
        {
            AddReward(2.0f);
            Debug.Log("🎉 Bob sunk it! +2 reward — great shot!");
            EndEpisode();
            return;
        }

        // Miss / out of bounds safety nets (prevent infinite flying)
        if (transform.position.y < 0.4f || Mathf.Abs(transform.position.x) > 20f || Mathf.Abs(transform.position.z) > 25f)
        {
            AddReward(-0.5f); // mild penalty for missing the court
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Keyboard control for rapid Play-mode testing before/without Python trainer.
        // This lets you manually "throw" Bob and see if the physics + rewards feel right.
        var continuous = actionsOut.ContinuousActions;

        // Left/Right (A/D or Left/Right arrows)
        continuous[0] = Input.GetAxis("Horizontal");

        // Up (Y) - Space gives strong lift impulse. Shift gives a little down nudge.
        if (Input.GetKey(KeyCode.Space))
            continuous[1] = 1.0f;
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            continuous[1] = -0.6f;
        else
            continuous[1] = 0f;

        // Forward throw power (negative Z toward hoop). W/S or Up/Down arrows.
        // We bias toward forward in OnActionReceived; here user mostly controls power.
        float zInput = Input.GetAxis("Vertical");
        if (zInput == 0f)
        {
            // Default gentle forward push so Space + arrow feels like a shot
            continuous[2] = 0.6f;
        }
        else
        {
            continuous[2] = zInput;
        }

        // Tip: Hold Space + Up arrow for a basic shot attempt. Watch the arc (or hover path).
    }
}
