using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// BobAgent - the cheerful orange cube that learns to sink free throws at Arc Academy.
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
    private Renderer bobRenderer;
    private BobEntranceController entrance;
    private bool scoredThisEpisode;
    private float shotPeakHeight;
    private float shotStartHeight;
    private bool trackingArc;
    private float scorePulseTimer;
    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private Color baseEmissive = new(1f, 0.38f, 0f);

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BobAgent: Rigidbody missing! Add it via Inspector.");
        }

        bobRenderer = GetComponent<Renderer>();
        entrance = GetComponent<BobEntranceController>();
        if (bobRenderer != null && bobRenderer.material.HasProperty(EmissiveColorId))
        {
            baseEmissive = bobRenderer.material.GetColor(EmissiveColorId);
        }

        Debug.Log("Bob the Free Throw Champion has entered Arc Academy! " +
                  "Ready to learn the perfect arc through PPO trial-and-error.");
    }

    private void Update()
    {
        if (scorePulseTimer <= 0f || bobRenderer == null)
        {
            return;
        }

        scorePulseTimer -= Time.deltaTime;
        float pulse = 1f + Mathf.Sin(scorePulseTimer * 20f) * 0.35f;
        bobRenderer.material.SetColor(EmissiveColorId, baseEmissive * pulse * ArcAcademyLayout.BobGlowIntensity);
    }

    public override void OnEpisodeBegin()
    {
        scoredThisEpisode = false;
        trackingArc = false;
        shotPeakHeight = transform.position.y;
        shotStartHeight = transform.position.y;

        if (ArcAcademyManager.Instance != null)
        {
            ArcAcademyManager.Instance.NotifyEpisodeBegin(this, CompleteEpisodeBegin);
        }
        else
        {
            ApplySpawn(ArcAcademyLayout.BobSpawnPosition);
            CompleteEpisodeBegin();
        }
    }

    public void ApplySpawn(Vector3 position)
    {
        transform.position = position;
        BobPhysicsUtility.ClearVelocitiesIfDynamic(rb);
    }

    private void CompleteEpisodeBegin()
    {
        trackingArc = true;
        shotStartHeight = transform.position.y;
        shotPeakHeight = shotStartHeight;
    }

    public void RegisterMadeShot(bool swish = false)
    {
        if (scoredThisEpisode)
        {
            return;
        }

        scoredThisEpisode = true;
        scorePulseTimer = 0.5f;

        float reward = ArcAcademyRewards.MadeBasket;
        if (swish)
        {
            reward += ArcAcademyRewards.SwishBonus;
        }

        AddReward(reward);
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
        if (entrance != null && entrance.IsActive)
        {
            return;
        }

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

        if (trackingArc)
        {
            if (transform.position.y > shotPeakHeight)
            {
                shotPeakHeight = transform.position.y;
            }

            float arcQuality = CalculateArcQuality(xzDist);
            AddReward(arcQuality * ArcAcademyLayout.ArcQualityRewardScale);

            if (rb.linearVelocity.y < -0.5f && transform.position.y < shotPeakHeight - 0.3f)
            {
                trackingArc = false;
            }
        }

        if (ArcAcademyLayout.IsOutOfBounds(transform.position))
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }

    private float CalculateArcQuality(float horizontalDistance)
    {
        if (hoop == null || horizontalDistance < 0.01f)
        {
            return 0f;
        }

        float idealApex = shotStartHeight + horizontalDistance * ArcAcademyLayout.IdealArcApexRatio;
        float apexError = Mathf.Abs(shotPeakHeight - idealApex);
        float apexScore = Mathf.Clamp01(1f - apexError / 2.5f);

        Vector3 toHoop = hoop.position - transform.position;
        Vector3 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f)
        {
            return apexScore * 0.5f;
        }

        float alignment = Vector3.Dot(velocity.normalized, toHoop.normalized);
        float alignmentScore = Mathf.Clamp01((alignment + 1f) * 0.5f);

        return (apexScore * 0.6f) + (alignmentScore * 0.4f);
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
