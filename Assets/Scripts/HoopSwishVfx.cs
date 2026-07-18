using UnityEngine;

/// <summary>
/// Swish and rim-contact particle bursts on the active hoop net.
/// Bursts use unscaled time and HDRP-friendly emissive billboards so makes stay
/// obvious during long training runs at high time scale.
/// </summary>
public class HoopSwishVfx : MonoBehaviour
{
    private ParticleSystem swishBurst;
    private ParticleSystem rimSpark;
    private ParticleSystem netRipple;
    private Material particleMaterial;

    private void Awake()
    {
        particleMaterial = CreateParticleMaterial();

        // Bright white cone through the net — primary "make" signal.
        swishBurst = CreateBurstSystem(
            "SwishBurst",
            new Color(1f, 1f, 1f, 1f),
            72,
            0.55f,
            0.14f,
            0.85f,
            ParticleSystemShapeType.Cone,
            new Vector3(-90f, 0f, 0f));

        // Warm orange sparks on rim clank (driven by HoopRimContact).
        rimSpark = CreateBurstSystem(
            "RimSpark",
            new Color(1f, 0.5f, 0.12f, 1f),
            36,
            0.35f,
            0.08f,
            0.55f,
            ParticleSystemShapeType.Hemisphere,
            Vector3.zero);

        // Soft cyan ripple under the net for depth on swish.
        netRipple = CreateBurstSystem(
            "NetRipple",
            new Color(0.75f, 0.95f, 1f, 0.85f),
            48,
            0.7f,
            0.1f,
            0.35f,
            ParticleSystemShapeType.Cone,
            new Vector3(-90f, 0f, 0f));
    }

    private void OnDestroy()
    {
        if (particleMaterial != null)
        {
            Destroy(particleMaterial);
        }
    }

    public void PlaySwish()
    {
        swishBurst?.Play(true);
        netRipple?.Play(true);
    }

    public void PlayRimContact()
    {
        rimSpark?.Play(true);
    }

    private ParticleSystem CreateBurstSystem(
        string name,
        Color startColor,
        int burstCount,
        float lifetime,
        float startSize,
        float startSpeed,
        ParticleSystemShapeType shapeType,
        Vector3 shapeRotation)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startColor = startColor;
        main.startLifetime = lifetime;
        main.startSize = startSize;
        main.startSpeed = startSpeed;
        main.maxParticles = burstCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        // Keep bursts visible when training runs at 10–20× time scale.
        main.useUnscaledTime = true;
        main.gravityModifier = shapeType == ParticleSystemShapeType.Cone ? 0.35f : 0.15f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

        var shape = ps.shape;
        shape.shapeType = shapeType;
        shape.angle = shapeType == ParticleSystemShapeType.Cone ? 28f : 40f;
        shape.radius = 0.18f;
        shape.rotation = shapeRotation;

        // Punch then fade — readable in a single glance at high speed.
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
            1f,
            AnimationCurve.EaseInOut(0f, 1.4f, 1f, 0.05f));

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(startColor, 0f),
                new GradientColorKey(startColor, 0.35f),
                new GradientColorKey(Color.white, 1f),
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.85f, 0.4f),
                new GradientAlphaKey(0f, 1f),
            });
        colorOverLifetime.color = gradient;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (particleMaterial != null)
        {
            renderer.sharedMaterial = particleMaterial;
        }

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return ps;
    }

    private static Material CreateParticleMaterial()
    {
        // Prefer HDRP unlit emissive so bursts punch through the lab lighting.
        var hdrpUnlit = Shader.Find("HDRP/Unlit");
        if (hdrpUnlit != null)
        {
            var mat = new Material(hdrpUnlit);
            var white = Color.white;
            mat.SetColor("_UnlitColor", white);
            mat.SetColor("_EmissiveColor", white * 2.5f);
            mat.SetFloat("_EmissiveIntensity", 2.5f * 300f);
            mat.EnableKeyword("_EMISSIVE_COLOR");
            return mat;
        }

        var particlesUnlit = Shader.Find("Particles/Standard Unlit");
        if (particlesUnlit != null)
        {
            var mat = new Material(particlesUnlit);
            mat.SetColor("_Color", Color.white);
            return mat;
        }

        return null;
    }
}
