# Bob — ML Training Recommendations

**Last updated:** 2026-07-17
**Audience:** Future agents implementing learning improvements
**Basis:** E2E evaluation of `BobAgent`, reward shaping, PPO config, bob-v2/v3, **bob-v4 PPO empirics**, and external review ([Grok share — ML Recommendations and Data Viz](https://grok.com/share/bGVnYWN5LWNvcHk_d98db033-14f7-4c3f-85f3-94c76e15d323))
**References:** [ML-Agents Training](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Training-ML-Agents.html) · [Imitation Learning](https://github.com/Unity-Technologies/ml-agents/blob/release-0.14.0/docs/Training-Imitation-Learning.md) · [Environment Design](https://github.com/Unity-Technologies/ml-agents/blob/release_2_verified_docs/docs/Learning-Environment-Design-Agents.md)

---

## Executive summary

Bob’s **infrastructure** (PPO handshake, scoring, HUD, CSV, shot logs) works. **Tier 1 episode design is shipped** and bob-v4 produced **real PPO** (~110k steps, 1001 episodes, **5 makes / 0.50%**).

**Learning is partial:** toward-hoop and rim_miss share rose; floor/oob fell; **success stays ≪ 5% gate**. Dense shaping still pays **rim_miss ~+1.7 mean episode RL** vs **make ~+5.1**, and **44% of misses finish with positive net RL** — PPO can optimize near-misses.

**Next (bob-v4.1 / Tier 1.5):** apply the concrete contrast + local-impulse patch below (aligned with [Grok review](https://grok.com/share/bGVnYWN5LWNvcHk_d98db033-14f7-4c3f-85f3-94c76e15d323)), upgrade the diagnostic dashboard, short validation train, then BC + 30+ min run — **not** more PPO on unchanged shaping.

---

## E2E pipeline (current)

```text
config/bob_free_throw.yaml (PPO)
  → ./scripts/train.sh (Docker, port 5004)
  → Unity Play @ 20× (BehaviorType Default)
  → BobAgent: 8 obs → policy → single impulse
  → physics → HoopScoreZone (sparse +3 make)
  → BobTrainingStats → HUD + summaries/bob_session.csv
```

| Stage          | Status                        | Issue                                                       |
| -------------- | ----------------------------- | ----------------------------------------------------------- |
| PPO config     | OK                            | No BC/GAIL/curriculum                                       |
| Observations   | Thin                          | Missing `vy`, speed, shot phase                             |
| Actions        | 3D Bob-local impulse          | `forwardBias=+6` local +Z toward hoop (world −Z at FT line) |
| Launch prior   | Bias OK after sign fix        | Still no expert/BC demos                                    |
| Dense rewards  | Tier 1.5 contrast shipped     | Validate rim_miss mean ≪ make; positive-miss %↓             |
| Episode length | OK after Tier 1               | Shot-resolved (~75 steps max)                               |
| Success metric | Misleading if misused         | High arc ≠ makes — track makes / episodes                   |
| Data viz       | Dashboard v2 + `--check-pass` | Half-split panel still optional                             |

---

## Diagnosis

### 1. Initial trajectory — Bob does not start knowing free throws

**What helps today:** neutral actions `c≈(0,0,0)` → **residual hybrid**: `BobSwishLaunchSolver` ideal impulse + zero residual (analytic swish prior at ~58°), with PPO learning small clamped corrections (`ResidualLateralScale` / `ResidualMaxMagnitude` in `ArcAcademyLayout`). Heuristic and BC demos emit residual ≈ 0. Legacy absolute bias path remains as solver fallback only.

**Next RUN_ID:** `bob-v4.7-curriculum` — P0 solver retune, make-hunt BC, hoop-distance curriculum ([training-chronicle.md](design/training-chronicle.md)).

**What breaks the prior:**

| Gap                | Detail                                                                                        |
| ------------------ | --------------------------------------------------------------------------------------------- |
| Local bias sign    | Legacy absolute fallback only — residual hybrid uses solver prior, not bias alone             |
| No imitation prior | No `.demo` file; YAML has no `behavioral_cloning` block                                       |
| No analytic seed   | ~~No parabolic v0 solver~~ — **DONE:** `BobSwishLaunchSolver` + residual hybrid in `BobAgent` |

**ML-Agents guidance:** For sparse rewards, pre-train with **Behavioral Cloning** or **GAIL** from demonstrations ([overview](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/ML-Agents-Overview.html)).

### 2. Reward / punishment — realistic physics, wrong economics

Typical episode reward ledger:

| Signal                    | When                          | Magnitude            |
| ------------------------- | ----------------------------- | -------------------- |
| Launch toward hoop        | Once                          | up to ~+0.45         |
| Launch upward / arc align | Once                          | up to ~+0.6          |
| Arc quality               | **Every step** while tracking | up to ~+0.1/step     |
| Distance penalty          | **Every step**                | −0.002 × xzDist/step |
| Made basket               | Rare                          | +3.0                 |
| OOB                       | End                           | −0.5                 |

After one shot the ball can bounce until OOB (**100–400+ steps** at 20×). Net RL drifts **very negative** while arc metrics look good. PPO learns “long episodes bad” faster than “make = great.”

**ML-Agents guidance:** Per-step penalties should pair with **early `EndEpisode()`** on goal or miss ([environment design](https://github.com/Unity-Technologies/ml-agents/blob/release_2_verified_docs/docs/Learning-Environment-Design-Agents.md)).

### 3. bob-v3 run outcome (2026-07-14)

Partial checkpoints under `results/bob-v3/Bob/`. **Do not resume bob-v3 for learning claims.**

### 4. bob-v4 empirics (2026-07-17) — Tier 1 validated, new bottleneck found

| Fact               | Evidence                                                                      |
| ------------------ | ----------------------------------------------------------------------------- |
| Real PPO           | 1001 shots with `training_connected=1`; checkpoint `Bob-113664.onnx`          |
| Makes              | 5 (eps 544, 554, 661, 872, 913) — **0.50%** session success                   |
| Make signature     | `a≈(0, +0.11, −0.25)`, `F≈(0, +5.7, −9.5)`, toward≈1.0, arc≈98%, net≈**+5.1** |
| Rim_miss economics | n=367, toward=+0.93, mean net RL **+1.69** — near-misses are profitable       |
| Timeout            | n=346, fy≈**+10.2** (overpowered), mean net **−3.0**                          |
| Floor              | n=244, fy≈**−1.1** (underpowered), declining in 2nd half                      |
| Positive misses    | **44%** of miss episodes still finish with net RL &gt; 0                      |
| Aim learning       | 1st→2nd half: toward +0.75→+0.82; more rim_miss, fewer floor                  |

Plots: `docs/results/bob_v4_learning.png`, `python/scripts/plot_learning_dashboard.py`, `python/scripts/review_training_run.py`.

**Conclusion:** Tier 1 fixed unbounded bounce penalties. Remaining failure mode is **weak make vs near-miss contrast** + **no imitation prior** into the make action island.

---

## Implementation tiers (priority order)

### Tier 1 — Episode design — **DONE (2026-07-17)**

1. ~~End episode when shot resolves~~ (rim / floor / settle / `ShotResolveMaxSteps=75`)
2. ~~Terminal miss proximity~~ (`MissProximityRewardScale`)
3. ~~Gate per-step distance penalty~~ (`IsShotInFlight` only)

### Tier 1.5 — Reward contrast + local impulse (bob-v4.1) — **DONE (code)**

Concrete constants applied; **bob-v4.1 resumed to 500k** (2026-07-18). Learning of aim/arc **yes**; make rate **~1%** last 1k. Economics: make ≫ rim_miss mean **pass**, but **positive-miss 57% FAIL** (near-miss farming). See [bob_v4.1_resume_500k_analysis.md](../results/bob_v4.1_resume_500k_analysis.md).

### Tier 1.6 — Stop rim_miss farming (bob-v4.2) — **CODE READY**

| File                  | Change                                              | Target                                                              |
| --------------------- | --------------------------------------------------- | ------------------------------------------------------------------- |
| `BobAgent.cs`         | Skip miss proximity when `applyRimPlaneMissPenalty` | rim-plane miss is not paid for “closeness”                          |
| `BobAgent.cs`         | Unify rim_miss via `IsPastRimPlane`                 | timeout/settled past plane → rim_miss + penalty; no label-only path |
| `BobAgent.cs`         | Remove rim height +1.2 gate                         | high arcs past rim still rim_miss                                   |
| `ArcAcademyLayout.cs` | `MissProximityRewardScale`                          | `0.20f` (floor/timeout only)                                        |
| `ArcAcademyLayout.cs` | `RimPlaneMissPenalty`                               | `1.25f` (above max launch shaping ~0.50)                            |
| `ArcAcademyLayout.cs` | Launch toward / up / arc-align                      | `0.20` / `0.15` / `0.15`                                            |
| `ArcAcademyLayout.cs` | `ArcQualityRewardScale`                             | `0.01f`                                                             |

**Pass check (short `RUN_ID=bob-v4.2`):** positive-miss **≤25%**; rim_miss mean net **&lt; 0** preferred; make mean still ~+8. Then extend train / BC.

### Tier 2 — “Bob knows what a free throw is”

1. **Behavioral cloning** — Record 30–50 expert shots near the empirical make island `a≈(0, 0.1, −0.25)` / `F≈(0, 5.5–7, −8.5…−10.5)`; add to `config/bob_free_throw.yaml`:
   ```yaml
   behavioral_cloning:
     demo_path: Assets/Demos/bob_free_throw.demo
     strength: 0.5
     steps: 100000
   ```
2. **Optional curriculum** — `environment_parameters` in YAML: start closer/shorter rim height; widen as success rises.
3. **Power shaping** — timeouts cluster at fy≈+10; soft-penalize |impulse| far above make band or add speed obs.
4. **Extended train** — `RUN_ID=bob-v4.1` (or successor) **30+ min @ 20×** uninterrupted after Tier 1.5 pass.

### Tier 3 — Trainer tuning & observations

1. **Observations** — Add `vy`, speed magnitude, normalized distance to rim / shot phase (validator + alignment tests if obs count changes).
2. **`time_horizon: 64`** — OK for short episodes; revisit only if episode length changes.
3. **Training stability** — No script saves, MCP bakes, or Play toggles during an active run ([ai-warehouse-ops.md](ai-warehouse-ops.md#training-stability-prevent-crashes)).
4. **In-scene diagnostics (lower priority)** — Enhance `BobTrainingSuccessGraph` or add `BobTrainingDiagnosticsHUD` with recent outcome mix + avg toward-hoop (last N shots).

---

## Data viz — diagnostic dashboard

Current tools (`plot_training_progress.py`, `review_training_run.py`, `bob_v4_learning.png`) are good for high-level progress but **not** sufficient to validate Tier 1.5. Extend `python/scripts/plot_learning_dashboard.py` (target: `docs/results/bob_v4.1_learning_dashboard.png`) with:

| Panel                   | Metric                                                              | Why                                    |
| ----------------------- | ------------------------------------------------------------------- | -------------------------------------- |
| Learning curves         | Rolling success %, mean net RL, arc quality (smoothed)              | Gate tracking                          |
| Outcome mix over time   | Stacked % make / rim_miss / floor / timeout / oob                   | Desired shift away from near-miss park |
| **Economics**           | Mean/median `episode_net_rl` by `end_reason` (+ 0-line)             | **Primary Tier 1.5 pass check**        |
| Positive-net-miss trend | % of non-makes with `episode_net_rl > 0`                            | Must fall after contrast patch         |
| Make signature          | Histograms/scatter of `fy`, `fz` (and actions) on scored shots only | Confirm convergence to make island     |
| Half-split summary      | 1st vs 2nd half: toward, floor/timeout, positive-miss               | Aim vs score separation                |

```bash
cd python && .venv/bin/python scripts/plot_learning_dashboard.py \
  --since 2026-07-17T22:59:00 \
  --output ../docs/results/bob_v4.1_learning_dashboard.png
```

Source review: [Grok — ML Recommendations and Data Viz Enhancements](https://grok.com/share/bGVnYWN5LWNvcHk_d98db033-14f7-4c3f-85f3-94c76e15d323).

---

## Cursor action order (bob-v4.1)

1. Apply Tier 1.5 table edits (`ArcAcademyRewards`, `ArcAcademyLayout`, Bob-local impulse in `BobAgent`).
2. Recompile Unity; heuristic smoke (no crash / impulse still launches).
3. Short `RUN_ID=bob-v4.1 ./scripts/train.sh --force` + Play once → `BOB_TRAINING_OK`.
4. Generate diagnostic dashboard PNG; validate pass checks.
5. If contrast OK → BC demos + YAML → long train toward **>5%** rolling success.
6. Refresh portfolio plots / GIF after success gate.

---

## Safe training workflow (mandatory)

```bash
# 1. Stop Play; wait for Unity compile idle
# 2. Start trainer
RUN_ID=bob-v4.1 ./scripts/train.sh --force   # after Tier 1.5; or resume bob-v4 only if unchanged rewards

# 3. Press Play ONCE after "Listening on port 5004"
# 4. Do NOT edit Assets/Scripts or run Unity MCP bakes until stopping training
# 5. Stop Play, then Ctrl+C trainer
# 6. Plot diagnostics
cd python && .venv/bin/python scripts/plot_learning_dashboard.py \
  --output ../docs/results/bob_v4.1_learning_dashboard.png
```

**Never** run `./scripts/train.sh --force` while Unity is in Play from a prior session without stopping first.

---

## Files to touch (when implementing)

| File                                           | Changes                                                                                       |
| ---------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `Assets/Scripts/ArcAcademyRewards.cs`          | `MadeBasket=7`, swish bump                                                                    |
| `Assets/Scripts/ArcAcademyLayout.cs`           | `ArcQualityRewardScale=0.02`, `MissProximityRewardScale=0.35`, optional `RimPlaneMissPenalty` |
| `Assets/Scripts/BobAgent.cs`                   | Bob-local impulse; wire rim-plane miss penalty                                                |
| `config/bob_free_throw.yaml`                   | Tier 2 BC block, curriculum                                                                   |
| `Assets/Demos/`                                | Expert `.demo` recordings (Tier 2)                                                            |
| `python/scripts/plot_learning_dashboard.py`    | Economics / positive-miss / make-signature panels                                             |
| `python/tests/test_unity_alignment.py`         | Guards if reward strings / obs count change                                                   |
| `docs/results/bob_v4.1_learning_dashboard.png` | Refresh after validation train                                                                |

---

## Dev tools — adopt vs reject

**Adopt (positive impact for Bob’s PPO loop):**

| Tool                                                                     | Why                                                                                        |
| ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------ |
| ML-Agents **StatsRecorder** (`BobTrainingStats`)                         | Toward-hoop, launch fy/fz, end-reason rates, success % in TensorBoard during connected PPO |
| **DemonstrationRecorder** + `Bob → Demo → Enable Demonstration Recorder` | Records `Assets/Demos/bob_free_throw.demo` for Tier 2 BC                                   |
| **`config/bob_free_throw_bc.yaml`**                                      | PPO + `behavioral_cloning` without breaking default `bob_free_throw.yaml`                  |
| **`./scripts/tensorboard.sh`**                                           | One-command `results/` TensorBoard (dev only; audience = in-scene HUD)                     |
| **`plot_learning_dashboard.py --check-pass`**                            | Tier 1.5 contrast gate from shot CSV                                                       |
| Unity MCP + bob-rag                                                      | Already required for Editor / repo-grounded edits                                          |

```bash
./scripts/tensorboard.sh
# BC after demos exist:
CONFIG=config/bob_free_throw_bc.yaml RUN_ID=bob-v4.2 ./scripts/train.sh --force
```

**Reject (marginal or wrong stack for this project):**

| Tool                                        | Why reject                                                     |
| ------------------------------------------- | -------------------------------------------------------------- |
| Weights & Biases / Neptune / ClearML        | Overlap with CSV + dashboard + TensorBoard; extra account/deps |
| Stable Baselines3 / Gymnasium skills        | Different env API — not Unity ML-Agents                        |
| verl / LLM-RL / OpenClaw-RL skills          | LLM policy training — irrelevant                               |
| Custom Ray RLlib trainers                   | Replaces `mlagents-learn`; breaks Docker `bob-train` path      |
| SAC swap “just because”                     | Optional later; BC + contrast first                            |
| Random marketplace `pytorch-patterns` skill | Generic; Bob’s `/bob-ml-agents-train` already owns the loop    |

---

## Related

- [Grok share — ML Recommendations and Data Viz](https://grok.com/share/bGVnYWN5LWNvcHk_d98db033-14f7-4c3f-85f3-94c76e15d323) — external review merged into Tier 1.5 + viz plan
- [ai-warehouse-ops.md](ai-warehouse-ops.md) — handshake + stability
- [bob-done-tracker.md](../bob-done-tracker.md) — live checklist
- [what-finished-looks-like.md](../what-finished-looks-like.md) — product Phase 3 gates
- [results/README.md](../results/README.md) — plot generation
