#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates Arc Academy Shader Graph materials and mountain backdrop texture (idempotent).
/// </summary>
public static class ArcAcademyShaderGraphSetup
{
    [MenuItem("Bob/Create Arc Academy Shader Graph Materials")]
    public static void CreateMaterialsMenu()
    {
        EnsureMaterialLibrary();
        AssetDatabase.SaveAssets();
        Debug.Log("Arc Academy material library ready.");
    }

    public static void EnsureMaterialLibraryFromCli()
    {
        EnsureMaterialLibrary();
        AssetDatabase.SaveAssets();
        Debug.Log("MATERIAL_LIBRARY_OK: Arc Academy HDRP materials configured");
        EditorApplication.Exit(0);
    }

    public static void EnsureMaterialLibrary()
    {
        Directory.CreateDirectory(ArcAcademyMaterialPaths.ShadersFolder);
        Directory.CreateDirectory(ArcAcademyMaterialPaths.MaterialsFolder);
        Directory.CreateDirectory(ArcAcademyMaterialPaths.TexturesFolder);

        EnsureMountainBackdropTexture();
        EnsureMaterialAsset(
            ArcAcademyMaterialPaths.GlossyFloorMat,
            ArcAcademyMaterialPaths.GlossyFloorGraph,
            "Arc Academy Glossy Floor",
            Color.white,
            smoothness: 0.88f,
            metallic: 0.18f);

        EnsureMaterialAsset(
            ArcAcademyMaterialPaths.MatteWallMat,
            ArcAcademyMaterialPaths.MatteWallGraph,
            "Arc Academy Matte Wall",
            Color.white,
            smoothness: 0.15f,
            metallic: 0f);

        EnsureMaterialAsset(
            ArcAcademyMaterialPaths.MetalMat,
            ArcAcademyMaterialPaths.MetalGraph,
            "Arc Academy Metal",
            Color.white,
            smoothness: 0.55f,
            metallic: 0.85f);

        EnsureGlassMaterial();
        EnsureRubberMaterial();
        EnsureMountainBackdropMaterial();
    }

    private static void EnsureMountainBackdropTexture()
    {
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(ArcAcademyMaterialPaths.MountainBackdropTexture) != null)
        {
            return;
        }

        var tex = ArcAcademyMaterialFactory.CreateMountainWindowTexture(1024, 512);
        var png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);
        File.WriteAllBytes(ArcAcademyMaterialPaths.MountainBackdropTexture, png);
        AssetDatabase.ImportAsset(ArcAcademyMaterialPaths.MountainBackdropTexture);
    }

    private static void EnsureMaterialAsset(
        string matPath,
        string graphPath,
        string displayName,
        Color baseColor,
        float smoothness,
        float metallic)
    {
        EnsureShaderGraphPlaceholder(graphPath, displayName);

        var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (existing != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateHdrpLit(baseColor, smoothness, metallic);
        mat.name = Path.GetFileNameWithoutExtension(matPath);
        AssetDatabase.CreateAsset(mat, matPath);
    }

    private static void EnsureGlassMaterial()
    {
        EnsureShaderGraphPlaceholder(ArcAcademyMaterialPaths.GlassGraph, "Arc Academy Glass");
        if (AssetDatabase.LoadAssetAtPath<Material>(ArcAcademyMaterialPaths.GlassMat) != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateGlassBackboard(Color.white);
        mat.name = "ArcAcademyGlass";
        AssetDatabase.CreateAsset(mat, ArcAcademyMaterialPaths.GlassMat);
    }

    private static void EnsureRubberMaterial()
    {
        EnsureShaderGraphPlaceholder(ArcAcademyMaterialPaths.RubberGraph, "Arc Academy Rubber");
        if (AssetDatabase.LoadAssetAtPath<Material>(ArcAcademyMaterialPaths.RubberMat) != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateHdrpLit(new Color(0.12f, 0.1f, 0.08f), 0.25f, 0f);
        mat.name = "ArcAcademyRubber";
        AssetDatabase.CreateAsset(mat, ArcAcademyMaterialPaths.RubberMat);
    }

    private static void EnsureMountainBackdropMaterial()
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(ArcAcademyMaterialPaths.MountainBackdropMat) != null)
        {
            return;
        }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ArcAcademyMaterialPaths.MountainBackdropTexture);
        var mat = ArcAcademyMaterialFactory.CreateHdrpLit(Color.white, 0.1f, 0f);
        if (tex != null)
        {
            if (mat.HasProperty("_BaseColorMap"))
            {
                mat.SetTexture("_BaseColorMap", tex);
            }
            else
            {
                mat.mainTexture = tex;
            }
        }

        ArcAcademyMaterialFactory.SetEmissivePublic(mat, new Color(0.85f, 0.92f, 1f), 0.35f);
        mat.name = "ArcAcademyMountainBackdrop";
        AssetDatabase.CreateAsset(mat, ArcAcademyMaterialPaths.MountainBackdropMat);
    }

    /// <summary>
    /// Lightweight Shader Graph marker asset (HDRP Lit preset drives runtime look until graphs are hand-authored).
    /// </summary>
    private static void EnsureShaderGraphPlaceholder(string graphPath, string displayName)
    {
        if (File.Exists(graphPath))
        {
            return;
        }

        var markerPath = graphPath + ".meta";
        if (!File.Exists(markerPath))
        {
            File.WriteAllText(
                graphPath,
                $"// Arc Academy Shader Graph placeholder: {displayName}\n// Material preset uses HDRP/Lit; replace with authored graph when ready.\n");
            AssetDatabase.ImportAsset(graphPath);
        }
    }
}
#endif
