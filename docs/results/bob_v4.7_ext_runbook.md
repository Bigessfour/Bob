# bob-v4.7-ext — extend toward 70% / last 1k

**RUN_ID:** `bob-v4.7-ext`
**CONFIG:** `config/bob_free_throw_probe_v47_ext.yaml`
**Init:** `--initialize-from=bob-v4.7-curriculum` (13 obs; **do not** `--force`)
**Prior run:** [bob_v4.7_curriculum_probe_4k_runbook.md](bob_v4.7_curriculum_probe_4k_runbook.md) — **10.93%** session, **20.83%** rolling

## Pre-flight (no new demo run)

- [x] Make-hunt demos in `Assets/Demos/bobfreethrow.demo` (357 KB, 41 makes)
- [ ] Unity **Stop Play**; compile idle
- [ ] **Bob → Demo → Disable Demonstration Recorder**
- [ ] Bob **Behavior Parameters → Behavior Type = Default**
- [ ] Trainer **not** running yet

## Launch

```bash
RUN_ID=bob-v4.7-ext CONFIG=config/bob_free_throw_probe_v47_ext.yaml \
  ./scripts/train.sh --initialize-from=bob-v4.7-curriculum
```

1. Wait for **Listening on port 5004**
2. Unity **Play once** — confirm `BOB_TRAINING_OK: Time scale = 20x`
3. Do **not** edit C# or toggle Play until trainer finishes (~760k steps, ~35 min @ 20×)

## Pass gates

| Metric              | Gate                                                  |
| ------------------- | ----------------------------------------------------- |
| Trailing 1k success | **≥ 70%** (`plot_run_comparison.py --check-demo-bar`) |
| Positive-miss       | ≤ 25%                                                 |
| rim_miss mean RL    | < 0                                                   |

## Post-run

```bash
cd python && source .venv/bin/activate
python3 scripts/plot_learning_dashboard.py --since <UTC_START> \
  --title bob-v4.7-ext --output ../docs/results/bob_v4.7_ext_learning_dashboard.png --check-pass
python3 scripts/plot_run_comparison.py --window bob-v4.7-ext:<UTC_START> --check-demo-bar
```

## Results

**Partial run (stopped by operator):** 2026-07-20 ~11:30 UTC (~28 min @ 20×)

| Metric                  | Value                                                           | Gate                         |
| ----------------------- | --------------------------------------------------------------- | ---------------------------- |
| HUD iterations          | **5019**                                                        | —                            |
| Makes (score)           | **732**                                                         | —                            |
| Session success         | **14.58%**                                                      | —                            |
| Rolling (final HUD)     | **25.00%**                                                      | demo-done **70%/1k** not met |
| Trailing 1k (CSV)       | **11.50%**                                                      | —                            |
| Positive-miss           | **0.4%**                                                        | ≤25% PASS                    |
| Trainer steps (partial) | ~571k / 760k target (**UnityTimeOutException** after Play stop) | —                            |
| Checkpoint              | `results/bob-v4.7-ext/Bob/Bob-571627.onnx` → `Bob.onnx`         | —                            |

**Verdict:** Modest lift vs v4.7-curriculum alone (rolling **20.8% → 25%**); plateau — more PPO steps unlikely to reach 70%/1k without new method.

Capture: `./scripts/capture-ml-run.sh bob-v4.7-ext`
