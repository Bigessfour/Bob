# bob-v4.3 — 4k-episode probe results (complete)

**RUN_ID:** `bob-v4.3`
**CONFIG:** `config/bob_free_throw_probe_4k.yaml`
**Init:** `--initialize-from=bob-v4.2`
**BC:** no
**Window:** `2026-07-19T22:40:51` → `2026-07-19T22:56:26` UTC
**Steps:** 380002 (checkpoint `Bob-380002`)

## Results

| Metric           | Value                                           | Gate                |
| ---------------- | ----------------------------------------------- | ------------------- |
| Connected shots  | **4150**                                        | ~4k target          |
| Makes / success  | **49 / 1.18%**                                  | ≪ 5% FAIL           |
| Last-1k success  | **0.90%**                                       | ≪ 5% FAIL           |
| Positive-miss    | **1.8%**                                        | ≤25% PASS           |
| rim_miss mean RL | **−1.21**                                       | &lt; 0 PASS         |
| make mean RL     | **+7.42**                                       | contrast OK         |
| Outcome mix      | rim_miss **78%**, timeout **12%**, floor **8%** | aim OK, finish FAIL |

**Make signature:** fy mean **8.62** (band [6,12] holds 45/49); fz mean **−15.5**.
**Aim:** toward-hoop ~95%+ — policy finds the rim plane, not the cylinder.

## Verdict

Tier 1.6 economics still hold. More PPO alone did not lift makes (v4.2 → v4.3 ≈ flat ~1%). Next lever: **Tier 2 BC + soft power-band** → **bob-v4.4**.

## Plots

- `docs/results/bob_v4.3_probe_4k_dashboard.png`
- `docs/results/bob_v4.3_probe_4k_comparison.png`
