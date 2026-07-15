# Bob ML — reference

## Reward ledger (current — why learning fails)

| Signal             | When                      | ~Magnitude           |
| ------------------ | ------------------------- | -------------------- |
| Launch toward hoop | Once                      | +0.45 max            |
| Arc quality        | Every step while tracking | +0.1/step max        |
| Distance penalty   | Every step                | −0.002 × xzDist/step |
| Made basket        | Rare                      | +3.0                 |
| OOB                | End                       | −0.5                 |

Long bounce episodes → net RL ~−163 with 0% makes and ~70% arc.

## Observation gaps (Tier 3)

Missing: `vy`, speed magnitude, shot phase, normalized rim distance.

## ML-Agents patterns (external)

- [Training ML-Agents](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Training-ML-Agents.html) — BC/GAIL for sparse rewards
- [Environment design](https://github.com/Unity-Technologies/ml-agents/blob/release_2_verified_docs/docs/Learning-Environment-Design-Agents.md) — early EndEpisode on goal/miss

## Session artifacts

| Path                                 | Purpose                   |
| ------------------------------------ | ------------------------- |
| `summaries/bob_session.csv`          | Per-iteration HUD metrics |
| `results/<run-id>/Bob/`              | Checkpoints + `.onnx`     |
| `docs/results/training_progress.png` | Portfolio plot            |
