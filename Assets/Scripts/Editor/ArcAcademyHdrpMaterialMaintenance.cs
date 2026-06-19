#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Batchmode-safe replacement for HDRP Wizard material conversion buttons.
/// Converts legacy Built-in materials under Assets/ and refreshes Arc Academy HDRP library.
/// </summary>
public static class ArcAcademyHdrpMaterialMaintenance
{
    private static readonly HashSet<string> LegacyShaderNames = new()
    {
        "Standard",
        "Standard (Specular setup)",
        "Standard (Roughness setup)",
        "Legacy Shaders/Diffuse",
        "Legacy Shaders/Specular",
        "Legacy Shaders/Transparent/Diffuse",
        "Mobile/Diffuse",
        "Mobile/Bumped Diffuse",
    };

    [MenuItem("Bob/HDRP/Upgrade Project Materials")]
    public static void UpgradeMaterialsMenu()
    {
        var report = UpgradeProjectMaterials();
        EditorUtility.DisplayDialog(
            "HDRP Material Upgrade",
            $"Converted legacy: {report.ConvertedLegacy}\n" +
            $"Refreshed HDRP: {report.RefreshedHdrp}\n" +
            $"Skipped (no upgrader): {report.Skipped}",
            "OK");
    }

    public static void UpgradeMaterialsFromCli()
    {
        var report = UpgradeProjectMaterials();
        Debug.Log(
            $"HDRP_MATERIALS_OK: converted={report.ConvertedLegacy} " +
            $"refreshed={report.RefreshedHdrp} skipped={report.Skipped}");
        EditorApplication.Exit(0);
    }

    public static MaterialUpgradeReport UpgradeProjectMaterials()
    {
        var report = new MaterialUpgradeReport();
        ArcAcademyShaderGraphSetup.EnsureMaterialLibrary();

        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Assets/Materials/HDRP/"))
            {
                continue;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null)
            {
                continue;
            }

            if (TryConvertLegacyMaterial(material))
            {
                report.ConvertedLegacy++;
                EditorUtility.SetDirty(material);
                continue;
            }

            if (IsHdrpShader(material.shader.name))
            {
                RefreshHdrpMaterial(material);
                report.RefreshedHdrp++;
                EditorUtility.SetDirty(material);
                continue;
            }

            report.Skipped++;
        }

        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials/HDRP" }))
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (material == null)
            {
                continue;
            }

            RefreshHdrpMaterial(material);
            report.RefreshedHdrp++;
            EditorUtility.SetDirty(material);
        }

        AssetDatabase.SaveAssets();
        return report;
    }

    private static bool TryConvertLegacyMaterial(Material material)
    {
        if (!LegacyShaderNames.Contains(material.shader.name))
        {
            return false;
        }

        var baseColor = material.HasProperty("_Color")
            ? material.GetColor("_Color")
            : Color.white;
        var smoothness = material.HasProperty("_Glossiness")
            ? material.GetFloat("_Glossiness")
            : material.HasProperty("_GlossMapScale") ? material.GetFloat("_GlossMapScale") : 0.35f;
        var metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
        var emission = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;

        var hdrpLit = Shader.Find("HDRP/Lit");
        if (hdrpLit == null)
        {
            Debug.LogWarning($"HDRP/Lit shader missing; cannot convert {material.name}");
            return false;
        }

        material.shader = hdrpLit;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", metallic);
        }

        if (emission.maxColorComponent > 0.001f && material.HasProperty("_EmissiveColor"))
        {
            material.SetColor("_EmissiveColor", emission);
            material.EnableKeyword("_EMISSIVE_COLOR");
        }

        return true;
    }

    private static void RefreshHdrpMaterial(Material material)
    {
        if (material.shader == null || !IsHdrpShader(material.shader.name))
        {
            return;
        }

        var latest = Shader.Find(material.shader.name);
        if (latest != null)
        {
            material.shader = latest;
        }

        if (material.HasProperty("_DoubleSidedEnable"))
        {
            material.SetFloat("_DoubleSidedEnable", 0f);
        }
    }

    private static bool IsHdrpShader(string shaderName)
    {
        return shaderName.StartsWith("HDRP/") || shaderName.Contains("High Definition Render Pipeline");
    }

    public struct MaterialUpgradeReport
    {
        public int ConvertedLegacy;
        public int RefreshedHdrp;
        public int Skipped;
    }
}
#endif
