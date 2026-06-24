#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Soft ceiling area lights for the Simple Arc lab court (HDRP Rectangle lights, no shadows).
/// Names prefixed with <c>LabGymFill</c> are whitelisted by <see cref="ArcAcademyLabRenderPreset"/>.
/// </summary>
public static class SimpleArcLabLightingBuilder
{
    public const string RootName = "LabGymLighting";
    public const string KeyFillName = "LabGymFill_Key";
    public const string CourtFillName = "LabGymFill_Court";

    public static void EnsureLabGymFillLights(Transform arenaRoot)
    {
        if (arenaRoot == null)
        {
            return;
        }

        var root = arenaRoot.Find(RootName);
        if (root == null)
        {
            var go = new GameObject(RootName);
            go.transform.SetParent(arenaRoot, false);
            root = go.transform;
        }

        EnsureAreaLight(
            root,
            KeyFillName,
            new Vector3(0f, 8.5f, -2.8f),
            new Vector2(5f, 7f),
            new Color(0.95f, 0.97f, 1f));

        EnsureAreaLight(
            root,
            CourtFillName,
            new Vector3(0f, 9f, 1.5f),
            new Vector2(8f, 10f),
            new Color(0.98f, 0.98f, 1f));
    }

    private static void EnsureAreaLight(
        Transform parent,
        string name,
        Vector3 localPosition,
        Vector2 areaSize,
        Color color)
    {
        var lightTransform = parent.Find(name);
        GameObject lightGo;
        if (lightTransform == null)
        {
            lightGo = new GameObject(name);
            lightGo.transform.SetParent(parent, false);
        }
        else
        {
            lightGo = lightTransform.gameObject;
        }

        lightGo.transform.localPosition = localPosition;
        lightGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        lightGo.transform.localScale = Vector3.one;

        var light = lightGo.GetComponent<Light>();
        if (light == null)
        {
            light = lightGo.AddComponent<Light>();
        }

        light.type = LightType.Rectangle;
        light.areaSize = areaSize;
        light.color = color;
        light.shadows = LightShadows.None;
        light.lightUnit = LightUnit.Lumen;
        light.intensity = ArcAcademyLabLightingValues.LabGymFillLumen;

        if (!lightGo.TryGetComponent(out HDAdditionalLightData hd))
        {
            hd = lightGo.AddComponent<HDAdditionalLightData>();
        }

        hd.SetLightDimmer(1f, 0f);
        hd.EnableShadows(false);
        hd.UpdateAllLightValues();
    }
}
#endif
