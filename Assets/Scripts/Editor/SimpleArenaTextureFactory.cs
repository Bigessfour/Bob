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
    public const string WallTileTexturePath = TexturesFolder + "/Tex_Wall_Tile.png";

    public static Texture2D EnsureFloorGridTexture()
    {
        return EnsureTexture(
            FloorGridTexturePath,
            CreateFloorGridTexture,
            filterMode: FilterMode.Bilinear,
            wrapMode: TextureWrapMode.Repeat);
    }

    public static Texture2D EnsureWallTileTexture()
    {
        return EnsureTexture(
            WallTileTexturePath,
            CreateWallTileTexture,
            filterMode: FilterMode.Bilinear,
            wrapMode: TextureWrapMode.Repeat);
    }

    private static Texture2D EnsureTexture(
        string assetPath,
        System.Func<Texture2D> create,
        FilterMode filterMode,
        TextureWrapMode wrapMode)
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
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = wrapMode;
            importer.filterMode = filterMode;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
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
