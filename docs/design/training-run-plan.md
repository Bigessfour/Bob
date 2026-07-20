# Bob — Training Run Plan (local M5)

**Audience:** Operator starting the next PPO session on a MacBook Pro M5
**Last updated:** 2026-07-18
**Scope:** **Local only** — no AWS / EC2 / S3 until explicitly requested.

Related: [ml-training-recommendations.md](ml-training-recommendations.md) · [ai-warehouse-ops.md](ai-warehouse-ops.md) · [bob-done-tracker.md](../bob-done-tracker.md)

---

## Repo status (do not start from zero)

| Item                                         | Status                                                                                                                 |
| -------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Tier 1 episode design                        | **Shipped**                                                                                                            |
| Tier 1.5 reward contrast + Bob-local impulse | **Shipped** — bob-v4.1 → **500k**; aim↑, makes **~1%**, positive-miss **57% FAIL**                                     |
| Tier 1.6 stop rim_miss farming               | **Code ready** — unified rim_miss; penalty **1.25**; launch scales cut; past-plane timeout/settled → rim_miss          |
| `bob-v4`                                     | Done (~110k steps, **0.50%** makes) — baseline only                                                                    |
| `bob-v4.1`                                   | **Complete** at `max_steps` 500k — see [bob_v4.1_resume_500k_analysis.md](../results/bob_v4.1_resume_500k_analysis.md) |
| Interim learning gate (>5% rolling)          | **Not met** (~1% last 1k)                                                                                              |
| Demo-done bar (70% / last 1k)                | **Not met**                                                                                                            |

**Next RUN_ID:** regulation FT only — do **not** move Bob off the free-throw line.

| Goal                           | Command / action                                                                                     |
| ------------------------------ | ---------------------------------------------------------------------------------------------------- |
| Quality make demos (preferred) | **Bob → Demo → Enable Demonstration Recorder** → confirmed makes at FT line → Disable → BC train     |
| BC after quality demos         | `CONFIG=config/bob_free_throw_bc.yaml RUN_ID=bob-v4.6 ./scripts/train.sh --initialize-from=bob-v4.4` |
| Discarded                      | `bob-v4.5` easy-range probe — aborted (spawn off FT line)                                            |

---

## Done framework (two bars)

### A — Interim learning gate (North Star / Phase 3)

- Rolling / session success **> 5%** over a sustained Play window (≈30+ min @ 20×).
- Proves “learning is visible,” not portfolio-complete.

### B — Official demo-done benchmark (committed)

| Role            | Criterion                                                                                                                                                     |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Primary**     | **≥ 70%** made free throws averaged over the **last 1,000 episodes** (`training_connected=1` shots), with **low variance** (few radical misses / OOB spikes). |
| **Secondary**   | Stable or rising **mean episode reward**; solid **arc quality**; clean shot resolution (`EndEpisode` on make/miss — no hangs).                                |
| **Qualitative** | Consistent makes; success graph trends up then plateaus; demo-ready visuals.                                                                                  |

```bash
cd python && source .venv/bin/activate
python scripts/plot_run_comparison.py \
  --window bob-v4.1:<UTC_START_OF_THIS_SESSION> \
  --check-demo-bar
```

Path to 70%/1k: resume `bob-v4.1` → hit interim **>5%** → keep extending (or BC via `bob-v4.2` if stuck) → re-measure trailing-1k until **≥70%**.

---

## Apple Silicon M5 — speed vs stability

### Choose a trainer path

| Path                                             | Device                                        | When to use                                                                                                 |
| ------------------------------------------------ | --------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| **A — Docker `./scripts/train.sh`** (default)    | **Linux/CPU** inside `bob-train` — **no MPS** | Stable, reproducible; preferred on Apple Silicon (historical `grpcio` arm64 pain). Use for clean long runs. |
| **B — Native `python/.venv` + `mlagents-learn`** | Host **MPS** (if available)                   | Faster PPO updates for iteration **only if** verify passes and handshake stays stable.                      |

**Critical:** Docker never uses Metal. `torch.backends.mps` only applies on path **B**.

### Verify MPS (path B only)

```bash
cd python && source .venv/bin/activate
python - <<'PY'
import torch
print("torch", torch.__version__)
print("mps_available", torch.backends.mps.is_available())
print("mps_built", torch.backends.mps.is_built())
PY
```

If `mps_available` is `False`, stay on Docker (path A) — do not invent GPU flags.

### YAML / command tweaks (speed)

**Already set (do not “optimize” downward blindly):**

- `config/bob_free_throw.yaml`: `batch_size: 1024`, `buffer_size: 10240`, `engine_settings.time_scale: 20`, `max_steps: 500000`

**Optional experiments only** (label runs `bob-v4.1-exp-*`; expect different learning dynamics):

- Slightly higher Editor time scale in Play (e.g. 30–40×) if physics stay stable — watch for tunneling / weird misses.
- Native path: `mlagents-learn … --torch-device mps` only after MPS verify succeeds (ML-Agents/`torch_settings.device` may also accept `mps` — confirm in your mlagents version).

**Do not:** drop `batch_size` to 64 as a “speed tip” on the main line; that changes sample efficiency, not sim speed.

### `--num-envs` — blocked on current path

| Claim                                | Reality                                                                                                                                           |
| ------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--num-envs 2` with one Unity Editor | **Does not work** as a free speedup                                                                                                               |
| What multi-env needs                 | Multiple environment **instances** (typically **standalone builds**)                                                                              |
| Status                               | **Future / blocked** until a standalone multi-env workflow exists (`scripts/build-standalone.sh` is the start, not wired for multi-env train yet) |

**Do not** pass `--num-envs 2` on the current Editor + Docker handshake.

### Unity Editor tips (faster sim)

1. Game view small or unfocused; avoid dragging huge Game/Scene windows during train.
2. Prefer **Quality** suitable for training (lower shadows/AA if FPS-bound) — HDRP beauty is for demos, not max step rate.
3. Disable **VSync** in Game view / Quality settings so Editor is not capped to display refresh.
4. Keep **time scale** at YAML default **20** unless experimenting (see above).
5. One Play session only — no domain reloads.

### System tips (M5 laptop)

1. Plug in power (avoid thermal/battery throttling).
2. Close Chrome / Electron apps with many tabs; free RAM for Unity + Docker/Python.
3. Do not sleep the Mac mid-run; disable displays sleep if leaving overnight.
4. Leave headroom for fans — a cooler chassis sustains time scale better.

---

## Pre-flight checklist

1. Stop Unity **Play**; wait until compile spinner is idle.
2. Optional: `.cursor/skills/bob-ml-agents-train/scripts/check-training-handshake.sh`
3. Confirm port 5004 free (`lsof -i :5004`). If stuck: `docker compose down && docker container prune -f`
4. Open `Assets/Scenes/BobTraining.unity`; Behavior Name **`Bob`**; Behavior Type **Default**.
5. Start trainer (path **A** or **B** below).
6. Wait for **Listening on port 5004**, then press **Play once**.
7. Console: **`BOB_TRAINING_OK`**.
8. **Do not** edit C#, save scripts, or Unity MCP-bake until you Stop Play.

---

## Metrics captured

### HUD (`BobTrainingStats` → `BobTrainingHUD` / wall panels)

| Field                         | Meaning                                         |
| ----------------------------- | ----------------------------------------------- |
| Iterations / Episodes         | `TotalIterations`                               |
| Score                         | `BasketballPoints` (makes)                      |
| Rewards / Penalties / Net RL  | Cumulative positive / negative magnitudes / net |
| Session success %             | `BasketballPoints / TotalIterations`            |
| Rolling success %             | Recent-window make rate (graph)                 |
| Rolling arc quality           | Recent-window peak arc (0–100%)                 |
| Last end reason / toward-hoop | Outcome chip diagnostics                        |

### CSV columns

**`summaries/bob_session.csv`:**
`timestamp, iteration, scored, basketball_points, session_success_pct, rolling_success_pct, rolling_arc_quality, net_rl`

**`summaries/bob_shots.csv`:**
`timestamp, iteration, training_connected, ax, ay, az, fx, fy, fz, toward_hoop_dot, scored, episode_net_rl, peak_arc_pct, end_reason`

**Trainer:** `results/<RUN_ID>/Bob/` — checkpoints, `.onnx`, TensorBoard (`./scripts/tensorboard.sh`)

**Gap:** CSVs do **not** store `run_id`. Compare with `--window label:since[:until]` or archive under `summaries/archive/<RUN_ID>/`.

---

## Plot / measurement over time

```bash
cd python && source .venv/bin/activate

python scripts/plot_training_progress.py \
  --output ../docs/results/training_progress.png

python scripts/plot_learning_dashboard.py \
  --since <UTC_START> \
  --output ../docs/results/bob_v4.1_learning_dashboard.png

python scripts/plot_learning_dashboard.py --since <UTC_START> --check-pass

# Multi-run overlay + demo-done check (70% / last 1k)
python scripts/plot_run_comparison.py \
  --window bob-v4:2026-07-17T22:59:00:2026-07-18T02:00:00 \
  --window bob-v4.1:<UTC_START> \
  --output ../docs/results/run_comparison.png

python scripts/plot_run_comparison.py \
  --window bob-v4.1:<UTC_START> \
  --check-demo-bar

./scripts/tensorboard.sh   # optional, dev only
```

---

## Ready to start training (gate)

- [x] Tier 1 + Tier 1.5 **code** in tree
- [x] `bob-v4.1` checkpoint exists (~102k / 500k)
- [x] CSV + dashboard + multi-run compare path in repo
- [x] Official **70% / last 1k** demo-done bar documented
- [x] M5 Docker vs native MPS caveats + `--num-envs` blocked note
- [ ] Operator available for uninterrupted Play (no C# edits)
- [ ] Handshake pre-flight (port free, scene ready)

**Plan ready: YES** — resume `bob-v4.1`. Demo-done is **not** claimed until trailing-1k ≥ 70%.

---

## Start training (exact commands)

### A — Docker (recommended stable / default)

```bash
# Stop Play first; port 5004 free
lsof -i :5004

# Linux/CPU trainer in bob-train — NOT MPS
RUN_ID=bob-v4.1 ./scripts/train.sh --resume

# After "Listening on port 5004" → Unity Play ONCE → BOB_TRAINING_OK
# Stop Play, then Ctrl+C trainer when done
```

### B — Native MPS (optional fast iteration)

```bash
cd python && source .venv/bin/activate

# Must print mps_available True — else use path A
python -c "import torch; print(torch.backends.mps.is_available())"

# From repo root (venv active). No --num-envs.
mlagents-learn config/bob_free_throw.yaml \
  --run-id=bob-v4.1 \
  --resume \
  --torch-device=mps

# Same Unity handshake: wait for listen → Play once → BOB_TRAINING_OK
```

If native path hits `grpcio` / communicator issues, fall back to **A** without changing reward code.

Budget: `max_steps: 500000`; `bob-v4.1` is ~102k in — resume continues toward that budget.

---

## After the run — decision tree

1. If interim **>5%** rolling → keep extending `bob-v4.1` toward **70% / last 1k**.
2. If stuck ≪5% with good economics → record BC demos → `bob-v4.2` + `config/bob_free_throw_bc.yaml`.
3. If communicator crashes → [ai-warehouse-ops.md](ai-warehouse-ops.md); never edit scripts mid-Play.
