using UnityEngine;

/// <summary>
/// Ensures Bob's body uses the committed orange lab material (guards against null/default-white prefab state).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class BobVisualApplier : MonoBehaviour
{
    [SerializeField] private Material bodyMaterial;

    private void Awake()
    {
        ApplyBodyMaterial();
    }

    public void ApplyBodyMaterial()
    {
        if (!TryGetComponent(out Renderer renderer) || bodyMaterial == null)
        {
            return;
        }

        if (BobVisualProfile.IsLikelyMissingBodyMaterial(renderer.sharedMaterial))
        {
            renderer.sharedMaterial = bodyMaterial;
        }
    }

#if UNITY_EDITOR
    public void SetBodyMaterialAsset(Material material)
    {
        bodyMaterial = material;
        ApplyBodyMaterial();
    }
#endif
}
