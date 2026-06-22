using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Applies readable lab HDRP volume settings and clamps scene lights (fixes white blowout).
/// </summary>
public static class ArcAcademyLabRenderPreset
{
    private static readonly int EmissiveIntensityId = Shader.PropertyToID("_EmissiveIntensity");

    public static void ApplyMinimalTrainerVolume(VolumeProfile profile)
    {
        if (profile == null)
        {
            return;
        }

        if (profile.TryGet(out Exposure exposure))
        {
            exposure.active = true;
            exposure.mode.overrideState = true;
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.overrideState = true;
            exposure.fixedExposure.value = 11f;
            exposure.limitMax.overrideState = true;
            exposure.limitMax.value = 13f;
        }

        if (profile.TryGet(out Bloom bloom))
        {
            bloom.active = true;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.12f;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 1f;
        }

        if (profile.TryGet(out ScreenSpaceReflection ssr))
        {
            ssr.active = true;
            ssr.enabled.overrideState = true;
            ssr.enabled.value = true;
        }

        if (profile.TryGet(out MotionBlur motionBlur))
        {
            motionBlur.active = false;
        }

        if (profile.TryGet(out Fog fog))
        {
            fog.enabled.overrideState = true;
            fog.enabled.value = false;
        }

        if (profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.ACES;
        }
    }

    public static void ApplyMinimalTrainerVolumeInScene()
    {
        var volume = Object.FindAnyObjectByType<Volume>();
        if (volume != null && volume.profile != null)
        {
            ApplyMinimalTrainerVolume(volume.profile);
        }

        EnforceSingleDirectionalShadow();
    }

    /// <summary>
    /// HDRP allows only one directional light to cast cascade shadows at a time.
    /// </summary>
    public static void EnforceSingleDirectionalShadow()
    {
        Light chosen = null;

        foreach (var light in Object.FindObjectsByType<Light>())
        {
            if (light == null
                || light.type != LightType.Directional
                || !light.enabled
                || !light.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (light.gameObject.name == "Sun")
            {
                chosen = light;
                break;
            }
        }

        if (chosen == null)
        {
            foreach (var light in Object.FindObjectsByType<Light>())
            {
                if (light != null
                    && light.type == LightType.Directional
                    && light.enabled
                    && light.gameObject.activeInHierarchy)
                {
                    chosen = light;
                    break;
                }
            }
        }

        foreach (var light in Object.FindObjectsByType<Light>())
        {
            if (light == null || light.type != LightType.Directional)
            {
                continue;
            }

            bool isCaster = light == chosen;
            light.shadows = isCaster ? LightShadows.Soft : LightShadows.None;

            if (light.TryGetComponent(out HDAdditionalLightData hd))
            {
                hd.EnableShadows(isCaster);
                hd.UpdateAllLightValues();
            }
        }
    }

    public static void ApplyVolume(VolumeProfile profile)
    {
        if (profile == null)
        {
            return;
        }

        if (profile.TryGet(out Exposure exposure))
        {
            exposure.active = true;
            exposure.mode.overrideState = true;
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.overrideState = true;
            exposure.fixedExposure.value = ArcAcademyLabLightingValues.FixedExposure;
            exposure.limitMax.overrideState = true;
            exposure.limitMax.value = 12f;
        }

        if (profile.TryGet(out MotionBlur motionBlur))
        {
            motionBlur.active = false;
        }

        if (profile.TryGet(out Bloom bloom))
        {
            bloom.active = false;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0f;
        }

        if (profile.TryGet(out ScreenSpaceReflection ssr))
        {
            ssr.enabled.overrideState = true;
            ssr.enabled.value = false;
        }

        if (profile.TryGet(out ScreenSpaceAmbientOcclusion ambientOcclusion))
        {
            ambientOcclusion.active = false;
        }

        if (profile.TryGet(out ContactShadows contactShadows))
        {
            contactShadows.active = false;
        }

        if (profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping.mode.overrideState = true;
            tonemapping.mode.value = TonemappingMode.ACES;
        }

        if (profile.TryGet(out VisualEnvironment env))
        {
            env.skyType.overrideState = true;
            env.skyType.value = (int)SkyType.HDRI;
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
            colorAdjustments.contrast.value = 4f;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 8f;
        }

        if (profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.05f;
        }
    }

    public static void ApplyVolumeInScene()
    {
        var volume = Object.FindAnyObjectByType<Volume>();
        if (volume != null && volume.profile != null)
        {
            ApplyVolume(volume.profile);
        }
    }

    public static int ClampSceneLights()
    {
        int adjusted = 0;
        int ceilingStrips = 0;

        foreach (var light in Object.FindObjectsByType<Light>())
        {
            if (light == null)
            {
                continue;
            }

            float before = light.intensity;
            bool beforeEnabled = light.enabled;

            if (light.gameObject.name == "Sun")
            {
                SetLightIntensity(light, ArcAcademyLabLightingValues.SunLux, LightUnit.Lux);
                light.shadows = LightShadows.Soft;
            }
            else if (light.gameObject.name.StartsWith("CeilingStripLight"))
            {
                ceilingStrips++;
                if (ceilingStrips > ArcAcademyLabLightingValues.MaxActiveCeilingStrips)
                {
                    light.enabled = false;
                    DisableEmissiveMeshes(light.transform);
                }
                else
                {
                    light.enabled = true;
                    SetLightIntensity(light, ArcAcademyLabLightingValues.CeilingStripLumen, LightUnit.Lumen);
                }
            }
            else if (light.gameObject.name.Contains("Window") || light.gameObject.name.Contains("Mountain"))
            {
                SetLightIntensity(light, ArcAcademyLabLightingValues.WindowFillLumen, LightUnit.Lumen);
            }
            else if (light.gameObject.name == "LabKeyFill" || light.gameObject.name == "WindowFill")
            {
                SetLightIntensity(light, ArcAcademyLabLightingValues.FillDirectionalLux, LightUnit.Lux);
                light.shadows = LightShadows.None;
            }
            else if (light.gameObject.name == "WarehouseLight_Center")
            {
                SetLightIntensity(light, ArcAcademyLabLightingValues.CenterPointLumen, LightUnit.Lumen);
            }
            else if (light.gameObject.name == "BobRimLight" || light.gameObject.name == "SpawnPadLight")
            {
                SetLightIntensity(light, ArcAcademyLabLightingValues.BobSpotLumen, LightUnit.Lumen);
            }
            else
            {
                float target = light.type switch
                {
                    LightType.Directional => ArcAcademyLabLightingValues.FillDirectionalLux,
                    LightType.Rectangle => ArcAcademyLabLightingValues.CeilingStripLumen,
                    LightType.Point => ArcAcademyLabLightingValues.CenterPointLumen,
                    LightType.Spot => ArcAcademyLabLightingValues.BobSpotLumen,
                    _ => light.intensity,
                };

                LightUnit unit = light.type == LightType.Directional ? LightUnit.Lux : LightUnit.Lumen;
                SetLightIntensity(light, Mathf.Min(light.intensity, target), unit);
                if (light.type == LightType.Directional && light.gameObject.name != "Sun")
                {
                    light.shadows = LightShadows.None;
                }
            }

            SyncHdrpLight(light);

            if (!Mathf.Approximately(before, light.intensity) || beforeEnabled != light.enabled)
            {
                adjusted++;
            }
        }

        EnforceSingleDirectionalShadow();

        return adjusted;
    }

    public static int ClampEmissiveBackdrops()
    {
        int adjusted = 0;

        foreach (var renderer in Object.FindObjectsByType<Renderer>())
        {
            if (renderer == null)
            {
                continue;
            }

            var name = renderer.gameObject.name;
            if (!name.Contains("Mountain") && !name.Contains("Backdrop") && !name.Contains("Panorama"))
            {
                continue;
            }

            var material = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
            if (material != null && material.HasProperty(EmissiveIntensityId))
            {
                float before = material.GetFloat(EmissiveIntensityId);
                float target = Mathf.Min(before, 60f);
                if (!Mathf.Approximately(before, target))
                {
                    if (Application.isPlaying)
                    {
                        material.SetFloat(EmissiveIntensityId, target);
                    }
                    else
                    {
                        renderer.sharedMaterial.SetFloat(EmissiveIntensityId, target);
                    }

                    adjusted++;
                }
            }
        }

        return adjusted;
    }

    public static void ApplyAll()
    {
        ApplyVolumeInScene();
        int lights = ClampSceneLights();
        int emissive = ClampEmissiveBackdrops();
        EnforceSingleDirectionalShadow();
        Debug.Log($"ARC_LAB_RENDER_OK: volume applied, {lights} lights clamped, {emissive} emissive backdrops toned down.");
    }

    private static void SetLightIntensity(Light light, float intensity, LightUnit unit)
    {
        light.lightUnit = unit;
        light.intensity = intensity;
    }

    private static void SyncHdrpLight(Light light)
    {
        if (!light.TryGetComponent(out HDAdditionalLightData hd))
        {
            return;
        }

        //hd.useVolumetric = false;
        hd.SetLightDimmer(1f, 0f);

        if (light.type == LightType.Directional && light.gameObject.name != "Sun")
        {
            hd.interactsWithSky = false;
        }

        hd.UpdateAllLightValues();
    }

    private static void DisableEmissiveMeshes(Transform lightRoot)
    {
        for (int i = 0; i < lightRoot.childCount; i++)
        {
            var child = lightRoot.GetChild(i);
            if (child.name.EndsWith("_Mesh"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
