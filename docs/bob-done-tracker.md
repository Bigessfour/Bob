# Bob — Done Tracker

**Last updated:** 2026-07-17 · **Branch:** `main` · **Merged:** [#10](https://github.com/Bigessfour/Bob/pull/10), [#11](https://github.com/Bigessfour/Bob/pull/11) (Ollama AI Review)
**Pin:** open this file in Cursor → right-click tab → **Pin Tab** (split beside Unity).

North Star: [what-finished-looks-like.md](what-finished-looks-like.md) · [what-right-looks-like.md](what-right-looks-like.md) · ML fixes: [design/ml-training-recommendations.md](design/ml-training-recommendations.md) · E2E runbook: [design/ai-warehouse-ops.md](design/ai-warehouse-ops.md)

---

## MVP verdict

**Infrastructure + polish: Done** — training handshake, dual HUD, audio, PlayMode score test, inference menus, hero + GIF artifacts on `feature/dual-hud-scoreboard`.

**Learning demo: Blocked on bob-v4 train** — Tier 1 reward/episode code landed in `BobAgent.cs`; run **`RUN_ID=bob-v4`** next.

**Publish:** Static portfolio in-repo — `docs/portfolio-site/` + README links. **No AWS hosting.**

---

## ML process evaluation (2026-07-14)

Full analysis: **[design/ml-training-recommendations.md](design/ml-training-recommendations.md)**

| Finding                                                 | Impact                                                             |
| ------------------------------------------------------- | ------------------------------------------------------------------ |
| Episodes run until OOB (ball bounces hundreds of steps) | Per-step `-0.002×dist` dominates; net RL ~−163                     |
| Sparse make (+3) almost never seen                      | PPO cannot credit launch action                                    |
| Arc quality ≠ makes                                     | Policy optimizes apex, not rim entry                               |
| World-space impulse + spawn rotation                    | Breaks “toward hoop” prior under jitter                            |
| No BC / expert demos                                    | Random policy has no free-throw prior                              |
| bob-v3 trainer crashes                                  | `Communicator has exited` when Play toggles / recompile during run |

**bob-v3 partial artifacts:** `results/bob-v3/Bob/` (~9k–41k steps, `.onnx` checkpoints). **Do not claim learning from bob-v3** — resume only after bob-v4 code lands.

---

## Week 1 gate (blocks "project complete")

- [x] `bash ./scripts/validate-scene.sh` → `VALIDATE_PASS`
- [x] `pytest tests/test_unity_alignment.py` → **35/35**
- [x] `./scripts/train.sh` → terminal shows **Listening on port 5004**
- [x] Unity **Play** → console **`BOB_TRAINING_OK`**
- [x] HUD **Episodes** increments each shot
- [x] PR #7 merged to `main` + CI green

### Handshake checklist (copy each run)

1. **Stop Play** if already running; wait for compile idle.
2. `./scripts/train.sh` — wait for **Listening on port 5004**.
3. Unity → `BobTraining.unity` → **Play once**.
4. Confirm console: `BOB_TRAINING_OK: Python trainer connected. Time scale = 20x`.
5. **Do not edit C# or MCP-bake during training** — causes `Communicator has exited`.

**If inference fallback:** Stop Play → confirm trainer listening → Play again.
**Port busy:** `docker compose down && docker container prune -f`
**Trainer crash:** [ai-warehouse-ops.md](design/ai-warehouse-ops.md#training-stability-prevent-crashes)

---

## Done Detector (demo-ready scope)

- [x] End-to-end manual test recorded
- [x] Core loop in code (scene + agent + HUD + scoring)
- [x] Dual HUD + audio + portfolio artifacts (hero `023`, GIF `023-training-gif`)
- [x] PlayMode test: make → +1 `BasketballPoints` (`BobScoreIncrementPlayModeTest`)
- [ ] **Visible learning** — rolling success **>5%**, rising plot ([ml-training-recommendations.md](design/ml-training-recommendations.md) Tier 1 + bob-v4) — **trainer running `bob-v4`**
- [x] Merge [PR #10](https://github.com/Bigessfour/Bob/pull/10) → `main` on green CI
- [x] Merge [PR #11](https://github.com/Bigessfour/Bob/pull/11) → `main` (Ollama AI Review)
- [ ] README portfolio section links `docs/portfolio-site/` + latest hero/GIF/plot

---

## Arc Training view (6 prompts)

| #   | Item                               | Code                              | Play verified |
| --- | ---------------------------------- | --------------------------------- | ------------- |
| 1   | Bob eyes + follow                  | done                              | [x]           |
| 2   | CameraRig + orbit (F1 reset)       | done                              | [x]           |
| 3   | Wall HUD + near-Bob float          | done                              | [x]           |
| 4   | HDRP silver rim + translucent net  | done                              | [x]           |
| 5   | `Bob → Polish → Fix Training View` | done (`ArcTrainingViewValidator`) | —             |
| 6   | Play capture + screenshot          | done (`023-dual-hud-hero`)        | [x]           |

---

## vNext — implementation backlog (future agents)

**Canonical 14-day stack:** [planning/next-14-days.md](planning/next-14-days.md) (priorities 1–5; no AWS hosting).

### Phase 3 — Learning (priority **1**)

- [x] **Tier 1 ML fixes** in `BobAgent.cs` — shot-resolved `EndEpisode`, terminal miss proximity, gate per-step dist penalty (2026-07-17)
- [x] **bob-v4 short PPO** — ~110k steps, 5/1001 makes (0.50%); diagnostic plots in `docs/results/`
- [x] **Tier 1.5 / bob-v4.1** — `MadeBasket=7`, `ArcQualityRewardScale=0.02`, `MissProximityRewardScale=0.35`, Bob-local impulse, `RimPlaneMissPenalty=0.2` (**code 2026-07-17**)
- [x] **Diagnostic dashboard v2** — economics / positive-miss / make-signature panels in `plot_learning_dashboard.py`
- [ ] **Short validation train** `RUN_ID=bob-v4.1` + confirm rim_miss mean net ≪ make / positive-miss % drops
- [ ] **Extended train** (30+ min @ 20× after Tier 1.5 pass) + refresh `docs/results/bob_v4.1_learning_dashboard.png`
- [ ] **Tier 2** — BC demos (`Assets/Demos/bob_free_throw.demo`), curriculum, power shaping — **recorder menu + `bob_free_throw_bc.yaml` scaffolded**; demos not recorded yet
- [x] **Dev ML tools** — StatsRecorder → TensorBoard, `./scripts/tensorboard.sh`, demo recorder menus (reject W&B/SB3/LLM-RL)
- [x] Inference demo menus — `Bob → Demo → Enable Inference Only`
- [x] Training GIF scaffold — `docs/progress/023-training-gif/capture.gif` (re-capture after bob-v4.1 policy)

### Phase 3–4 — Production bar **I** (priorities **2**, **4**, **5**)

- [ ] Unity Recorder + `scripts/build-standalone.sh` + `scripts/capture-hero-video.sh` — **scripts added; Recorder package optional**
- [ ] `BobGameStateMachine.cs` + post-process / particle juice pass (after learning proves) — **state machine scaffold done**
- [ ] `scripts/release-checklist.sh` + optional CI build/capture step — **checklist script done**

### Phase 4 — Ship (in-repo portfolio)

- [x] Merge **PR #10** → `main`
- [x] Merge **PR #11** → `main` (Ollama AI Review)
- [ ] README portfolio section + `docs/portfolio-site/` synced with latest GIF/plot/hero

### Code / test debt

- [ ] EditMode tests for reward calculation (`BobAgentTests.cs`) — see [testing-strategy.md](testing-strategy.md)
- [ ] PlayMode test: full make via `HoopScoreZone` trigger (beyond `RecordBasketballPoint` unit path)
- [ ] Minimal trainer path: wire `ArcAcademyScorePopup` in `EnsureCoreMvpComponents`

---

## Last run log

| Date       | Run ID | train.sh | BOB_TRAINING_OK | Makes | Notes                                                                          |
| ---------- | ------ | -------- | --------------- | ----- | ------------------------------------------------------------------------------ |
| 2026-06-23 | bob-v0 | yes      | yes             | low   | Step 5000/10000; pre–launch-shaping                                            |
| 2026-06-24 | bob-v2 | yes      | yes             | low   | 865 iter batchmode; plot refreshed                                             |
| 2026-07-14 | bob-v3 | yes      | yes (brief)     | **0** | ~40k trainer steps; arc ~74%, net RL −163; **crashed** on communicator timeout |

---

## Quick commands

```bash
bash ./scripts/validate-scene.sh
cd python && pytest tests/test_unity_alignment.py -q
lsof -i :5004

# After Tier 1 ML fixes land:
RUN_ID=bob-v4 ./scripts/train.sh --force
# Play ONCE after Listening on port 5004 — do not touch scripts until done

python scripts/plot_training_progress.py --output ../docs/results/training_progress.png
```

---

## Agent handoff rules

1. **Read** [ml-training-recommendations.md](design/ml-training-recommendations.md) before changing rewards, obs count, or YAML.
2. **Do not** resume bob-v3 expecting visible learning — implement bob-v4 Tier 1 first.
3. **Do not** run training and Unity MCP scene bakes in the same session.
4. **Query** `bob-rag` before code; **Unity MCP** before scene edits.
5. **Update this file** when bob-v4 train completes or ML tiers ship.
