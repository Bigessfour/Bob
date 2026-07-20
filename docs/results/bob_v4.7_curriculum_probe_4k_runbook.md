# bob-v4.7-curriculum — 4k distance curriculum + make-hunt BC

**RUN_ID:** `bob-v4.7-curriculum`
**CONFIG:** `config/bob_free_throw_probe_4k_v47.yaml`
**Init:** `--force` (13 obs; do not init from v4.6 11-obs checkpoints)
**Chronicle:** [training-chronicle.md](../design/training-chronicle.md)

## Pre-flight

1. Play smoke: solver prior make rate (`scripts/solver_prior_eval.py`)
2. **Make-hunt demos:** Bob → Demo → **Enable Make-Hunt Demonstration Recorder** → ≥40 makes → copy to `bobfreethrow.demo`
3. Stop Play; compile idle

**2026-07-20 snapshot:** 41 makes recorded → `bobfreethrowmake.demo` (357 KB) promoted to `bobfreethrow.demo`; prior file archived under `Assets/Demos/_archive_pre_v49/`.

## Launch

```bash
RUN_ID=bob-v4.7-curriculum CONFIG=config/bob_free_throw_probe_4k_v47.yaml \
  ./scripts/train.sh --force
```

## Results

**Completed:** 2026-07-20 ~03:24 UTC (~18 min wall @ 20×)

| Metric           | Value                                             | Gate                   |
| ---------------- | ------------------------------------------------- | ---------------------- |
| Episodes         | 3286                                              | ~4000                  |
| Success          | **10.93%**                                        | >5% stretch / trend up |
| Positive-miss    | **0.2%**                                          | ≤25%                   |
| rim_miss mean RL | **-3.045**                                        | < 0                    |
| Rolling (final)  | 20.83%                                            | —                      |
| Curriculum       | CloseRange → MidRange → **Regulation**            | —                      |
| Checkpoint       | `results/bob-v4.7-curriculum/Bob/Bob-380035.onnx` | —                      |
