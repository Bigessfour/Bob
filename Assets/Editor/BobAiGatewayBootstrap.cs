using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Applies xAI Grok BYOM credentials to Unity AI Gateway (Codex agent) from environment
/// or repo .env on Editor load. Never logs or persists raw API keys in the project.
/// </summary>
[InitializeOnLoad]
public static class BobAiGatewayBootstrap
{
    private const string AgentCodex = "codex";
    private const string DefaultModel = "grok-4.3";
    private const string BaseUrlEnv = "OPENAI_BASE_URL";
    private const string BaseUrlDefault = "https://api.x.ai/v1";

    static BobAiGatewayBootstrap()
    {
        if (Application.isBatchMode &&
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UNITY_AI_GATEWAY_ALLOW_BATCH")))
        {
            return;
        }

        EditorApplication.delayCall += TryConfigureGateway;
    }

    [MenuItem("Bob/AI Gateway/Apply Grok BYOM Settings")]
    public static void ApplyFromMenu()
    {
        TryConfigureGateway();
    }

    static void TryConfigureGateway()
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.Log(
                "BOB_AI_GATEWAY: XAI_API_KEY / OPENAI_API_KEY not set. " +
                "Run ./scripts/setup-unity-ai-gateway.sh --sync-env");
            return;
        }

        var baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnv) ?? BaseUrlDefault;
        var model = Environment.GetEnvironmentVariable("GROK_MODEL") ?? DefaultModel;

        Environment.SetEnvironmentVariable("OPENAI_API_KEY", apiKey);
        Environment.SetEnvironmentVariable("XAI_API_KEY", apiKey);
        Environment.SetEnvironmentVariable(BaseUrlEnv, baseUrl);

        EditorPrefs.SetBool("AIAssistant.ForceAiGateway", true);
        TrySetSelectedModel(AgentCodex, model);
        TryEnableProvider(AgentCodex);

        if (TryApplyGatewayPreferences(apiKey, baseUrl))
        {
            Debug.Log(
                $"BOB_AI_GATEWAY_OK: Codex → xAI ({baseUrl}), model={model}. " +
                "Select Codex in Assistant agent dropdown to bypass Unity credits.");
        }
        else
        {
            Debug.Log(
                $"BOB_AI_GATEWAY_PARTIAL: env set for Codex → xAI ({baseUrl}), model={model}. " +
                "Open Project Settings → AI → Gateway to save credentials if Assistant prompts.");
        }
    }

    static string ResolveApiKey()
    {
        var fromEnv = Environment.GetEnvironmentVariable("XAI_API_KEY")
            ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(fromEnv))
        {
            return fromEnv;
        }

        return LoadKeyFromDotEnv();
    }

    static string LoadKeyFromDotEnv()
    {
        var envPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".env");
        if (!File.Exists(envPath))
        {
            return null;
        }

        foreach (var line in File.ReadAllLines(envPath))
        {
            if (line.StartsWith("XAI_API_KEY=", StringComparison.Ordinal))
            {
                return line.Substring("XAI_API_KEY=".Length).Trim();
            }

            if (line.StartsWith("OPENAI_API_KEY=", StringComparison.Ordinal))
            {
                return line.Substring("OPENAI_API_KEY=".Length).Trim();
            }
        }

        return null;
    }

    static bool TryApplyGatewayPreferences(string apiKey, string baseUrl)
    {
        var gatewayType = Type.GetType(
            "Unity.AI.Assistant.Editor.Settings.GatewayPreferenceService, Unity.AI.Assistant.Editor");
        if (gatewayType == null)
        {
            return false;
        }

        var instance = gatewayType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null);
        if (instance == null)
        {
            return false;
        }

        var preferencesSignal = gatewayType.GetProperty("Preferences")?.GetValue(instance);
        if (preferencesSignal == null)
        {
            return false;
        }

        var valueProp = preferencesSignal.GetType().GetProperty("Value");
        var prefs = valueProp?.GetValue(preferencesSignal);
        if (prefs == null)
        {
            return false;
        }

        var providers = prefs.GetType().GetProperty("ProviderInfoList")?.GetValue(prefs) as IList;
        if (providers == null)
        {
            return false;
        }

        object codexProvider = null;
        foreach (var provider in providers)
        {
            var typeProp = provider.GetType().GetProperty("ProviderType");
            if (typeProp?.GetValue(provider) as string == AgentCodex)
            {
                codexProvider = provider;
                break;
            }
        }

        if (codexProvider == null)
        {
            return false;
        }

        var variables = codexProvider.GetType().GetProperty("Variables")?.GetValue(codexProvider) as IList;
        if (variables == null)
        {
            return false;
        }

        UpsertEnvVar(variables, "OPENAI_API_KEY", apiKey, inKeychain: true);
        UpsertEnvVar(variables, BaseUrlEnv, baseUrl, inKeychain: false);

        var selectedAgentField = gatewayType.GetField(
            "SelectedAgentType", BindingFlags.Instance | BindingFlags.NonPublic);
        var selectedAgent = selectedAgentField?.GetValue(instance);
        selectedAgent?.GetType().GetProperty("Value")?.SetValue(selectedAgent, AgentCodex);

        valueProp.SetValue(preferencesSignal, prefs);
        return true;
    }

    static void UpsertEnvVar(IList variables, string name, string value, bool inKeychain)
    {
        var envVarType = Type.GetType("Unity.AI.Assistant.Editor.Settings.EnvVar, Unity.AI.Assistant.Editor");
        if (envVarType == null)
        {
            return;
        }

        object existing = null;
        foreach (var item in variables)
        {
            if (item.GetType().GetProperty("Name")?.GetValue(item) as string == name)
            {
                existing = item;
                break;
            }
        }

        if (existing == null)
        {
            existing = Activator.CreateInstance(envVarType, name, value, inKeychain);
            variables.Add(existing);
        }

        existing.GetType().GetProperty("Value")?.SetValue(existing, value);
        existing.GetType().GetProperty("InKeychain")?.SetValue(existing, inKeychain);
        existing.GetType().GetProperty("IsUpdated")?.SetValue(existing, true);
        existing.GetType().GetProperty("IsSet")?.SetValue(existing, true);
    }

    static void TrySetSelectedModel(string providerId, string modelId)
    {
        var prefsType = Type.GetType(
            "Unity.AI.Assistant.Editor.AssistantEditorPreferences, Unity.AI.Assistant.Editor");
        prefsType?.GetMethod("SetSelectedModel", BindingFlags.Public | BindingFlags.Static)
            ?.Invoke(null, new object[] { providerId, modelId });
    }

    static void TryEnableProvider(string providerId)
    {
        var prefsType = Type.GetType(
            "Unity.AI.Assistant.Editor.AssistantEditorPreferences, Unity.AI.Assistant.Editor");
        prefsType?.GetMethod("SetProviderEnabled", BindingFlags.Public | BindingFlags.Static)
            ?.Invoke(null, new object[] { providerId, true });
    }
}
