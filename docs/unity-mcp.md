# Unity MCP for Bob

Agents use **MCP for Unity** ([CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp)) to inspect and modify the live Unity Editor with validated tool parameters. The Cursor MCP server is registered as **`unityMCP`** in [`.cursor/mcp.json`](../.cursor/mcp.json), matching the [official Cursor install guide](https://coplaydev.github.io/unity-mcp/getting-started/install).

## Why MCP for Unity (not just RAG)

| Source       | What it provides                                                                 |
| ------------ | -------------------------------------------------------------------------------- |
| **bob-rag**  | Repo code, docs, and config patterns                                             |
| **unityMCP** | Live scene hierarchy, GameObject/component state, console output, Editor actions |

RAG tells you how Bob _should_ be built; Unity MCP tells you what the Editor _currently contains_ and applies changes with schema-valid parameters.

## Prerequisites

- Unity **6.0.5f1** (see [unity-dev.md](unity-dev.md))
- **uv** / **uvx** — `brew install uv`
- Cursor with project MCP config enabled
- Unity Editor **open** on this repo while using `unityMCP` tools

## One-time setup

### 1. Package (repo-tracked)

[`Packages/manifest.json`](../Packages/manifest.json) includes:

```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main"
```

Open the project in Unity so Package Manager resolves the dependency.

### 2. Unity Editor wizard (official flow)

1. **Window → MCP for Unity** (setup wizard may open on first import)
2. Confirm Python and **uv** are detected (install via wizard if needed)
3. Click **Auto-Setup** — registers Cursor and starts the local HTTP server
4. Ensure transport is **HTTP** (CoplayDev default for Cursor; matches `.cursor/mcp.json`)
5. Click **Start Bridge** if Unity Bridge shows **Stopped**
6. Status panel should read **Connected ✓**

Per-client note from upstream docs: after Auto-Setup, **enable the MCP toggle for this project in Cursor Settings**.

Optional: **Cursor → Auto Configure** in the MCP window merges the `unityMCP` block into [`.cursor/mcp.json`](../.cursor/mcp.json) without removing `bob-rag`.

### 3. Cursor

1. Restart Cursor after config changes
2. Enable **`unityMCP`** and **`bob-rag`** in MCP settings
3. Verify tools appear (e.g. `manage_scene`, `find_gameobjects`, `manage_components`)

Committed HTTP entry (same as [official manual config](https://coplaydev.github.io/unity-mcp/getting-started/install)):

```json
"unityMCP": {
  "url": "http://127.0.0.1:8080/mcp"
}
```

Stdio fallback (only if you switch Unity transport to stdio **and** change `.cursor/mcp.json`):

```bash
chmod +x scripts/unity-mcp.sh
./scripts/unity-mcp.sh
```

## Required agent workflow

Before **any Unity development task** (see [AGENTS.md](../AGENTS.md)):

1. Open Unity Editor on Bob project; MCP bridge connected (HTTP server on `127.0.0.1:8080`, bridge **Running**)
2. Consult **`unityMCP`** MCP tool schemas — never guess parameter shapes
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

| Symptom                       | Fix                                                                                                                      |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| Cursor: unityMCP won't start  | Unity → **Auto-Setup**; confirm local HTTP server on `127.0.0.1:8080`; `brew install uv` if wizard fails                 |
| Tools list empty / calls fail | Unity Editor closed, bridge **Stopped**, or HTTP server not running — **Window → MCP for Unity**                         |
| `instance_count: 0`           | Click **Start Bridge**; keep transport **HTTP** to match `.cursor/mcp.json`                                              |
| uv not found from Unity GUI   | In MCP window: **Choose UV Install Location** → `/opt/homebrew/bin/uv`                                                   |
| Package import errors         | Re-open project; check Package Manager for `com.coplaydev.unity-mcp`                                                     |
| HTTP vs stdio mismatch        | Bob defaults to **HTTP** (`unityMCP` url). If using stdio, switch Unity transport **and** `mcp.json`                     |
| Duplicate MCP entries         | Unity Auto-Configure writes to **`~/.cursor/mcp.json`** — keep **`Bob/.cursor/mcp.json`** only; remove global `unityMCP` |
| HTTP server not running       | Unity → **Start Local HTTP Server**, or `./scripts/unity-mcp-http.sh` in a terminal; then **Start Bridge** in Editor     |

## Official Unity MCP (alternative)

Unity also ships a first-party MCP bridge in **`com.unity.ai.assistant`** (preview). It uses a relay binary at `~/.unity/relay/` and **Edit → Project Settings → AI → Unity MCP** for client approval. Bob standardizes on **CoplayDev MCP for Unity** because it is repo-installable, works with Unity 6 + ML-Agents without the AI Assistant preview package, and documents Cursor stdio setup clearly.

To try official Unity MCP instead, add `com.unity.ai.assistant` in Package Manager and configure Cursor per [Unity docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-get-started.html).

## Related

- [unity-dev.md](unity-dev.md) — Unity CLI, ML-Agents, progress captures
- [cursor-setup.md](cursor-setup.md) — IDE + MCP enablement
- [rag.md](rag.md) — repository RAG (`bob-rag`)
