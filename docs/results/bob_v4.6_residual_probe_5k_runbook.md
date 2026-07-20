# bob-v4.6-residual — 5k residual hybrid + BC probe

**Date:** 2026-07-20
**RUN_ID:** `bob-v4.6-residual`
**CONFIG:** `config/bob_free_throw_probe_5k_residual_bc.yaml`
**Init:** `--force` (fresh policy — do **not** init from absolute-impulse bob-v4.x)
**BC:** yes (`bobfreethrow.demo` re-recorded with residual c≈0, strength 0.5, steps 150k)
**Obs:** 11 (added vy, speed norm, shot phase)
**Launch angle / damping:** 58° / 1.18 (`BobSwishLaunchSolver`)

## Launch

```bash
# Stop Play; Unity compile idle; port 5004 free
RUN_ID=bob-v4.6-residual CONFIG=config/bob_free_throw_probe_5k_residual_bc.yaml \
  ./scripts/train.sh --force
# Play ONCE after "Listening on port 5004" → BOB_TRAINING_OK
```

## Results (2026-07-20 — complete)

| Metric           | Value             | Gate                              |
| ---------------- | ----------------- | --------------------------------- |
| Connected shots  | **4035**          | ~5k target (475k steps)           |
| Makes / success  | **84 / 2.08%**    | >2% early signal ✓ (≪ 5% interim) |
| Positive-miss    | **35.5%**         | ≤25% **FAIL**                     |
| rim_miss mean RL | **−0.843**        | < 0 **PASS**                      |
| make mean RL     | **+7.914**        | contrast **PASS**                 |
| Checkpoint       | `Bob-475017.onnx` | exported                          |

**vs bob-v4.4 (BC, absolute impulse):** success **1.24% → 2.08%** (+67% relative). Residual hybrid + fresh policy + re-recorded c≈0 demos lifted makes without breaking economics.

**Next:** tighten positive-miss (Tier 1.6 residual tuning or `ArcQualityRewardScale` cut) → extend train or BC strength bump.

Plot: `docs/results/bob_v4.6_residual_probe_5k_dashboard.png`

## Post-run plots

```bash
cd python && source .venv/bin/activate
python scripts/plot_learning_dashboard.py \
  --since <UTC_START> \
  --output ../docs/results/bob_v4.6_residual_probe_5k_dashboard.png \
  --check-pass
```

## Gap closure (this run)

- Residual hybrid prior (solver + clamped residual)
- Fresh policy for new action semantics
- BC demos re-recorded at c≈0
- Tier 3 obs slice: vy, speed, shot phase (11 total)
