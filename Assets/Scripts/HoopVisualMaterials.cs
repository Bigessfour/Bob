using UnityEngine;

/// <summary>
/// Runtime HDRP materials for gym/pro hoop visuals — tempered glass, breakaway rim, TuffGuard padding.
/// </summary>
public static class HoopVisualMaterials
{
    /// <summary>Safety-orange powder-coated breakaway steel (NCAA/NFHS gym standard).</summary>
    public static readonly Color RimOrangeColor = new(1f, 0.42f, 0.08f);

    /// <summary>Legacy alias — rim is orange in current art direction.</summary>
    public static readonly Color RimSilverColor = RimOrangeColor;

    public static readonly Color NetWhiteColor = new(0.97f, 0.98f, 1f, 1f);

    /// <summary>Legacy translucent net tint (prefer <see cref="CreateOpaqueNet"/>).</summary>
    public static readonly Color NetTranslucentColor = new(0.95f, 0.96f, 0.98f, 0.38f);

    /// <summary>42×72 tempered glass — mostly clear with a cool neutral tint.</summary>
    public static readonly Color BackboardGlassTint = new(0.92f, 0.95f, 0.98f, 0.18f);

    /// <summary>Royal blue bolt-on TuffGuard / PMCE edge padding.</summary>
    public static readonly Color BackboardPadBlue = new(0.06f, 0.16f, 0.48f);

    /// <summary>Anodized aluminum frame extrusion on competition backboards.</summary>
    public static readonly Color FrameAluminumColor = new(0.93f, 0.94f, 0.96f);

    /// <summary>Regulation white border and shooter's square fired onto glass.</summary>
    public static readonly Color RegulationMarkingWhite = new(0.98f, 0.98f, 0.99f);

    /// <summary>Reinforced steel rim-support channel along bottom of glass backboard.</summary>
    public static readonly Color SteelSupportColor = new(0.32f, 0.34f, 0.36f);

    /// <summary>White tubular net hanger loops welded to breakaway rim.</summary>
    public static readonly Color RimPigtailColor = new(0.95f, 0.96f, 0.98f);

    public const float RimMetallic = 0.88f;
    public const float RimSmoothness = 0.92f;
    public const float RimEmissiveIntensity = 0.06f;
    public const float NetSmoothness = 0.12f;
    public const float BackboardGlassSmoothness = 0.98f;
    public const float GlassIor = 1.52f;
    public const float FrameAluminumSmoothness = 0.72f;
    public const float FrameAluminumMetallic = 0.82f;
    public const float PadVinylSmoothness = 0.18f;
    public const float SteelSupportSmoothness = 0.55f;
    public const float SteelSupportMetallic = 0.78f;

    public static Material CreateRimOrange()
    {
        var mat = CreateHdrpLit(RimOrangeColor, RimSmoothness, RimMetallic);
        ApplySubtleEmissive(mat, RimOrangeColor, RimEmissiveIntensity);
        return mat;
    }

    public static Material CreateRimSilver() => CreateRimOrange();

    public static Material CreateOpaqueNet()
    {
        return CreateHdrpLit(NetWhiteColor, NetSmoothness, 0f);
    }

    public static Material CreateTranslucentNet()
    {
        var mat = CreateHdrpLit(NetTranslucentColor, NetSmoothness, 0f);
        ApplyTransparentSurface(mat);
        return mat;
    }

    public static Material CreateGlassBackboard() => CreateGymProGlassBackboard();

    public static Material CreateGymProGlassBackboard()
    {
        var mat = CreateHdrpLit(BackboardGlassTint, BackboardGlassSmoothness, 0.02f);
        ApplyTransparentSurface(mat);
        ApplyGlassRefraction(mat);
        return mat;
    }

    public static Material CreateFrameAluminum()
    {
        return CreateHdrpLit(FrameAluminumColor, FrameAluminumSmoothness, FrameAluminumMetallic);
    }

    public static Material CreatePadVinyl()
    {
        return CreateHdrpLit(BackboardPadBlue, PadVinylSmoothness, 0f);
    }

    public static Material CreateSteelSupport()
    {
        return CreateHdrpLit(SteelSupportColor, SteelSupportSmoothness, SteelSupportMetallic);
    }

    public static Material CreateRegulationMarking()
    {
        return CreateHdrpLit(RegulationMarkingWhite, 0.35f, 0f);
    }

    public static Material CreateRimPigtail()
    {
        return CreateHdrpLit(RimPigtailColor, 0.65f, 0.55f);
    }

    public static void ApplyTransparentSurface(Material mat)
    {
        if (mat == null || !mat.shader.name.Contains("HDRP"))
        {
            return;
        }

        mat.SetFloat("_SurfaceType", 1f);
        mat.SetFloat("_BlendMode", 0f);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_BLENDMODE_ALPHA");
        mat.renderQueue = 3000;
    }

    public static void ApplyGlassRefraction(Material mat)
    {
        if (mat == null || !mat.shader.name.Contains("HDRP"))
        {
            return;
        }

        if (mat.HasProperty("_Ior"))
        {
            mat.SetFloat("_Ior", GlassIor);
        }

        if (mat.HasProperty("_Thickness"))
        {
            mat.SetFloat("_Thickness", 0.012f);
        }

        if (mat.HasProperty("_RefractionModel"))
        {
            mat.SetFloat("_RefractionModel", 1f);
        }

        if (mat.HasProperty("_TransmittanceColor"))
        {
            mat.SetColor("_TransmittanceColor", Color.white);
        }
    }

    private static Material CreateHdrpLit(Color baseColor, float smoothness, float metallic)
    {
        var shader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", baseColor);
        }
        else
        {
            mat.color = baseColor;
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

    private static void ApplySubtleEmissive(Material mat, Color color, float intensity)
    {
        if (mat.HasProperty("_EmissiveColor"))
        {
            mat.SetColor("_EmissiveColor", color);
            mat.SetFloat("_EmissiveIntensity", intensity * 300f);
            mat.EnableKeyword("_EMISSIVE_COLOR");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * intensity);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }
}
