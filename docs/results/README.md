# Bob Training Results

Artifacts from extended training runs (Phase 3 learning demo). Generated locally — not committed by default.

## Classmate takeaway

Visible learning, not a lucky checkpoint: **0.5% → 2% → 11% → 34.5%**. Canonical model is **bob-v4.8-tight-prior** (`Assets/Models/Bob.onnx`). The **70%/1k** bar is documented and **deferred**. Story: [`../portfolio-site/story.html`](../portfolio-site/story.html).

## Generate plots

After a Play session with `./scripts/train.sh` connected:

```bash
cd python && source .venv/bin/activate
python scripts/plot_training_progress.py \
  --output ../docs/results/training_progress.png

python scripts/plot_rewards.py --run-id bob-v4 --output ../docs/results/reward_curve.png

# Per-shot action review (impulse direction, end reason, training flag)
python scripts/review_training_run.py
python scripts/review_training_run.py --since 2026-07-17T22:40:00

# Multi-panel learning dashboard (success, toward-hoop, end reasons)
python scripts/plot_learning_dashboard.py --since 2026-07-17T22:59:00 \
  --output ../docs/results/bob_v4_learning.png

# Tier 1.5 contrast gate (exit 0 = pass)
python scripts/plot_learning_dashboard.py --since … --check-pass

# Dev TensorBoard (Environment/* StatsRecorder + PPO curves)
./scripts/tensorboard.sh
```

| Plot                          | File                                          |
| ----------------------------- | --------------------------------------------- |
| Session success + arc         | `training_progress.png`                       |
| Multi-run timeline            | `bob_ml_timeline_comparison.png`              |
| v4.8 learning dashboard       | `bob_v4_8-tight-prior_learning_dashboard.png` |

## BC training (Tier 2)

```bash
# After Bob → Demo → Enable Demonstration Recorder + Play session:
CONFIG=config/bob_free_throw_bc.yaml RUN_ID=bob-v4.2 ./scripts/train.sh --force
```

## Run IDs

| Run ID                   | Status | Notes                                                                                          |
| ------------------------ | ------ | ---------------------------------------------------------------------------------------------- |
| bob-v2                   | Done   | 865 iter batchmode (2026-06-24); plot baseline                                                 |
| bob-v3                   | Partial| ~40k steps; **0% makes**, high arc; crashed — **do not use for learning claims**               |
| **bob-v4**               | Done   | ~110k steps, **5/1001 makes (0.50%)**; shot-resolved episodes                                  |
| bob-v4.1                 | Done   | 500k; ~1%; positive-miss 57% FAIL                                                              |
| bob-v4.6-residual        | Done   | Solver + PPO residual; **2.08%**                                                               |
| bob-v4.7-curriculum      | Done   | Make-hunt BC + hoop curriculum; **10.93%** (interim >5% gate)                                  |
| bob-v4.7-ext             | Done   | Plateau ~11.5% trail-1k / 25% roll                                                             |
| **bob-v4.8-tight-prior** | **Peak** | Session **34.43%**, trail-1k **34.5%**; InferenceOnly Play **~31–35%**                      |

## Success criteria (two bars)

Canonical runbook: [training-run-plan.md](../design/training-run-plan.md).

- **Interim:** rolling success **>5%** — **met** (v4.7)
- **Portfolio peak:** v4.8 trail-1k **34.5%** — **this is the number we show**
- **70% / last 1k:** documented, **not claimed**, **not the next sprint**
- Tier 1.5 economics: rim_miss mean net RL ≪ make; v4.8 positive-miss **0.2% PASS**
- Plots: `bob_ml_timeline_comparison.png`, `bob_v4_8-tight-prior_learning_dashboard.png`

## Inputs

| File                                           | Source                                                                  |
| ---------------------------------------------- | ----------------------------------------------------------------------- |
| `summaries/bob_session.csv`                    | `BobTrainingSessionLog` — one row per ML-Agents iteration               |
| `summaries/bob_shots.csv`                      | `BobShotActionLog` — per-shot actions, impulse, toward-hoop, end reason |
| Console `BOB_SHOT:` lines                      | Same launch data mirrored for live review                               |
| `docs/results/bob_v4_learning.png`             | Multi-panel learning dashboard (bob-v4)                                 |
| `docs/results/bob_v4.1_learning_dashboard.png` | Target after Tier 1.5 — economics / positive-miss / make signature      |
| `results/<run-id>/Bob/TrainingRewards.csv`     | ML-Agents trainer                                                       |

Session CSVs are gitignored; copy finished PNGs here for portfolio references. Plan source: [Grok review](https://grok.com/share/bGVnYWN5LWNvcHk_d98db033-14f7-4c3f-85f3-94c76e15d323).
