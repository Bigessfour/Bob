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
        SetActiveDeep($"{ArcAcademyLayout.ArenaName}/{ArcAcademyLayout.SpawnPadName}", false);
        SetActiveDeep($"{ArcAcademyLayout.ArenaName}/{ArcAcademyLayout.SignageArcAcademyName}", false);

        // Deactivate East Wall to allow unobstructed East sideline camera view
        SetActiveDeep($"{SimpleArcAcademyArena.RootName}/{SimpleArcAcademyArena.WallEastName}", false);

        HideByName("Label_Bob");
        HideByName("Label_ArcAcademy");
        HideByName("SpawnPadBranding");
        HideByName("MountainWindow");
        HideByName("WarehouseShell");
        HideByName("ComplexRenderGroup");

        HideLegacyDecorativeHoops();
    }

    /// <summary>
    /// Runtime-safe, idempotent guard that deactivates the legacy decorative basketball geometry
    /// (training bays, decorative hoop markers, portable hoop stands, robotic launchers) so the lab
    /// stays a clean single-hoop court every time Play is pressed. Mirrors the editor builder's
    /// HideExtraDecorativeHoops/StripBackgroundHoopDecorations logic but only ever calls SetActive(false)
    /// — never Destroy — so it is non-destructive and safe to run repeatedly.
    /// </summary>
    private static void HideLegacyDecorativeHoops()
    {
        // Whole legacy training-bay cluster (Bay_1..Bay_8 with BayHoop/PortableHoopStand/RoboticLauncher).
        SetActiveDeep($"{ArcAcademyLayout.ArenaName}/{ArcAcademyLayout.TrainingBaysName}", false);

        var activeHoop = GameObject.Find(ArcAcademyLayout.HoopName);

        // Deactivate every decorative hoop marker that is not part of the single active scoring hoop.
        var markers = Object.FindObjectsByType<DecorativeHoopMarker>(FindObjectsInactive.Include);
        foreach (var marker in markers)
        {
            if (marker == null)
            {
                continue;
            }

            if (activeHoop != null && marker.transform.IsChildOf(activeHoop.transform))
            {
                continue;
            }

            if (marker.gameObject.activeSelf)
            {
                marker.gameObject.SetActive(false);
            }
        }

        // Deactivate every PortableHoopStand / RoboticLauncher transform not under the active scoring hoop.
        var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (var t in allTransforms)
        {
            if (t == null)
            {
                continue;
            }

            if (t.name != ArcAcademyLayout.PortableHoopStandName && t.name != "RoboticLauncher")
            {
                continue;
            }

            if (activeHoop != null && t.IsChildOf(activeHoop.transform))
            {
                continue;
            }

            if (t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(false);
            }
        }
    }

    public static void EnsureLabCamera()
    {
        if (!SimpleArcAcademyArena.IsLabViewActive)
        {
            return;
        }

        var rig = GameObject.Find("CameraRig");
        var demoCamera = Object.FindAnyObjectByType<ArcAcademyDemoCamera>();
        var cam = Camera.main;

        if (rig != null && rig.TryGetComponent(out CameraOrbit orbit))
        {
            // Write pose to rig (so it is at the exact documented position), snap child, invoke reset
            rig.transform.position = SimpleArcAcademyArena.LabCameraPosition;
            rig.transform.rotation = Quaternion.LookRotation(
                SimpleArcAcademyArena.LabCameraLookAt - SimpleArcAcademyArena.LabCameraPosition,
                Vector3.up);

            var childCam = rig.GetComponentInChildren<Camera>();
            if (childCam != null)
            {
                childCam.fieldOfView = SimpleArcAcademyArena.LabCameraFieldOfView;
                if (childCam.transform.parent == rig.transform)
                {
                    childCam.transform.localPosition = Vector3.zero;
                    childCam.transform.localRotation = Quaternion.identity;
                }
            }

            orbit.ResetToDefault();
            cam = childCam != null ? childCam : cam;
        }
        else if (demoCamera != null)
        {
            demoCamera.ResetToLabHero();
        }
        else
        {
            ApplyLabCameraTransform(cam);
        }

        EnsureHdrpCameraSanity(cam);
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

        // When no rig, fall back to direct (and attempt to keep child local zero if applicable)
        var rig = cam.transform.parent != null && cam.transform.parent.name == "CameraRig" ? cam.transform.parent : null;
        var targetXform = rig != null ? rig : cam.transform;

        targetXform.position = SimpleArcAcademyArena.LabCameraPosition;
        targetXform.rotation = Quaternion.LookRotation(
            SimpleArcAcademyArena.LabCameraLookAt - SimpleArcAcademyArena.LabCameraPosition,
            Vector3.up);

        cam.fieldOfView = SimpleArcAcademyArena.LabCameraFieldOfView;

        if (rig != null && cam.transform.parent == rig)
        {
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
        }
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
