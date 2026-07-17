#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Batchmode entry point for macOS standalone builds.
/// </summary>
public static class BobBuildCli
{
    private const string DefaultOutputDir = "builds/macos";

    public static void BuildStandaloneMacFromCli()
    {
        string outputDir = Path.Combine(Application.dataPath, "..", DefaultOutputDir);
        outputDir = Path.GetFullPath(outputDir);
        Directory.CreateDirectory(outputDir);

        string productName = PlayerSettings.productName;
        if (string.IsNullOrWhiteSpace(productName))
        {
            productName = "Bob";
        }

        string appPath = Path.Combine(outputDir, $"{productName}.app");
        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            string trainingScene = "Assets/Scenes/BobTraining.unity";
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(trainingScene, true) };
            scenes = EditorBuildSettings.scenes;
        }

        var scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = appPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"BOB_BUILD_FAIL: {report.summary.result} errors={report.summary.totalErrors}");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"BOB_BUILD_OK: {appPath}");
        EditorApplication.Exit(0);
    }
}
#endif
