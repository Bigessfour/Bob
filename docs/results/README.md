# Bob Training Results

Artifacts from extended training runs (Phase 3 learning demo). Generated locally — not committed by default.

## Generate plots

After a Play session with `./scripts/train.sh` connected:

```bash
# In-scene success + arc quality (from summaries/bob_session.csv)
cd python && source .venv/bin/activate
python scripts/plot_training_progress.py \
  --output ../docs/results/training_progress.png

# ML-Agents reward curve only
python scripts/plot_rewards.py --run-id bob-v0 --output ../docs/results/reward_curve.png
```

## Inputs

| File | Source |
|------|--------|
| `summaries/bob_session.csv` | `BobTrainingSessionLog` — one row per ML-Agents iteration |
| `results/<run-id>/Bob/TrainingRewards.csv` | ML-Agents trainer |

Both paths are gitignored. Copy finished PNGs here for portfolio references.
