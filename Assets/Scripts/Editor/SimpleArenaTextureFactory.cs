#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Procedural grid/tile textures for AI Warehouse–style SimpleArcAcademyArena materials.
/// </summary>
public static class SimpleArenaTextureFactory
{
    public const string TexturesFolder = "Assets/Materials/SimpleArena";
    public const string FloorGridTexturePath = TexturesFolder + "/Tex_Floor_Grid.png";
    public const string FloorHardwoodTexturePath = TexturesFolder + "/Tex_Floor_Hardwood.png";
    public const string FloorHardwoodNormalPath = TexturesFolder + "/Tex_Floor_Hardwood_Normal.png";
    public const string WallTileTexturePath = TexturesFolder + "/Tex_Wall_Tile.png";

    private const int HardwoodTextureVersion = 2;

    public static Texture2D EnsureFloorGridTexture()
    {
        return EnsureTexture(
            FloorGridTexturePath,
            CreateFloorGridTexture,
            filterMode: FilterMode.Bilinear,
            wrapMode: TextureWrapMode.Repeat,
            sRgb: true,
            normalMap: false);
    }

    public static Texture2D EnsureHardwoodFloorTexture()
    {
        RegenerateHardwoodTexturesIfStale();
        return EnsureTexture(
            FloorHardwoodTexturePath,
            CreateHardwoodFloorTexture,
            filterMode: FilterMode.Bilinear,
            wrapMode: TextureWrapMode.Repeat,
            sRgb: true,
            normalMap: false);
    }

    public static Texture2D EnsureHardwoodFloorNormalTexture()
    {
        RegenerateHardwoodTexturesIfStale();
        return EnsureTexture(
            FloorHardwoodNormalPath,
            CreateHardwoodFloorNormalTexture,
            filterMode: FilterMode.Bilinear,
            wrapMode: TextureWrapMode.Repeat,
            sRgb: false,
            normalMap: true);
    }

    public static Texture2D EnsureWallTileTexture()
    {
        return EnsureTexture(
            WallTileTexturePath,
            CreateWallTileTexture,
            filterMode: FilterMode.Bilinear,
            wrapMode: TextureWrapMode.Repeat,
            sRgb: true,
            normalMap: false);
    }

    private static Texture2D EnsureTexture(
        string assetPath,
        System.Func<Texture2D> create,
        FilterMode filterMode,
        TextureWrapMode wrapMode,
        bool sRgb,
        bool normalMap)
    {
        Directory.CreateDirectory(TexturesFolder);

        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (existing != null)
        {
            return existing;
        }

        var tex = create();
        tex.filterMode = filterMode;
        tex.wrapMode = wrapMode;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(assetPath);

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.wrapMode = wrapMode;
            importer.filterMode = filterMode;
            importer.sRGBTexture = sRgb;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
    }

    private static void RegenerateHardwoodTexturesIfStale()
    {
        var versionPath = TexturesFolder + "/.hardwood_version.txt";
        var fullVersionPath = Path.Combine(Directory.GetCurrentDirectory(), versionPath);
        var expected = HardwoodTextureVersion.ToString();
        var current = File.Exists(fullVersionPath) ? File.ReadAllText(fullVersionPath).Trim() : string.Empty;

        if (current == expected)
        {
            return;
        }

        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), TexturesFolder));
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(FloorHardwoodTexturePath) != null)
        {
            AssetDatabase.DeleteAsset(FloorHardwoodTexturePath);
        }

        if (AssetDatabase.LoadAssetAtPath<Texture2D>(FloorHardwoodNormalPath) != null)
        {
            AssetDatabase.DeleteAsset(FloorHardwoodNormalPath);
        }

        File.WriteAllText(fullVersionPath, expected);
    }

    private static Texture2D CreateFloorGridTexture()
    {
        const int size = 128;
        const int cells = 8;
        const int line = 2;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        var dark = new Color(0.18f, 0.18f, 0.2f, 1f);
        var lineColor = new Color(0.92f, 0.92f, 0.94f, 1f);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int cellX = x % (size / cells);
                int cellY = y % (size / cells);
                bool onLine = cellX < line || cellY < line;
                pixels[y * size + x] = onLine ? lineColor : dark;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(true);
        return tex;
    }

    private static Texture2D CreateHardwoodFloorTexture()
    {
        const int width = 256;
        const int height = 128;
        const int plankCount = 16;
        const int plankGap = 1;

        var tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
        var pixels = new Color[width * height];

        var plankLight = new Color(0.86f, 0.74f, 0.58f, 1f);
        var plankMid = new Color(0.78f, 0.66f, 0.50f, 1f);
        var plankDark = new Color(0.68f, 0.56f, 0.42f, 1f);
        var seam = new Color(0.48f, 0.38f, 0.28f, 1f);

        int plankWidth = width / plankCount;

        // Planks run along texture V (world Z / court length), seams across U (world X).
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int plankIndex = x / plankWidth;
                int localX = x % plankWidth;
                bool onSeam = localX < plankGap;

                Color basePlank = (plankIndex % 3) switch
                {
                    0 => plankLight,
                    1 => plankMid,
                    _ => plankDark,
                };

                float alongPlank = y / (float)height;
                float grain = Mathf.PerlinNoise(plankIndex * 0.37f, alongPlank * 4f) * 0.05f;
                float streak = Mathf.PerlinNoise(plankIndex * 0.21f, y * 0.06f) * 0.06f;
                Color pixel = onSeam ? seam : basePlank * (1f + grain + streak);
                pixel.a = 1f;
                pixels[y * width + x] = pixel;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(true);
        return tex;
    }

    private static Texture2D CreateHardwoodFloorNormalTexture()
    {
        var albedo = CreateHardwoodFloorTexture();
        const int width = 256;
        const int height = 128;
        var normal = new Texture2D(width, height, TextureFormat.RGBA32, true);
        var pixels = new Color[width * height];
        const float strength = 2.4f;

        float SampleHeight(int x, int y)
        {
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);
            return albedo.GetPixel(x, y).grayscale;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (SampleHeight(x + 1, y) - SampleHeight(x - 1, y)) * strength;
                float dy = (SampleHeight(x, y + 1) - SampleHeight(x, y - 1)) * strength;
                var normalVector = new Vector3(-dx, -dy, 1f).normalized;
                pixels[y * width + x] = new Color(
                    normalVector.x * 0.5f + 0.5f,
                    normalVector.y * 0.5f + 0.5f,
                    normalVector.z * 0.5f + 0.5f,
                    1f);
            }
        }

        Object.DestroyImmediate(albedo);
        normal.SetPixels(pixels);
        normal.Apply(true);
        return normal;
    }

    private static Texture2D CreateWallTileTexture()
    {
        const int size = 64;
        const int tile = 14;
        const int grout = 2;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        var tileColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        var groutColor = new Color(0.78f, 0.78f, 0.8f, 1f);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int modX = x % (tile + grout);
                int modY = y % (tile + grout);
                bool isGrout = modX < grout || modY < grout;
                pixels[y * size + x] = isGrout ? groutColor : tileColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(true);
        return tex;
    }
}
#endif
