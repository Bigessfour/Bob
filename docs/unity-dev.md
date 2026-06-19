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
| `-buildTarget StandaloneOSX`            | Set active build target   |

## Cursor / VS Code Integration

### Installed via workspace

| Tool       | Extension ID                      | Purpose                           |
| ---------- | --------------------------------- | --------------------------------- |
| Unity      | `visualstudiotoolsforunity.vstuc` | Debug, attach to Editor           |
| C#         | `ms-dotnettools.csharp`           | IntelliSense for `Assets/Scripts` |
| C# Dev Kit | `ms-dotnettools.csdevkit`         | Solution explorer                 |

Workspace settings (`.vscode/settings.json`) point Unity to `6000.5.0f1` and the repo root.

### AI Agent Assistants for Unity

| Tool                       | Type              | Notes                                                                                                                                      |
| -------------------------- | ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| **Cursor + AGENTS.md**     | IDE agent         | Primary — project rules in `AGENTS.md`, `.cursor/rules/bob.mdc`                                                                            |
| **Unity MCP (`unityMCP`)** | MCP server        | **Required for agents** — live Editor access via [MCP for Unity](https://github.com/CoplayDev/unity-mcp); see [unity-mcp.md](unity-mcp.md) |
| **Unity Muse**             | Unity cloud AI    | Paid Unity service; texture/code assist inside Editor                                                                                      |
| **Unity Sentis**           | Runtime inference | For deployed models, not training loop setup                                                                                               |

**Recommended for Bob:** Cursor with `AGENTS.md` + Unity Tools extension + **`unityMCP` MCP** (Editor open, HTTP bridge connected). Batchmode CLI handles automation (builds, tests, scene rebuild); Cursor + Unity MCP handles live Editor inspection and parameterized changes.

### Unity MCP (`unityMCP`)

Repo-configured via [`.cursor/mcp.json`](../.cursor/mcp.json) and `com.coplaydev.unity-mcp` in [`Packages/manifest.json`](../Packages/manifest.json).

```bash
brew install uv                    # if missing
chmod +x scripts/unity-mcp.sh
```

1. Open Bob in Unity → **Window → MCP for Unity** → setup wizard → transport **stdio** → Configure Cursor
2. Restart Cursor; enable **`unityMCP`** and **`bob-rag`** in MCP settings
3. Agents must consult `unityMCP` tools before Unity edits (see [unity-mcp.md](unity-mcp.md))

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

Or in the Editor: **Bob → Rebuild Arc Academy (HDRP)** or **Bob → Create Training Scene**

### Render pipeline (HDRP)

Arc Academy targets **HDRP** photoreal quality (Volume bloom/SSR, APV, Physical Sky, glossy floor). `./scripts/validate-scene.sh` runs `ArcAcademyHdrpSetup.EnsureHdrpFromCli` before rebuilding the scene.

**No WebGL** — HDRP is Editor-only for this project. Week 3 live demo is a **static portfolio site** (gallery, GIFs, write-up) on S3/CloudFront.

### Training arena layout (Arc Academy HDRP build)

Visual target: [`docs/design/arc-academy-reference.jpg`](design/arc-academy-reference.jpg) (from Example.jpg).

`BobTrainingSceneBuilder` creates a **warehouse Arc Academy** under `TrainingArena`:

| Element                                      | Purpose                                                                         |
| -------------------------------------------- | ------------------------------------------------------------------------------- |
| `HdrpVolume` / `AdaptiveProbeVolume`         | Post-processing (Bloom, SSR, Exposure) + APV lighting                           |
| `WarehouseShell`                             | Glossy dark floor, corrugated walls, ceiling trusses                            |
| `MountainWindow`                             | Panoramic left-wall window with procedural mountains                            |
| `CourtFloor` + markings                      | Orange court, key, 3pt arc, center circle, distance marks                       |
| `SpawnPad`                                   | Central platform with purple glow, particles + **Bob** / **Arc Academy** labels |
| `TrainingBays` (8)                           | Perimeter cubicles with decorative hoops + `RoboticLauncherVisual` placeholders |
| `DecorativeHoops`                            | 3 static pedestal hoops on court (`DecorativeHoopMarker`, no scoring)           |
| `TrajectoryVisuals`                          | 3 glowing parabolic `LineRenderer` arcs (portfolio visual)                      |
| `LightingRig`                                | Window fill + HDRP rectangle ceiling lights                                     |
| `ReflectionProbe`                            | Floor reflections (with SSR in Volume)                                          |
| `Hoop` / `MovableHoop` / `Rim` / `ScoreZone` | **Single active scoring hoop** — glass backboard, net, physics colliders        |
| `ArcAcademyManager`                          | `PrepareEpisode()` keeps layout stable unless `randomizeEpisodeLayout` enabled  |
| `Bob`                                        | 8 obs, 3 actions, purple emissive glow, gravity arcs                            |

Shared dimensions: [`Assets/Scripts/ArcAcademyLayout.cs`](../../Assets/Scripts/ArcAcademyLayout.cs). Rebuild:

```bash
./scripts/validate-scene.sh
```

### Behavior Setup Checklist

- Behavior Name: `Bob` (must match `config/bob_free_throw.yaml`)
- Behavior Type: `Default` for training, `Inference Only` for demo
- Vector Observation / Continuous Actions configured in agent script

## Python ↔ Unity Connection

ML-Agents uses gRPC on port **5004** by default.

| Environment            | How trainer reaches Unity                                                                           |
| ---------------------- | --------------------------------------------------------------------------------------------------- |
| Local venv (Intel Mac) | `mlagents-learn` on host                                                                            |
| Docker (Apple Silicon) | `./scripts/train.sh` or `docker compose run --rm --service-ports train ...` (publishes `5004:5004`) |
| Dev Container          | Reopen in Container; forward port 5004                                                              |

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

### Capture (play mode)

Waits for Play mode to enter, settles HDRP for `BOB_CAPTURE_PLAY_FRAMES` physics frames (default **120**), then captures `Camera.main` with `mode: "play"` in `meta.json`. **Close the Unity Editor** before running — batchmode cannot share the project with a live Editor instance.

```bash
./scripts/capture-progress.sh --play arc-academy-playmode-hero

# Optional: wait longer for probes/SSR (e.g. ~4s at 60fps)
BOB_CAPTURE_PLAY_FRAMES=240 ./scripts/capture-progress.sh --play arc-academy-playmode-hero
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
| `BOB_CAPTURE_PLAY_FRAMES`                  | Play-mode settle frames before capture (default 120)        |

**Important:** Do **not** use `-nographics` for screenshot capture — GPU rendering is required. Scene build and validation CLI commands still use `-nographics`; progress capture uses a separate invocation without it.

### Play-mode vs edit-mode

| Mode | CLI                                            | `meta.json` `mode` |
| ---- | ---------------------------------------------- | ------------------ |
| Edit | `./scripts/capture-progress.sh <label>`        | `edit`             |
| Play | `./scripts/capture-progress.sh --play <label>` | `play`             |

Play-mode captures run HDRP with physics and lighting active (reflection probes, bloom, spawn pad VFX). Use play mode for portfolio hero shots that match in-Editor **Play** view.

## Quick Commands

```bash
# Progress screenshot (GPU required; no -nographics)
./scripts/capture-progress.sh arc-academy-hdrp-v1
./scripts/capture-progress.sh --play arc-academy-playmode-hero
```
