# Unity MCP for Bob

Agents use **MCP for Unity** ([CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp)) to inspect and modify the live Unity Editor with validated tool parameters. The Cursor MCP server is registered as **`bob-unity`** in [`.cursor/mcp.json`](../.cursor/mcp.json).

## Why MCP for Unity (not just RAG)

| Source        | What it provides                                                                 |
| ------------- | -------------------------------------------------------------------------------- |
| **bob-rag**   | Repo code, docs, and config patterns                                             |
| **bob-unity** | Live scene hierarchy, GameObject/component state, console output, Editor actions |

RAG tells you how Bob _should_ be built; Unity MCP tells you what the Editor _currently contains_ and applies changes with schema-valid parameters.

## Prerequisites

- Unity **6.0.5f1** (see [unity-dev.md](unity-dev.md))
- **uv** / **uvx** — `brew install uv`
- Cursor with project MCP config enabled
- Unity Editor **open** on this repo while using `bob-unity` tools

## One-time setup

### 1. Package (repo-tracked)

[`Packages/manifest.json`](../Packages/manifest.json) includes:

```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main"
```

Open the project in Unity so Package Manager resolves the dependency.

### 2. Unity Editor wizard

1. **Window → MCP for Unity**
2. Confirm Python and **uv** are detected (install via wizard if needed)
3. Set transport to **stdio** (matches `./scripts/unity-mcp.sh`)
4. Select **Cursor** → **Configure** (merges with project `.cursor/mcp.json`)
5. Confirm bridge status is **Connected**

### 3. Cursor

1. Restart Cursor after config changes
2. Enable **`bob-unity`** in MCP settings (alongside **`bob-rag`**)
3. Verify tools appear (e.g. `manage_scene`, `find_gameobjects`, `manage_components`)

Manual stdio entrypoint (same as MCP config):

```bash
chmod +x scripts/unity-mcp.sh
./scripts/unity-mcp.sh
```

## Required agent workflow

Before **any Unity development task** (see [AGENTS.md](../AGENTS.md)):

1. Open Unity Editor on Bob project; MCP bridge connected
2. Consult **`bob-unity`** MCP tool schemas — never guess parameter shapes
3. Inspect before mutating:
   - `manage_scene` → `get_active`, `get_hierarchy`
   - `find_gameobjects` → locate targets by name/path/component
4. Apply changes with documented actions:
   - `manage_gameobject` → create/modify/delete GameObjects
   - `manage_components` → add/remove/set component properties
5. Verify with `read_console`

Cursor hook [`.cursor/hooks/unity-pre-code.sh`](../.cursor/hooks/unity-pre-code.sh) injects this checklist when editing paths under `Assets/`, `ProjectSettings/`, or `Packages/`.

## Key tools for Bob

| Tool                | Typical use                                                        |
| ------------------- | ------------------------------------------------------------------ |
| `manage_scene`      | Confirm `BobTraining` is active; read hierarchy before scene edits |
| `find_gameobjects`  | Find Bob, hoop, ball, Main Camera                                  |
| `manage_components` | Verify Behavior Name `Bob`, observation/action sizes, physics      |
| `manage_gameobject` | Place or adjust scene objects during Week 2+ iteration             |
| `read_console`      | Catch ML-Agents or script errors after Play / edits                |

Full tool reference: [MCP for Unity docs](https://coplaydev.github.io/unity-mcp/reference/tools).

## Bob-specific constraints

- **Behavior Name:** `Bob` — must match `config/bob_free_throw.yaml` `behaviors: Bob:`
- **Training scene:** `BobTraining` (built via `BobTrainingSceneBuilder` or **Bob → Create Training Scene**)
- **CLI fallbacks** when Editor MCP is unavailable:
  - `./scripts/unity.sh -executeMethod BobTrainingSceneBuilder.CreateTrainingSceneFromCli`
  - `./scripts/unity.sh -executeMethod BobSceneValidator.VerifyFromCli`
  - `./scripts/capture-progress.sh <label>` (GPU required; no `-nographics`)

## Troubleshooting

| Symptom                       | Fix                                                                                 |
| ----------------------------- | ----------------------------------------------------------------------------------- |
| Cursor: bob-unity won't start | `brew install uv`; run `./scripts/unity-mcp.sh` in terminal to see errors           |
| Tools list empty / calls fail | Unity Editor closed or bridge disconnected — open **Window → MCP for Unity**        |
| uv not found from Unity GUI   | In MCP window: **Choose UV Install Location** → `/opt/homebrew/bin/uv`              |
| Package import errors         | Re-open project; check Package Manager for `com.coplaydev.unity-mcp`                |
| HTTP vs stdio mismatch        | Bob repo uses **stdio** via `./scripts/unity-mcp.sh`; align transport in MCP window |

## Official Unity MCP (alternative)

Unity also ships a first-party MCP bridge in **`com.unity.ai.assistant`** (preview). It uses a relay binary at `~/.unity/relay/` and **Edit → Project Settings → AI → Unity MCP** for client approval. Bob standardizes on **CoplayDev MCP for Unity** because it is repo-installable, works with Unity 6 + ML-Agents without the AI Assistant preview package, and documents Cursor stdio setup clearly.

To try official Unity MCP instead, add `com.unity.ai.assistant` in Package Manager and configure Cursor per [Unity docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-get-started.html).

## Related

- [unity-dev.md](unity-dev.md) — Unity CLI, ML-Agents, progress captures
- [cursor-setup.md](cursor-setup.md) — IDE + MCP enablement
- [rag.md](rag.md) — repository RAG (`bob-rag`)
