using UnityEngine;

/// <summary>
/// Swish and rim-contact particle bursts on the active hoop net.
/// </summary>
public class HoopSwishVfx : MonoBehaviour
{
    private ParticleSystem swishBurst;
    private ParticleSystem rimSpark;

    private void Awake()
    {
        swishBurst = CreateBurstSystem(
            "SwishBurst",
            new Color(1f, 1f, 1f, 0.85f),
            28,
            0.4f,
            0.06f,
            0.35f,
            ParticleSystemShapeType.Cone,
            new Vector3(-90f, 0f, 0f));

        rimSpark = CreateBurstSystem(
            "RimSpark",
            new Color(1f, 0.45f, 0.12f, 0.9f),
            16,
            0.25f,
            0.04f,
            0.22f,
            ParticleSystemShapeType.Hemisphere,
            Vector3.zero);
    }

    public void PlaySwish()
    {
        swishBurst?.Play(true);
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

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

        var shape = ps.shape;
        shape.shapeType = shapeType;
        shape.angle = shapeType == ParticleSystemShapeType.Cone ? 18f : 25f;
        shape.radius = 0.12f;
        shape.rotation = shapeRotation;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return ps;
    }
}
