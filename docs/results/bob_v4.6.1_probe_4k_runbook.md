# bob-v4.6.1 — 4k anti-farming probe (40× sim)

**RUN_ID:** `bob-v4.6.1`  
**CONFIG:** `config/bob_free_throw_probe_4k_v461.yaml`  
**Init:** `--initialize-from=bob-v4.6-residual`  
**Sim speed:** `time_scale: 40` (2× bob-v4.6-residual @ 20×)

## C# reward changes (aggressive vs v4.6-residual)

| Constant                      | v4.6-residual | v4.6.1    |
| ----------------------------- | ------------- | --------- |
| `MadeBasket`                  | 7             | **8**     |
| `ArcQualityRewardScale`       | 0.01          | **0**     |
| `MissProximityRewardScale`    | 0.20          | **0**     |
| `RimPlaneMissPenalty`         | 1.25          | **2.5**   |
| `LaunchTowardHoopRewardScale` | 0.20          | **0.08**  |
| `IdealSolverMatchRewardScale` | 0.45          | **0.20**  |
| `PerStepDistancePenaltyScale` | 0.002         | **0.004** |

**Target:** positive-miss **≤25%** while holding success **≥2%**.

## Launch

```bash
RUN_ID=bob-v4.6.1 CONFIG=config/bob_free_throw_probe_4k_v461.yaml \
  ./scripts/train.sh --initialize-from=bob-v4.6-residual
```
