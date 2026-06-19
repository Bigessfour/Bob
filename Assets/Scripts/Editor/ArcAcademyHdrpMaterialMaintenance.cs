#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Batchmode-safe replacement for HDRP Wizard material conversion buttons.
/// Converts legacy Built-in / URP materials under Assets/ and refreshes Arc Academy HDRP library.
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
        "Legacy Shaders/Transparent/Specular",
        "Mobile/Diffuse",
        "Mobile/Bumped Diffuse",
        "Unlit/Color",
        "Unlit/Texture",
        "Sprites/Default",
        "Universal Render Pipeline/Lit",
        "Universal Render Pipeline/Unlit",
        "Universal Render Pipeline/Simple Lit",
    };

    [MenuItem("Bob/HDRP/Upgrade Project Materials")]
    public static void UpgradeMaterialsMenu()
    {
        var report = UpgradeProjectMaterials();
        var message = new StringBuilder();
        message.AppendLine($"Converted legacy: {report.ConvertedLegacy}");
        message.AppendLine($"Refreshed HDRP: {report.RefreshedHdrp}");
        message.AppendLine($"Skipped (no upgrader): {report.Skipped}");
        if (report.SkippedPaths.Count > 0)
        {
            message.AppendLine();
            message.AppendLine("Skipped materials (safe if package/UI only):");
            foreach (var entry in report.SkippedPaths)
            {
                message.AppendLine($"  {entry}");
            }
        }

        EditorUtility.DisplayDialog("HDRP Material Upgrade", message.ToString(), "OK");
    }

    [MenuItem("Bob/HDRP/Apply Lab Lighting")]
    public static void ApplyLabLightingMenu()
    {
        ArcAcademyHdrpSetup.EnsureHdrpPipeline();
        var profile = ArcAcademyHdrpSetup.LoadVolumeProfile();
        ArcAcademyHdrpSetup.ApplyLabVolumePolish(profile);
        ArcAcademyLabRenderPreset.ClampSceneLights();
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog(
            "Lab Lighting",
            "Applied lab HDRP volume + clamped scene lights.\n\n" +
            "If still white: Bob → HDRP → Fix White Blowout (In-Place).",
            "OK");
    }

    public static void UpgradeMaterialsFromCli()
    {
        var report = UpgradeProjectMaterials();
        Debug.Log(
            $"HDRP_MATERIALS_OK: converted={report.ConvertedLegacy} " +
            $"refreshed={report.RefreshedHdrp} skipped={report.Skipped}");
        foreach (var entry in report.SkippedPaths)
        {
            Debug.LogWarning($"HDRP_MATERIAL_SKIP: {entry}");
        }

        EditorApplication.Exit(0);
    }

    public static MaterialUpgradeReport UpgradeProjectMaterials()
    {
        var report = new MaterialUpgradeReport { SkippedPaths = new List<string>() };
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
            report.SkippedPaths.Add($"{path} [{material.shader.name}]");
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
        var shaderName = material.shader.name;
        if (!LegacyShaderNames.Contains(shaderName) && !shaderName.StartsWith("Universal Render Pipeline/"))
        {
            return false;
        }

        var baseColor = material.HasProperty("_Color")
            ? material.GetColor("_Color")
            : material.HasProperty("_BaseColor")
                ? material.GetColor("_BaseColor")
                : Color.white;
        var smoothness = material.HasProperty("_Glossiness")
            ? material.GetFloat("_Glossiness")
            : material.HasProperty("_Smoothness")
                ? material.GetFloat("_Smoothness")
                : 0.35f;
        var metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
        var emission = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;

        var targetShader = shaderName.Contains("Unlit") || shaderName == "Sprites/Default"
            ? Shader.Find("HDRP/Unlit")
            : Shader.Find("HDRP/Lit");
        if (targetShader == null)
        {
            Debug.LogWarning($"HDRP shader missing; cannot convert {material.name}");
            return false;
        }

        material.shader = targetShader;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }
        else if (material.HasProperty("_UnlitColor"))
        {
            material.SetColor("_UnlitColor", baseColor);
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

        // Clamp runaway emissive from earlier photoreal tuning.
        if (material.HasProperty("_EmissiveIntensity"))
        {
            var emissive = material.GetFloat("_EmissiveIntensity");
            if (emissive > 600f)
            {
                material.SetFloat("_EmissiveIntensity", Mathf.Min(emissive, 600f));
            }
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
        public List<string> SkippedPaths;
    }
}
#endif
