# Bob Project Rules for Cursor / AI Agents

You are **Bob's AI development partner**. Focus on building a fun, portfolio-quality Deep RL demo with production-ready structure and DevOps practices.

> **North Star (workflow):** [What Right Looks Like](docs/what-right-looks-like.md) — week milestones + PR/CI diagrams. Read before planning or merging.  
> **North Star (product):** [What Finished Looks Like](docs/what-finished-looks-like.md) — agent, hoop, scoreboard, success graph.  
> **North Star (visuals):** [Visual vision](docs/design/visual-vision.md) — Arc Academy Lab (AI Warehouse–inspired).  
> **Training ops:** [AI Warehouse ops](docs/design/ai-warehouse-ops.md) — PPO patterns, log anomalies, handshake.
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

| Layer               | Technology                                   |
| ------------------- | -------------------------------------------- |
| Game engine         | Unity 6 LTS                                  |
| RL framework        | Unity ML-Agents Toolkit                      |
| Agent / environment | C# (clean, well-commented)                   |
| Training            | Python 3.10 (`mlagents-learn`)               |
| Infrastructure      | Terraform (S3 + CloudFront on AWS Free Tier) |
| CI/CD               | GitHub Actions                               |
| Container           | Dockerfile for reproducible training deps    |

## Priorities

1. **Clean C#** — readable `Agent` subclasses, clear reward logic, match Behavior Name `Bob` to YAML config
2. **Reproducible training** — configs in `config/`, venv in `python/`, optional Docker image
3. **In-scene progress UI** — scoreboard + success graph (`BobTrainingStats`); TensorBoard for dev only
4. **Visual portfolio assets** — training GIFs, progress gallery, reward/success plots
5. **IaC-first DevOps** — Terraform bootstrap + dev stack; document every major step
6. **Local-first on Mac** — Apple Silicon compatible Python/torch setup

## Always

- Use clear commit messages and update documentation alongside code changes
- Keep secrets out of the repo (use `*.tfvars.example`, GitHub Secrets for CI)
- Prioritize MVP (working training loop) before polish or deployment
- **Align with North Star** — [what-finished-looks-like.md](docs/what-finished-looks-like.md) (product), [visual-vision.md](docs/design/visual-vision.md) (look), [what-right-looks-like.md](docs/what-right-looks-like.md) (workflow)
- Point to [PROJECT.md](PROJECT.md) for current status and [docs/project-plan.md](docs/project-plan.md) for milestones
- **Ship complete work** — see [Completion standard](#completion-standard) below
- **Query RAG before code** — see [Repository RAG](#repository-rag) below
- **Consult Unity MCP before Unity work** — see [Unity MCP](#unity-mcp) below

## Unity MCP

Bob uses **[MCP for Unity](https://github.com/CoplayDev/unity-mcp)** (CoplayDev) so agents can inspect and modify the live Unity Editor with validated tool parameters. The server is registered in [`.cursor/mcp.json`](.cursor/mcp.json) as **`unityMCP`** (HTTP — [official Cursor default](https://coplaydev.github.io/unity-mcp/getting-started/install)).

### Before any Unity development task

Before editing **`Assets/`**, **`ProjectSettings/`**, **`Packages/manifest.json`**, Unity Editor scripts, scenes, prefabs, or running Unity CLI that affects the project:

1. **Open Unity Editor** on this repo and confirm **Window → MCP for Unity** shows a connected bridge (green status).
2. Call MCP tools on server **`unityMCP`** to inspect current state — do **not** guess parameter shapes; read tool schemas from MCP descriptors:
   - **`manage_scene`** — `action: get_active`, `get_hierarchy` to verify scene context before scene/hierarchy changes
   - **`find_gameobjects`** — locate Bob, hoop, ball, and other targets before modifying GameObjects
   - **`manage_components`** — read/set Behavior Parameters, Rigidbody, colliders; Behavior Name must be **`Bob`** (matches `config/bob_free_throw.yaml`)
   - **`manage_gameobject`** — create/modify/delete GameObjects using documented `action`, `target`, and `component_properties`
   - **`read_console`** — check for errors after applying changes
3. Prefer MCP-driven Editor changes for scene/component work; use batchmode CLI (`./scripts/unity.sh -executeMethod ...`) for scripted rebuilds and validation.

Cursor **hooks** (`.cursor/hooks/unity-pre-code.sh`) inject Unity MCP reminders on Unity path edits; do not skip explicit **`unityMCP`** consultation when making non-trivial Unity changes.

### Setup (once per machine)

```bash
brew install uv          # if missing — provides uvx for the MCP server
chmod +x scripts/unity-mcp.sh
```

1. Open the Bob project in Unity 6 — the **`com.coplaydev.unity-mcp`** package resolves from [`Packages/manifest.json`](Packages/manifest.json).
2. **Window → MCP for Unity** → complete setup wizard (Python + uv) → **Auto-Setup** (HTTP transport — matches `.cursor/mcp.json` `unityMCP` url) → **Start Bridge** when needed.
3. Restart Cursor and enable **`unityMCP`** and **`bob-rag`** in MCP settings.

See [docs/unity-mcp.md](docs/unity-mcp.md) for tool reference, troubleshooting, and the official Unity AI Assistant alternative.

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

## Key Paths

| Path                 | Purpose                                                                                      |
| -------------------- | -------------------------------------------------------------------------------------------- |
| `Assets/`            | Unity scenes, scripts, prefabs                                                               |
| `config/`            | ML-Agents YAML trainer configs                                                               |
| `python/`            | venv, training scripts, visualization, **RAG** (`python/rag/`)                               |
| `python/.rag/`       | ChromaDB vector index (gitignored; rebuild via `./scripts/rag-index.sh`)                     |
| `terraform/`         | AWS infrastructure (bootstrap + dev)                                                         |
| `.github/workflows/` | CI pipelines                                                                                 |
| `docs/`              | Setup guides, project plan, portfolio write-ups, **North Star** (`what-right-looks-like.md`) |

## Related Files

- [docs/what-finished-looks-like.md](docs/what-finished-looks-like.md) — **Product north star** (agent, hoop, scoreboard, graph)
- [docs/design/visual-vision.md](docs/design/visual-vision.md) — **Visual north star** (Arc Academy Lab + workflow)
- [docs/what-right-looks-like.md](docs/what-right-looks-like.md) — **Workflow north star** (milestones + PR/CI)
- [PROJECT.md](PROJECT.md) — living status document
- [.cursor/rules/bob.mdc](.cursor/rules/bob.mdc) — always-on Cursor rules
- [.cursor/project-rules.md](.cursor/project-rules.md) — DevOps emphasis
- [docs/cursor-setup.md](docs/cursor-setup.md) — IDE configuration checklist

## Key Commands

```bash
# Train (from python/ with venv active)
mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0

# TensorBoard
tensorboard --logdir ../results

# Terraform bootstrap (one-time)
cd terraform/bootstrap && terraform init && terraform apply

# Docker training image
docker build -t bob-train . && docker run --rm bob-train

# RAG (query before code edits; re-index after significant changes)
./scripts/rag-setup.sh && ./scripts/rag-index.sh
cd python && python scripts/rag_query.py -q "BobAgent reward shaping"
./scripts/rag-index.sh --paths Assets/Scripts/BobAgent.cs
```
