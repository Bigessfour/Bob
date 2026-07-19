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
///   - Body position (x,y,z) — basketball when projectileBody is set, else Bob
///   - Relative vector to hoop (dx,dy,dz)
///   - Horizontal velocity (vx, vz)
///
/// Actions (3 continuous):
///   - X force, Y force (lift), Z force (toward hoop)
///
/// Reward shaping at launch penalizes sideways/backward/downward impulses and rewards
/// upward arc toward the hoop (see ApplyLaunchDirectionRewards).
///
/// bob-v4 Tier 1: episodes end when the shot resolves (rim miss, floor, settle, or step cap),
/// with terminal miss proximity reward and gated per-step distance penalty.
/// bob-v4.1: Bob-local impulse + stronger make / capped miss proximity / rim-plane miss penalty.
/// bob-v4.2 / Tier 1.6+: no proximity on rim_miss; rim penalty exceeds launch shaping; past-plane
/// timeouts count as rim_miss.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BobAgent : Agent
{
    [Header("Environment References")]
    [Tooltip("Rim transform on the Hoop assembly")]
    public Transform hoop;

    [Tooltip("Optional basketball rigidbody — Bob stays at spawn and launches this body")]
    [SerializeField] private Rigidbody projectileBody;

    [Header("Force Tuning (for Heuristic + initial training)")]
    public float lateralForceScale = 10f;
    public float verticalForceScale = 16f;
    public float verticalBias = 4f;
    public float forwardForceScale = 14f;
    // Local +Z is toward the hoop when spawn facing is applied; +6 maps neutral
    // actions to the same world prior as the old world-space bias of -6.
    public float forwardBias = 6f;

    private Rigidbody rb;
    private Renderer bobRenderer;
    private BobEntranceController entrance;
    private bool scoredThisEpisode;
    private float shotPeakHeight;
    private float shotStartHeight;
    private bool trackingArc;
    private float scorePulseTimer;
    private float episodePeakArcQuality;
    private bool shotImpulseThisEpisode;
    private int stepsSinceShot;
    private int settledStepCount;
    private int rimApproachSign;
    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private Color baseEmissive = new(1f, 0.38f, 0f);

    private bool UsesProjectile => projectileBody != null;

    private Transform ObservationTransform => UsesProjectile ? projectileBody.transform : transform;

    private Rigidbody ActionRigidbody => UsesProjectile ? projectileBody : rb;

    public Rigidbody ProjectileBody => projectileBody;

    public void ConfigureProjectileLauncher(Rigidbody body)
    {
        projectileBody = body;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            BobPhysicsUtility.ClearVelocitiesIfDynamic(rb);
        }
    }

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
        bobRenderer.material.SetColor(
            EmissiveColorId,
            baseEmissive * pulse * ArcAcademyLayout.BobGlowIntensity * BobVisualProfile.ScorePulseGlowMultiplier);
    }

    public override void OnEpisodeBegin()
    {
        GetComponent<BobFaceExpression>()?.OnEpisodeEnded(scoredThisEpisode);
        BobTrainingStats.Instance?.FlushEpisodeArcQuality(episodePeakArcQuality);
        BobTrainingStats.Instance?.BeginIteration(scoredThisEpisode);

        scoredThisEpisode = false;
        trackingArc = false;
        shotImpulseThisEpisode = false;
        stepsSinceShot = 0;
        settledStepCount = 0;
        rimApproachSign = 0;
        episodePeakArcQuality = 0f;
        shotPeakHeight = ObservationTransform.position.y;
        shotStartHeight = ObservationTransform.position.y;

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
        ApplySpawn(position, ResolveSpawnRotation(position));
    }

    public void ApplySpawn(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        BobPhysicsUtility.ClearVelocitiesIfDynamic(rb);
        ResetProjectile(position, rotation);
    }

    public void ResetProjectile(Vector3 bobSpawn)
    {
        ResetProjectile(bobSpawn, transform.rotation);
    }

    public void ResetProjectile(Vector3 bobSpawn, Quaternion bobRotation)
    {
        if (projectileBody == null)
        {
            return;
        }

        projectileBody.transform.position =
            BasketballProjectileSetup.GetReleasePosition(bobSpawn, bobRotation);
        BobPhysicsUtility.ClearVelocitiesIfDynamic(projectileBody);
    }

    private Quaternion ResolveSpawnRotation(Vector3 spawnPosition)
    {
        return SimpleArcAcademyArena.GetSpawnFacingRotation(spawnPosition, hoop);
    }

    private void CompleteEpisodeBegin()
    {
        trackingArc = true;
        shotStartHeight = ObservationTransform.position.y;
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
        GetComponent<BobFaceExpression>()?.SetHappy();

        float reward = ArcAcademyRewards.MadeBasket;
        if (swish)
        {
            reward += ArcAcademyRewards.SwishBonus;
        }

        GiveReward(reward);
        FinalizeShotLog(scored: true, endReason: swish ? "swish" : "make");
        EndEpisode();
    }

    private void ResolveEpisodeAsMiss(
        bool applyOutOfBoundsPenalty = false,
        bool applyRimPlaneMissPenalty = false)
    {
        if (scoredThisEpisode)
        {
            return;
        }

        // Unified rim_miss: never pay proximity; always apply rim-plane penalty.
        // Proximity remains for floor / timeout / settled wild misses only.
        if (hoop != null && !applyRimPlaneMissPenalty)
        {
            Vector3 pos = ObservationTransform.position;
            float xzDist = new Vector2(pos.x - hoop.position.x, pos.z - hoop.position.z).magnitude;
            float proximity = 1f - Mathf.Clamp01(xzDist / ArcAcademyLayout.MissProximityMaxDist);
            GiveReward(proximity * ArcAcademyLayout.MissProximityRewardScale);
        }

        if (applyRimPlaneMissPenalty)
        {
            GiveReward(-ArcAcademyLayout.RimPlaneMissPenalty);
        }

        if (applyOutOfBoundsPenalty)
        {
            GiveReward(-0.5f);
        }

        BobAudioFeedback.Instance?.PlayMiss();
        string reason = applyOutOfBoundsPenalty
            ? "oob"
            : applyRimPlaneMissPenalty
                ? "rim_miss"
                : ResolveMissReason();
        FinalizeShotLog(scored: false, endReason: reason);
        EndEpisode();
    }

    private string ResolveMissReason()
    {
        if (stepsSinceShot >= ArcAcademyLayout.ShotResolveMaxSteps)
        {
            return "timeout";
        }

        if (TryResolveFloorContact())
        {
            return "floor";
        }

        if (settledStepCount >= ArcAcademyLayout.BallSettledStepsRequired)
        {
            return "settled";
        }

        // rim_miss is only labeled via applyRimPlaneMissPenalty (single definition).
        return "miss";
    }

    private void FinalizeShotLog(bool scored, string endReason)
    {
        var stats = BobTrainingStats.Instance;
        stats?.RecordShotEndReason(endReason);
        float net = stats != null ? stats.CurrentEpisodeNetReward : 0f;
        BobShotActionLog.RecordResolution(scored, net, episodePeakArcQuality, endReason);
    }

    private bool IsShotInFlight()
    {
        return shotImpulseThisEpisode
               && ObservationTransform.position.y > ArcAcademyLayout.CourtFloorContactHeight;
    }

    private bool TryResolveShotAfterImpulse()
    {
        if (!shotImpulseThisEpisode || scoredThisEpisode)
        {
            return false;
        }

        stepsSinceShot++;

        if (stepsSinceShot >= ArcAcademyLayout.ShotResolveMaxSteps)
        {
            // Past-plane then timeout was farming proximity under a "timeout" label — treat as rim_miss.
            if (IsPastRimPlane())
            {
                ResolveEpisodeAsMiss(applyRimPlaneMissPenalty: true);
            }
            else
            {
                ResolveEpisodeAsMiss();
            }

            return true;
        }

        if (TryResolveRimPlaneMiss())
        {
            ResolveEpisodeAsMiss(applyRimPlaneMissPenalty: true);
            return true;
        }

        if (TryResolveFloorContact())
        {
            ResolveEpisodeAsMiss();
            return true;
        }

        if (TryResolveBallSettled())
        {
            // Settled past the rim plane without a make = rim_miss (same economics).
            if (IsPastRimPlane())
            {
                ResolveEpisodeAsMiss(applyRimPlaneMissPenalty: true);
            }
            else
            {
                ResolveEpisodeAsMiss();
            }

            return true;
        }

        return false;
    }

    /// <summary>True when the ball has crossed the rim plane along the approach axis.</summary>
    private bool IsPastRimPlane()
    {
        if (hoop == null || rimApproachSign == 0)
        {
            return false;
        }

        float pastRim = rimApproachSign * (ObservationTransform.position.z - hoop.position.z);
        return pastRim >= ArcAcademyLayout.RimPlaneMissTolerance;
    }

    private bool TryResolveRimPlaneMiss()
    {
        if (!IsPastRimPlane())
        {
            return false;
        }

        // No upper height gate — high arcs past the rim still count as rim_miss (Tier 1.6 review).
        return ObservationTransform.position.y >= ArcAcademyLayout.CourtFloorContactHeight;
    }

    private bool TryResolveFloorContact()
    {
        return ObservationTransform.position.y <= ArcAcademyLayout.CourtFloorContactHeight;
    }

    private bool TryResolveBallSettled()
    {
        if (ActionRigidbody == null)
        {
            return false;
        }

        if (ActionRigidbody.linearVelocity.sqrMagnitude
            > ArcAcademyLayout.BallSettledSpeedThreshold * ArcAcademyLayout.BallSettledSpeedThreshold)
        {
            settledStepCount = 0;
            return false;
        }

        settledStepCount++;
        return settledStepCount >= ArcAcademyLayout.BallSettledStepsRequired;
    }

    private void GiveReward(float amount)
    {
        AddReward(amount);
        BobTrainingStats.Instance?.RecordReward(amount);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ObservationTransform.position);

        if (hoop != null)
        {
            sensor.AddObservation(hoop.position - ObservationTransform.position);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        if (ActionRigidbody != null)
        {
            Vector3 v = ActionRigidbody.linearVelocity;
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
        if (BobGameStateMachine.Instance != null && BobGameStateMachine.Instance.IsPaused)
        {
            return;
        }

        if (entrance != null && entrance.IsActive)
        {
            return;
        }

        if (ActionRigidbody == null || hoop == null)
        {
            return;
        }

        if (!shotImpulseThisEpisode)
        {
            var c = actions.ContinuousActions;

            // === LOCAL IMPULSE (robust to spawn rotation/jitter) ===
            float fx = c[0] * lateralForceScale;                           // local X (lateral)
            float fy = c[1] * verticalForceScale + verticalBias;           // Y = world up
            float fz = c[2] * forwardForceScale + forwardBias;             // local Z (forward)

            Vector3 localImpulse = new Vector3(fx, fy, fz);
            Vector3 worldImpulse = transform.rotation * localImpulse;      // Convert using Bob's facing

            ActionRigidbody.AddForce(worldImpulse, ForceMode.Impulse);

            // Preserve variable name for minimal downstream changes
            Vector3 impulse = worldImpulse;

            shotImpulseThisEpisode = true;
            stepsSinceShot = 0;
            settledStepCount = 0;
            if (hoop != null)
            {
                rimApproachSign = (int)Mathf.Sign(hoop.position.z - transform.position.z);
                if (rimApproachSign == 0)
                {
                    rimApproachSign = 1;
                }
            }

            float towardDot = ComputeTowardHoopDot(impulse);
            int iteration = BobTrainingStats.Instance != null ? BobTrainingStats.Instance.TotalIterations : 0;
            bool training = BobTrainingConnectionMonitor.Instance != null
                            && BobTrainingConnectionMonitor.Instance.IsTrainingConnected;
            BobTrainingStats.Instance?.RecordLaunch(new Vector3(c[0], c[1], c[2]), impulse, towardDot);
            BobShotActionLog.RecordLaunch(iteration, c[0], c[1], c[2], impulse, towardDot, training);

            ApplyLaunchDirectionRewards(impulse);

            GetComponent<BobProceduralAnimator>()?.NotifyShotImpulse();
            GetComponent<BobFaceExpression>()?.SetFocus();
            if (hoop != null)
            {
                ArcAcademyPowerPathPulse.Instance?.PlayPulse(transform.position, hoop.position);
            }

            GiveReward(-0.005f);
        }

        if (TryResolveShotAfterImpulse())
        {
            return;
        }

        ApplyFlightDirectionPenalties();

        Vector3 toHoop = hoop.position - ObservationTransform.position;
        float xzDist = new Vector2(toHoop.x, toHoop.z).magnitude;

        if (IsShotInFlight())
        {
            GiveReward(-ArcAcademyLayout.PerStepDistancePenaltyScale * xzDist);
        }

        if (trackingArc && IsShotInFlight())
        {
            if (ObservationTransform.position.y > shotPeakHeight)
            {
                shotPeakHeight = ObservationTransform.position.y;
            }

            float arcQuality = CalculateArcQuality(xzDist);
            episodePeakArcQuality = Mathf.Max(episodePeakArcQuality, arcQuality);
            GiveReward(arcQuality * ArcAcademyLayout.ArcQualityRewardScale);

            if (ActionRigidbody.linearVelocity.y < -0.5f
                && ObservationTransform.position.y < shotPeakHeight - 0.3f)
            {
                trackingArc = false;
            }
        }

        if (ArcAcademyLayout.IsOutOfBounds(ObservationTransform.position))
        {
            ResolveEpisodeAsMiss(applyOutOfBoundsPenalty: true);
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

        Vector3 toHoop = hoop.position - ObservationTransform.position;
        Vector3 velocity = ActionRigidbody.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f)
        {
            return apexScore * 0.5f;
        }

        float alignment = Vector3.Dot(velocity.normalized, toHoop.normalized);
        float alignmentScore = Mathf.Clamp01((alignment + 1f) * 0.5f);

        return (apexScore * 0.6f) + (alignmentScore * 0.4f);
    }

    private float ComputeTowardHoopDot(Vector3 impulse)
    {
        if (hoop == null)
        {
            return 0f;
        }

        Vector3 toHoop = hoop.position - ObservationTransform.position;
        Vector3 toHoopFlat = new Vector3(toHoop.x, 0f, toHoop.z);
        float horizontalDist = toHoopFlat.magnitude;
        if (horizontalDist < 0.05f)
        {
            return 0f;
        }

        Vector3 impulseFlat = new Vector3(impulse.x, 0f, impulse.z);
        float flatMag = impulseFlat.magnitude;
        if (flatMag < 0.01f)
        {
            return 0f;
        }

        return Vector3.Dot(impulseFlat / flatMag, toHoopFlat / horizontalDist);
    }

    /// <summary>
    /// Shapes the first shot each episode: reward upward arc toward the hoop, punish sideways/backward/downward launch.
    /// </summary>
    private void ApplyLaunchDirectionRewards(Vector3 impulse)
    {
        if (hoop == null)
        {
            return;
        }

        Vector3 toHoop = hoop.position - ObservationTransform.position;
        Vector3 toHoopFlat = new Vector3(toHoop.x, 0f, toHoop.z);
        float horizontalDist = toHoopFlat.magnitude;
        if (horizontalDist < 0.05f)
        {
            return;
        }

        Vector3 towardHoop = toHoopFlat / horizontalDist;

        Vector3 impulseFlat = new Vector3(impulse.x, 0f, impulse.z);
        float flatMag = impulseFlat.magnitude;
        if (flatMag > 0.01f)
        {
            float horizDot = Vector3.Dot(impulseFlat / flatMag, towardHoop);
            if (horizDot >= 0f)
            {
                GiveReward(horizDot * ArcAcademyLayout.LaunchTowardHoopRewardScale);
            }
            else
            {
                GiveReward(horizDot * ArcAcademyLayout.LaunchAwayFromHoopPenaltyScale);
                if (horizDot < -0.5f)
                {
                    GiveReward(-ArcAcademyLayout.LaunchRadicallyWrongFlatPenalty);
                }
                else if (horizDot < 0f)
                {
                    GiveReward(-ArcAcademyLayout.LaunchBackwardFlatPenalty);
                }
            }
        }

        if (impulse.y < 0f)
        {
            GiveReward(impulse.y * ArcAcademyLayout.LaunchDownwardPenaltyScale);
        }
        else
        {
            float normalizedUp = Mathf.Clamp01(impulse.y / Mathf.Max(verticalForceScale + verticalBias, 0.01f));
            GiveReward(normalizedUp * ArcAcademyLayout.LaunchUpwardRewardScale);
        }

        Vector3 idealArcDir = (towardHoop + Vector3.up * ArcAcademyLayout.IdealLaunchUpRatio).normalized;
        float impulseMag = impulse.magnitude;
        if (impulseMag > 0.01f)
        {
            float arcDot = Vector3.Dot(impulse / impulseMag, idealArcDir);
            if (arcDot >= 0f)
            {
                GiveReward(arcDot * ArcAcademyLayout.LaunchArcAlignRewardScale);
            }
            else
            {
                GiveReward(arcDot * ArcAcademyLayout.LaunchArcMisalignPenaltyScale);
            }
        }
    }

    /// <summary>Penalizes mid-flight velocity that points away from the hoop.</summary>
    private void ApplyFlightDirectionPenalties()
    {
        if (!shotImpulseThisEpisode || hoop == null || ActionRigidbody == null)
        {
            return;
        }

        Vector3 toHoop = hoop.position - ObservationTransform.position;
        Vector3 velocity = ActionRigidbody.linearVelocity;
        if (velocity.sqrMagnitude < 0.25f || toHoop.sqrMagnitude < 0.01f)
        {
            return;
        }

        float alignment = Vector3.Dot(velocity.normalized, toHoop.normalized);
        if (alignment < 0f)
        {
            GiveReward(alignment * ArcAcademyLayout.FlightAwayFromHoopPenaltyScale);
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
        // Local +Z is forward (toward hoop); default matches prior world −Z shot.
        continuous[2] = zInput == 0f ? 0.5f : zInput;
    }
}
