#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates Arc Academy HDRP material library (.mat presets on HDRP/Lit).
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
        Directory.CreateDirectory(ArcAcademyMaterialPaths.MaterialsFolder);
        Directory.CreateDirectory(ArcAcademyMaterialPaths.TexturesFolder);

        EnsureMountainBackdropTexture();
        EnsureMaterialAsset(
            ArcAcademyMaterialPaths.GlossyFloorMat,
            "ArcAcademyGlossyFloor",
            Color.white,
            smoothness: 0.88f,
            metallic: 0.18f);

        EnsureMaterialAsset(
            ArcAcademyMaterialPaths.MatteWallMat,
            "ArcAcademyMatteWall",
            Color.white,
            smoothness: 0.15f,
            metallic: 0f);

        EnsureMaterialAsset(
            ArcAcademyMaterialPaths.MetalMat,
            "ArcAcademyMetal",
            Color.white,
            smoothness: 0.55f,
            metallic: 0.85f);

        EnsureGlassMaterial();
        EnsureRubberMaterial();
        EnsureRimMaterial();
        EnsureNetMaterial();
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
        string assetName,
        Color baseColor,
        float smoothness,
        float metallic)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (existing != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateHdrpLit(baseColor, smoothness, metallic);
        mat.name = assetName;
        AssetDatabase.CreateAsset(mat, matPath);
    }

    private static void EnsureGlassMaterial()
    {
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
        if (AssetDatabase.LoadAssetAtPath<Material>(ArcAcademyMaterialPaths.RubberMat) != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateHdrpLit(new Color(0.12f, 0.1f, 0.08f), 0.25f, 0f);
        mat.name = "ArcAcademyRubber";
        AssetDatabase.CreateAsset(mat, ArcAcademyMaterialPaths.RubberMat);
    }

    private static void EnsureRimMaterial()
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(ArcAcademyMaterialPaths.RimMat) != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateRimOrangeMaterial();
        mat.name = "ArcAcademyRim";
        AssetDatabase.CreateAsset(mat, ArcAcademyMaterialPaths.RimMat);
    }

    private static void EnsureNetMaterial()
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(ArcAcademyMaterialPaths.NetMat) != null)
        {
            return;
        }

        var mat = ArcAcademyMaterialFactory.CreateOpaqueNetMaterial();
        mat.name = "ArcAcademyNet";
        AssetDatabase.CreateAsset(mat, ArcAcademyMaterialPaths.NetMat);
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

        ArcAcademyMaterialFactory.SetEmissivePublic(mat, new Color(0.85f, 0.92f, 1f), 0.2f);
        mat.name = "ArcAcademyMountainBackdrop";
        AssetDatabase.CreateAsset(mat, ArcAcademyMaterialPaths.MountainBackdropMat);
    }
}
#endif
