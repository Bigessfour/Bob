using UnityEngine;

/// <summary>
/// Lightweight chain net under the active rim — reacts to Bob passing through.
/// </summary>
public class HoopNetPhysics : MonoBehaviour
{
    [SerializeField] private int strandCount = 10;
    [SerializeField] private int segmentsPerStrand = 3;
    [SerializeField] private float rimAttachRadius = 0.32f;
    [SerializeField] private float segmentLength = 0.12f;
    [SerializeField] private float segmentMass = 0.015f;

    [Header("Swish Visual & Audio Feedback")]
    public ParticleSystem swishParticles;
    public AudioClip swishSound;

    private void Awake()
    {
        if (swishParticles == null)
        {
            swishParticles = EnsureSwishParticles();
        }
    }

    /// <summary>
    /// Called by <see cref="HoopScoreZone"/> on a clean swish (no recent rim contact).
    /// </summary>
    public void PlaySwishFeedback()
    {
        if (swishParticles != null)
        {
            swishParticles.Play();
        }

        if (swishSound != null)
        {
            AudioSource.PlayClipAtPoint(swishSound, transform.position, 1.0f);
        }
    }

    /// <summary>Runtime burst so swishes show without Inspector wiring.</summary>
    private ParticleSystem EnsureSwishParticles()
    {
        var go = new GameObject("NetSwishParticles");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.4f;
        main.startLifetime = 0.45f;
        main.startSize = 0.1f;
        main.startSpeed = 1.8f;
        main.startColor = new Color(0.85f, 1f, 0.95f, 0.95f);
        main.maxParticles = 64;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.15f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        return ps;
    }

    public void BuildNet(Transform rim, Material strandMaterial, PhysicsMaterial strandPhysic)
    {
        BuildNet(rim, strandMaterial, strandPhysic, physicsColliders: false);
    }

    public void BuildNet(
        Transform rim,
        Material strandMaterial,
        PhysicsMaterial strandPhysic,
        bool physicsColliders)
    {
        ClearExistingStrands();

        if (physicsColliders)
        {
            BuildPhysicsNet(rim, strandMaterial, strandPhysic);
        }
        else
        {
            BuildVisualNet(strandMaterial);
        }
    }

    /// <summary>Replaces a physics net with stable visual-only strands (training default).</summary>
    public void RebuildVisualOnly(Transform rim, Color strandColor)
    {
        ClearExistingStrands();
        BuildVisualNet(HoopVisualMaterials.CreateOpaqueNet());
    }

    private Material strandMaterialFromColor(Color color)
    {
        var mat = HoopVisualMaterials.CreateOpaqueNet();
        if (mat.HasProperty("_BaseColor"))
        {
            var tinted = color;
            tinted.a = 1f;
            mat.SetColor("_BaseColor", tinted);
        }

        return mat;
    }

    private void ClearExistingStrands()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            // Keep runtime swish VFX child across net rebuilds.
            if (child.name == "NetSwishParticles"
                || (swishParticles != null && child == swishParticles.gameObject))
            {
                continue;
            }

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    private void BuildVisualNet(Material strandMaterial)
    {
        for (int s = 0; s < strandCount; s++)
        {
            float angle = s / (float)strandCount * Mathf.PI * 2f;
            var segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segment.name = $"NetStrand_{s}";
            segment.transform.SetParent(transform);
            segment.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * rimAttachRadius,
                -0.18f,
                Mathf.Sin(angle) * rimAttachRadius);
            segment.transform.localScale = new Vector3(0.014f, 0.2f, 0.014f);

            if (strandMaterial != null)
            {
                segment.GetComponent<Renderer>().sharedMaterial = strandMaterial;
            }

            if (Application.isPlaying)
                Destroy(segment.GetComponent<Collider>());
            else
                DestroyImmediate(segment.GetComponent<Collider>());
        }
    }

    private void BuildPhysicsNet(Transform rim, Material strandMaterial, PhysicsMaterial strandPhysic)
    {
        var rimBody = rim.GetComponent<Rigidbody>();

        for (int s = 0; s < strandCount; s++)
        {
            float angle = s / (float)strandCount * Mathf.PI * 2f;
            Vector3 attachLocal = new Vector3(
                Mathf.Cos(angle) * rimAttachRadius,
                -0.06f,
                Mathf.Sin(angle) * rimAttachRadius);

            Rigidbody previousBody = rimBody;
            Transform previousTransform = rim;

            for (int i = 0; i < segmentsPerStrand; i++)
            {
                var segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                segment.name = $"NetSeg_{s}_{i}";
                segment.transform.SetParent(transform);
                segment.transform.localScale = new Vector3(0.015f, segmentLength * 0.5f, 0.015f);

                if (strandMaterial != null)
                {
                    segment.GetComponent<Renderer>().sharedMaterial = strandMaterial;
                }

                var col = segment.GetComponent<CapsuleCollider>();
                if (col != null && strandPhysic != null)
                {
                    col.material = strandPhysic;
                }

                var rb = segment.AddComponent<Rigidbody>();
                rb.mass = segmentMass;
                rb.linearDamping = 0.8f;
                rb.angularDamping = 0.9f;
                rb.useGravity = true;

                Vector3 worldAttach = rim.TransformPoint(attachLocal + Vector3.down * (i * segmentLength));
                segment.transform.position = worldAttach;

                var joint = segment.AddComponent<ConfigurableJoint>();
                joint.connectedBody = previousBody;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = new Vector3(0f, 1f, 0f);
                joint.connectedAnchor = i == 0
                    ? previousTransform.InverseTransformPoint(rim.TransformPoint(attachLocal))
                    : new Vector3(0f, -1f, 0f);

                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Locked;
                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularZMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit linearLimit = joint.linearLimit;
                linearLimit.limit = segmentLength * 1.1f;
                joint.linearLimit = linearLimit;

                previousBody = rb;
                previousTransform = segment.transform;
            }
        }
    }
}
