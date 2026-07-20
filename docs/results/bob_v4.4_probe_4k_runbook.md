# bob-v4.4 — 4k BC + power-band probe results (complete)

**Date:** 2026-07-19  
**RUN_ID:** `bob-v4.4`  
**CONFIG:** `config/bob_free_throw_probe_4k_bc.yaml`  
**Init:** `--initialize-from=bob-v4.3`  
**BC:** yes (`bobfreethrow.demo`, strength 0.5, steps 120k)  
**Power-band:** `IdealLaunchFy=8.5` / scale `0.04`  
**Window:** `2026-07-19T23:01:45` → `2026-07-19T23:17:32` UTC  
**Steps:** 380055 (checkpoint `Bob-380055`)

## Results

| Metric           | Value                                           | Gate             |
| ---------------- | ----------------------------------------------- | ---------------- |
| Connected shots  | **4290**                                        | ~4k target       |
| Makes / success  | **53 / 1.24%**                                  | ≪ 5% FAIL        |
| Last-1k success  | **1.10%**                                       | ≪ 5% FAIL        |
| Positive-miss    | **3.0%**                                        | ≤25% PASS        |
| rim_miss mean RL | **−1.28**                                       | &lt; 0 PASS      |
| make mean RL     | **+7.35**                                       | contrast OK      |
| Outcome mix      | rim_miss **76%**, floor **15%**, timeout **8%** | still rim-parked |

## vs bob-v4.3

|               | v4.3  | v4.4 (BC)        |
| ------------- | ----- | ---------------- |
| Success       | 1.18% | **1.24%** (flat) |
| Last-1k       | 0.90% | **1.10%**        |
| Positive-miss | 1.8%  | 3.0%             |

BC + power-band did **not** lift makes past the ~1% plateau. Economics still hold.

## Verdict

Next lever is **demo quality** (verify recorded shots are actual makes near the make island), not more PPO on the same prior. Re-record focused make demos → new BC run, or Tier 3 obs (`vy` / speed) if demos check out.

## Plots

- `docs/results/bob_v4.4_probe_4k_dashboard.png`
- `docs/results/bob_v4.4_probe_4k_comparison.png`
