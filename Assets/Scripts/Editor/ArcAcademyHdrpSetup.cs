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
        AssetDatabase.SaveAssets();
        Debug.Log("HDRP_SETUP_OK: pipeline, volume profile, and linear color space configured");
        EditorApplication.Exit(0);
    }

    public static void EnsureHdrpPipeline()
    {
        Directory.CreateDirectory(SettingsFolder);
        Directory.CreateDirectory("Assets/Materials/HDRP");

        EnsureLinearColorSpace();
        var pipeline = EnsurePipelineAsset();
        EnsureVolumeProfile();
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

        var exposure = profile.Add<Exposure>(true);
        exposure.mode.overrideState = true;
        exposure.mode.value = ExposureMode.Fixed;
        exposure.fixedExposure.overrideState = true;
        exposure.fixedExposure.value = 11f;

        var tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.overrideState = true;
        tonemapping.mode.value = TonemappingMode.ACES;

        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 0.45f;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 0.85f;

        var ssr = profile.Add<ScreenSpaceReflection>(true);
        ssr.enabled.overrideState = true;
        ssr.enabled.value = true;

        var env = profile.Add<VisualEnvironment>(true);
        env.skyType.overrideState = true;
        env.skyType.value = (int)SkyType.PhysicallyBased;

        var physSky = profile.Add<PhysicallyBasedSky>(true);
        physSky.horizonZenithShift.overrideState = true;
        physSky.horizonZenithShift.value = -0.15f;

        var fog = profile.Add<Fog>(true);
        fog.enabled.overrideState = true;
        fog.enabled.value = false;

        EditorUtility.SetDirty(profile);
        return profile;
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
        EnsureVolumeProfile();
        return AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
    }
}
#endif
