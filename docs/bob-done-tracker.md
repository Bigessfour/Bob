# Bob — Done Tracker

**Last updated:** 2026-08-16 · **Branch:** `feature/classmate-showcase` · **Status:** **Classmate-showcase ready (v4.8)**
**Pin:** open this file in Cursor → right-click tab → **Pin Tab** (split beside Unity).

North Star: [what-finished-looks-like.md](what-finished-looks-like.md) · [what-right-looks-like.md](what-right-looks-like.md) · ML fixes: [design/ml-training-recommendations.md](design/ml-training-recommendations.md) · Chronicle: [design/training-chronicle.md](design/training-chronicle.md) · Timeline: [design/ml-project-timeline.md](design/ml-project-timeline.md)

---

## MVP verdict

**Infrastructure + polish: Done** — training handshake, dual HUD, audio, PlayMode score test, inference menus, hero captures, portfolio site.

**Learning demo: Done (portfolio scope)** — **bob-v4.8-tight-prior** @ **34.5%** trailing-1k; **InferenceOnly Play validated ~35%** (`Assets/Models/Bob.onnx`). Interim **>5%** gate passed. Demo-done **70%/1k** not pursued — see [ml-project-timeline.md](design/ml-project-timeline.md).

**Publish:** Static portfolio in-repo — `docs/portfolio-site/story.html` + `index.html` + README. **No AWS hosting.** HUD chip names Inference vs Solver so the 50/50 streak cannot be mislabeled.

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
- [x] **Visible learning** — interim **>5%** rolling (**bob-v4.7-curriculum 10.93%**, v4.8 trail-1k **34.5%**)
- [x] **Tier 1.5 / bob-v4.1** — code + full 500k resume; aim↑ / makes ~1%; positive-miss **FAIL**
- [x] **Tier 1.6 reward patch** — unified rim_miss; penalty **1.25**; launch scales cut; past-plane timeout/settled → rim_miss (**2026-07-18**)
- [x] **v4.6–v4.8 residual + curriculum** — superseded bob-v4.2 probe; contrast pass at v4.8 (positive-miss **0.2%**)
- [x] **Diagnostic dashboard v2** — economics / positive-miss / make-signature panels in `plot_learning_dashboard.py`
- [x] **bob-v4.1 extended to max_steps** — 500k; analysis in [bob_v4.1_resume_500k_analysis.md](results/bob_v4.1_resume_500k_analysis.md)
- [x] Merge [PR #10](https://github.com/Bigessfour/Bob/pull/10) → `main` on green CI
- [x] Merge [PR #11](https://github.com/Bigessfour/Bob/pull/11) → `main` (Ollama AI Review)
- [x] README + `docs/portfolio-site/story.html` synced to v4.8 / honest demos

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
- [x] **Tier 1.5 / bob-v4.1** — contrast + local impulse; full **500k** resume (~1% last 1k; positive-miss **57% FAIL**)
- [x] **Diagnostic dashboard v2** — economics / positive-miss / make-signature panels in `plot_learning_dashboard.py`
- [x] **bob-v4.1 → max_steps** — analysis [bob_v4.1_resume_500k_analysis.md](results/bob_v4.1_resume_500k_analysis.md)
- [x] **Tier 1.6** — unified rim_miss; penalty **1.25**; launch scales cut; past-plane timeout/settled → rim_miss; arc 0.01
- [x] **v4.6 residual → v4.8 tight-prior** — 2.08% → 10.93% → **34.5%**; 70%/1k **not pursued**
- [x] **Tier 2** — BC make-hunt demos + hoop curriculum shipped in v4.7
- [x] **Training run plan** — [design/training-run-plan.md](design/training-run-plan.md) (M5 Docker vs MPS, 70%/1k bar, plot commands)
- [x] **Dev ML tools** — StatsRecorder → TensorBoard, `./scripts/tensorboard.sh`, demo recorder menus (reject W&B/SB3/LLM-RL)
- [x] Inference demo menus — `Bob → Demo → Prepare Classmate Showcase` / `Prepare Solver Wow`
- [x] Honest HUD chip — `BobShowcaseMode` (replaces "Inference fallback" lie)
- [x] Training GIF scaffold — `docs/progress/023-training-gif/capture.gif` (re-capture after bob-v4.2+ policy)

### Phase 3–4 — Production bar **I** (priorities **2**, **4**, **5**)

- [ ] Unity Recorder + `scripts/build-standalone.sh` + `scripts/capture-hero-video.sh` — **scripts added; Recorder package optional**
- [ ] `BobGameStateMachine.cs` + post-process / particle juice pass (after learning proves) — **state machine scaffold done**
- [ ] `scripts/release-checklist.sh` + optional CI build/capture step — **checklist script done**

### Phase 4 — Ship (in-repo portfolio)

- [x] Merge **PR #10** → `main`
- [x] Merge **PR #11** → `main` (Ollama AI Review)
- [x] README portfolio section + `docs/portfolio-site/story.html` synced with v4.8
- [ ] Record classmate QuickTime — [showcase-capture.md](showcase-capture.md) (**local Unity**)

### Code / test debt

- [ ] EditMode tests for reward calculation (`BobAgentTests.cs`) — see [testing-strategy.md](testing-strategy.md)
- [x] PlayMode test: full make via `HoopScoreZone` trigger (`BobScoreZonePhysicsPlayModeTest`)
- [ ] Minimal trainer path: wire `ArcAcademyScorePopup` in `EnsureCoreMvpComponents`

---

## Last run log

| Date       | Run ID              | train.sh | BOB_TRAINING_OK | Makes     | Notes                                                                                             |
| ---------- | ------------------- | -------- | --------------- | --------- | ------------------------------------------------------------------------------------------------- |
| 2026-06-23 | bob-v0              | yes      | yes             | low       | Step 5000/10000; pre–launch-shaping                                                               |
| 2026-06-24 | bob-v2              | yes      | yes             | low       | 865 iter batchmode; plot refreshed                                                                |
| 2026-07-14 | bob-v3              | yes      | yes (brief)     | **0**     | ~40k trainer steps; arc ~74%, net RL −163; **crashed** on communicator timeout                    |
| 2026-07-17 | bob-v4              | yes      | yes             | 5/1001    | 0.50%; Tier 1 validated; profitable rim_miss                                                      |
| 2026-07-18 | bob-v4.1            | yes      | partial         | ~1.0%     | 500k done; aim↑; **positive-miss 57%** → Tier 1.6                                                 |
| 2026-07-20 | bob-v4.6-residual   | yes      | yes             | **2.08%** | 4035 eps; residual hybrid; positive-miss 35% FAIL                                                 |
| 2026-07-20 | bob-v4.6.1          | —        | —               | —         | Anti-farming; interrupted                                                                         |
| 2026-07-20 | bob-v4.7-curriculum | —        | —               | **10.93%** | Solver retune + curriculum + make-hunt BC                                   |
| 2026-07-20 | bob-v4.8-tight-prior | yes     | yes             | **34.5%**  | Trail-1k; InferenceOnly Play ~31–35%; **portfolio peak**                    |
| 2026-08-16 | classmate-showcase   | —        | —               | —          | Honest HUD + menus + story.html — no new PPO                                |

Full timeline: **[design/training-chronicle.md](design/training-chronicle.md)**

---

## Quick commands

```bash
bash ./scripts/validate-scene.sh
cd python && pytest tests/test_unity_alignment.py -q
lsof -i :5004

# After Unity recompile (Tier 1.6 rewards) — fresh run, do not resume bob-v4.1
RUN_ID=bob-v4.2 ./scripts/train.sh --force
# Play once → BOB_TRAINING_OK
# Then:
cd python && source .venv/bin/activate
python scripts/plot_learning_dashboard.py --since <UTC_START> --check-pass \
  --output ../docs/results/bob_v4.2_learning_dashboard.png
python scripts/plot_run_comparison.py --window bob-v4.2:<UTC_START> --check-demo-bar
# Play ONCE after Listening on port 5004 — do not touch scripts until done

cd python && source .venv/bin/activate
python scripts/plot_learning_dashboard.py --since <UTC_START> \
  --output ../docs/results/bob_v4.1_learning_dashboard.png
python scripts/plot_run_comparison.py --window bob-v4.1:<UTC_START> --check-demo-bar
```

---

## Agent handoff rules

1. **Read** [ml-training-recommendations.md](design/ml-training-recommendations.md) before changing rewards, obs count, or YAML.
2. **Do not** resume bob-v3 expecting visible learning — implement bob-v4 Tier 1 first.
3. **Do not** run training and Unity MCP scene bakes in the same session.
4. **Query** `bob-rag` before code; **Unity MCP** before scene edits.
5. **Update this file** when bob-v4 train completes or ML tiers ship.
