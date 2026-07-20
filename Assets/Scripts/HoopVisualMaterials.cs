using UnityEngine;

/// <summary>
/// Runtime HDRP materials for gym/pro hoop visuals — tempered glass, breakaway rim, TuffGuard padding.
/// Tuned for Arc Academy Lab readability (Unity HDRP Lit + Thin refraction docs).
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

    /// <summary>
    /// Tempered glass tint — higher alpha so the board reads as one solid face (less layered).
    /// </summary>
    public static readonly Color BackboardGlassTint = new(0.94f, 0.95f, 0.97f, 0.58f);

    /// <summary>Cool transmittance for Thin glass refraction (RGB &lt; 1 for absorption).</summary>
    public static readonly Color GlassTransmittance = new(0.82f, 0.90f, 0.96f, 1f);

    /// <summary>Royal blue bolt-on TuffGuard / PMCE edge padding.</summary>
    public static readonly Color BackboardPadBlue = new(0.06f, 0.16f, 0.48f);

    /// <summary>Anodized aluminum frame extrusion on competition backboards.</summary>
    public static readonly Color FrameAluminumColor = new(0.93f, 0.94f, 0.96f);

    /// <summary>Regulation white border and shooter's square fired onto glass.</summary>
    public static readonly Color RegulationMarkingWhite = new(1f, 1f, 1f);

    /// <summary>Product-style orange/red shooter's square (mini-hoop target photo).</summary>
    public static readonly Color ProductMarkingColor = new(0.95f, 0.28f, 0.12f);

    /// <summary>Reinforced steel rim-support channel along bottom of glass backboard.</summary>
    public static readonly Color SteelSupportColor = new(0.32f, 0.34f, 0.36f);

    /// <summary>White tubular net hanger loops welded to breakaway rim.</summary>
    public static readonly Color RimPigtailColor = new(0.95f, 0.96f, 0.98f);

    // Powder-coat: mid metallic + clear coat for lab key-light pop (HDRP Lit docs).
    public const float RimMetallic = 0.57f;
    public const float RimSmoothness = 0.92f;
    public const float RimCoatMask = 0.48f;
    public const float RimEmissiveIntensity = 0.15f;
    public const float RimHeroEmissiveIntensity = 0.28f;
    public const float NetSmoothness = 0.18f;
    public const float NetEmissiveIntensity = 0.04f;
    public const float BackboardGlassSmoothness = 0.97f;
    public const float GlassIor = 1.52f;
    /// <summary>HDRP ScreenSpaceRefraction.RefractionModel.Thin — flat backboard.</summary>
    public const float GlassRefractionModelThin = 3f;
    public const float GlassAbsorptionDistance = 0.30f;
    public const float FrameAluminumSmoothness = 0.78f;
    public const float FrameAluminumMetallic = 0.82f;
    public const float PadVinylSmoothness = 0.18f;
    public const float SteelSupportSmoothness = 0.55f;
    public const float SteelSupportMetallic = 0.78f;
    public const float MarkingSmoothness = 0.60f;

    public static Material CreateRimOrange()
    {
        var mat = CreateHdrpLit(RimOrangeColor, RimSmoothness, RimMetallic);
        ApplyClearCoat(mat, RimCoatMask);
        float emissive = IsLabShowcase() ? RimHeroEmissiveIntensity : RimEmissiveIntensity;
        ApplySubtleEmissive(mat, RimOrangeColor, emissive);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    public static Material CreateRimSilver() => CreateRimOrange();

    public static Material CreateOpaqueNet()
    {
        var mat = CreateHdrpLit(NetWhiteColor, NetSmoothness, 0f);
        ApplySubtleEmissive(mat, NetWhiteColor, NetEmissiveIntensity);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    public static Material CreateTranslucentNet()
    {
        var mat = CreateHdrpLit(NetTranslucentColor, NetSmoothness, 0f);
        ApplyTransparentSurface(mat);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    public static Material CreateGlassBackboard() => CreateGymProGlassBackboard();

    public static Material CreateGymProGlassBackboard()
    {
        var mat = CreateHdrpLit(BackboardGlassTint, BackboardGlassSmoothness, 0.02f);
        ApplyTransparentSurface(mat);
        ApplyGlassRefraction(mat);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    /// <summary>
    /// Product-style training target: higher-opacity white/glass face, still Thin-refractive.
    /// </summary>
    public static Material CreateProductBackboard()
    {
        var mat = CreateHdrpLit(BackboardGlassTint, BackboardGlassSmoothness, 0.02f);
        ApplyTransparentSurface(mat);
        ApplyGlassRefraction(mat);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    public static Material CreateFrameAluminum()
    {
        var mat = CreateHdrpLit(FrameAluminumColor, FrameAluminumSmoothness, FrameAluminumMetallic);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    public static Material CreatePadVinyl()
    {
        return CreateHdrpLit(BackboardPadBlue, PadVinylSmoothness, 0f);
    }

    public static Material CreateSteelSupport()
    {
        var mat = CreateHdrpLit(SteelSupportColor, SteelSupportSmoothness, SteelSupportMetallic);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    public static Material CreateRegulationMarking()
    {
        // Smooth white so border / shooter's square pop against Thin glass.
        var mat = CreateHdrpLit(RegulationMarkingWhite, MarkingSmoothness, 0f);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    /// <summary>Opaque orange/red shooter's square for the product-style mini-hoop target.</summary>
    public static Material CreateProductMarking()
    {
        var mat = CreateHdrpLit(ProductMarkingColor, MarkingSmoothness, 0f);
        ApplyLabReadabilityBoost(mat);
        return mat;
    }

    public static Material CreateRimPigtail()
    {
        return CreateHdrpLit(RimPigtailColor, 0.65f, 0.55f);
    }

    /// <summary>
    /// Slight smoothness bump for LabShowcase portfolio / hero readability.
    /// No-op in Warehouse training mode.
    /// </summary>
    public static void ApplyLabReadabilityBoost(Material mat)
    {
        if (mat == null || !IsLabShowcase() || !mat.HasProperty("_Smoothness"))
        {
            return;
        }

        float current = mat.GetFloat("_Smoothness");
        mat.SetFloat("_Smoothness", Mathf.Min(0.98f, current + 0.03f));
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

    /// <summary>
    /// HDRP Thin glass: IoR 1.52, transmittance tint, absorption for readable thickness
    /// (see Unity Manual — Create a refractive material).
    /// </summary>
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
            // Thin model uses a fixed plate approximation; keep a small thickness hint.
            mat.SetFloat("_Thickness", 0.02f);
        }

        if (mat.HasProperty("_RefractionModel"))
        {
            mat.SetFloat("_RefractionModel", GlassRefractionModelThin);
        }

        if (mat.HasProperty("_TransmittanceColor"))
        {
            mat.SetColor("_TransmittanceColor", GlassTransmittance);
        }

        if (mat.HasProperty("_ATDistance"))
        {
            mat.SetFloat("_ATDistance", GlassAbsorptionDistance);
        }

        mat.EnableKeyword("_REFRACTION_THIN");
        mat.DisableKeyword("_REFRACTION_PLANE");
        mat.DisableKeyword("_REFRACTION_SPHERE");
    }

    private static bool IsLabShowcase()
    {
        return ArcAcademyLayout.CurrentMode == ArcAcademyLayout.VisualMode.LabShowcase;
    }

    private static void ApplyClearCoat(Material mat, float coatMask)
    {
        if (mat == null || !mat.shader.name.Contains("HDRP"))
        {
            return;
        }

        if (mat.HasProperty("_CoatMask"))
        {
            mat.SetFloat("_CoatMask", coatMask);
        }

        if (coatMask > 0f)
        {
            mat.EnableKeyword("_MATERIAL_FEATURE_CLEAR_COAT");
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

        if (mat.HasProperty("_OcclusionStrength"))
        {
            mat.SetFloat("_OcclusionStrength", 1f);
        }

        if (mat.HasProperty("_SpecularOcclusionMode"))
        {
            // From AO — better integration with lab probes / ambient.
            mat.SetFloat("_SpecularOcclusionMode", 1f);
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
