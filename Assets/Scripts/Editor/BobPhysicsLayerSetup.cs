#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ensures Bob training physics layers exist and decorative geometry does not collide with Bob.
/// </summary>
public static class BobPhysicsLayerSetup
{
    private static readonly string[] RequiredLayers =
    {
        BobPhysicsLayers.Bob,
        BobPhysicsLayers.TrainingArena,
        BobPhysicsLayers.Decoration,
    };

    public static void EnsureLayersAndCollisionMatrix()
    {
        foreach (var layerName in RequiredLayers)
        {
            EnsureLayer(layerName);
        }

        if (!BobPhysicsLayers.LayersConfigured)
        {
            Debug.LogError("BobPhysicsLayerSetup: one or more training layers missing from TagManager");
            return;
        }

        Physics.IgnoreLayerCollision(BobPhysicsLayers.BobLayer, BobPhysicsLayers.DecorationLayer, true);
        Physics.IgnoreLayerCollision(BobPhysicsLayers.TrainingArenaLayer, BobPhysicsLayers.DecorationLayer, true);
        BobPhysicsLayers.EnsureCollisionMatrix();
    }

    private static void EnsureLayer(string layerName)
    {
        if (LayerMask.NameToLayer(layerName) >= 0)
        {
            return;
        }

        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty slot = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(slot.stringValue))
            {
                slot.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }

        Debug.LogError($"BobPhysicsLayerSetup: no free layer slot for {layerName}");
    }
}
#endif
