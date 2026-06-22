# Unity MCP for Bob

Agents use **Unity MCP** (official bridge in [`com.unity.ai.assistant`](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-overview.html)) to inspect and modify the live Unity Editor through validated MCP tools. Cursor connects via the **`unity-mcp`** server in [`.cursor/mcp.json`](../.cursor/mcp.json), which launches Unity’s relay at `~/.unity/relay/` with `--mcp`.

## Why MCP for Unity (not just RAG)

| Source        | What it provides                                                                 |
| ------------- | -------------------------------------------------------------------------------- |
| **bob-rag**   | Repo code, docs, and config patterns                                             |
| **unity-mcp** | Live scene hierarchy, GameObject/component state, console output, Editor actions |

RAG tells you how Bob _should_ be built; Unity MCP tells you what the Editor _currently contains_ and applies changes with schema-valid parameters.

## Prerequisites

- Unity **6.0.5f1** (see [unity-dev.md](unity-dev.md))
- [`com.unity.ai.assistant`](../Packages/manifest.json) in Package Manager (repo-tracked)
- Cursor with project MCP config enabled
- Unity Editor **open** on this repo while using `unity-mcp` tools

## One-time setup

### 1. Unity Editor

1. Open the Bob project in Unity 6 so Package Manager resolves `com.unity.ai.assistant`.
2. Go to **Edit → Project Settings → AI → Unity MCP**.
3. Confirm **Unity Bridge** status is **Running** (starts automatically on Editor load; use **Start** if stopped).
4. Under **Tools**, enable the groups you need for Bob development (at minimum: **Scene**, **GameObject**, **Components**, **Console**, **Scripting**).
5. Expand **Integrations** and use **Configure** for Cursor if offered, or rely on the committed [`.cursor/mcp.json`](../.cursor/mcp.json).

The relay binary installs to `~/.unity/relay/` on first Editor startup.

### 2. Cursor

```bash
chmod +x scripts/unity-mcp.sh
```

1. Restart Cursor after config changes.
2. Enable **`unity-mcp`** and **`bob-rag`** in MCP settings.
3. Verify built-in tools appear (e.g. `manage_scene`, `find_gameobjects`, `read_console`) and Bob custom tools (`bob_open_training_scene`, `bob_setup_simple_arena`).

Committed relay entry:

```json
"unity-mcp": {
  "command": "./scripts/unity-mcp.sh",
  "args": []
}
```

### 3. Approve Cursor (first connection)

1. **Edit → Project Settings → AI → Unity MCP**
2. Under **Pending Connections**, review the Cursor client entry.
3. Select **Accept**.

Approved clients reconnect automatically. AI Gateway (in-Editor Assistant) connections are auto-approved separately.

## Required agent workflow

Before **any Unity development task** (see [AGENTS.md](../AGENTS.md)):

1. Open Unity Editor on Bob project; Unity MCP bridge **Running**; Cursor approved under Connected Clients.
2. Consult **`unity-mcp`** MCP tool schemas — never guess parameter shapes.
3. If multiple Unity instances are connected, call `set_active_instance` or pass `unity_instance` on tool calls.
4. Inspect before mutating:
   - `manage_scene` → `get_active`, `get_hierarchy`
   - `find_gameobjects` → locate Bob, hoop, ball, camera
5. Apply changes with documented actions:
   - `manage_gameobject` → create/modify/delete GameObjects
   - `manage_components` → add/remove/set component properties
6. Use Bob custom tools when appropriate:
   - `bob_open_training_scene` — open `BobTraining.unity`
   - `bob_setup_simple_arena` — run `SimpleArcAcademyArenaBuilder` wiring (spawn, manager, prefab)
7. Verify with `read_console`.

Cursor hook [`.cursor/hooks/unity-pre-code.sh`](../.cursor/hooks/unity-pre-code.sh) injects this checklist when editing paths under `Assets/`, `ProjectSettings/`, or `Packages/`.

Human menu shortcuts: **Bob → MCP → …** in the Unity Editor.

## Key tools for Bob

| Tool                      | Typical use                                                        |
| ------------------------- | ------------------------------------------------------------------ |
| `manage_scene`            | Confirm `BobTraining` is active; read hierarchy before scene edits |
| `find_gameobjects`        | Find Bob, hoop, ball, Main Camera                                  |
| `manage_components`       | Verify Behavior Name `Bob`, observation/action sizes, physics      |
| `manage_gameobject`       | Place or adjust scene objects                                      |
| `read_console`            | Catch ML-Agents or script errors after Play / edits                |
| `bob_setup_simple_arena`  | Wire SimpleArcAcademyArena + single Bob training loop              |
| `bob_open_training_scene` | Open the training scene from MCP                                   |

Full built-in tool reference: [Unity MCP overview](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-overview.html). Bob custom tools are registered in [`BobUnityMcpTools.cs`](../Assets/Scripts/Editor/BobUnityMcpTools.cs).

## Bob-specific constraints

- **Behavior Name:** `Bob` — must match `config/bob_free_throw.yaml` `behaviors: Bob:`
- **Training scene:** `BobTraining` (built via `BobTrainingSceneBuilder` or **Bob → Create Training Scene**)
- **CLI fallbacks** when Editor MCP is unavailable:
  - `./scripts/unity.sh -executeMethod SimpleArcAcademyArenaBuilder.ApplyFromCli`
  - `./scripts/unity.sh -executeMethod BobTrainingSceneBuilder.CreateTrainingSceneFromCli`
  - `./scripts/unity.sh -executeMethod BobSceneValidator.VerifyFromCli`
  - `./scripts/capture-progress.sh <label>` (GPU required; no `-nographics`)

## Troubleshooting

| Symptom                         | Fix                                                                                                       |
| ------------------------------- | --------------------------------------------------------------------------------------------------------- |
| Cursor: `unity-mcp` won't start | Run `chmod +x scripts/unity-mcp.sh`; open Unity once so relay installs to `~/.unity/relay/`               |
| Tools list empty / calls fail   | Unity Editor closed, bridge **Stopped**, or Cursor not **Accepted** in Unity MCP settings                 |
| `Session not ready` / timeouts  | Wait for domain reload after script compile; call `refresh_unity`; check bridge is Running                |
| Multiple Unity instances        | `set_active_instance` with exact `Name@hash` from `unity_instances` resource                              |
| Tool disabled                   | **Edit → Project Settings → AI → Unity MCP → Tools** — enable the tool or its group                       |
| Package import errors           | Re-open project; confirm `com.unity.ai.assistant` in Package Manager                                      |
| Duplicate MCP entries           | Remove global `unityMCP` / CoplayDev entries from `~/.cursor/mcp.json`; use **Bob/.cursor/mcp.json** only |

## Unity AI Gateway (Grok BYOM)

For **in-Editor** prompts through **xAI Grok** (bypass Unity credits), see [unity-ai-gateway.md](unity-ai-gateway.md). Bob configures the Codex agent against `https://api.x.ai/v1` via `./scripts/setup-unity-ai-gateway.sh`. This is separate from the Cursor `unity-mcp` bridge.

## Related

- [unity-dev.md](unity-dev.md) — Unity CLI, ML-Agents, progress captures
- [cursor-setup.md](cursor-setup.md) — IDE + MCP enablement
- [rag.md](rag.md) — repository RAG (`bob-rag`)
