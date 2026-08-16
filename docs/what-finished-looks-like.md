# What Finished Looks Like — Bob Product Definition

**Audience:** Team, agents, reviewers — runtime behavior and training UX when the project is **done** (MVP + demo-ready).
**Visual style:** [docs/design/visual-vision.md](design/visual-vision.md) (Arc Academy Lab, AI Warehouse–inspired).
**Dev workflow:** [what-right-looks-like.md](what-right-looks-like.md) (PRs, CI, weeks).

---

## Finished experience (30-second summary)

Press **Play** after **Bob → Demo → Prepare Classmate Showcase**. You see a **clean training lab**, an **orange cube (Bob)** at the line, and **one hoop**. The HUD chip reads **INFERENCE · Bob** (green) — not SOLVER. Each **iteration**, Bob shoots. A make is **+1 basketball point** only if the ball **falls through** the rim. The v4.8 ONNX policy makes about **one in three**. Over training history, residual PPO moved honest success **0.5% → 34.5%**. Cumulative **RL rewards** and **penalties** stay on the wall board. Decorative geometry never scores.

The **solver wow** path (HeuristicOnly, chip **SOLVER**) is a separate demo of the environment. Do not mix the two.

---

## Core loop

```mermaid
flowchart TD
  start[Episode begin] --> spawn[Bob at spawn / ball ready]
  spawn --> act[ML-Agents actions: aim + shoot]
  act --> flight[Ball travels toward hoop]
  flight --> score{Through hoop?}
  score -->|Yes| point[+1 basketball point + RL reward + EndEpisode]
  score -->|No| miss[Terminal miss proximity + EndEpisode]
  miss -->|OOB / timeout| end[EndEpisode]
  point --> boards[Update scoreboards + success graph]
  miss --> boards
  end --> boards
  boards --> start
```

---

## Finished components

| Component         | Finished behavior                                                                                        | Current status                                                                                                         |
| ----------------- | -------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| **Agent**         | Orange cube launcher; Behavior Name `Bob`; learns via PPO                                                | Implemented (`BobAgent`)                                                                                               |
| **Projectile**    | Basketball rigidbody shot from spawn toward hoop                                                         | Implemented — `BasketballProjectileSetup` + single `Basketball` in simple arena                                        |
| **Goal**          | Exactly **one** active `HoopScoreZone`                                                                   | Implemented + validated                                                                                                |
| **Decoration**    | Bays/walls optional; **no collision** with Bob/ball                                                      | Physics layers implemented                                                                                             |
| **Scoreboard**    | In-scene panels: **iterations**, **score**, **cumulative rewards**, **cumulative penalties**, **net RL** | World-space wall HUD when simple arena active; OnGUI fallback for warehouse                                            |
| **Success graph** | Rolling **success rate %** + **arc quality** over recent iterations                                      | Wall HUD dual graph + `BobTrainingSuccessGraph` fallback                                                               |
| **Feedback**      | Speech bubble / popup on made basket                                                                     | Implemented (`BobSpeechBubble` + `ArcAcademyScorePopup`)                                                               |
| **Training**      | `./scripts/train.sh` + Play; visible **rising success rate**                                             | Handshake verified; **bob-v4.8-tight-prior 34.5%** trail-1k — [training-chronicle.md](design/training-chronicle.md) |
| **Mode chip**     | HUD names InferenceOnly / Heuristic solver / live PPO / Default-no-trainer                               | Implemented (`BobShowcaseMode`) — never say "Inference fallback"                           |
| **Portfolio**     | Story page + optional 3–4 min video                                                                      | [`story.html`](portfolio-site/story.html) + [`showcase-capture.md`](showcase-capture.md)   |

---

## Scoreboard variables (canonical)

All values come from [`BobTrainingStats`](../Assets/Scripts/BobTrainingStats.cs):

| Display label       | Field                      | Meaning                                       |
| ------------------- | -------------------------- | --------------------------------------------- |
| **Iterations**      | `TotalIterations`          | ML-Agents episodes (shot attempts)            |
| **Score**           | `BasketballPoints`         | Made baskets (+1 each, basketball rules)      |
| **Rewards**         | `TotalRewards`             | Sum of positive RL `AddReward` values         |
| **Penalties**       | `TotalPenalties`           | Sum of negative RL magnitudes                 |
| **Net RL**          | `NetSessionReward`         | Rewards − penalties                           |
| **Success rate**    | `SessionSuccessRate`       | `BasketballPoints / TotalIterations` (0–100%) |
| **Rolling success** | `RollingSuccessRate`       | Recent-window rate for the graph              |
| **Arc quality**     | `RollingAverageArcQuality` | Recent-window peak arc quality (0–100%)       |

Session rows append to `summaries/bob_session.csv` via [`BobTrainingSessionLog`](../Assets/Scripts/BobTrainingSessionLog.cs) for offline plots.

TensorBoard remains a **developer** tool (`tensorboard --logdir results`) — not the audience-facing progress UI.

---

## Development workflow — actions to ship

Work on `feature/*` → PR → green CI. See [visual-vision.md](design/visual-vision.md) for visual phases.

### Phase 1 — Training loop

- [x] `./scripts/validate-scene.sh` → `VALIDATE_PASS`
- [x] `./scripts/train.sh` → Play → training steps in console (`BOB_TRAINING_OK`)
- [x] Scoreboard + success graph update in Play
- [x] PR #7 merge to `main`

### Phase 1.5 — Basketball projectile

- [x] `Basketball` at spawn release point (orange sphere, `Rigidbody`, `SimpleBasketball`)
- [x] `BobAgent` applies force to ball; launcher cube kinematic at pad (`BasketballProjectileSetup`)
- [x] `./scripts/validate-scene.sh` → `VALIDATE_PASS` with projectile wired
- [x] `./scripts/train.sh` + Play → single-shot training loop verified
- [x] `HoopScoreZone` detects ball via `SimpleBasketball` (8 obs / 3 actions unchanged)
- [x] Validator + alignment tests (32/32)

### Phase 2 — Arc Academy Lab visuals

- [x] Lab room builder (grid floor, white walls, sideline `LabHero` camera)
- [x] Wall-mounted training HUD (`BobWallTrainingHud` on `Wall_South`, back wall behind hoop)
- [x] Near-Bob floating hero board (`BobNearBobTrainingHud`)
- [x] Bob eyes + speech bubble + squash/stretch + power-path pulse
- [x] Audio feedback (`BobAudioFeedback` bounce/swish/score/miss)
- [x] `--play` captures: `arc-academy-lab-incremental-v1`, `arc-academy-ball-v1`, `arc-academy-lab-ux-v1`

### Phase 3 — Learning demo

- [x] Session CSV export + `python/scripts/plot_training_progress.py`
- [x] Plot copied to `docs/results/training_progress.png` (bob-v2 segment, 2026-06-24)
- [x] Extended **bob-v2** training run after launch-direction rewards + refresh plot
- [x] Inference demo menus (`Bob → Demo → Prepare Classmate Showcase` / `Prepare Solver Wow`)
- [x] Training GIF scaffold — `docs/progress/023-training-gif/` (re-capture after bob-v4)
- [x] **ML Tier 1** — shot-resolved episodes, terminal miss proximity, gate per-step dist penalty ([ml-training-recommendations.md](design/ml-training-recommendations.md))
- [x] **`RUN_ID=bob-v4`** through **v4.8** — rolling success **0.5% → 34.5%** (70%/1k deferred)
- [x] **ML Tier 2** — residual hybrid + BC makes + hoop curriculum (v4.6–v4.8)

### Phase 4 — Publish

- [ ] Terraform bootstrap + dev apply (**out of scope** — no AWS hosting for Bob)
- [x] Portfolio site (`docs/portfolio-site/index.html` + `story.html`) — honest 0.5% → 34.5% arc
- [x] Capture recipe — [`showcase-capture.md`](showcase-capture.md)
- [ ] Record classmate QuickTime (local Unity) and link from `story.html`
- [ ] CloudFront live demo URL in README (**not planned**)

---

## Agent rules (for Cursor / `AGENTS.md`)

1. **Do not** scope photoreal warehouse as default — [visual-vision.md](design/visual-vision.md) Lab is primary.
2. **Do not** add second scoring hoop or change Behavior Name from `Bob` without YAML + validator updates.
3. **Do** keep scoreboard metrics in sync with `BobTrainingStats` — single source of truth.
4. **Do** read [ml-training-recommendations.md](design/ml-training-recommendations.md) before reward/obs/training changes.
5. **Query** `bob-rag` before code; **Unity MCP** before scene edits.

---

## Related

- [**ML training recommendations**](design/ml-training-recommendations.md) — bob-v4 reward/episode fixes
- [**AI Warehouse ops**](design/ai-warehouse-ops.md) — training patterns + log anomaly guide
- [PROJECT.md](../PROJECT.md) — status
- [docs/project-plan.md](project-plan.md) — milestones
- [AGENTS.md](../AGENTS.md) — agent instructions
