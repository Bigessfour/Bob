using UnityEngine;

/// <summary>
/// Physics layer names for Bob training — Bob collides with TrainingArena only, not Decoration.
/// </summary>
public static class BobPhysicsLayers
{
    public const string Bob = "Bob";
    public const string TrainingArena = "TrainingArena";
    public const string Decoration = "Decoration";

    public static int BobLayer => LayerMask.NameToLayer(Bob);

    public static int TrainingArenaLayer => LayerMask.NameToLayer(TrainingArena);

    public static int DecorationLayer => LayerMask.NameToLayer(Decoration);

    public static bool LayersConfigured =>
        BobLayer >= 0 && TrainingArenaLayer >= 0 && DecorationLayer >= 0;

    public static void EnsureCollisionMatrix()
    {
        if (!LayersConfigured)
        {
            return;
        }

        Physics.IgnoreLayerCollision(BobLayer, DecorationLayer, true);
        Physics.IgnoreLayerCollision(TrainingArenaLayer, DecorationLayer, true);
    }

    public static void SetLayerRecursively(GameObject root, int layer)
    {
        if (root == null || layer < 0)
        {
            return;
        }

        root.layer = layer;
        foreach (Transform child in root.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
