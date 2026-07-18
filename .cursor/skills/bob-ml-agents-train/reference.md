# Bob ML — reference

## Reward ledger (bob-v4.1 Tier 1.5)

| Signal             | When                      | ~Magnitude                   |
| ------------------ | ------------------------- | ---------------------------- |
| Made basket        | Rare                      | **+7.0** (+0.75 swish)       |
| Launch toward hoop | Once                      | +0.45 max                    |
| Arc quality        | Every step while tracking | **+0.02**/step max (was 0.1) |
| Miss proximity     | Terminal miss             | **≤ +0.35** (was 0.75 scale) |
| Rim-plane miss     | Rim cross without make    | **−0.2**                     |
| Distance penalty   | In-flight only            | −0.002 × xzDist/step         |
| OOB                | End                       | −0.5                         |

Impulse is **Bob-local**: `transform.rotation * localImpulse`.

## Observation gaps (Tier 3)

Missing: `vy`, speed magnitude, shot phase, normalized rim distance.

## ML-Agents patterns (external)

- [Training ML-Agents](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Training-ML-Agents.html) — BC/GAIL for sparse rewards
- [Environment design](https://github.com/Unity-Technologies/ml-agents/blob/release_2_verified_docs/docs/Learning-Environment-Design-Agents.md) — early EndEpisode on goal/miss

## Session artifacts

| Path                                           | Purpose                       |
| ---------------------------------------------- | ----------------------------- |
| `summaries/bob_session.csv`                    | Per-iteration HUD metrics     |
| `summaries/bob_shots.csv`                      | Per-shot actions / economics  |
| `results/<run-id>/Bob/`                        | Checkpoints + `.onnx`         |
| `docs/results/bob_v4.1_learning_dashboard.png` | Tier 1.5 diagnostic dashboard |
