#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// Procedural Standard materials for Arc Academy scene builder (no external texture assets).
/// </summary>
public static class ArcAcademyMaterialFactory
{
    public static Material CreateStandard(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }

    public static Material CreateGlossyFloor(Color color, float smoothness)
    {
        var mat = CreateStandard(color);
        mat.SetFloat("_Glossiness", smoothness);
        mat.SetFloat("_Metallic", 0.15f);
        return mat;
    }

    public static Material CreateEmissive(Color color, float intensity)
    {
        var mat = CreateStandard(color);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        return mat;
    }

    public static Texture2D CreateMountainWindowTexture(int width = 256, int height = 128)
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
        var mat = CreateStandard(Color.white);
        mat.mainTexture = CreateMountainWindowTexture();
        mat.SetFloat("_Glossiness", 0.2f);
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
}
#endif
