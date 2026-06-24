#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BobPolishMenu
{
    [MenuItem("Bob/Polish/Rebuild Scene")]
    public static void RebuildScene()
    {
        ArcAcademyLabRenderPreset.ApplyAll();
        ArcAcademyHdrpSetup.ApplyLabVolumePolish(ArcAcademyHdrpSetup.LoadVolumeProfile());
        BobTrainingSceneBuilder.CreateTrainingSceneMenu();
        EditorUtility.DisplayDialog(
            "Arc Academy",
            "Scene rebuilt with Arc Academy Lab lighting.\n\n" +
            "If the scene still looks dark or blown out:\n" +
            "1. Bob → HDRP → Fix White Blowout (In-Place)\n" +
            "2. Bob → HDRP → Apply Lab Lighting\n" +
            "3. Press Play and check Game view (not Scene wireframe)",
            "OK");
    }

    [MenuItem("Bob/Polish/Bake Probe Volumes")]
    public static void BakeProbeVolumes()
    {
        Lightmapping.BakeAsync();
        Debug.Log(
            "APV_BAKE_STARTED: If indirect lighting stays flat, open Window → Rendering → Lighting " +
            "and click Bake Probe Volumes (HDRP APV).");
    }

    // Note: The main "Fix Training View" is registered in ArcTrainingViewValidator.cs under Bob/Polish and Bob/Test
    // to avoid duplicate MenuItem registration.
    public static void FixTrainingView()
    {
        ArcTrainingViewValidator.FixTrainingViewMenu();
    }
}
#endif
