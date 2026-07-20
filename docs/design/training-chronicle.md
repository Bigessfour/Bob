# Bob — Training Chronicle (chronological)

**Purpose:** Single timeline of ML training runs, code changes between runs, and pass/fail gates.
**Update rule:** Append a row after every completed or aborted `./scripts/train.sh` session.
**Related:** [training-run-plan.md](training-run-plan.md) · [ml-training-recommendations.md](ml-training-recommendations.md) · [bob-done-tracker.md](../bob-done-tracker.md)

---

## Method evolution (summary)

| Era           | Action space                  | Prior                  | Demos                    | Curriculum        | Best success    |
| ------------- | ----------------------------- | ---------------------- | ------------------------ | ----------------- | --------------- |
| v4–v4.1       | Absolute local impulse        | forward bias           | None                     | No                | **0.5–1%**      |
| v4.2–v4.4     | Absolute + Tier 1.6 economics | bias + shaping         | Solver auto (c≠0 island) | No                | **~1.2%**       |
| v4.6-residual | Residual hybrid               | `BobSwishLaunchSolver` | c≈0 auto batch           | No                | **2.08%**       |
| v4.6.1        | Residual + anti-farming       | Same solver            | Same demos               | No                | _(interrupted)_ |
| **v4.7**      | Residual **tight** (max 2.0)  | Solver **retuned**     | **Make-hunt** required   | **Hoop distance** | **10.93%**      |

**Plateau diagnosis (2026-07-20):** Aim learned; makes stuck ~1–2%. Root cause: BC demos teach near-miss solver shots; sparse make signal; no curriculum. **v4.7** addresses prior quality, demo quality, distance curriculum, tighter residual band, 13 obs.

---

## Chronological run log

| Date (UTC) | RUN_ID                   | Steps           | Eps      | Success                     | Positive-miss  | Init              | Config                 | Code / method notes                                  |
| ---------- | ------------------------ | --------------- | -------- | --------------------------- | -------------- | ----------------- | ---------------------- | ---------------------------------------------------- |
| 2026-07-17 | bob-v4                   | 113k            | 1001     | **0.50%**                   | ~44%           | force             | bob_free_throw.yaml    | Tier 1 episodes; profitable rim_miss                 |
| 2026-07-18 | bob-v4.1                 | 500k            | —        | **~1.0%**                   | **57% FAIL**   | resume            | bob_free_throw.yaml    | Tier 1.5 contrast + local impulse                    |
| 2026-07-19 | bob-v4.2                 | 372k            | 4002     | **0.57%**                   | **2.9% PASS**  | force             | bob_free_throw.yaml    | Tier 1.6 rim_miss economics                          |
| 2026-07-19 | bob-v4.3                 | 380k            | ~4290    | **1.18%**                   | 1.8%           | init v4.2         | probe_4k               | More PPO; flat vs v4.2                               |
| 2026-07-19 | bob-v4.4                 | 380k            | 4290     | **1.24%**                   | 3.0%           | init v4.3         | probe_4k_bc            | BC + power-band; flat                                |
| 2026-07-19 | bob-v4.5                 | —               | —        | —                           | —              | —                 | probe_4k_easy          | **ABORTED** — spawn off FT line                      |
| 2026-07-20 | bob-v4.6-residual        | 475k            | 4035     | **2.08%**                   | **35.5% FAIL** | force             | probe_5k_residual_bc   | Residual hybrid; 11 obs; c≈0 demos                   |
| 2026-07-20 | bob-v4.6.1               | —               | —        | —                           | —              | init v4.6-res     | probe_4k_v461          | Anti-farming; 40× sim; **interrupted**               |
| 2026-07-20 | **bob-v4.7-curriculum**  | 380k            | 3286     | **10.93%**                  | **0.2% PASS**  | **force**         | **probe_4k_v47**       | Make-hunt BC; curriculum 0.65→1.0; rolling **20.8%** |
| 2026-07-20 | **bob-v4.7-ext**         | ~571k (partial) | 5019 HUD | **14.58%** / roll **25%**   | **0.4% PASS**  | **init v4.7**     | **probe_v47_ext**      | **UnityTimeOutException**; `Bob-571627.onnx`         |
| 2026-07-20 | **bob-v4.7-test-4k**     | 380k            | ~3360    | **15.12%** / roll **12.5%** | **0.2% PASS**  | **init v4.7-ext** | **probe_4k_v47_test**  | Validation complete; `Bob-380018.onnx`               |
| 2026-07-20 | **bob-v4.8-tight-prior** | 380k            | ~3419    | **34.43%** / roll **33%**   | **0.2% PASS**  | **init test-4k**  | **probe_4k_v48_tight** | Trail-1k **34.5%**; `Bob-380037.onnx`                |

---

## v4.7 method block (2026-07-20)

**RUN_ID:** `bob-v4.7-curriculum`
**CONFIG:** `config/bob_free_throw_probe_4k_v47.yaml`
**Init:** `--force` (do **not** init from absolute-impulse or mismatched-obs checkpoints)

### C# changes

| Area           | Change                                                     |
| -------------- | ---------------------------------------------------------- |
| **Solver**     | 56°, damping **1.08**, AimPastRim **0.05**                 |
| **Residual**   | scales **1.5 / 2.0 / 1.5**, max magnitude **2.0**          |
| **Curriculum** | `BobCurriculum` + `distance_scale` env param; hoop +Z only |
| **Obs**        | **13** (+ curriculum scale, normalized flight phase)       |
| **Demos**      | **Bob → Demo → Enable Make-Hunt Demonstration Recorder**   |

### YAML

- BC strength **0.8**, steps 100k
- Curriculum: distance_scale **0.65 → 0.80 → 1.0** (progress gates)
- `time_scale: 20` (40× optional after physics check)

### Pre-train gates

```bash
# Play: 100 heuristic c≈0 shots — want ≥10% makes before long PPO
cd python && .venv/bin/python scripts/solver_prior_eval.py --since <UTC> --residual-max 0.35

# Record ≥40 makes → Assets/Demos/bobfreethrow.demo
# Bob → Demo → Enable Make-Hunt Demonstration Recorder (preferred)
```

### Launch

```bash
RUN_ID=bob-v4.7-curriculum CONFIG=config/bob_free_throw_probe_4k_v47.yaml \
  ./scripts/train.sh --force
```

### Pass checks (~4k eps)

- Success **>5%** on close-range lesson (stretch) or clear upward trend
- Positive-miss **≤25%**
- rim_miss mean RL **< 0**
- Low-residual make rate in CSV **≥ prior run**

### Run log

| Started (UTC)    | RUN_ID              | Demo snapshot                                                                                  | Notes                                                                                                    |
| ---------------- | ------------------- | ---------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| 2026-07-20T03:06 | bob-v4.7-curriculum | 41 makes, 357 KB (`bobfreethrowmake.demo` → `bobfreethrow.demo`; prior in `_archive_pre_v49/`) | **Done** ~18 min @ 20× — **10.93%** (359/3286), rolling **20.83%**, pos-miss **0.2%**; `Bob-380035.onnx` |

---

## Aborted / invalid runs

| RUN_ID   | Reason                                           |
| -------- | ------------------------------------------------ |
| bob-v4.5 | Spawn moved off free-throw line — not comparable |
| bob-v3   | Communicator crash; 0% makes — do not resume     |

---

## References

- Runbooks: `docs/results/bob_v4.*_runbook.md`
- Dashboard: `python/scripts/plot_learning_dashboard.py --check-pass`
- Solver eval: `python/scripts/solver_prior_eval.py`
