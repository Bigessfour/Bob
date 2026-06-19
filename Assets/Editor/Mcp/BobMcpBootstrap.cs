using System;
using System.IO;
using System.Threading.Tasks;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Setup;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Configures CoplayDev MCP for Unity for the Bob repo: HTTP transport on 127.0.0.1:8080,
/// local server + Unity bridge, and BobTraining as the active scene.
/// </summary>
[InitializeOnLoad]
public static class BobMcpBootstrap
{
    // Mirror MCPForUnity.Editor.Constants.EditorPrefKeys (internal to package).
    private const string PrefAutoStartOnLoad = "MCPForUnity.AutoStartOnLoad";
    private const string PrefLockCursorConfig = "MCPForUnity.LockCursorConfig";
    private const string PrefClientProjectDirOverride = "MCPForUnity.ClientProjectDir";

    private const string ScenePath = "Assets/Scenes/BobTraining.unity";
    private const string SessionConnectKey = "BobMcpBootstrap.ConnectAttempted";

    static BobMcpBootstrap()
    {
        if (Application.isBatchMode &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UNITY_MCP_ALLOW_BATCH")))
        {
            return;
        }

        EditorApplication.delayCall += TryAutoConnectOnce;
    }

    [MenuItem("Bob/MCP/Configure Project Connection")]
    public static void ConfigureFromMenu()
    {
        _ = ConfigureAsync(openTrainingScene: true, logPrefix: "BOB_MCP");
    }

    [MenuItem("Bob/MCP/Verify MCP Connection")]
    public static void VerifyFromMenu()
    {
        var result = MCPServiceLocator.Bridge.VerifyAsync().GetAwaiter().GetResult();
        if (IsBridgeVerifySuccess(result))
        {
            Debug.Log($"BOB_MCP_VERIFY_OK: {result.Message}");
            return;
        }

        Debug.LogError($"BOB_MCP_VERIFY_FAIL: {result.Message}");
    }

    private static void TryAutoConnectOnce()
    {
        if (SessionState.GetBool(SessionConnectKey, false))
        {
            return;
        }

        SessionState.SetBool(SessionConnectKey, true);
        _ = ConfigureAsync(openTrainingScene: false, logPrefix: "BOB_MCP_AUTO");
    }

    private static async Task ConfigureAsync(bool openTrainingScene, string logPrefix)
    {
        if (EditorApplication.isCompiling)
        {
            EditorApplication.delayCall += () => _ = ConfigureAsync(openTrainingScene, logPrefix);
            return;
        }

        try
        {
            ApplyBobMcpEditorPrefs();

            if (!HttpEndpointUtility.IsHttpLocalUrlAllowedForLaunch(
                    HttpEndpointUtility.GetLocalBaseUrl(), out string policyError))
            {
                Debug.LogError($"{logPrefix}_FAIL: HTTP local URL blocked — {policyError}");
                return;
            }

            var server = MCPServiceLocator.Server;
            if (!server.IsLocalHttpServerReachable())
            {
                if (!server.StartLocalHttpServer(quiet: true))
                {
                    Debug.LogWarning(
                        $"{logPrefix}: Local HTTP server not reachable; start ./scripts/unity-mcp-http.sh or use Window → MCP for Unity → Start Local HTTP Server.");
                }
            }

            await WaitForServerAsync(server, TimeSpan.FromSeconds(30));

            bool bridgeStarted = await MCPServiceLocator.Bridge.StartAsync();
            if (!bridgeStarted)
            {
                Debug.LogError($"{logPrefix}_FAIL: Unity MCP bridge did not start. Open Window → MCP for Unity and click Start Bridge.");
                return;
            }

            if (openTrainingScene && File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
            }

            var verify = await MCPServiceLocator.Bridge.VerifyAsync();
            if (!IsBridgeVerifySuccess(verify))
            {
                Debug.LogError($"{logPrefix}_FAIL: Bridge verify — {verify.Message}");
                return;
            }

            Debug.Log(
                $"{logPrefix}_OK: HTTP {HttpEndpointUtility.GetMcpRpcUrl()} | bridge connected | scene={(openTrainingScene ? ScenePath : "unchanged")}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"{logPrefix}_FAIL: {ex.Message}");
        }
    }

    private static bool IsBridgeVerifySuccess(BridgeVerificationResult verify)
    {
        if (verify.Success)
        {
            return true;
        }

        // HTTP transport may report websocket hub connectivity in Message while Success stays false.
        return !string.IsNullOrEmpty(verify.Message)
               && verify.Message.IndexOf("connected", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void ApplyBobMcpEditorPrefs()
    {
        string repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        EditorConfigurationCache.Instance.SetUseHttpTransport(true);
        EditorConfigurationCache.Instance.SetHttpTransportScope("local");
        EditorConfigurationCache.Instance.SetHttpBaseUrl("http://127.0.0.1:8080");
        EditorPrefs.SetBool(PrefAutoStartOnLoad, true);
        EditorPrefs.SetBool(PrefLockCursorConfig, true);
        EditorPrefs.SetString(PrefClientProjectDirOverride, repoRoot);
        SetupWindowService.MarkSetupCompleted();
    }

    private static async Task WaitForServerAsync(IServerManagementService server, TimeSpan timeout)
    {
        var start = EditorApplication.timeSinceStartup;
        while (EditorApplication.timeSinceStartup - start < timeout.TotalSeconds)
        {
            if (server.IsLocalHttpServerReachable())
            {
                return;
            }

            try
            {
                await Task.Delay(500);
            }
            catch
            {
                return;
            }
        }
    }
}
