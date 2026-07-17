# Bob Project Rules for Cursor / AI Agents

You are **Bob's AI development partner**. Focus on building a fun, portfolio-quality Deep RL demo with production-ready structure and DevOps practices.

> **North Star (workflow):** [What Right Looks Like](docs/what-right-looks-like.md) — week milestones + PR/CI diagrams. Read before planning or merging.
> **North Star (product):** [What Finished Looks Like](docs/what-finished-looks-like.md) — agent, hoop, scoreboard, success graph.
> **North Star (visuals):** [Visual vision](docs/design/visual-vision.md) — Arc Academy Lab (AI Warehouse–inspired).
> **Training ops:** [AI Warehouse ops](docs/design/ai-warehouse-ops.md) — PPO patterns, log anomalies, handshake.
> **ML learning fixes:** [ML training recommendations](docs/design/ml-training-recommendations.md) — **read before reward/obs/training changes**; bob-v4 plan.
> **Live checklist:** [bob-done-tracker.md](docs/bob-done-tracker.md) — pin in Cursor; update when ML tiers or training runs complete.
> **Quick reference:** [docs/instructions.md](docs/instructions.md) — agent instruction sheet (this file + North Stars).
> **Never propose direct commits to `main`** — use `feature/*` branches and PRs with green CI.

## What finished looks like (product)

When Bob is **done** (MVP + demo-ready):

1. **Orange cube agent (`Bob`)** at the free-throw line shoots toward **one active hoop** each iteration (ML-Agents PPO, Behavior Name `Bob`).
2. **Made basket → +1 score** on the in-scene **scoreboard** (basketball points, separate from RL reward).
3. **Scoreboard tracks:** iterations, score, cumulative RL **rewards**, cumulative **penalties**, net RL, **success rate %**.
4. **Success-rate graph** shows learning progress over recent iterations (`BobTrainingSuccessGraph`).
5. **Decorative geometry** does not collide with Bob (`BobPhysicsLayers`).
6. **Training:** `./scripts/train.sh` + Play; audience sees scene UI — **not** TensorBoard as the primary display.

**Projectile note:** Week 1 uses the **cube as the projectile** (impulse on `Bob`). Phase 1.5 adds a separate **basketball** rigidbody — see [what-finished-looks-like.md](docs/what-finished-looks-like.md).

## Role

Help design, implement, and document Bob — a cheerful orange cube that learns free throws via Unity ML-Agents PPO training in a **readable AI Warehouse–style lab** ([visual-vision.md](docs/design/visual-vision.md)).

## Tech Stack

| Layer               | Technology                                             |
| ------------------- | ------------------------------------------------------ |
| Game engine         | Unity 6 LTS                                            |
| RL framework        | Unity ML-Agents Toolkit                                |
| Agent / environment | C# (clean, well-commented)                             |
| Training            | Python 3.10 (`mlagents-learn`)                         |
| CI/CD               | GitHub Actions                                         |
| Container           | Dockerfile for reproducible training deps              |
| IaC (CI only)       | `terraform/` — fmt/validate in CI; **not hosting Bob** |

## Priorities

1. **Visible learning (Phase 3 gate)** — implement [ml-training-recommendations.md](docs/design/ml-training-recommendations.md) Tier 1, then **bob-v4** train until rolling success **>5%**
2. **Clean C#** — readable `Agent` subclasses, clear reward logic, match Behavior Name `Bob` to YAML config
3. **Reproducible training** — configs in `config/`, venv in `python/`, Docker `bob-train`; **no script edits during Play training**
4. **In-scene progress UI** — scoreboard + success graph (`BobTrainingStats`); TensorBoard for dev only
5. **Visual portfolio assets** — training GIFs, progress gallery, reward/success plots; README links to `docs/portfolio-site/`
6. **DevOps** — green CI, Docker training, PR workflow — **no AWS hosting for Bob**

## ML training rules (mandatory)

Before changing `BobAgent.cs`, reward constants, observations, or `config/bob_free_throw.yaml`:

1. **Invoke skill** `/bob-ml-agents-train` or read [ml-training-recommendations.md](docs/design/ml-training-recommendations.md)
2. **Query RAG** with task-specific terms (`bob-v4`, `miss proximity`, `EndEpisode`, `behavioral_cloning`)
3. **Do not** claim learning from bob-v2/v3 — high arc ≠ makes; bob-v3 had **0% success**
4. **Episode design** — single-shot free throws must `EndEpisode()` on make, miss resolution, or short timeout
5. **Training stability** — `./scripts/train.sh` + Play once; no C# saves or Unity MCP bakes until training stops ([ai-warehouse-ops.md](docs/design/ai-warehouse-ops.md))
6. **Success metric** — track **BasketballPoints / TotalIterations** and rolling success % — not arc quality alone
7. **Obs count changes** — update `BobSceneValidator`, `test_unity_alignment.py`, and YAML together

After ML code or doc changes: **`rag_index_paths`** on touched files + update **bob-done-tracker** if gates move.

## 14-day priority stack (2026-07-14)

Canonical plan: **[docs/planning/next-14-days.md](docs/planning/next-14-days.md)** — do not lose scope across agent handoffs.

| P   | Area                                             | Status                              |
| --- | ------------------------------------------------ | ----------------------------------- |
| 1   | ML Tier 1 → bob-v4                               | **Not started (code)** — #1 blocker |
| 2   | Standalone build + Recorder + hero video scripts | Not started                         |
| 3   | State machine + Cuphead juice                    | Partial (audio/HUD only)            |
| 4   | `release-checklist.sh` + CI extension            | Not started                         |
| 5   | Cursor workflow (RAG, MCP, skills)               | Partial                             |

Work top-down unless the user explicitly reprioritizes. **Do not** plan AWS/CloudFront deploy.

## Missing agent capabilities

Agents **cannot** reliably do the following — do not pretend otherwise; use workarounds in [next-14-days.md](docs/planning/next-14-days.md#missing-agent-capabilities-workarounds):

1. **Unity Play mode / live Recorder capture** — user runs Play; use `unity-mcp` when bridge is up; batchmode `./scripts/validate-scene.sh` when Editor is closed.
2. **Headless Unity builds in CI** — host Mac build via future `build-standalone.sh`; no Linux Unity runner in CI today.
3. **Live training session monitor** — use post-hoc CSV, plots, and in-scene HUD; not streaming TensorBoard in agent context.
4. **Auto-merge PRs** — create/push PRs; merge only after green CI and user intent.

Future optional: agent-callable validate hook in `scripts/` after code edits.

## Always

- Use clear commit messages and update documentation alongside code changes
- Keep secrets out of the repo (use `*.tfvars.example`, GitHub Secrets for CI)
- Prioritize MVP (working training loop) before polish or deployment
- **Align with North Star** — [what-finished-looks-like.md](docs/what-finished-looks-like.md) (product), [visual-vision.md](docs/design/visual-vision.md) (look), [what-right-looks-like.md](docs/what-right-looks-like.md) (workflow), [ml-training-recommendations.md](docs/design/ml-training-recommendations.md) (learning)
- Point to [PROJECT.md](PROJECT.md) for current status, [docs/bob-done-tracker.md](docs/bob-done-tracker.md) for live gates, and [docs/project-plan.md](docs/project-plan.md) for milestones
- **Ship complete work** — see [Completion standard](#completion-standard) below
- **Query RAG before code** — see [Repository RAG](#repository-rag) below
- **Consult Unity MCP before Unity work** — see [Unity MCP](#unity-mcp) below

## Unity MCP

Bob uses **[Unity MCP](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-overview.html)** (official bridge in `com.unity.ai.assistant`) so agents can inspect and modify the live Unity Editor with validated tool parameters. The server is registered in [`.cursor/mcp.json`](.cursor/mcp.json) as **`unity-mcp`** (stdio → `~/.unity/relay/` relay → Unity Editor bridge).

### Before any Unity development task

**Invoke skill** `/bob-unity-mcp` or follow [docs/unity-mcp.md](docs/unity-mcp.md).

Before editing **`Assets/`**, **`ProjectSettings/`**, **`Packages/manifest.json`**, Unity Editor scripts, scenes, prefabs, or running Unity CLI that affects the project:

1. **Open Unity Editor** on this repo and confirm **Edit → Project Settings → AI → Unity MCP** shows bridge **Running**; approve Cursor under **Connected Clients** / **Pending Connections**.
2. Call MCP tools on server **`unity-mcp`** to inspect current state — do **not** guess parameter shapes; read tool schemas from MCP descriptors:
   - **`manage_scene`** — `action: get_active`, `get_hierarchy` to verify scene context before scene/hierarchy changes
   - **`find_gameobjects`** — locate Bob, hoop, ball, and other targets before modifying GameObjects
   - **`manage_components`** — read/set Behavior Parameters, Rigidbody, colliders; Behavior Name must be **`Bob`** (matches `config/bob_free_throw.yaml`)
   - **`manage_gameobject`** — create/modify/delete GameObjects using documented `action`, `target`, and `component_properties`
   - **`read_console`** — check for errors after applying changes
   - **`bob_setup_simple_arena`**, **`bob_open_training_scene`** — Bob custom tools (see [`BobUnityMcpTools.cs`](Assets/Scripts/Editor/BobUnityMcpTools.cs))
3. Prefer MCP-driven Editor changes for scene/component work; use batchmode CLI (`./scripts/unity.sh -executeMethod ...`) for scripted rebuilds and validation.

Cursor **hooks** (`.cursor/hooks/unity-pre-code.sh`) inject Unity MCP reminders on Unity path edits; do not skip explicit **`unity-mcp`** consultation when making non-trivial Unity changes.

### Setup (once per machine)

```bash
chmod +x scripts/unity-mcp.sh
```

1. Open the Bob project in Unity 6 — **`com.unity.ai.assistant`** resolves from [`Packages/manifest.json`](Packages/manifest.json).
2. **Edit → Project Settings → AI → Unity MCP** → bridge **Running** → enable needed tool groups → **Accept** Cursor when prompted.
3. Restart Cursor and enable **`unity-mcp`** and **`bob-rag`** in MCP settings.

See [docs/unity-mcp.md](docs/unity-mcp.md) for tool reference and troubleshooting.

### When Unity MCP is unavailable

If the Editor is closed or the bridge is disconnected, implement only what batchmode CLI can complete and end the turn with **Further development required** (see [Completion standard](#completion-standard)).

## Repository RAG

Bob maintains a **local vector index** (ChromaDB) of repo code, docs, and config. Agents must use it to avoid contradicting existing patterns and to stay aligned with project conventions.

### Before every code action

Before **Write**, **StrReplace**, **EditNotebook**, or creating new source files:

1. Call MCP tool **`rag_query`** on server **`bob-rag`** with a task-specific query (file path, feature, constraints).
2. Read retrieved chunks and match naming, patterns, and config already in the repo.
3. If MCP is unavailable, run:
   ```bash
   cd python && source .venv/bin/activate
   python scripts/rag_query.py -q "your task" --file path/to/target.cs
   ```

Cursor **hooks** (`.cursor/hooks.json`) also inject RAG context on code-editing tools; do not skip explicit `rag_query` when making non-trivial changes.

### After significant development each turn

When you add or materially change **methods, Editor CLI entry points, agent logic, scripts, or architecture docs**:

1. Call MCP **`rag_index_paths`** with the touched repo-relative paths, **or**
2. Run `./scripts/rag-index.sh --paths file1 file2`

The **stop hook** re-indexes changed text files automatically, but agents must still call `rag_index_paths` when methods/workflows change significantly so the index reflects intent before the next turn.

### RAG setup (once per machine)

```bash
./scripts/setup-python.sh
./scripts/rag-setup.sh
./scripts/rag-index.sh
```

Enable MCP: project [`.cursor/mcp.json`](.cursor/mcp.json) registers **`bob-rag`**. Restart Cursor after setup.

See [docs/rag.md](docs/rag.md) for architecture and troubleshooting.

## Agent skills (Cursor)

Project skills in [`.cursor/skills/`](.cursor/skills/) — invoke with `/skill-name` or let Agent auto-apply when relevant:

| Skill                     | When                                                                            |
| ------------------------- | ------------------------------------------------------------------------------- |
| **`bob-ml-agents-train`** | PPO training, rewards, bob-v4, `BobAgent.cs`, `config/*.yaml`, learning metrics |
| **`bob-unity-mcp`**       | Scenes, prefabs, Behavior Parameters, unity-mcp tools, validate-scene           |

See [docs/instructions.md](docs/instructions.md) for quick reference.

## Completion standard

During development, **do not leave tech debt, stubs, placeholders, or TODO comments** that will cause a gotcha later. Every method, script, and workflow you touch should be **fully implemented and working** before you consider the task done.

| Do not leave behind                                                   | Instead                                                  |
| --------------------------------------------------------------------- | -------------------------------------------------------- |
| `// TODO`, `// FIXME`, `NotImplementedException`, empty method bodies | Implement the behavior or do not add the API surface yet |
| Commented-out “temporary” code                                        | Delete it or finish and enable it                        |
| Hardcoded stubs that fail at runtime                                  | Wire real logic or remove the call path                  |
| “Week 2” hooks with no working path today                             | Either implement Phase 1 fully or omit until scoped      |

**Scope discipline:** If the user asks for feature X, deliver X end-to-end—not a partial skeleton “for later.” Prefer a smaller, finished slice over a larger unfinished one.

**When completion is unavoidable blocked** (missing credentials, Unity Editor-only step, external dependency not in repo, user decision required):

1. Implement everything that _can_ be completed in the current turn.
2. End your response with an explicit **Further development required** block that states:
   - What is incomplete and why
   - Exact file(s) / method(s) still needed
   - Concrete next steps for the user or a follow-up agent turn

Example end-of-turn prompt:

```text
## Further development required

- Play-mode capture requires the Unity Editor closed; batchmode cannot open a project already loaded in another instance.
- Next: close Unity, run `./scripts/capture-progress.sh --play arc-academy-playmode-hero`, verify `docs/progress/.../meta.json` has `"mode": "play"`.
```

Do not silently defer work in code comments—surface it in the turn summary so it is tracked and not forgotten.

## Avoid

- Web frameworks (Next.js, React, etc.) — this is a Unity project
- Hardcoding hyperparameters in Python when they belong in `config/*.yaml`
- Committing `results/`, `summaries/`, `.venv/`, Unity `Library/`, or `.tfstate` files
- **Extended PPO on current reward design** without Tier 1 fixes (bob-v3 proved 0% makes)
- **Training + live code edits** in the same session (causes `Communicator has exited`)

## Key Paths

| Path                 | Purpose                                                                                                                          |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `Assets/`            | Unity scenes, scripts, prefabs                                                                                                   |
| `config/`            | ML-Agents YAML trainer configs                                                                                                   |
| `python/`            | venv, training scripts, visualization, **RAG** (`python/rag/`)                                                                   |
| `python/.rag/`       | ChromaDB vector index (gitignored; rebuild via `./scripts/rag-index.sh`)                                                         |
| `terraform/`         | IaC scaffold — **CI fmt/validate only**; not used to host Bob                                                                    |
| `.github/workflows/` | CI pipelines                                                                                                                     |
| `docs/`              | Setup guides, project plan, portfolio write-ups, **North Star**, **ML recommendations**, [instructions.md](docs/instructions.md) |

## Related Files

- [docs/what-finished-looks-like.md](docs/what-finished-looks-like.md) — **Product north star** (agent, hoop, scoreboard, graph)
- [docs/design/ml-training-recommendations.md](docs/design/ml-training-recommendations.md) — **ML learning fixes + bob-v4 plan**
- [docs/bob-done-tracker.md](docs/bob-done-tracker.md) — **Live done / vNext checklist**
- [docs/planning/next-14-days.md](docs/planning/next-14-days.md) — **14-day priority stack + missing capabilities**
- [docs/instructions.md](docs/instructions.md) — **Agent quick reference**
- [docs/ai-review.md](docs/ai-review.md) — **Ollama PR review** (advisory, cached models)
- [docs/design/ai-warehouse-ops.md](docs/design/ai-warehouse-ops.md) — training handshake + stability
- [docs/design/visual-vision.md](docs/design/visual-vision.md) — **Visual north star** (Arc Academy Lab + workflow)
- [docs/what-right-looks-like.md](docs/what-right-looks-like.md) — **Workflow north star** (milestones + PR/CI)
- [PROJECT.md](PROJECT.md) — living status document
- [.cursor/rules/bob.mdc](.cursor/rules/bob.mdc) — always-on Cursor rules
- [.cursor/project-rules.md](.cursor/project-rules.md) — DevOps emphasis
- [docs/cursor-setup.md](docs/cursor-setup.md) — IDE configuration checklist

## Key Commands

```bash
# Train (recommended — Docker on Apple Silicon)
./scripts/train.sh
# After ML Tier 1: RUN_ID=bob-v4 ./scripts/train.sh --force

# Plot session progress
cd python && source .venv/bin/activate
python scripts/plot_training_progress.py --output ../docs/results/training_progress.png

# TensorBoard (dev only — not audience UI)
tensorboard --logdir ../results

# Docker training image
docker build -t bob-train . && docker run --rm bob-train

# RAG (query before code edits; re-index after significant changes)
./scripts/rag-setup.sh && ./scripts/rag-index.sh
cd python && python scripts/rag_query.py -q "BobAgent reward shaping"
./scripts/rag-index.sh --paths Assets/Scripts/BobAgent.cs
```
