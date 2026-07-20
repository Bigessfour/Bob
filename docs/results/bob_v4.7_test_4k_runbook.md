# bob-v4.7-test-4k — 4000-iteration validation run

**RUN_ID:** `bob-v4.7-test-4k`
**CONFIG:** `config/bob_free_throw_probe_4k_v47_test.yaml`
**Init:** `--initialize-from=bob-v4.7-ext`
**Target:** ~4000 HUD iterations (~380k trainer steps @ 20×)

## Launch

```bash
RUN_ID=bob-v4.7-test-4k CONFIG=config/bob_free_throw_probe_4k_v47_test.yaml \
  ./scripts/train.sh --initialize-from=bob-v4.7-ext
```

Unity: **Bob → Demo → Restore Training Behavior (Default)** before Play.

## Results

**Completed:** 2026-07-20 ~11:53 UTC (~18 min @ 20×)

| Metric              | Value                                          |
| ------------------- | ---------------------------------------------- |
| HUD iterations      | **~3360**                                      |
| Makes (score)       | **508**                                        |
| Session success     | **15.12%**                                     |
| Rolling (final HUD) | **12.50%**                                     |
| Trailing 1k (CSV)   | **13.90%**                                     |
| Positive-miss       | **0.2%** PASS                                  |
| Checkpoint          | `results/bob-v4.7-test-4k/Bob/Bob-380018.onnx` |

**Verdict:** Stable vs v4.7-ext — slight session lift; policy holds at regulation FT.

Capture: `./scripts/capture-ml-run.sh bob-v4.7-test-4k`
