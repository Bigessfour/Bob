#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// Procedural HDRP Lit materials for Arc Academy scene builder (falls back to Standard if HDRP unavailable).
/// </summary>
public static class ArcAcademyMaterialFactory
{
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

    public static Texture2D CreateMountainWindowTexture(int width = 512, int height = 256)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int y = 0; y < height; y++)
        {
            float v = y / (float)(height - 1);
            for (int x = 0; x < width; x++)
            {
                Color pixel;
                if (v > 0.55f)
                {
                    float skyT = (v - 0.55f) / 0.45f;
                    pixel = Color.Lerp(new Color(0.55f, 0.72f, 0.95f), new Color(0.35f, 0.55f, 0.9f), skyT);
                }
                else if (v > 0.25f)
                {
                    float mountainT = (v - 0.25f) / 0.3f;
                    float ridge = Mathf.PerlinNoise(x * 0.08f, v * 6f);
                    pixel = Color.Lerp(
                        new Color(0.25f, 0.45f, 0.3f),
                        new Color(0.5f, 0.55f, 0.45f),
                        mountainT * 0.6f + ridge * 0.4f);
                }
                else
                {
                    pixel = new Color(0.15f, 0.22f, 0.12f);
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
        var tex = CreateMountainWindowTexture();
        if (mat.HasProperty("_BaseColorMap"))
        {
            mat.SetTexture("_BaseColorMap", tex);
        }
        else
        {
            mat.mainTexture = tex;
        }

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
