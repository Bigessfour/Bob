#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BobPolishMenu
{
    [MenuItem("Bob/Polish/Rebuild Scene")]
    public static void RebuildScene()
    {
        ArcAcademyHdrpMaterialMaintenance.UpgradeProjectMaterials();
        BobTrainingSceneBuilder.CreateTrainingSceneMenu();
        EditorUtility.DisplayDialog(
            "Arc Academy",
            "Scene rebuilt with Example.jpg visual pass.\n\n" +
            "Next: Bob → Polish → Bake Probe Volumes\n" +
            "Then Game view + F1 for hero camera.",
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
}
#endif
