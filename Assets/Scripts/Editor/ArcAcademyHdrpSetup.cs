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
        AssetDatabase.SaveAssets();
        Debug.Log("HDRP_SETUP_OK: pipeline, volume profile, materials, and linear color space configured");
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

    private static VolumeProfile EnsureVolumeProfile()
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
        if (profile != null)
        {
            return profile;
        }

        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, VolumeProfilePath);

        profile.Add<Exposure>(true);
        profile.Add<Tonemapping>(true);
        profile.Add<Bloom>(true);
        profile.Add<ScreenSpaceReflection>(true);
        profile.Add<VisualEnvironment>(true);
        profile.Add<PhysicallyBasedSky>(true);
        profile.Add<Fog>(true);
        profile.Add<ColorAdjustments>(true);

        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static void ApplyVolumePolish(VolumeProfile profile)
    {
        if (profile.TryGet(out Exposure exposure))
        {
            exposure.mode.overrideState = true;
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.overrideState = true;
            exposure.fixedExposure.value = 13.5f;
        }

        if (profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.ACES;
        }

        if (profile.TryGet(out Bloom bloom))
        {
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.55f;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.75f;
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
            colorAdjustments.contrast.value = 5f;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 8f;
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
