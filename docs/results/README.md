# Bob Training Results

Artifacts from extended training runs (Phase 3 learning demo). Generated locally — not committed by default.

## Generate plots

After a Play session with `./scripts/train.sh` connected:

```bash
cd python && source .venv/bin/activate
python scripts/plot_training_progress.py \
  --output ../docs/results/training_progress.png

python scripts/plot_rewards.py --run-id bob-v4 --output ../docs/results/reward_curve.png
```

## Run IDs

| Run ID     | Status   | Notes                                                                                                    |
| ---------- | -------- | -------------------------------------------------------------------------------------------------------- |
| bob-v2     | Done     | 865 iter batchmode (2026-06-24); plot baseline                                                           |
| bob-v3     | Partial  | ~40k steps; **0% makes**, high arc; crashed on communicator timeout — **do not use for learning claims** |
| **bob-v4** | **Next** | After [ml-training-recommendations.md](../design/ml-training-recommendations.md) Tier 1 ships            |

## Success criteria (bob-v4)

From [ml-training-recommendations.md](../design/ml-training-recommendations.md):

- Rolling session success **>5%** over 30+ min uninterrupted Play
- Net RL trend **up** when makes occur (not just arc quality)
- Plot copied here as `training_progress.png`

## Inputs

| File                                       | Source                                                    |
| ------------------------------------------ | --------------------------------------------------------- |
| `summaries/bob_session.csv`                | `BobTrainingSessionLog` — one row per ML-Agents iteration |
| `results/<run-id>/Bob/TrainingRewards.csv` | ML-Agents trainer                                         |

Both paths are gitignored. Copy finished PNGs here for portfolio references.
