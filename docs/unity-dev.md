# Unity Development Setup for Bob

Unity **6.0.5f1** is installed at:

```text
/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app
```

## Project Location

Create (or open) the Unity project at the **repo root** so `Assets/` sits alongside `config/`, `python/`, and `terraform/`.

If your project lives elsewhere, set in `.env`:

```bash
UNITY_PROJECT_PATH=/path/to/your/unity/project
```

## Unity CLI (Batch Mode)

Use the repo wrapper script:

```bash
chmod +x scripts/unity.sh

# Print Unity version
./scripts/unity.sh -batchmode -quit -logFile - 2>&1 | head -20

# Run tests (when test assemblies exist)
./scripts/unity.sh -batchmode -runTests -testPlatform playmode -quit -logFile logs/unity-tests.log
```

Direct invocation:

```bash
/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app/Contents/MacOS/Unity \
  -projectPath "$(pwd)" \
  -batchmode -quit
```

### Useful CLI Flags

| Flag | Purpose |
|------|---------|
| `-batchmode` | Run without UI |
| `-quit` | Exit after task completes |
| `-logFile <path>` | Write editor log |
| `-executeMethod Namespace.Class.Method` | Run static C# entry point |
| `-buildTarget WebGL` | Set active build target |

## Cursor / VS Code Integration

### Installed via workspace

| Tool | Extension ID | Purpose |
|------|--------------|---------|
| Unity | `visualstudiotoolsforunity.vstuc` | Debug, attach to Editor |
| C# | `ms-dotnettools.csharp` | IntelliSense for `Assets/Scripts` |
| C# Dev Kit | `ms-dotnettools.csdevkit` | Solution explorer |

Workspace settings (`.vscode/settings.json`) point Unity to `6000.5.0f1` and the repo root.

### AI Agent Assistants for Unity

| Tool | Type | Notes |
|------|------|-------|
| **Cursor + AGENTS.md** | IDE agent | Primary — project rules in `AGENTS.md`, `.cursor/rules/bob.mdc` |
| **Unity MCP** (community) | MCP server | Optional — exposes scene hierarchy, assets to AI via Model Context Protocol; search GitHub for `unity-mcp` |
| **Unity Muse** | Unity cloud AI | Paid Unity service; texture/code assist inside Editor |
| **Unity Sentis** | Runtime inference | For deployed models, not training loop setup |

**Recommended for Bob:** Cursor with `AGENTS.md` + Unity Tools extension. No official Unity CLI AI agent exists; batchmode CLI handles automation (builds, tests), while Cursor handles code generation.

### Optional: Unity MCP

To give Cursor read/write access to the Unity Editor (scene objects, play mode):

1. Install a community Unity MCP server (e.g. `coplaydev/unity-mcp` or similar)
2. Add to Cursor MCP settings pointing at the local server
3. Keep Unity Editor open while using MCP tools

This is optional for MVP — manual scene setup in Week 1 is fine.

## ML-Agents Package

In Unity Package Manager:

1. **Window → Package Manager → + → Add package by name**
2. Enter: `com.unity.ml-agents` version **4.0.3** (Unity 6; pairs with `mlagents==1.1.0` in Python)
3. Unity 6 also pulls `com.unity.ai.inference` (replaces Sentis)

### Automated scene setup (CLI)

```bash
/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -nographics -projectPath "$(pwd)" \
  -logFile logs/unity-scene-build.log \
  -executeMethod BobTrainingSceneBuilder.CreateTrainingSceneFromCli

/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -nographics -projectPath "$(pwd)" \
  -logFile logs/unity-validate.log \
  -executeMethod BobSceneValidator.VerifyFromCli
```

Or in the Editor: **Bob → Create Training Scene**

### Behavior Setup Checklist

- Behavior Name: `Bob` (must match `config/bob_free_throw.yaml`)
- Behavior Type: `Default` for training, `Inference Only` for demo
- Vector Observation / Continuous Actions configured in agent script

## Python ↔ Unity Connection

ML-Agents uses gRPC on port **5004** by default.

| Environment | How trainer reaches Unity |
|-------------|---------------------------|
| Local venv (Intel Mac) | `mlagents-learn` on host |
| Docker (Apple Silicon) | `docker compose run --rm train ...` with `network_mode: host` |
| Dev Container | Reopen in Container; forward port 5004 |

Set Unity external Python (optional for Editor tools):

**Edit → Preferences → External Tools → Python** → `python/.venv/bin/python`

## Quick Commands

```bash
# Training via Docker (Apple Silicon)
./scripts/train.sh

# Local training (Intel Mac or working mlagents venv)
cd python && source .venv/bin/activate
mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0

# WebGL build (Week 3)
./scripts/unity.sh -batchmode -quit -buildTarget WebGL -buildPath Build/WebGL
```
