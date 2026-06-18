#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// HDRP material library for Arc Academy — loads committed .mat assets with procedural fallback.
/// </summary>
public static class ArcAcademyMaterialFactory
{
    private static readonly Dictionary<string, Material> MaterialCache = new();

    private static Shader HdrpLitShader => Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
    private static Shader HdrpUnlitShader => Shader.Find("HDRP/Unlit") ?? Shader.Find("Standard");

    public static Material CreateStandard(Color color) => CreateHdrpLit(color, 0.35f, 0f);

    public static Material CreateHdrpLit(Color baseColor, float smoothness, float metallic)
    {
        var mat = new Material(HdrpLitShader);
        SetBaseColor(mat, baseColor);
        SetSmoothness(mat, smoothness);
        SetMetallic(mat, metallic);
        return mat;
    }

    public static Material CreateGlossyFloor(Color color, float smoothness)
    {
        return CreateHdrpLit(color, smoothness, 0.18f);
    }

    public static Material CreateEmissive(Color color, float intensity)
    {
        var mat = CreateHdrpLit(color, 0.4f, 0f);
        SetEmissive(mat, color, intensity);
        return mat;
    }

    public static Material CreateGlassBackboard(Color tint)
    {
        var mat = CreateHdrpLit(new Color(tint.r, tint.g, tint.b, 0.35f), 0.92f, 0.05f);
        if (HdrpLitShader.name.Contains("HDRP"))
        {
            mat.SetFloat("_SurfaceType", 1f);
            mat.SetFloat("_BlendMode", 0f);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_BLENDMODE_ALPHA");
            mat.renderQueue = 3000;
        }

        return mat;
    }

    public static Material GetGlossyFloor(Color tint)
    {
        return TintMaterial(
            LoadMaterial(ArcAcademyMaterialPaths.GlossyFloorMat)
            ?? CreateGlossyFloor(Color.white, 0.88f),
            tint);
    }

    public static Material GetMatteWall(Color tint)
    {
        return TintMaterial(
            LoadMaterial(ArcAcademyMaterialPaths.MatteWallMat)
            ?? CreateHdrpLit(Color.white, 0.15f, 0f),
            tint);
    }

    public static Material GetMetal(Color tint)
    {
        return TintMaterial(
            LoadMaterial(ArcAcademyMaterialPaths.MetalMat)
            ?? CreateHdrpLit(Color.white, 0.55f, 0.85f),
            tint);
    }

    public static Material GetGlass(Color tint)
    {
        var baseMat = LoadMaterial(ArcAcademyMaterialPaths.GlassMat);
        if (baseMat == null)
        {
            return CreateGlassBackboard(tint);
        }

        var inst = new Material(baseMat);
        SetBaseColor(inst, new Color(tint.r, tint.g, tint.b, 0.35f));
        return inst;
    }

    public static Material GetRubber(Color tint)
    {
        return TintMaterial(
            LoadMaterial(ArcAcademyMaterialPaths.RubberMat)
            ?? CreateHdrpLit(new Color(0.12f, 0.1f, 0.08f), 0.25f, 0f),
            tint);
    }

    public static Material GetMountainBackdrop()
    {
        var mat = LoadMaterial(ArcAcademyMaterialPaths.MountainBackdropMat);
        if (mat != null)
        {
            return mat;
        }

        return CreateMountainWindowMaterial();
    }

    public static bool MaterialLibraryLoaded =>
        LoadMaterial(ArcAcademyMaterialPaths.GlossyFloorMat) != null;

    public static Texture2D CreateMountainWindowTexture(int width = 1024, int height = 512)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int y = 0; y < height; y++)
        {
            float v = y / (float)(height - 1);
            for (int x = 0; x < width; x++)
            {
                float u = x / (float)(width - 1);
                Color pixel;
                if (v > 0.62f)
                {
                    float skyT = (v - 0.62f) / 0.38f;
                    float haze = Mathf.PerlinNoise(u * 3f, v * 2f) * 0.08f;
                    pixel = Color.Lerp(
                        new Color(0.58f, 0.76f, 0.98f),
                        new Color(0.32f, 0.52f, 0.92f),
                        skyT + haze);
                }
                else if (v > 0.22f)
                {
                    float mountainT = (v - 0.22f) / 0.4f;
                    float ridge = Mathf.PerlinNoise(u * 12f + 0.5f, v * 8f);
                    float ridge2 = Mathf.PerlinNoise(u * 5f, v * 14f) * 0.5f;
                    float heightMix = mountainT * 0.55f + ridge * 0.3f + ridge2 * 0.15f;
                    Color baseForest = new(0.18f, 0.32f, 0.16f);
                    Color midRock = new(0.38f, 0.42f, 0.36f);
                    Color snow = new(0.92f, 0.94f, 0.98f);
                    pixel = Color.Lerp(baseForest, midRock, heightMix);
                    if (heightMix > 0.72f)
                    {
                        pixel = Color.Lerp(pixel, snow, (heightMix - 0.72f) / 0.28f);
                    }
                }
                else
                {
                    pixel = new Color(0.12f, 0.18f, 0.1f);
                }

                tex.SetPixel(x, y, pixel);
            }
        }

        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    public static Material CreateMountainWindowMaterial()
    {
        var mat = CreateHdrpLit(Color.white, 0.15f, 0f);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ArcAcademyMaterialPaths.MountainBackdropTexture)
                  ?? CreateMountainWindowTexture();
        if (mat.HasProperty("_BaseColorMap"))
        {
            mat.SetTexture("_BaseColorMap", tex);
        }
        else
        {
            mat.mainTexture = tex;
        }

        SetEmissive(mat, new Color(0.85f, 0.92f, 1f), 0.35f);
        return mat;
    }

    public static Material CreateArcLineMaterial(Color color, float intensity)
    {
        var mat = new Material(HdrpUnlitShader);
        if (HdrpUnlitShader.name.Contains("HDRP"))
        {
            mat.SetColor("_UnlitColor", color);
            SetEmissive(mat, color, intensity);
        }
        else
        {
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * intensity);
        }

        return mat;
    }

    public static void ApplyMaterial(GameObject go, Material mat)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = mat;
        }
    }

    public static void SetEmissivePublic(Material mat, Color color, float intensity)
    {
        SetEmissive(mat, color, intensity);
    }

    private static Material LoadMaterial(string assetPath)
    {
        if (MaterialCache.TryGetValue(assetPath, out var cached))
        {
            return cached;
        }

        var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (mat != null)
        {
            MaterialCache[assetPath] = mat;
        }

        return mat;
    }

    private static Material TintMaterial(Material source, Color tint)
    {
        var inst = new Material(source);
        SetBaseColor(inst, tint);
        return inst;
    }

    private static void SetBaseColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        else
        {
            mat.color = color;
        }
    }

    private static void SetSmoothness(Material mat, float smoothness)
    {
        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", smoothness);
        }
        else if (mat.HasProperty("_Glossiness"))
        {
            mat.SetFloat("_Glossiness", smoothness);
        }
    }

    private static void SetMetallic(Material mat, float metallic)
    {
        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }
    }

    private static void SetEmissive(Material mat, Color color, float intensity)
    {
        if (mat.HasProperty("_EmissiveColor"))
        {
            mat.SetColor("_EmissiveColor", color);
            mat.SetFloat("_EmissiveIntensity", intensity * 1200f);
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
#endif
