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

| Flag                                    | Purpose                   |
| --------------------------------------- | ------------------------- |
| `-batchmode`                            | Run without UI            |
| `-quit`                                 | Exit after task completes |
| `-logFile <path>`                       | Write editor log          |
| `-executeMethod Namespace.Class.Method` | Run static C# entry point |
| `-buildTarget WebGL`                    | Set active build target   |

## Cursor / VS Code Integration

### Installed via workspace

| Tool       | Extension ID                      | Purpose                           |
| ---------- | --------------------------------- | --------------------------------- |
| Unity      | `visualstudiotoolsforunity.vstuc` | Debug, attach to Editor           |
| C#         | `ms-dotnettools.csharp`           | IntelliSense for `Assets/Scripts` |
| C# Dev Kit | `ms-dotnettools.csdevkit`         | Solution explorer                 |

Workspace settings (`.vscode/settings.json`) point Unity to `6000.5.0f1` and the repo root.

### AI Agent Assistants for Unity

| Tool                        | Type              | Notes                                                                                                                                      |
| --------------------------- | ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| **Cursor + AGENTS.md**      | IDE agent         | Primary — project rules in `AGENTS.md`, `.cursor/rules/bob.mdc`                                                                            |
| **Unity MCP (`bob-unity`)** | MCP server        | **Required for agents** — live Editor access via [MCP for Unity](https://github.com/CoplayDev/unity-mcp); see [unity-mcp.md](unity-mcp.md) |
| **Unity Muse**              | Unity cloud AI    | Paid Unity service; texture/code assist inside Editor                                                                                      |
| **Unity Sentis**            | Runtime inference | For deployed models, not training loop setup                                                                                               |

**Recommended for Bob:** Cursor with `AGENTS.md` + Unity Tools extension + **`bob-unity` MCP** (Editor open for scene/agent work). Batchmode CLI handles automation (builds, tests, scene rebuild); Cursor + Unity MCP handles live Editor inspection and parameterized changes.

### Unity MCP (`bob-unity`)

Repo-configured via [`.cursor/mcp.json`](../.cursor/mcp.json) and `com.coplaydev.unity-mcp` in [`Packages/manifest.json`](../Packages/manifest.json).

```bash
brew install uv                    # if missing
chmod +x scripts/unity-mcp.sh
```

1. Open Bob in Unity → **Window → MCP for Unity** → setup wizard → transport **stdio** → Configure Cursor
2. Restart Cursor; enable **`bob-unity`** in MCP settings
3. Agents must consult `bob-unity` tools before Unity edits (see [unity-mcp.md](unity-mcp.md))

Keep Unity Editor open while using MCP tools.

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

| Environment            | How trainer reaches Unity                                     |
| ---------------------- | ------------------------------------------------------------- |
| Local venv (Intel Mac) | `mlagents-learn` on host                                      |
| Docker (Apple Silicon) | `docker compose run --rm train ...` with `network_mode: host` |
| Dev Container          | Reopen in Container; forward port 5004                        |

Set Unity external Python (optional for Editor tools):

**Edit → Preferences → External Tools → Python** → `python/.venv/bin/python`

## Progress Screenshots

Capture versioned scene screenshots into [`docs/progress/`](progress/) to document build milestones in git.

### Capture (edit mode)

```bash
chmod +x scripts/capture-progress.sh

# Label becomes a slug in the folder name: docs/progress/NNN-YYYY-MM-DD-<label>/
./scripts/capture-progress.sh week1-scene-baseline
```

Or in the Editor: **Bob → Capture Progress Screenshot**

Each capture writes:

- `capture.png` — Main Camera render of `BobTraining` (default 1280×720)
- `meta.json` — timestamp, Unity version, git commit, mode (`edit`)
- Updates `docs/progress/README.md` gallery index

Optional environment variables:

| Variable                                   | Purpose                                                     |
| ------------------------------------------ | ----------------------------------------------------------- |
| `BOB_CAPTURE_LABEL`                        | Milestone slug (set automatically by `capture-progress.sh`) |
| `BOB_CAPTURE_GIT_SHA`                      | Short git commit (set automatically by script)              |
| `BOB_CAPTURE_WIDTH` / `BOB_CAPTURE_HEIGHT` | Override resolution (default 1280×720)                      |

**Important:** Do **not** use `-nographics` for screenshot capture — GPU rendering is required. Scene build and validation CLI commands still use `-nographics`; progress capture uses a separate invocation without it.

### Week 2 — Play-mode captures (planned)

Edit-mode captures show the static scene. Week 2 adds play-mode frames for training progress:

| Approach                         | Use case                                                                                |
| -------------------------------- | --------------------------------------------------------------------------------------- |
| **Inference demo** (recommended) | Load `results/**/Bob.onnx`, set Behavior Type to Inference Only, capture mid-shot frame |
| **Unity Test Framework**         | Deterministic play-mode test that waits N physics frames then captures                  |
| **Unity Recorder**               | Export frame sequences or GIFs from play mode for portfolio clips                       |

Future CLI entry point: `BobProgressCapture.CapturePlayModeFromCli` with `mode: "play"` in `meta.json`. Optional `scripts/capture-training-frame.sh` to pair with training checkpoints.

## Quick Commands

```bash
# Progress screenshot (GPU required; no -nographics)
./scripts/capture-progress.sh milestone-label

# Training via Docker (Apple Silicon)
./scripts/train.sh

# Local training (Intel Mac or working mlagents venv)
cd python && source .venv/bin/activate
mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0

# WebGL build (Week 3)
./scripts/unity.sh -batchmode -quit -buildTarget WebGL -buildPath Build/WebGL
```
