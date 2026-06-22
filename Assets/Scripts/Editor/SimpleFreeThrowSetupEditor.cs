#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(SimpleFreeThrowSetup))]
public class SimpleFreeThrowSetupEditor : Editor
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "Strips warehouse decoration, applies minimal HDRP lighting, builds court/hoop/ball, " +
            "and wires Bob as launcher. Idempotent — safe to run multiple times.",
            MessageType.Info);

        if (GUILayout.Button("Setup Minimal Trainer", GUILayout.Height(32f)))
        {
            RunSetupWithBackup();
        }

        DrawDefaultInspector();
    }

    [MenuItem("Bob/Setup/Simple Free Throw Trainer")]
    [MenuItem("Tools/Bob/Setup Simple Free Throw Trainer")]
    [MenuItem("GameObject/Bob/Setup Simple Free Throw Trainer", false, 10)]
    public static void MenuSetup()
    {
        var arena = GameObject.Find(ArcAcademyLayout.ArenaName);
        if (arena == null)
        {
            EditorUtility.DisplayDialog(
                "Simple Free Throw",
                "Open BobTraining.unity first — TrainingArena not found.",
                "OK");
            return;
        }

        Selection.activeGameObject = SimpleFreeThrowSetup.EnsureOnArena(arena.transform).gameObject;
        RunSetupWithBackup();
    }

    private static void RunSetupWithBackup()
    {
        if (!EditorSceneManager.GetActiveScene().isDirty)
        {
            BackupScene();
        }
        else
        {
            if (!EditorUtility.DisplayDialog(
                    "Save scene first?",
                    "The scene has unsaved changes. Save before backup + setup?",
                    "Save & Continue",
                    "Continue Without Save"))
            {
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            BackupScene();
        }

        var arena = GameObject.Find(ArcAcademyLayout.ArenaName)?.transform;
        if (arena == null)
        {
            Debug.LogError("SIMPLE_FREE_THROW_FAIL: TrainingArena not found.");
            return;
        }

        var setup = SimpleFreeThrowSetup.EnsureOnArena(arena);
        setup.ApplyAll();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ SIMPLE FREE THROW SETUP COMPLETE — scene saved. Backup in Assets/Scenes/ if created.");
    }

    private static void BackupScene()
    {
        if (!File.Exists(ScenePath))
        {
            return;
        }

        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var backupPath = $"Assets/Scenes/BobTraining.backup.{stamp}.unity";
        if (AssetDatabase.CopyAsset(ScenePath, backupPath))
        {
            Debug.Log($"SIMPLE_FREE_THROW_BACKUP: {backupPath}");
        }
        else
        {
            Debug.LogWarning("SIMPLE_FREE_THROW_WARN: Scene backup failed — continuing setup.");
        }
    }

    public static void ApplyFromCli()
    {
        var arena = GameObject.Find(ArcAcademyLayout.ArenaName)?.transform;
        if (arena == null)
        {
            Debug.LogError("VALIDATE_FAIL: TrainingArena missing for minimal setup");
            EditorApplication.Exit(1);
            return;
        }

        var setup = SimpleFreeThrowSetup.EnsureOnArena(arena);
        setup.ApplyAll();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("SIMPLE_FREE_THROW_OK: Minimal trainer applied via CLI.");
        EditorApplication.Exit(0);
    }
}
#endif
