# bob-v4.8-tight-prior — 4k run after plateau diagnosis

**RUN_ID:** `bob-v4.8-tight-prior`
**CONFIG:** `config/bob_free_throw_probe_4k_v48_tight.yaml`
**Init:** `--initialize-from=bob-v4.7-test-4k`
**Hypothesis:** Policy |c|≈0.52 hurts makes; low-residual (|c|<0.35) makes **~35%**. Tighten band + BC.

## C# changes (v4.8)

| Constant                    | v4.7        | v4.8             |
| --------------------------- | ----------- | ---------------- |
| ResidualMaxMagnitude        | 2.0         | **1.0**          |
| Residual scales             | 1.5/2.0/1.5 | **1.0/1.25/1.0** |
| IdealSolverMatchRewardScale | 0.20        | **0.35**         |

## Dark screen during training

Not an ML failure — usually **Mac display sleep** or Game view unfocused @ 20×. Prevent:

```bash
caffeinate -i RUN_ID=bob-v4.8-tight-prior CONFIG=... ./scripts/train.sh --initialize-from=bob-v4.7-test-4k
```

Keep Unity **Game tab visible**; disable "Turn display off on battery" in System Settings.

## Launch

```bash
caffeinate -i env RUN_ID=bob-v4.8-tight-prior CONFIG=config/bob_free_throw_probe_4k_v48_tight.yaml \
  ./scripts/train.sh --initialize-from=bob-v4.7-test-4k
```

## Results

**Completed:** 2026-07-20 ~12:22 UTC (~19 min @ 20×)

| Metric          | v4.7-test-4k | **v4.8**          |
| --------------- | ------------ | ----------------- |
| Session success | 15.12%       | **34.43%**        |
| Rolling (HUD)   | 12.50%       | **33.33%**        |
| Trailing 1k     | 13.90%       | **34.50%**        |
| Low-\|c\| makes | ~35%         | **58.5%**         |
| Checkpoint      | —            | `Bob-380037.onnx` |

**Verdict:** Breakthrough — tighter prior band + solver-match reward **~2.5×** make rate.

Capture: `./scripts/capture-ml-run.sh bob-v4.8-tight-prior`
