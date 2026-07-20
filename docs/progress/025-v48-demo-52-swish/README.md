# Portfolio demo — 52 consecutive swish (2026-07-20)

**Captured:** Play paused in watch mode @ iter **53**, score **52**, rolling **100%**.

| Field    | Value                                  |
| -------- | -------------------------------------- |
| Mode     | **HeuristicOnly** (solver prior c≈0)   |
| Makes    | **52/52** swish (100%)                 |
| Duration | ~76 s                                  |
| Impulse  | (0, **4.72**, **-3.18**) @ 56°         |
| HUD      | Episodes 53, Success 98%, Rolling 100% |

## What this demonstrates (portfolio)

- **Product:** Arc Academy lab, dual HUD, swish feedback, scoreboard climbing
- **Solver prior:** v4.7 retuned analytic launch — audience sees perfect free throws
- **ML story (separate):** PPO bob-v4.8-tight-prior **34.5%** trailing-1k — see [ml-project-timeline.md](../design/ml-project-timeline.md)

## Assets

- Screenshot: `docs/progress/025-v48-demo-52-swish/capture.png`
- Session meta: `docs/progress/025-v48-demo-52-swish/meta.json`
- ML proof: `docs/results/bob_v4_8-tight-prior_learning_dashboard.png`

## To demo trained PPO (not solver)

1. **Bob → Demo → Enable Inference Only** (Behavior **InferenceOnly**)
2. Load `Assets/Models/Bob.onnx` from `results/bob-v4.8-tight-prior/`
3. Play without `./scripts/train.sh` — expect ~30–35% makes, non-zero actions
