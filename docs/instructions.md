# Bob — Agent Instructions (quick reference)

**Audience:** Cursor agents, Unity AI Assistant, contributors
**Full rules:** [AGENTS.md](../AGENTS.md) · **Live checklist:** [bob-done-tracker.md](bob-done-tracker.md) · **14-day plan:** [planning/next-14-days.md](planning/next-14-days.md)

---

## North Star (read before planning)

| Doc                                                                            | Purpose                                                         |
| ------------------------------------------------------------------------------ | --------------------------------------------------------------- |
| [what-finished-looks-like.md](what-finished-looks-like.md)                     | Product — agent, hoop, scoreboard, success graph                |
| [what-right-looks-like.md](what-right-looks-like.md)                           | Workflow — PRs, CI, milestones                                  |
| [design/visual-vision.md](design/visual-vision.md)                             | Arc Academy Lab look                                            |
| [design/ml-training-recommendations.md](design/ml-training-recommendations.md) | **ML learning fixes — read before reward/obs/training changes** |
| [design/ai-warehouse-ops.md](design/ai-warehouse-ops.md)                       | Training handshake + stability                                  |

Never commit directly to `main`. Use `feature/*` → PR → green CI.

---

## Current priority (2026-07-14)

**Full stack:** [planning/next-14-days.md](planning/next-14-days.md)

**Infrastructure + polish: done** (dual HUD, audio, PlayMode score test, PR #10).
**Learning demo: blocked** until ML Tier 1 ships in `BobAgent.cs`, then **`RUN_ID=bob-v4`**.

| P   | Next                             | Status             |
| --- | -------------------------------- | ------------------ |
| 1   | ML Tier 1 in `BobAgent.cs`       | Not started (code) |
| 2   | `build-standalone.sh` + Recorder | Not started        |
| 3   | `BobGameStateMachine` + juice    | Partial            |
| 4   | `release-checklist.sh` + CI      | Not started        |
| 5   | Cursor RAG/MCP/skills            | Partial            |

**No AWS hosting** — portfolio is in-repo (`docs/portfolio-site/` + README).

### Do next (ML Tier 1 — Priority 1)

1. **End episode when shot resolves** — rim pass, floor hit, or `MaxStep` ~60–90 (not OOB-only).
2. **Terminal miss proximity reward** — distance-to-rim on episode end without make.
3. **Gate per-step `-0.002 × xzDist`** — never unbounded post-bounce accumulation.

### Do not

- Resume **bob-v3** expecting visible learning (0% makes, high arc only).
- Edit C# or MCP-bake during an active `./scripts/train.sh` session (`Communicator has exited`).
- Optimize arc quality alone — success metric is **makes / episodes** (>5% rolling target).

Details: [ml-training-recommendations.md](design/ml-training-recommendations.md)

---

## Agent & training (non-negotiable)

- Behavior Name **`Bob`** — matches `config/bob_free_throw.yaml`
- **8 observations**, **3 continuous actions** (until Tier 3 obs changes — update validator + alignment tests)
- Exactly **one** `HoopScoreZone` / active scoring hoop
- Scene: `Assets/Scenes/BobTraining.unity`
- Hyperparameters in `config/` — not hardcoded in Python
- Train: `./scripts/train.sh` → wait **Listening on port 5004** → Unity **Play once**

```bash
# After ML Tier 1:
RUN_ID=bob-v4 ./scripts/train.sh --force
```

---

## RAG workflow

**Before code edits:** `rag_query` on **`bob-rag`** (or `python scripts/rag_query.py -q "..." --file path/to/file`)

**After significant changes:** `rag_index_paths` or `./scripts/rag-index.sh --paths file1 file2`

**Full rebuild:** `./scripts/rag-index.sh`

See [rag.md](rag.md)

---

## Agent skills (Cursor)

Invoke in chat or let Agent auto-apply when file context matches:

| Skill                  | Use for                                                    |
| ---------------------- | ---------------------------------------------------------- |
| `/bob-ml-agents-train` | Training, rewards, bob-v4, config YAML, session plots      |
| `/bob-unity-mcp`       | Assets/, scenes, unity-mcp, validate-scene, dual HUD bakes |

Location: [`.cursor/skills/`](../.cursor/skills/)

---

## Unity MCP workflow

Before editing `Assets/`, scenes, or prefabs with Editor open:

**Invoke skill** `/bob-unity-mcp` or follow [unity-mcp.md](unity-mcp.md).

1. Bridge **Running** — Edit → Project Settings → AI → Unity MCP
2. Inspect via **`unity-mcp`** (scene, hierarchy, Bob, hoop, console)
3. Do **not** run MCP scene bakes during active training

See [unity-mcp.md](unity-mcp.md)

---

## Key paths

| Path                                 | Purpose                           |
| ------------------------------------ | --------------------------------- |
| `Assets/Scripts/BobAgent.cs`         | Rewards, impulse, episode flow    |
| `Assets/Scripts/BobTrainingStats.cs` | Scoreboard single source of truth |
| `config/bob_free_throw.yaml`         | PPO hyperparameters               |
| `docs/bob-done-tracker.md`           | Live done / vNext checklist       |
| `summaries/bob_session.csv`          | Session metrics (gitignored)      |

---

## Completion standard

Ship complete slices — no `TODO` stubs in code. If blocked (Editor-only), end turn with **Further development required** and concrete next steps.
