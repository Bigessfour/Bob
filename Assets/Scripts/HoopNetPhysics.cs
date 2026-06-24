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
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
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
