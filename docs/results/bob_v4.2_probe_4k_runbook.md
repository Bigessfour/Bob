# bob-v4.2 — ~4000-episode probe runbook

**Date:** 2026-07-19  
**Goal:** Extend Tier 1.6 PPO (~4000 more shots), measure make-rate / economics — **no reward rewrite**, **no BC**.

## Why this run

| Prior session                      | Result                                                                   |
| ---------------------------------- | ------------------------------------------------------------------------ |
| bob-v4.1 @ 500k                    | Aim↑, makes ~1%, **positive-miss 57% FAIL** (near-miss farming)          |
| bob-v4.2 first block (~4002 shots) | **positive-miss 2.9% PASS**; rim_miss mean RL **−1.42**; makes **0.57%** |
| Checkpoint                         | `results/bob-v4.2/Bob/checkpoint.pt` @ **372371** steps                  |

Tier 1.6 economics worked. Next lever is **more PPO steps**, not another reward patch. Demos under `Assets/Demos/` exist but are **unverified for make quality** — skip BC for this probe.

## Budget math

| Quantity             | Value                                     |
| -------------------- | ----------------------------------------- |
| Mean steps / episode | **≈93** (372371 steps ÷ 4002 shots)       |
| Target new episodes  | **~4000**                                 |
| Additional steps     | 4000 × 93 ≈ **372k**                      |
| Resume from          | 372371                                    |
| YAML `max_steps`     | **800000** (headroom; stop early via HUD) |

Trainer will keep going until `max_steps` unless you Stop Play first. Prefer: watch HUD **Iterations ≈ 4000**, then Stop Play → Ctrl+C trainer.

## Mode checklist (before train)

- [x] Behavior Type **Default** (`m_BehaviorType: 0`) — not HeuristicOnly
- [x] `DemonstrationRecorder.Record = false`
- [ ] Port **5004** free; Unity compile idle; Play **stopped**

If unsure: **Bob → Demo → Disable Demonstration Recorder**, then save scene.

## Start commands

```bash
# 1. Preflight
lsof -i :5004
# optional: .cursor/skills/bob-ml-agents-train/scripts/check-training-handshake.sh

# 2. Note UTC start (for plots)
date -u +%Y-%m-%dT%H:%M:%S

# 3. Trainer (Docker / Apple Silicon default)
RUN_ID=bob-v4.2 CONFIG=config/bob_free_throw.yaml ./scripts/train.sh --resume

# 4. After "Listening on port 5004" → Unity Play ONCE on BobTraining.unity
#    Console must show BOB_TRAINING_OK
# 5. Do not edit C# / MCP-bake until Stop Play
# 6. Stop Play at ~4000 HUD episodes (or when trainer hits max_steps), then Ctrl+C
```

**Do not** use `config/bob_free_throw_bc.yaml` for this probe.  
**Do not** `--force` (wipes the 372k checkpoint).

## Capture improvement

**During:** HUD session success %, rolling success %, last end reason, toward-hoop.

**After** (set `SINCE` to the UTC from step 2):

```bash
cd python && source .venv/bin/activate

python scripts/plot_learning_dashboard.py \
  --since "$SINCE" \
  --output ../docs/results/bob_v4.2_probe_4k_dashboard.png \
  --check-pass

python scripts/plot_run_comparison.py \
  --window bob-v4.2-prior:2026-07-19T00:31:13:2026-07-19T00:46:30 \
  --window bob-v4.2-probe:"$SINCE" \
  --output ../docs/results/bob_v4.2_probe_4k_comparison.png

python scripts/plot_training_progress.py \
  --output ../docs/results/training_progress.png
```

### Pass / decide

| Check                          | Pass bar                       | Then                                      |
| ------------------------------ | ------------------------------ | ----------------------------------------- |
| Positive-miss                  | ≤25% (already ~3% — must hold) | Keep Tier 1.6                             |
| Rim_miss mean net RL           | &lt; 0                         | Keep                                      |
| Rolling / last-1k success      | **>5%**                        | Extend same RUN_ID toward 70%/1k          |
| Still ≪5% after 4k + good econ | —                              | Record **quality** BC demos → bob-v4.3 BC |

## Baseline to beat (bob-v4.2 first block)

- Makes: **23 / 4002 (0.57%)**
- Positive-miss: **2.9%**
- rim_miss mean net RL: **≈ −1.42**; make mean **≈ +7.39**
