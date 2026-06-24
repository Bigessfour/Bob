using UnityEngine;

/// <summary>
/// Shared Bob body/face visual constants — AI Warehouse–style readable orange cube agent.
/// </summary>
public static class BobVisualProfile
{
    public const string BodyMaterialAssetPath = "Assets/Materials/HDRP/BobBodyOrange.mat";

    /// <summary>Matte training-lab orange (Albert-style, not neon).</summary>
    public static readonly Color BodyOrange = new(1f, 0.48f, 0.06f);

    /// <summary>Subtle warm emissive — readable under flat lab lighting without bloom washout.</summary>
    public const float BodyGlowIntensity = 0.24f;

    public const float BodySmoothness = 0.22f;
    public const float BodyMetallic = 0f;

    /// <summary>Orange cube agent scale in the lab (half-height ≈ 0.21 m on floor).</summary>
    public const float AgentCubeScale = 0.42f;

    /// <summary>Score celebration pulse multiplier on base emissive.</summary>
    public const float ScorePulseGlowMultiplier = 1.65f;

    /// <summary>Rich-text orange for speech bubble name highlight.</summary>
    public static readonly Color NameHighlightColor = BodyOrange;

    public static string NameHighlightHex => "#FF7A0F";

    public static string FormatPraise(string message, bool includeName = true)
    {
        if (!includeName)
        {
            return message;
        }

        return message.Replace("Bob", $"<color={NameHighlightHex}>Bob</color>");
    }

    public static bool IsLikelyMissingBodyMaterial(Material material)
    {
        if (material == null)
        {
            return true;
        }

        if (!material.HasProperty("_BaseColor"))
        {
            return false;
        }

        Color baseColor = material.GetColor("_BaseColor");
        return baseColor.grayscale > 0.92f && baseColor.r - baseColor.g < 0.08f;
    }
}
