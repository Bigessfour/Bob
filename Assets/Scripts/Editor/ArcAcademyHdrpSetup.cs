#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Idempotent HDRP pipeline bootstrap for batchmode CLI and Editor menu.
/// </summary>
public static class ArcAcademyHdrpSetup
{
    public const string SettingsFolder = "Assets/Settings/HDRP";
    public const string PipelineAssetPath = SettingsFolder + "/HDRenderPipelineAsset.asset";
    public const string VolumeProfilePath = SettingsFolder + "/ArcAcademyVolumeProfile.asset";

    [MenuItem("Bob/Ensure HDRP Pipeline")]
    public static void EnsureHdrpPipelineMenu()
    {
        EnsureHdrpPipeline();
    }

    public static void EnsureHdrpFromCli()
    {
        EnsureHdrpPipeline();
        ArcAcademyShaderGraphSetup.EnsureMaterialLibrary();
        var materialReport = ArcAcademyHdrpMaterialMaintenance.UpgradeProjectMaterials();
        AssetDatabase.SaveAssets();
        Debug.Log(
            "HDRP_SETUP_OK: pipeline, volume profile, materials, and linear color space configured " +
            $"(converted={materialReport.ConvertedLegacy} refreshed={materialReport.RefreshedHdrp} " +
            $"skipped={materialReport.Skipped})");
        EditorApplication.Exit(0);
    }

    public static void EnsureHdrpPipeline()
    {
        Directory.CreateDirectory(SettingsFolder);
        Directory.CreateDirectory("Assets/Materials/HDRP");

        EnsureLinearColorSpace();
        var pipeline = EnsurePipelineAsset();
        var profile = EnsureVolumeProfile();
        ApplyVolumePolish(profile);
        AssignPipelineToGraphics(pipeline);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureLinearColorSpace()
    {
        if (PlayerSettings.colorSpace != ColorSpace.Linear)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
        }
    }

    private static HDRenderPipelineAsset EnsurePipelineAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(PipelineAssetPath);
        if (existing != null)
        {
            return existing;
        }

        var asset = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
        AssetDatabase.CreateAsset(asset, PipelineAssetPath);
        return asset;
    }

    private const string DefaultVolumeProfilePath =
        "Assets/HDRPDefaultResources/DefaultSettingsVolumeProfile.asset";

    private static VolumeProfile EnsureVolumeProfile()
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
        if (ProfileIsInvalid(profile))
        {
            if (profile != null)
            {
                AssetDatabase.DeleteAsset(VolumeProfilePath);
            }

            if (!AssetDatabase.CopyAsset(DefaultVolumeProfilePath, VolumeProfilePath))
            {
                Debug.LogError(
                    $"HDRP setup failed: could not copy default volume profile from {DefaultVolumeProfilePath}");
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumeProfilePath);
                EnsureVolumeComponents(profile);
            }
            else
            {
                profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            }

            AssetDatabase.SaveAssets();
        }
        else
        {
            EnsureVolumeComponents(profile);
        }

        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static bool ProfileIsInvalid(VolumeProfile profile)
    {
        if (profile == null)
        {
            return true;
        }

        if (profile.components == null || profile.components.Count == 0)
        {
            return true;
        }

        foreach (var component in profile.components)
        {
            if (component == null)
            {
                return true;
            }
        }

        return !profile.Has<Exposure>();
    }

    private static void EnsureVolumeComponents(VolumeProfile profile)
    {
        if (!profile.Has<Exposure>())
        {
            profile.Add<Exposure>(true);
        }

        if (!profile.Has<Tonemapping>())
        {
            profile.Add<Tonemapping>(true);
        }

        if (!profile.Has<Bloom>())
        {
            profile.Add<Bloom>(true);
        }

        if (!profile.Has<ScreenSpaceReflection>())
        {
            profile.Add<ScreenSpaceReflection>(true);
        }

        if (!profile.Has<VisualEnvironment>())
        {
            profile.Add<VisualEnvironment>(true);
        }

        if (!profile.Has<PhysicallyBasedSky>())
        {
            profile.Add<PhysicallyBasedSky>(true);
        }

        if (!profile.Has<Fog>())
        {
            profile.Add<Fog>(true);
        }

        if (!profile.Has<ColorAdjustments>())
        {
            profile.Add<ColorAdjustments>(true);
        }

        if (!profile.Has<Vignette>())
        {
            profile.Add<Vignette>(true);
        }
    }

    private static void ApplyVolumePolish(VolumeProfile profile)
    {
        ApplyLabVolumePolish(profile);
    }

    /// <summary>Readable AI Warehouse lab lighting — default for training demos.</summary>
    public static void ApplyLabVolumePolish(VolumeProfile profile)
    {
        ArcAcademyLabRenderPreset.ApplyVolume(profile);
        EditorUtility.SetDirty(profile);
    }

    /// <summary>Photoreal warehouse stretch preset (optional portfolio stills).</summary>
    public static void ApplyWarehouseVolumePolish(VolumeProfile profile)
    {
        if (profile.TryGet(out Exposure exposure))
        {
            exposure.mode.overrideState = true;
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.overrideState = true;
            // Lowered from 13.5 for brighter interior warehouse without crushing darks (tune in editor if needed)
            exposure.fixedExposure.value = 10.5f;
        }

        if (profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.ACES;
        }

        if (profile.TryGet(out Bloom bloom))
        {
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.88f;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.55f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.72f;
        }

        if (profile.TryGet(out ScreenSpaceReflection ssr))
        {
            ssr.enabled.overrideState = true;
            ssr.enabled.value = true;
        }

        if (profile.TryGet(out VisualEnvironment env))
        {
            env.skyType.overrideState = true;
            env.skyType.value = (int)SkyType.PhysicallyBased;
        }

        if (profile.TryGet(out PhysicallyBasedSky physSky))
        {
            physSky.horizonZenithShift.overrideState = true;
            physSky.horizonZenithShift.value = -0.15f;
        }

        if (profile.TryGet(out Fog fog))
        {
            fog.enabled.overrideState = true;
            fog.enabled.value = false;
        }

        if (profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.active = true;
            colorAdjustments.contrast.overrideState = true;
            // Reduced for more natural bright warehouse look (reference has good dynamic range, not crushed shadows)
            colorAdjustments.contrast.value = 2f;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 8f;
        }

        if (profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.15f;
        }

        EditorUtility.SetDirty(profile);
    }

    private static void AssignPipelineToGraphics(HDRenderPipelineAsset pipeline)
    {
        GraphicsSettings.defaultRenderPipeline = pipeline;

        var qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
            QualitySettings.renderPipeline = pipeline;
        }

        QualitySettings.SetQualityLevel(Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, qualityNames.Length - 1), true);
    }

    public static VolumeProfile LoadVolumeProfile()
    {
        var profile = EnsureVolumeProfile();
        ApplyVolumePolish(profile);
        return profile;
    }
}
#endif
