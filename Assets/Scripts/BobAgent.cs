using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// BobAgent - the cheerful orange cube that learns to sink free throws.
///
/// Bob uses continuous 3D force control with gravity to arc toward the hoop.
/// Trained with PPO via ML-Agents. Behavior name must be "Bob" to match config/bob_free_throw.yaml.
///
/// Observations (8):
///   - Bob position (x,y,z)
///   - Relative vector to hoop (dx,dy,dz)
///   - Horizontal velocity (vx, vz)
///
/// Actions (3 continuous):
///   - X force, Y force (lift), Z force (toward hoop)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BobAgent : Agent
{
    [Header("Environment References")]
    [Tooltip("Rim transform on the Hoop assembly")]
    public Transform hoop;

    [Header("Force Tuning (for Heuristic + initial training)")]
    public float lateralForceScale = 10f;
    public float verticalForceScale = 16f;
    public float verticalBias = 4f;
    public float forwardForceScale = 14f;
    public float forwardBias = -6f;

    private Rigidbody rb;
    private bool scoredThisEpisode;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BobAgent: Rigidbody missing! Add it via Inspector.");
        }

        Debug.Log("Bob the Free Throw Champion has entered the arena! " +
                  "Ready to learn the perfect arc through PPO trial-and-error.");
    }

    public override void OnEpisodeBegin()
    {
        scoredThisEpisode = false;

        float xJitter = Random.Range(-BobCourtLayout.SpawnLateralJitter, BobCourtLayout.SpawnLateralJitter);
        transform.position = BobCourtLayout.BobSpawnPosition + new Vector3(xJitter, 0f, 0f);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void RegisterMadeShot()
    {
        if (scoredThisEpisode)
        {
            return;
        }

        scoredThisEpisode = true;
        AddReward(2.0f);
        Debug.Log("Bob sunk it! +2 reward — great shot!");
        EndEpisode();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);

        if (hoop != null)
        {
            sensor.AddObservation(hoop.position - transform.position);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

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
        if (rb == null || hoop == null)
        {
            return;
        }

        var c = actions.ContinuousActions;

        float fx = c[0] * lateralForceScale;
        float fy = c[1] * verticalForceScale + verticalBias;
        float fz = c[2] * forwardForceScale + forwardBias;

        rb.AddForce(new Vector3(fx, fy, fz), ForceMode.Impulse);

        AddReward(-0.005f);

        Vector3 toHoop = hoop.position - transform.position;
        float xzDist = new Vector2(toHoop.x, toHoop.z).magnitude;
        AddReward(-0.002f * xzDist);

        if (BobCourtLayout.IsOutOfBounds(transform.position))
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;

        continuous[0] = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.Space))
        {
            continuous[1] = 1.0f;
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            continuous[1] = -0.6f;
        }
        else
        {
            continuous[1] = 0f;
        }

        float zInput = Input.GetAxis("Vertical");
        continuous[2] = zInput == 0f ? 0.75f : zInput;
    }
}
