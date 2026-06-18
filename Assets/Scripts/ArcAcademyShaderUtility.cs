using UnityEngine;

/// <summary>
/// Runtime shader helpers shared by trajectory visuals and other play-mode effects.
/// </summary>
public static class ArcAcademyShaderUtility
{
    public static Material CreateEmissiveLineMaterial(Color color, float intensity)
    {
        var hdrpUnlit = Shader.Find("HDRP/Unlit");
        if (hdrpUnlit != null)
        {
            var mat = new Material(hdrpUnlit);
            mat.SetColor("_UnlitColor", color);
            mat.SetColor("_EmissiveColor", color * intensity);
            mat.SetFloat("_EmissiveIntensity", intensity * 1200f);
            mat.EnableKeyword("_EMISSIVE_COLOR");
            return mat;
        }

        var standard = Shader.Find("Standard");
        var fallback = new Material(standard);
        fallback.EnableKeyword("_EMISSION");
        fallback.color = color;
        fallback.SetColor("_EmissionColor", color * intensity);
        fallback.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        return fallback;
    }
}
