using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// BobAgent — orange cube free-throw learner (ML-Agents PPO, Behavior Name <c>Bob</c>).
///
/// <para><b>Ideal free-throw kinematics</b> (what Bob must learn — not ricochet lottery):</para>
/// <list type="number">
/// <item>Single impulse from free-throw distance, then pure Rigidbody flight under gravity.</item>
/// <item>Steep elevation (≈55–65°); <see cref="BobSwishLaunchSolver"/> uses 65° for a clear lob.</item>
/// <item>Parabolic trajectory under <see cref="Physics.gravity"/>.</item>
/// <item>Apex well above the rim (~3.05 m) so the ball is already descending
/// (<c>linearVelocity.y &lt; 0</c>) when it reaches the rim’s horizontal plane.</item>
/// <item>Enters the rim cylinder top-down. Clean swish is ideal; bank/rim-in still score
/// via <see cref="HoopScoreZone"/>, but the skill target is the high-arc root — not luck bounces.</item>
/// </list>
///
/// Mathematical reference: <see cref="BobSwishLaunchSolver.TryComputeWorldImpulse"/> solves the
/// high-arc speed for a given angle; <see cref="BobSwishLaunchSolver.WorldImpulseToActions"/>
/// maps that impulse into Bob’s continuous actions (local impulse + scales/biases).
///
/// Observations (11): body pos, vector to hoop, velocity (vx, vy, vz), speed, shot phase.
/// Actions (3): local residual corrections around the analytic free throw (once per episode).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BobAgent : Agent
{
    #region IdealFreeThrowKinematics
    // Ground truth for a proper free throw lives in BobSwishLaunchSolver:
    //   Prefer 58° launch → apex above rim → descending entry through HoopScoreZone.
    // Neutral actions c≈(0,0,0) → idealImpulse + zero residual (analytic swish prior).
    // PPO learns small residuals; shaped by IdealSolverMatchRewardScale + MadeBasket ≫ near-miss.
    #endregion

    [Header("Environment References")]
    [Tooltip("Rim transform on the Hoop assembly")]
    public Transform hoop;

    [Tooltip("Optional basketball rigidbody — Bob stays at spawn and launches this body")]
    [SerializeField] private Rigidbody projectileBody;

    [Header("Force Tuning (fallback when analytic solver fails)")]
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
    private bool rimContactThisEpisode;
    private bool squareHitRewardedThisEpisode;
    private bool descendingNearRimThisEpisode;
    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private Color baseEmissive = new(1f, 0.38f, 0f);

    private bool UsesProjectile => projectileBody != null;

    private Transform ObservationTransform => UsesProjectile ? projectileBody.transform : transform;

    private Rigidbody ActionRigidbody => UsesProjectile ? projectileBody : rb;

    /// <summary>Ball rigidbody when Phase 1.5 projectile mode is active; otherwise null.</summary>
    public Rigidbody ProjectileBody => projectileBody;

    /// <summary>
    /// Ideal high-arc impulse from <see cref="BobSwishLaunchSolver"/> for previews / Heuristic.
    /// </summary>
    public bool TryGetIdealWorldImpulse(float launchAngleDegrees, out Vector3 worldImpulse)
    {
        worldImpulse = Vector3.zero;
        Vector3 launchPos = ActionRigidbody != null
            ? ActionRigidbody.position
            : BasketballProjectileSetup.GetReleasePosition(transform.position, transform.rotation);
        Vector3 rimPos = hoop != null ? hoop.position : ArcAcademyLayout.MainRimWorldPosition;
        float mass = ActionRigidbody != null
            ? ActionRigidbody.mass
            : BasketballProjectileSetup.BallMass;
        return BobSwishLaunchSolver.TryComputeWorldImpulse(
            launchPos, rimPos, mass, launchAngleDegrees, out worldImpulse);
    }

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
        rimContactThisEpisode = false;
        squareHitRewardedThisEpisode = false;
        descendingNearRimThisEpisode = false;
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

    /// <summary>
    /// Ball entered the backboard shooter's square. Small curriculum RL reward once per
    /// episode if the shot already peaked (high arch), always ≪ <see cref="ArcAcademyRewards.MadeBasket"/>.
    /// </summary>
    public void NotifyBackboardSquareHit()
    {
        if (squareHitRewardedThisEpisode || scoredThisEpisode)
        {
            return;
        }

        float apexRise = shotPeakHeight - shotStartHeight;
        if (apexRise < ArcAcademyLayout.SquareHitMinApexRise)
        {
            return;
        }

        squareHitRewardedThisEpisode = true;
        GiveReward(ArcAcademyRewards.BackboardSquareHit);
    }

    /// <summary>
    /// Ball/Bob touched the rim this episode (for rim-out miss labeling / swish VFX).
    /// No RL penalty — rim-in is a valid make; rim-out is simply a miss.
    /// </summary>
    public void NotifyRimContact()
    {
        if (rimContactThisEpisode || scoredThisEpisode)
        {
            return;
        }

        rimContactThisEpisode = true;
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

        // Basketball rule: any path through the hoop is the same make reward.
        GiveReward(ArcAcademyRewards.MadeBasket);
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

        // Touched iron then never scored (fell outside / settled) — still a miss, not a point.
        if (rimContactThisEpisode)
        {
            return "rim_out";
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
            sensor.AddObservation(v.y);
            sensor.AddObservation(v.z);
            float speedNorm = Mathf.Clamp01(
                v.magnitude / Mathf.Max(ArcAcademyLayout.MaxObsSpeedMagnitude, 0.01f));
            sensor.AddObservation(speedNorm);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        sensor.AddObservation(shotImpulseThisEpisode ? 1f : 0f);
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
            // Demo / HeuristicOnly: wait for Space (or Fire1) so the shot isn't taken on the
            // first Academy step before the player can hold an arc. While DemonstrationRecorder
            // is recording, auto-fire each episode (agent-friendly quality demo capture).
            if (IsHeuristicDemoMode() && !IsHeuristicShootHeld() && !IsDemonstrationRecording())
            {
                return;
            }

            var c = actions.ContinuousActions;

            // === HYBRID: analytic free-throw prior + clamped residual RL ===
            Vector3 idealImpulse = Vector3.zero;
            bool hasIdeal = BobSwishLaunchSolver.TryComputeWorldImpulse(
                ObservationTransform.position,
                hoop.position,
                ActionRigidbody.mass,
                GetEffectiveLaunchAngleDegrees(),
                out idealImpulse);

            Vector3 worldImpulse;
            if (hasIdeal)
            {
                float rx = c[0] * ArcAcademyLayout.ResidualLateralScale;
                float ry = c[1] * ArcAcademyLayout.ResidualVerticalScale;
                float rz = c[2] * ArcAcademyLayout.ResidualForwardScale;
                Vector3 residualLocal = new Vector3(rx, ry, rz);
                Vector3 residualWorld = transform.rotation * residualLocal;
                if (residualWorld.magnitude > ArcAcademyLayout.ResidualMaxMagnitude)
                {
                    residualWorld = residualWorld.normalized * ArcAcademyLayout.ResidualMaxMagnitude;
                }

                worldImpulse = idealImpulse + residualWorld;
            }
            else
            {
                // Degenerate geometry — legacy absolute local impulse fallback.
                float fx = c[0] * lateralForceScale;
                float fy = c[1] * verticalForceScale + verticalBias;
                float fz = c[2] * forwardForceScale + forwardBias;
                Vector3 localImpulse = new Vector3(fx, fy, fz);
                worldImpulse = transform.rotation * localImpulse;
            }

            ActionRigidbody.AddForce(worldImpulse, ForceMode.Impulse);

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
            float launchAngleDeg = BobSwishLaunchSolver.LaunchAngleDegreesFromImpulse(impulse);
            float solverMatch = 0f;
            if (hasIdeal
                && idealImpulse.sqrMagnitude > 0.01f
                && impulse.sqrMagnitude > 0.01f)
            {
                solverMatch = Vector3.Dot(impulse.normalized, idealImpulse.normalized);
            }

            int iteration = BobTrainingStats.Instance != null ? BobTrainingStats.Instance.TotalIterations : 0;
            bool training = BobTrainingConnectionMonitor.Instance != null
                            && BobTrainingConnectionMonitor.Instance.IsTrainingConnected;
            BobTrainingStats.Instance?.RecordLaunch(new Vector3(c[0], c[1], c[2]), impulse, towardDot);
            BobShotActionLog.RecordLaunch(
                iteration, c[0], c[1], c[2], impulse, towardDot, training, launchAngleDeg, solverMatch);

            ApplyLaunchDirectionRewards(impulse, idealImpulse, solverMatch);

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
        TrackDescendingNearRim();

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
    /// Shapes the first shot each episode: toward-hoop + upward arc, plus attraction to the
    /// analytic high-arc impulse manifold (<see cref="BobSwishLaunchSolver"/>).
    /// </summary>
    private void ApplyLaunchDirectionRewards(Vector3 impulse, Vector3 idealImpulse, float solverMatch)
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
            bool hasIdeal = idealImpulse.sqrMagnitude > 0.01f;
            float upDenom = hasIdeal
                ? Mathf.Max(idealImpulse.y, 0.01f)
                : Mathf.Max(verticalForceScale + verticalBias, 0.01f);
            float normalizedUp = Mathf.Clamp01(impulse.y / upDenom);
            GiveReward(normalizedUp * ArcAcademyLayout.LaunchUpwardRewardScale);
            float fyTarget = hasIdeal ? idealImpulse.y : ArcAcademyLayout.IdealLaunchFy;
            float fyError = Mathf.Abs(impulse.y - fyTarget);
            GiveReward(-fyError * ArcAcademyLayout.LaunchPowerBandPenaltyScale);
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

        // Attract policy to the solved high-arc direction (max ≪ MadeBasket).
        if (idealImpulse.sqrMagnitude > 0.01f && solverMatch > 0f)
        {
            GiveReward(solverMatch * ArcAcademyLayout.IdealSolverMatchRewardScale);
        }
    }

    /// <summary>
    /// Marks whether the ball was descending while near the rim plane (ideal free-throw entry).
    /// </summary>
    private void TrackDescendingNearRim()
    {
        if (!shotImpulseThisEpisode || hoop == null || ActionRigidbody == null || descendingNearRimThisEpisode)
        {
            return;
        }

        Vector3 toRim = hoop.position - ObservationTransform.position;
        float xz = new Vector2(toRim.x, toRim.z).magnitude;
        if (xz <= ArcAcademyLayout.RimScoreRadius * 2.5f
            && ActionRigidbody.linearVelocity.y < 0f)
        {
            descendingNearRimThisEpisode = true;
            BobShotActionLog.NoteDescendingNearRim();
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
        const float LateralScale = 0.25f; // A/D max |c0|=0.25 → small lateral residual

        // Residual hybrid: pure expert / BC demos emit zero correction (solver prior only).
        // Manual Heuristic allows small A/D lateral residual; E/Shift nudge solver angle at launch.
        bool expertLock = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool pureExpert = expertLock || IsDemonstrationRecording();

        if (pureExpert)
        {
            continuous[0] = 0f;
            continuous[1] = 0f;
            continuous[2] = 0f;
        }
        else
        {
            continuous[0] = Input.GetAxis("Horizontal") * LateralScale;
            continuous[1] = 0f;
            continuous[2] = 0f;
        }
    }

    /// <summary>
    /// Launch angle for the analytic solver — E/Shift nudge in Heuristic demo mode only.
    /// </summary>
    private float GetEffectiveLaunchAngleDegrees()
    {
        if (IsDemonstrationRecording())
        {
            return BobSwishLaunchSolver.PreferredLaunchAngleDegrees;
        }

        if (IsHeuristicDemoMode())
        {
            float angle = BobSwishLaunchSolver.PreferredLaunchAngleDegrees;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                angle -= BobSwishLaunchSolver.AngleNudgeDegrees;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                angle += BobSwishLaunchSolver.AngleNudgeDegrees;
            }

            return angle;
        }

        return BobSwishLaunchSolver.PreferredLaunchAngleDegrees;
    }

    private bool IsHeuristicDemoMode()
    {
        var behavior = GetComponent<BehaviorParameters>();
        return behavior != null && behavior.BehaviorType == BehaviorType.HeuristicOnly;
    }

    private bool IsDemonstrationRecording()
    {
        var recorder = GetComponent<Unity.MLAgents.Demonstrations.DemonstrationRecorder>();
        return recorder != null && recorder.Record;
    }

    private static bool IsHeuristicShootHeld()
    {
        return Input.GetKey(KeyCode.Space)
               || Input.GetButton("Jump")
               || Input.GetButton("Fire1");
    }
}
