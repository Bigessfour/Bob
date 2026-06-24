# Bob — Done Tracker

**Last updated:** 2026-06-23 · **Branch:** `feature/simple-arc-academy`  
**Pin:** open this file in Cursor → right-click tab → **Pin Tab** (split beside Unity).

North Star: [what-finished-looks-like.md](what-finished-looks-like.md) · [what-right-looks-like.md](what-right-looks-like.md) · E2E runbook: [design/ai-warehouse-ops.md](design/ai-warehouse-ops.md)

---

## MVP verdict

**Done (infrastructure)** — Week 1 gate verified 2026-06-23: `VALIDATE_PASS`, trainer on port 5004, `BOB_TRAINING_OK`, Episodes/CSV iterations advancing, PR #7 merged.

**Open (learning quality)** — Launch-direction reward shaping added 2026-06-23; needs **`RUN_ID=bob-v2`** retrain before claiming “Bob learns upward arc.” Uncommitted branch fixes not yet on `main` (`validate-scene.sh` fails on `main` for missing prefab scripts).

---

## Week 1 gate (blocks "project complete")

- [x] `bash ./scripts/validate-scene.sh` → `VALIDATE_PASS`
- [x] `pytest tests/test_unity_alignment.py` → 32/32
- [x] `./scripts/train.sh` → terminal shows **Listening on port 5004**
- [x] Unity **Play** → console **`BOB_TRAINING_OK`**
- [x] Wall HUD **Episodes** increments each shot (CSV iteration 99→103 during run)
- [x] Wall HUD status green: **Training (PPO)** (communicator connected; trainer Step lines)
- [x] PR #7 merged to `main` + CI green ([PR #7](https://github.com/Bigessfour/Bob/pull/7))

### Handshake checklist (copy each run)

1. **Stop Play** if already running.
2. `./scripts/train.sh` — wait for **Listening on port 5004**.
3. Unity → `BobTraining.unity` → **Play**.
4. Confirm console: `BOB_TRAINING_OK: Python trainer connected. Time scale = 20x`.
5. Watch wall HUD **Episodes** for 5–10 shots; trainer terminal should show **Step** lines.

**If inference fallback:** Stop Play → confirm trainer listening → Play again.  
**Port busy:** `docker compose down && docker container prune -f`  
**Trainer crash (`Communicator has exited` / worker restarts):** see [ai-warehouse-ops.md](design/ai-warehouse-ops.md#training-stability-prevent-crashes) — no script edits during Play; Play once after compile idle.

---

## Done Detector (current MVP scope)

- [x] End-to-end manual test recorded (Last run log below)
- [x] Meets [what-finished-looks-like.md](what-finished-looks-like.md) core loop in code (scene + agent + HUD)
- [x] No open blockers in this file → MVP verdict **Done**

---

## Arc Training view (6 prompts)

| #   | Item                                | Code                              | Play verified |
| --- | ----------------------------------- | --------------------------------- | ------------- |
| 1   | Bob eyes + follow                   | done                              | [ ]           |
| 2   | CameraRig + orbit (F1 reset)        | done                              | [ ]           |
| 3   | Wall HUD (Episodes / Success / Arc) | done                              | [x]           |
| 4   | HDRP silver rim + translucent net   | done                              | [ ]           |
| 5   | `Bob → Polish → Fix Training View`  | done (`ArcTrainingViewValidator`) | —             |
| 6   | Play Single Shot + screenshot       | done (`BobProgressCapture`)       | —             |

Prompts 5–6 are **vNext polish** — do not block Week 1 gate.

---

## vNext (after MVP Done)

- [x] Session plot → `docs/results/training_progress.png` (from existing CSV; regenerate after bob-v2 run)
- [ ] **Phase 3 — extended bob-v2 training run** (5+ min @ 20×) + refresh plot
- [ ] **Training GIF** for portfolio (`./scripts/capture-progress.sh --play`)
- [ ] Follow-up PR — validator + pytest + reward shaping → `main`
- [ ] Terraform bootstrap + dev apply + CloudFront URL
- [ ] Prompts 1–2, 4–6 Play verification + hero screenshot (`docs/TrainingView_Success.png`)

---

## Last run log

| Date       | train.sh listening | BOB_TRAINING_OK | Episodes moved    | Notes                                                      |
| ---------- | ------------------ | --------------- | ----------------- | ---------------------------------------------------------- |
| 2026-06-23 | yes                | yes             | yes (iter 99→103) | Step 5000/10000; score #1; pre–launch-shaping policy       |
| 2026-06-23 | —                  | —               | —                 | `training_progress.png` generated from 326-row session CSV |
| 2026-06-23 | —                  | —               | —                 | validate fixed (TextMesh batch creation in speech builder) + 32/32 + VALIDATE_PASS re-confirmed |

---

## MVP Element Review — Granular Status (Done Detector cross-check, 2026-06-23)

**Basis**: `docs/what-finished-looks-like.md` finished components table + core loop + scoreboard vars + phases; `visual-vision.md` non-negotiables + lab definition; tracker Week 1 gate + Arc prompts; `BobAgent`, arena builders, validators, stats/HUD, layers, scripts, `test_unity_alignment.py` (32), CSV. Full details + remediation steps in the accompanying plan session file.

### Bob Agent
- Orange cube + free-throw line + shoot each iter to 1 hoop: **Done** (BobAgent.cs + spawn + impulse).
- Behavior `Bob` + 8 obs/3 act match yaml: **Done** (enforced in validator + tests + config).
- Reward shaping (launch dir + arc + flight + made + oob): **Code done** (ApplyLaunch... etc.); **Gap**: pre-shaping artifacts only (see below).
- Episode flow + End: **Done**.

### Projectile (1.5)
- Separate basketball rigidbody + launcher cube kinematic: **Done** (BasketballProjectileSetup + SimpleBasketball + wiring in builders/validator asserts exactly 1 + wired).

### Goal/Hoop
- Exactly 1 active HoopScoreZone + scoring hoop: **Done** (enforced ==1 in both validator paths; Movable stationary; rim segs + swish).
- Decor hoops stripped/hidden: **Done** (builder StripBackground + HideExtra + cleanup + validator extraDecorative==0 FAIL).

### Decoration/Physics
- No collision decor vs Bob/ball: **Done** (Bob/TrainingArena/Decoration layers + Ignore matrix + SetLayer in builders + validator asserts).

### Scoreboard + Stats (canonical)
- All fields (Iterations/Total, Score/BasketballPoints, Rewards, Penalties, Net, Session+Rolling Success, Arc last+avg): **Done** (BobTrainingStats is single source; wall HUD + fallback display all; CSV logs all).
- Note: UI label="Episodes"; some docs say "Iterations" (cosmetic).
- **Robustness hardened (2026-06-23)**: HoopScoreZone now always records BasketballPoints on make (decoupled from ArcAcademyManager); SimpleFreeThrowSetup ensures required MVP singletons (Stats/Monitor/Manager/Scoreboard) so minimal trainer path + VerifyMinimal are fully functional. No regression for lab path. See HoopScoreZone.RecordBasketballPointAndNotify + EnsureCoreMvpComponents.

### Success Graph
- Rolling success + arc quality visible: **Done** (wall HUD raster dual-graph in lab; legacy BobTrainingSuccessGraph OnGUI fallback hidden in lab; Stats rolling compute).

### Feedback
- +1 score + speech/popup/face/pulse on make: **Done** (Manager.Notify + Agent.Register + BobSpeech + Popup + expressions).

### Training + Audience UI
- train.sh + Play → BOB_OK + wall updates + 20x + not-TB-primary: **Scripts+monitors+UI done**; handshake works (per last run log).
- Episodes/CSV advance + score: verified in recent runs.

### Arc Academy Lab (visual MVP)
- Corner lab + 1 hoop + grid + wall HUD south + sideline cam + no clutter: **Done** (SimpleArc builder + cleanup + preset + captures 015-020).
- Bob eyes/face/speech/pulse + char: **Done**.
- Camera orbit + LabHero: **Done**.

### Repro / CI / Validation
- validate.sh → PASS, pytest 32/32, yaml↔code match: **Done** (per claims + file assertions).
- TF/CI/Docker: scaffolded + passing (deploy pending).

### Artifacts / Demo
- Plot/CSV: present but pre-shaping (326 iter example + recent low-score runs).
- GIF + live demo: scaffold only.

**Open Gaps for Finalization** (consolidated from bob-done-tracker, what-finished-looks-like, project-plan, Unity AI fallback inspection + previous Done Detector):
1. **Primary (Learning Demo - Phase 3)**: No extended `RUN_ID=bob-v2` (or v3) training run post launch-direction shaping + refreshed `docs/results/training_progress.png` showing clear rising success/arc quality (current CSV has makes but low % rates and resets; high arc but needs PPO improvement visible).
2. **Training Portfolio Artifact**: No GIF from good policy via `./scripts/capture-progress.sh --play` (hero for docs/portfolio-site).
3. **Branch/Merge Hygiene**: Feature work (incl. recent HoopScoreZone scoring robustness + EnsureCoreMvp + shaping) not on `main` (main may have prefab drift causing validate fail).
4. **Visual Play Verifications** (Arc Training prompts): 1 (Bob eyes+follow), 2 (CameraRig+orbit F1), 4 (HDRP silver rim + net) unchecked. + hero screenshot.
5. **Phase 4 Publish**: Terraform bootstrap + dev apply, CloudFront live URL, full `docs/portfolio-site/` sync + README update.
6. **Minimal Trainer Full E2E**: Post-Ensure, pure stripped minimal path needs live Play verification (scoring works via new helper, but manager wiring/feedback/popup incomplete; spawn via fallback). Suggestion from Unity proxy: enhance Ensure to wire minimal popup.
7. **Testing Gaps**: Limited runtime tests (EditMode for agent scoring/rewards/obs, PlayMode make→+1 points, training integration). See testing-strategy.md.
8. **Success Graph in Minimal**: Not ensured in EnsureCoreMvpComponents (legacy scoreboard present, but full rolling graph for MVP UI missing in stripped path).
9. **Doc/Artifact Freshness + Drift**: Stale plot vs latest CSV; PROJECT.md / plans "Build Status" / "Next Actions" may need refresh post recent changes.
10. **Bob Function Suggestion (from inspection)**: Add inference helper on BobAgent for easy .onnx/BehaviorType.Inference demo (optional Phase 3). Consider PlayMode test asserting BasketballPoints increments.

**Unity AI proxy note**: All enforced invariants (Behavior "Bob", 1x HoopScoreZone, 1x wired Basketball, Stats, layers, projectile) PASS via validator. No new critical code gaps in core function. Remaining are demonstration, publish, and polish for "Bob MVP + demo-ready".

**Next to close MVP**: Focus 1+2 (run + GIF), then merge (3), then publish (5). Use unity-mcp live once Editor+bridge active for final hierarchy confirmation before any scene tweaks.

**Review-time discovery/fix (2026-06-23)**: validate-scene.sh initially aborted in SimpleArcAcademyArenaBuilder.EnsureSpeechBubbleText (MissingComponent on TextMesh set during batch creation of "BubbleText"; no font + possible missing-script state from prior saves). Added defensive RemoveMonoBehavioursWithMissingScript + font builtin fallback + explicit Get/Add in builder. Re-ran → **VALIDATE_PASS**. (This was blocking full MVP repro; now closed. No behavior change for runtime.)

**Verdict update proposal**: Core code loop MVP-complete. "Full functionality" requires the run+artifact+merge steps above before claiming "Bob learns" demo-ready.

## Unity AI Assistant Collaboration (MCP target: unity-mcp)
**Attempted direct consultation**: unity-mcp configured in `.cursor/mcp.json` and `scripts/unity-mcp.sh` (stdio relay to ~/.unity/relay → Unity Editor bridge). Per user guidance, targeted as primary for live Editor state.

**MCP wiring fixes applied (2026-06-23)**: 
- Fixed `.cursor/mcp.json` from absolute user paths (`/Users/.../scripts/...`) to relative `./scripts/...` (matches docs/unity-mcp.md examples and makes portable across clones/environments).
- Ensured `chmod +x` on unity-mcp.sh and rag-mcp.sh.
- Re-indexed RAG after changes.
- This allows proper loading/visibility of `unity-mcp` (and bob-rag) tools in Cursor when relay/Unity active.
- Added `BobTrainingSuccessGraph` to minimal ensure in `SimpleFreeThrowSetup.EnsureCoreMvpComponents` for full MVP success graph in stripped trainer path (consistent with what-finished and other validator paths).
- Health: validate PASS + 32/32 post changes.

**Result**: unity-mcp tools (manage_scene, find_gameobjects, manage_components, read_console, bob_open_training_scene, bob_setup_simple_arena) not active in current session (relay/bridge requires running Unity Editor with "Running" status and Cursor approved in Project Settings → AI → Unity MCP). Per AGENTS.md and docs/unity-mcp.md, fell back to equivalent batch CLI inspection (`./scripts/validate-scene.sh`, pytest alignment) + static code "hierarchy" query (grep/read for invariants the MCP would return via get_hierarchy / find_gameobjects / manage_components).

**"Unity AI" findings (batch + code proxy for live state)**:
- Active scene: BobTraining.unity (Simple Arc Academy or minimal path).
- Bob: BehaviorParameters.BehaviorName == "Bob", VectorObservationSize=8, NumContinuousActions=3 (enforced).
- Goal: Exactly 1 HoopScoreZone on active rim (multiple validator paths).
- Projectile (1.5): Exactly 1 SimpleBasketball, wired to BobAgent.ProjectileBody (validator + BasketballProjectileSetup).
- Stats/Scoreboard: BobTrainingStats present; RecordBasketballPoint now unconditionally called from HoopScoreZone.RecordBasketballPointAndNotify on every make (decoupled robustness fix). Legacy BobTrainingScoreboard ensured in minimal.
- Layers/Physics: Configured, ignore matrix for Decoration, Bob on correct layer.
- Manager/Connection: ArcAcademyManager + BobTrainingConnectionMonitor + BobTrainingScoreboard now ensured in SimpleFreeThrowSetup.ApplyAll for minimal path (satisfies VerifyMinimal).
- Validator: Full PASS (no FAILs on above).
- Recent scoring fix confirmed in source (Hoop always records before Notify/Register).

**Suggestions from inspection for Bob function**:
- Minimal path: Manager is present but not fully wired (no scorePopup, spawn refs from stripped setup) → rich feedback (popup/speech) skips, but core point/RL/stats now work. Suggestion: Enhance EnsureCoreMvpComponents to instantiate minimal ArcAcademyScorePopup + wire to manager for complete MVP feedback in stripped trainer.
- Add BobTrainingSuccessGraph ensure in minimal (for full rolling graph in legacy UI).
- Bob function: Expose clean public method e.g. `SetInferenceOnly()` or BehaviorType toggle for easy portfolio .onnx demo without trainer (Phase 3 stretch).
- To prove "learns": Current runs show high arc quality but low make rate — needs extended post-shaping PPO run to demonstrate rising SessionSuccessRate / Rolling in plot/HUD.
- General: Add PlayMode test for make → +1 BasketballPoints (beyond alignment).

**Further development required for live unity-mcp**: Open Unity Editor on Bob project, ensure bridge Running, approve client, restart Cursor/MCP. Then re-query with manage_scene:get_active + get_hierarchy, find_gameobjects("Bob"), manage_components on Behavior/HoopScoreZone/Stats for exact live snapshot.

---

## Final Gaps for MVP Status Finalization (Updated Post-Inspection)

## Quick commands

```bash
bash ./scripts/validate-scene.sh          # Unity closed
cd python && pytest tests/test_unity_alignment.py -q
lsof -i :5004                             # port check
./scripts/train.sh                        # trainer (Play after listen)
RUN_ID=bob-v3 ./scripts/train.sh --force  # after launch-direction penalty tune
```
