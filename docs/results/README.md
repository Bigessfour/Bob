# Bob Training Results

Artifacts from extended training runs (Phase 3 learning demo). Generated locally — not committed by default.

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
```

## Run IDs

| Run ID     | Status      | Notes                                                                                                    |
| ---------- | ----------- | -------------------------------------------------------------------------------------------------------- |
| bob-v2     | Done        | 865 iter batchmode (2026-06-24); plot baseline                                                           |
| bob-v3     | Partial     | ~40k steps; **0% makes**, high arc; crashed on communicator timeout — **do not use for learning claims** |
| **bob-v4** | In progress | Tier 1 code landed; short connected runs (~5k steps) then `Communicator has exited` — resume carefully   |

## Success criteria (bob-v4)

From [ml-training-recommendations.md](../design/ml-training-recommendations.md):

- Rolling session success **>5%** over 30+ min uninterrupted Play
- Net RL trend **up** when makes occur (not just arc quality)
- Plot copied here as `training_progress.png`

## Inputs

| File                                       | Source                                                                  |
| ------------------------------------------ | ----------------------------------------------------------------------- |
| `summaries/bob_session.csv`                | `BobTrainingSessionLog` — one row per ML-Agents iteration               |
| `summaries/bob_shots.csv`                  | `BobShotActionLog` — per-shot actions, impulse, toward-hoop, end reason |
| Console `BOB_SHOT:` lines                  | Same launch data mirrored for live review                               |
| `results/<run-id>/Bob/TrainingRewards.csv` | ML-Agents trainer                                                       |

Both paths are gitignored. Copy finished PNGs here for portfolio references.
