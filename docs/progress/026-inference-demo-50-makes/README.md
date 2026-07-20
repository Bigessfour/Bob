# Inference demo session (2026-07-20)

## Validated inference run (confirmed)

After **Bob → Demo → Enable Inference Only** and console:

`BOB_INFERENCE_OK: BehaviorType=InferenceOnly model=Assets/Models/Bob.onnx`

| Metric                      | Value                                   |
| --------------------------- | --------------------------------------- |
| User observed               | **~35%** makes                          |
| CSV block (18:08–18:10 UTC) | **87 shots, 27 makes (31.0%)**          |
| Training reference          | bob-v4.8-tight-prior trail-1k **34.5%** |

**Verdict:** PPO policy in Play matches training — not the solver streak.

## Demo types (portfolio)

| Demo type                     | Makes        | Use                           |
| ----------------------------- | ------------ | ----------------------------- |
| Solver / Heuristic c≈0        | 50/50 (100%) | Product wow — lab + HUD       |
| **InferenceOnly (v4.8 ONNX)** | **~31–35%**  | **ML proof** — learned policy |

## Earlier misread (same day)

## User report

Enable Inference Only → Play → **50 makes**.

## CSV verdict (initial)

| Session                        | Makes | Success  | Actions      | Mode                                     |
| ------------------------------ | ----- | -------- | ------------ | ---------------------------------------- |
| **50-make streak** (18:04:44+) | 50/50 | **100%** | all **c=0**  | **Solver prior** (HeuristicOnly path)    |
| Prior burst (18:04:00–44)      | 12/23 | **52%**  | all non-zero | Likely **real inference** before restart |

Unity reported `behavior=HeuristicOnly` when checked — the menu may not persist across Play, or Play restarted in heuristic/demo mode.

## Portfolio use

- **50 swish streak:** product demo (solver + HUD) — `025-v48-demo-52-swish/`
- **ML proof:** bob-v4.8 PPO **34.5%** trailing-1k — dashboard PNG
- **Real inference demo:** validated above (~31% CSV / ~35% user eyeball)

## Fix checklist

1. Stop Play
2. Bob → Demo → **Enable Inference Only**
3. Console: `BOB_INFERENCE_OK: BehaviorType=InferenceOnly model=Assets/Models/Bob.onnx`
4. Play once — HUD should **not** show only identical c=0 every shot
