using UnityEngine;

/// <summary>
/// Shared setup for the single basketball projectile (Phase 1.5).
/// Used by SimpleArcAcademyArenaBuilder and SimpleFreeThrowSetup.
/// </summary>
public static class BasketballProjectileSetup
{
    public const string BasketballName = "Basketball";

    /// <summary>Offset from Bob spawn to ball release (matches BobAgent.ResetProjectile).</summary>
    public static readonly Vector3 ReleaseOffset = new(0f, 0.15f, 0.2f);

    public const float BallScale = 0.24f;
    public const float BallMass = 0.6f;

    /// <summary>
    /// Ensures exactly one basketball under parent at the release position.
    /// </summary>
    public static GameObject EnsureBasketball(Transform parent, Vector3 worldReleasePosition)
    {
        var ball = FindExistingBall(parent);
        if (ball == null)
        {
            ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = BasketballName;
            ball.transform.SetParent(parent, false);
        }

        ball.transform.localScale = Vector3.one * BallScale;
        ball.transform.position = worldReleasePosition;
        ball.SetActive(true);
        BobPhysicsLayers.SetLayerRecursively(ball, BobPhysicsLayers.TrainingArenaLayer);

        var rb = ball.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = ball.AddComponent<Rigidbody>();
        }

        rb.mass = BallMass;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;
        BobPhysicsUtility.ClearVelocitiesIfDynamic(rb);

        var col = ball.GetComponent<SphereCollider>();
        if (col != null)
        {
            col.material = GetOrCreateBouncyMaterial();
        }

        ApplyBasketballMaterial(ball.GetComponent<Renderer>());

        if (!ball.TryGetComponent(out SimpleBasketball _))
        {
            ball.AddComponent<SimpleBasketball>();
        }

        EnsureTrailRenderer(ball);
        return ball;
    }

    public static void WireLauncher(BobAgent agent, Rigidbody ballRb)
    {
        if (agent == null || ballRb == null)
        {
            return;
        }

        agent.ConfigureProjectileLauncher(ballRb);

        if (!ballRb.TryGetComponent(out SimpleBasketball marker))
        {
            marker = ballRb.gameObject.AddComponent<SimpleBasketball>();
        }

        marker.Wire(agent);

        if (agent.TryGetComponent(out BobShotArcPreview preview))
        {
            preview.Bind(agent);
        }
    }

    public static Vector3 GetReleasePosition(Vector3 bobSpawnWorld)
    {
        return bobSpawnWorld + ReleaseOffset;
    }

    private static GameObject FindExistingBall(Transform parent)
    {
        var child = parent.Find(BasketballName);
        if (child != null)
        {
            return child.gameObject;
        }

        var sceneBall = GameObject.Find(BasketballName);
        return sceneBall;
    }

    private static void EnsureTrailRenderer(GameObject ball)
    {
        if (!ball.TryGetComponent(out TrailRenderer trail))
        {
            trail = ball.AddComponent<TrailRenderer>();
        }

        trail.time = 1.5f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0.02f;
        trail.minVertexDistance = 0.05f;
        trail.autodestruct = false;
        trail.emitting = false;

        var orange = new Color(1f, 0.45f, 0.08f, 0.85f);
        trail.startColor = orange;
        trail.endColor = new Color(orange.r, orange.g, orange.b, 0f);
        trail.material = ArcAcademyShaderUtility.CreateEmissiveLineMaterial(orange, 1.2f);
    }

    /// <summary>Enable trail only while the ball is moving (called from runtime helper).</summary>
    public static void UpdateTrailEmit(Rigidbody ballRb, float speedThreshold = 0.35f)
    {
        if (ballRb == null || !ballRb.TryGetComponent(out TrailRenderer trail))
        {
            return;
        }

        trail.emitting = ballRb.linearVelocity.sqrMagnitude > speedThreshold * speedThreshold;
    }

    private static void ApplyBasketballMaterial(Renderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = CreateLitMaterial(new Color(0.92f, 0.45f, 0.12f), 0.45f, 0f);
    }

    private static Material CreateLitMaterial(Color color, float smoothness, float metallic)
    {
        var shader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        else
        {
            mat.color = color;
        }

        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", smoothness);
        }

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }

        return mat;
    }

    private static PhysicsMaterial s_bouncyMaterial;

    private static PhysicsMaterial GetOrCreateBouncyMaterial()
    {
        if (s_bouncyMaterial != null)
        {
            return s_bouncyMaterial;
        }

        s_bouncyMaterial = new PhysicsMaterial("BasketballBounce")
        {
            bounciness = 0.75f,
            dynamicFriction = 0.35f,
            staticFriction = 0.35f,
            bounceCombine = PhysicsMaterialCombine.Maximum,
        };
        return s_bouncyMaterial;
    }
}
