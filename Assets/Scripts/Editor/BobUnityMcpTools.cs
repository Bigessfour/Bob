#if UNITY_EDITOR
using System.IO;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Bob-specific MCP tools exposed through Unity's official MCP bridge (com.unity.ai.assistant).
/// Discovered automatically at Editor startup via <see cref="McpToolRegistry"/>.
/// </summary>
public static class BobUnityMcpTools
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

    [McpTool("bob_open_training_scene", "Open BobTraining.unity as the active Editor scene.")]
    public static object OpenTrainingScene()
    {
        if (!File.Exists(ScenePath))
        {
            return new { success = false, message = $"Scene not found: {ScenePath}" };
        }

        EditorSceneManager.OpenScene(ScenePath);
        return new { success = true, scene = ScenePath };
    }

    public class BobArenaSetupParams
    {
        public bool SaveScene { get; set; } = true;
    }

    [McpTool(
        "bob_setup_simple_arena",
        "Build or refresh SimpleArcAcademyArena, wire SpawnPoint + SimpleArcArenaManager, reparent Bob, and save Prefab_Bob.")]
    public static object SetupSimpleArena(BobArenaSetupParams parameters)
    {
        if (!File.Exists(ScenePath))
        {
            return new { success = false, message = $"Scene not found: {ScenePath}" };
        }

        var activePath = EditorSceneManager.GetActiveScene().path;
        if (activePath != ScenePath)
        {
            EditorSceneManager.OpenScene(ScenePath);
        }

        SimpleArcAcademyArenaBuilder.ApplySilently();

        if (parameters.SaveScene)
        {
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
        }

        return new
        {
            success = true,
            scene = ScenePath,
            saved = parameters.SaveScene,
            message = "Simple Arc Academy arena applied. Check read_console for SIMPLE_ARENA or errors."
        };
    }

    [MenuItem("Bob/MCP/Open Training Scene")]
    public static void MenuOpenTrainingScene()
    {
        var result = OpenTrainingScene();
        Debug.Log($"BOB_MCP: {result}");
    }

    [MenuItem("Bob/MCP/Setup Simple Arc Academy Arena")]
    public static void MenuSetupSimpleArena()
    {
        var result = SetupSimpleArena(new BobArenaSetupParams());
        Debug.Log($"BOB_MCP: {result}");
    }

    [MenuItem("Bob/MCP/Unity MCP Setup Help")]
    public static void MenuSetupHelp()
    {
        EditorUtility.DisplayDialog(
            "Unity MCP (official)",
            "1. Edit → Project Settings → AI → Unity MCP — bridge must show Running.\n" +
            "2. Enable tool groups you need (Scene, GameObject, Scripting, Console, etc.).\n" +
            "3. In Cursor MCP settings, enable unity-mcp (relay via scripts/unity-mcp.sh).\n" +
            "4. Approve Cursor under Pending Connections in Unity MCP settings.\n\n" +
            "See docs/unity-mcp.md in the repo.",
            "OK");
    }
}
#endif
