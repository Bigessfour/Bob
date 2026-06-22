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

| Tool                        | Type              | Notes                                                                                                                                                                                       |
| --------------------------- | ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Cursor + AGENTS.md**      | IDE agent         | Primary — project rules in `AGENTS.md`, `.cursor/rules/bob.mdc`                                                                                                                             |
| **Unity MCP (`unity-mcp`)** | MCP server        | **Required for agents** — live Editor access via [Unity MCP](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-overview.html); see [unity-mcp.md](unity-mcp.md) |
| **Unity Muse**              | Unity cloud AI    | Paid Unity service; texture/code assist inside Editor                                                                                                                                       |
| **Unity Sentis**            | Runtime inference | For deployed models, not training loop setup                                                                                                                                                |

**Recommended for Bob:** Cursor with `AGENTS.md` + Unity Tools extension + **`unity-mcp` MCP** (Editor open, Unity MCP bridge Running). Batchmode CLI handles automation (builds, tests, scene rebuild); Cursor + Unity MCP handles live Editor inspection and parameterized changes.

### Unity MCP (`unity-mcp`)

Repo-configured via [`.cursor/mcp.json`](../.cursor/mcp.json) and `com.unity.ai.assistant` in [`Packages/manifest.json`](../Packages/manifest.json).

```bash
chmod +x scripts/unity-mcp.sh
```

1. Open Bob in Unity → **Edit → Project Settings → AI → Unity MCP** → bridge **Running** → approve Cursor
2. Restart Cursor; enable **`unity-mcp`** and **`bob-rag`** in MCP settings
3. Agents must consult `unity-mcp` tools before Unity edits (see [unity-mcp.md](unity-mcp.md))

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

Arc Academy uses **HDRP** in the Unity Editor. **Default visual target** is the clean **Arc Academy Lab** (AI Warehouse–inspired): flat materials, readable lighting, in-scene scoreboards — see [`docs/design/visual-vision.md`](design/visual-vision.md). A photoreal warehouse preset remains an optional stretch reference.

`./scripts/validate-scene.sh` runs `ArcAcademyHdrpSetup.EnsureHdrpFromCli` before rebuilding the scene (applies **lab** exposure/bloom).

**No WebGL** — HDRP is Editor-only for this project. Week 3 live demo is a **static portfolio site** (gallery, GIFs, write-up) on S3/CloudFront.

### Where Bob tools live

| What you want               | Where to click                                                               |
| --------------------------- | ---------------------------------------------------------------------------- |
| Fix white / blown-out scene | **Hierarchy → TrainingArena** → Inspector → **Fix White Blowout (In-Place)** |
| Same fix via menu           | **Top menu bar** → **Bob** → **HDRP** → **Fix White Blowout (In-Place)**     |
| ML-Agents / shooting tuning | **Hierarchy → Bob** → Inspector (Behavior Parameters, forces)                |
| MCP / AI Gateway            | Top menu **Bob → MCP** or **Bob → AI Gateway**                               |

**Common mix-up:** Selecting the orange **Bob** cube only shows the agent (ML-Agents). HDRP and scene tools are on **TrainingArena** or the **Bob** top menu — not on the Bob GameObject.

**No Bob top menu?** Check Console for red compile errors; wait for scripts to finish compiling, then look on the macOS menu bar between **Window** and **Help**.

**CLI (Editor must be closed):** `./scripts/fix-hdrp-blowout.sh`

### Scene looks dark, blown out, or blurry

The scoreboard is OnGUI — it can work even when HDRP lighting is wrong. Fix the 3D view:

1. **Bob → HDRP → Fix White Blowout (In-Place)** — clamps 40+ warehouse lights + exposure 6
2. **Bob → Rebuild Arc Academy (HDRP)** — applies lab lighting (fewer lights, sane intensities)
3. **Bob → HDRP → Apply Lab Lighting** — exposure/bloom/motion blur tuned for readability
4. Press **Play** and watch the **Game** tab (not Scene view with gizmos)
5. Optional: **Bob → HDRP → Upgrade Project Materials**

Root cause is usually **too many HDRP lights** stacked (warehouse preset). Lab lighting targets AI Warehouse readability — see [visual-vision.md](design/visual-vision.md).

### HDRP material migration warning

Unity may show: _"materials that will be skipped in the automated migration process as there is not a Material Upgrader defined for them."_

This is **expected** for package/UI shaders (ML-Agents defaults, etc.). Bob’s committed materials under `Assets/Materials/HDRP/` already use **HDRP/Lit**. Scene geometry gets fresh HDRP materials when the scene rebuilds.

**Fix in Editor:**

1. **Bob → HDRP → Apply Lab Lighting** — lowers exposure/bloom (fixes white blowout)
2. **Bob → HDRP → Upgrade Project Materials** — converts any legacy `Assets/` materials
3. **Bob → Polish → Rebuild Scene** — reapplies lab materials + lighting

Dismiss the Unity Render Pipeline Converter dialog if `Assets/Materials/HDRP/` is already populated; use the Bob menu steps above instead.

### Log anomalies (what to ignore vs fix)

See [docs/design/ai-warehouse-ops.md](design/ai-warehouse-ops.md) for the full table. Key training signal:

| Message                                        | Meaning                                                          |
| ---------------------------------------------- | ---------------------------------------------------------------- |
| `BOB_TRAINING_OK`                              | Trainer connected; time scale applied                            |
| `BOB_TRAINING_WARN` / scoreboard orange status | Inference fallback — no trainer on 5004                          |
| `Couldn't connect to trainer on port 5004`     | Same as above; stop Play, start `./scripts/train.sh`, Play again |

Ignore: Unity Licensing 404, `NoSubscription` AI generators, MCP WebSocket errors when bridge is off.

### Training arena layout

**Visual north star:** [`docs/design/visual-vision.md`](design/visual-vision.md)  
**Primary reference:** [`docs/design/ai-warehouse-lab-reference.png`](design/ai-warehouse-lab-reference.png)  
**Stretch reference:** [`docs/design/arc-academy-reference.jpg`](design/arc-academy-reference.jpg)

`BobTrainingSceneBuilder` creates the training scene under `TrainingArena`. **Lab mode** (target) simplifies the warehouse build; current builder still produces the full warehouse until Phase 2 lands.

| Element                                      | Purpose                                                                          |
| -------------------------------------------- | -------------------------------------------------------------------------------- |
| `BobTrainingStats` / `BobTrainingScoreboard` | In-scene training progress (iterations, basketball points, RL rewards/penalties) |
| `Hoop` / `MovableHoop` / `ScoreZone`         | **Single active scoring hoop**                                                   |
| `DecorativeHoopMarker` / physics layers      | Peripheral hoops and warehouse geo do not affect Bob                             |
| `TrajectoryVisuals`                          | Optional white arc lines (learning paths)                                        |
| `ArcAcademyManager`                          | `PrepareEpisode()` keeps layout stable unless `randomizeEpisodeLayout` enabled   |
| `Bob`                                        | 8 obs, 3 actions, purple emissive glow, gravity arcs                             |

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

### Training handshake

1. Run `./scripts/train.sh` and wait for **`Listening on port 5004`**
2. Open **BobTraining** in Unity; stop Play if already running
3. Press **Play** — trainer and Editor console should show training steps (not inference fallback)
4. Scoreboard + success graph update each episode

### Troubleshooting port 5004

If the trainer fails to bind port 5004 or Unity never connects, a **stale `bob-train` container** may still hold the port (common after Ctrl+C or a crashed run):

```bash
docker ps -a --filter name=bob-train
docker compose down
docker container prune -f   # removes stopped containers
./scripts/train.sh
```

Also check nothing else is listening: `lsof -i :5004` (macOS).

Existing run data (`results/bob-v0`) is handled automatically — `train.sh` adds `--resume` unless you pass `--force` or set `RUN_ID=bob-v1`.

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
