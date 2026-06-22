using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Hides leftover warehouse branding/decals and ensures lab camera framing when Simple Arc Academy is active.
/// </summary>
public static class ArcAcademyLabSceneCleanup
{
    private const float CameraPositionEpsilon = 0.05f;

    public static void HideLegacyClutter()
    {
        if (!SimpleArcAcademyArena.IsLabViewActive)
        {
            return;
        }

        SetActiveDeep($"{ArcAcademyLayout.ArenaName}/{ArcAcademyLayout.SpawnPadBrandingName}", false);
        SetActiveDeep($"{ArcAcademyLayout.ArenaName}/{ArcAcademyLayout.SignageArcAcademyName}", false);

        HideByName("Label_Bob");
        HideByName("Label_ArcAcademy");
        HideByName("SpawnPadBranding");
    }

    public static void EnsureLabCamera()
    {
        if (!SimpleArcAcademyArena.IsLabViewActive)
        {
            return;
        }

        var demoCamera = Object.FindAnyObjectByType<ArcAcademyDemoCamera>();
        if (demoCamera != null)
        {
            demoCamera.ResetToLabHero();
        }
        else
        {
            ApplyLabCameraTransform(Camera.main);
        }

        EnsureHdrpCameraSanity(Camera.main);
    }

    public static bool IsLabCameraPosition(Vector3 position)
    {
        return Vector3.Distance(position, SimpleArcAcademyArena.LabCameraPosition) <= CameraPositionEpsilon;
    }

    private static void ApplyLabCameraTransform(Camera cam)
    {
        if (cam == null)
        {
            return;
        }

        cam.transform.position = SimpleArcAcademyArena.LabCameraPosition;
        cam.transform.rotation = Quaternion.LookRotation(
            SimpleArcAcademyArena.LabCameraLookAt - SimpleArcAcademyArena.LabCameraPosition,
            Vector3.up);
        cam.fieldOfView = SimpleArcAcademyArena.LabCameraFieldOfView;
    }

    private static void EnsureHdrpCameraSanity(Camera cam)
    {
        if (cam == null || !cam.TryGetComponent(out HDAdditionalCameraData hdCamera))
        {
            return;
        }

        if (!hdCamera.customRenderingSettings)
        {
            return;
        }

        var mask = hdCamera.renderingPathCustomFrameSettingsOverrideMask;
        if (mask.mask[(int)FrameSettingsField.OpaqueObjects])
        {
            hdCamera.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.OpaqueObjects, true);
        }

        if (mask.mask[(int)FrameSettingsField.TransparentObjects])
        {
            hdCamera.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.TransparentObjects, true);
        }
    }

    private static void HideByName(string objectName)
    {
        var found = GameObject.Find(objectName);
        if (found != null)
        {
            found.SetActive(false);
        }
    }

    private static void SetActiveDeep(string path, bool active)
    {
        var parts = path.Split('/');
        if (parts.Length != 2)
        {
            return;
        }

        var parent = GameObject.Find(parts[0]);
        if (parent == null)
        {
            return;
        }

        var child = FindDeepChild(parent.transform, parts[1]);
        if (child != null)
        {
            child.gameObject.SetActive(active);
        }
    }

    private static Transform FindDeepChild(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindDeepChild(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
