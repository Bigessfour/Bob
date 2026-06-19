#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class ArcAcademyLabSceneFix
{
    [MenuItem("Bob/HDRP/Fix White Blowout (In-Place)")]
    public static void FixWhiteBlowoutInPlace()
    {
        var arena = GameObject.Find("TrainingArena");
        if (arena != null && arena.GetComponent<ArcAcademyLabPlayFix>() == null)
        {
            arena.AddComponent<ArcAcademyLabPlayFix>();
        }

        ArcAcademyLabRenderPreset.ApplyAll();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        var volume = Object.FindAnyObjectByType<Volume>();
        if (volume != null && volume.profile != null)
        {
            EditorUtility.SetDirty(volume.profile);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();

        EditorUtility.DisplayDialog(
            "Lab render fix applied",
            "Clamped scene lights and applied lab HDRP volume (exposure 6, bloom off).\n\n" +
            "Press Play and check the Game tab.\n\n" +
            "For a full reset later: Bob → Rebuild Arc Academy (HDRP).",
            "OK");

        Debug.Log("ARC_LAB_FIX_OK: In-place white blowout fix applied and scene saved.");
    }

    public static void FixWhiteBlowoutFromCli()
    {
        var arena = GameObject.Find("TrainingArena");
        if (arena != null && arena.GetComponent<ArcAcademyLabPlayFix>() == null)
        {
            arena.AddComponent<ArcAcademyLabPlayFix>();
        }

        ArcAcademyLabRenderPreset.ApplyAll();
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("ARC_LAB_FIX_OK: In-place lab render fix applied via CLI.");
        EditorApplication.Exit(0);
    }
}
#endif
