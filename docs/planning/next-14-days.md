# Bob — Next 14 Days (Priority Stack)

**Captured:** 2026-07-14 (bedtime handoff) · **Updated:** 2026-07-14 — **no AWS hosting** (Terraform in repo is CI validate only)
**Branch:** `feature/dual-hud-scoreboard` · **PR:** [#10](https://github.com/Bigessfour/Bob/pull/10)
**Live status:** [bob-done-tracker.md](../bob-done-tracker.md) · **ML detail:** [ml-training-recommendations.md](../design/ml-training-recommendations.md)

Agents: read this before proposing scope. Do not start lower-priority work until Priority **1** ships (unless the user explicitly reprioritizes).

**Hosting decision:** Bob is **not** deployed to AWS. Do not plan S3, CloudFront, or `terraform apply` for this project. The `terraform/` folder remains for **CI fmt/validate** only.

---

## Repo snapshot (2026-07-14)

| Area                 | Status                                                                               |
| -------------------- | ------------------------------------------------------------------------------------ |
| `main`               | Healthy — CI green via `.github/workflows/ci.yml`                                    |
| MVP handshake        | Works — `./scripts/train.sh` + Play → `BOB_TRAINING_OK`                              |
| Dual HUD + audio     | Shipped on PR #10                                                                    |
| Learning demo        | **Blocked** — bob-v2/v3: ~67–74% arc, **0% makes**, net RL negative                  |
| Production bar **I** | **Not met** — no standalone builds or automated hero video pipeline yet              |
| Portfolio            | Static write-up in `docs/portfolio-site/` + README links (no hosted deploy required) |
| Agent skills         | `/bob-ml-agents-train`, `/bob-unity-mcp` in `.cursor/skills/`                        |

---

## Priority stack (7–14 days)

| P     | Skill / area                                   | Why it moves the needle                              | Quick win                                                                                                                                                                                       | Est.    | **Status**                                     |
| ----- | ---------------------------------------------- | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- | ---------------------------------------------- |
| **1** | **ML-Agents Tier 1 reward / episode design**   | bob-v4 learning is the #1 blocker                    | Read [ml-training-recommendations.md](../design/ml-training-recommendations.md) → implement shot-resolved `EndEpisode()`, terminal miss proximity, gated per-step dist penalty in `BobAgent.cs` | 2–4 hr  | **Done (code)** — train bob-v4 next            |
| **2** | **Unity production builds + Recorder**         | Bar **I** — no standalone build or hero video yet    | Add `com.unity.recorder` → `scripts/build-standalone.sh` + `scripts/capture-hero-video.sh` (Apple Silicon native)                                                                               | 1–2 hr  | **Partial** — scripts added; run build locally |
| **3** | **C# state machine + juice systems**           | Cuphead-level polish (menus, pause, onboarding, VFX) | `BobGameStateMachine.cs` + post-process volume + particle systems                                                                                                                               | 3–5 hr  | **Partial** — state machine scaffold; VFX TBD  |
| **4** | **Automated release checklist + CI extension** | Prevent regression before merge                      | Extend `ci.yml` + add `./scripts/release-checklist.sh` (build + capture smoke)                                                                                                                  | 1 hr    | **Partial** — checklist script + CI note       |
| **5** | **Cursor advanced workflow**                   | Leverage 3.8.23 fully                                | Composer multi-file, `@bob-rag`, inline `validate-scene.sh`, `.cursor/rules/bob.mdc`                                                                                                            | ongoing | **Partial** — rules, RAG, MCP, skills wired    |

**Execution order:** 1 → 2 → 4 → 3 (polish pass after learning proves).

---

## Priority 1 — ML Tier 1 (implementation spec)

**Current `BobAgent.cs` gap:** episodes end only on **make** (`RegisterMadeShot`) or **OOB** — long bounce phases accumulate `-0.002 × xzDist` every step.

### Required changes

1. **Shot-resolved `EndEpisode()`** — on rim pass (miss), floor contact, or `MaxStep` ~60–90 (not OOB-only).
2. **Terminal miss proximity reward** — at episode end without make: reward ∝ `1 / (1 + rimDistance)`.
3. **Gate per-step distance penalty** — apply `-0.002 × xzDist` only while ball is in flight (post-impulse, pre-resolution); not during idle bounce.

### After code lands

```bash
RUN_ID=bob-v4 ./scripts/train.sh --force
# Play ONCE after Listening on port 5004 — no C# edits until training stops
cd python && python scripts/plot_training_progress.py --output ../docs/results/training_progress.png
```

**Pass gate:** rolling success **>5%** over 30+ min @ 20×.

Skill: `/bob-ml-agents-train`

---

## Priority 2 — Builds + capture pipeline

**Deliverables (not in repo yet):**

| Artifact                    | Path / action                                                                        |
| --------------------------- | ------------------------------------------------------------------------------------ |
| Unity Recorder package      | `Packages/manifest.json` → `com.unity.recorder`                                      |
| macOS standalone build      | `scripts/build-standalone.sh` → `builds/macos/Bob.app`                               |
| Hero video / GIF automation | `scripts/capture-hero-video.sh` (Recorder or existing `capture-progress.sh` wrapper) |
| CI smoke (optional P4)      | Headless/batchmode build step or documented manual gate                              |

Existing manual path: `./scripts/capture-progress.sh --play …` → `docs/progress/`.

Portfolio: link README → `docs/portfolio-site/index.html` + progress gallery (no AWS deploy).

Skill: `/bob-unity-mcp`

---

## Priority 3 — Juice / state machine (Cuphead bar)

**Partial today:** `BobAudioFeedback`, `BobFaceExpression`, `BobProceduralAnimator`, dual HUD, entrance controller.

**Still needed:**

- `BobGameStateMachine.cs` — Training / Pause / Demo / Onboarding states
- HDRP post-process volume polish pass
- Particle systems (swish trail, rim spark, score burst)
- Menu flow for inference demo (beyond current `Bob → Demo` menus)

Defer heavy juice until Priority **1** shows visible learning.

---

## Priority 4 — Release checklist + CI

**Deliverables:**

- `scripts/release-checklist.sh` — validate-scene, pytest, optional build, capture artifact exists, no open P1 blockers
- `.github/workflows/ci.yml` — optional: macOS build job or release-checklist script step (document if Editor-required)

---

## Priority 5 — Cursor workflow (ongoing)

- **Rules:** `.cursor/rules/bob.mdc` — pin quality bar **I** from [what-right-looks-like.md](../what-right-looks-like.md)
- **RAG:** `bob-rag` before code; reindex after significant edits
- **Unity MCP:** `unity-mcp` before scene work; never during active training
- **Composer prompt template:**
  _“Implement bob-v4 Tier 1 + [scope] while respecting what-right-looks-like.md and what-finished-looks-like.md.”_

---

## Out of scope (do not plan)

| Removed                             | Reason                                                              |
| ----------------------------------- | ------------------------------------------------------------------- |
| AWS S3 + CloudFront deploy          | **Not hosting Bob on AWS** — user decision 2026-07-14               |
| `terraform apply` (bootstrap/dev)   | Terraform kept for CI validate only; no infrastructure to provision |
| Portfolio AWS profile / `aws login` | Not required for this project                                       |

---

## Missing agent capabilities (workarounds)

| Gap                                | Impact                                                      | Workaround                                                                                                        |
| ---------------------------------- | ----------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| **Direct Unity Play / scene bake** | Agents cannot press Play or run Recorder live               | User runs Play; use `unity-mcp` when bridge connected; batchmode `./scripts/validate-scene.sh` when Editor closed |
| **Headless Unity builds in CI**    | No Linux/macOS Unity runner in CI today                     | `build-standalone.sh` on Mac host; optional Docker headless later                                                 |
| **Live training monitor**          | No streaming TensorBoard/parser during `./scripts/train.sh` | Post-hoc: `summaries/bob_session.csv`, `plot_training_progress.py`, HUD in Play                                   |
| **PR auto-merge**                  | Agents create PRs; merge requires green CI + user           | `gh pr merge` after checks; never force-push `main`                                                               |

Future idea: REST/CLI hook in `scripts/` for agent-triggered validate-after-edit (low priority).

---

## Agent handoff (wake-up checklist)

1. [ ] Priority **1** — Tier 1 patch in `BobAgent.cs` (+ alignment tests if obs/reward API changes)
2. [ ] `RUN_ID=bob-v4` train uninterrupted → refresh plot
3. [ ] Priority **2** — build + capture scripts
4. [ ] Merge PR #10 (or successor) on green CI
5. [ ] README portfolio section links `docs/portfolio-site/` + latest hero/GIF
6. [ ] Update [bob-done-tracker.md](../bob-done-tracker.md) as each gate clears

---

## Related

- [AGENTS.md](../../AGENTS.md) — canonical agent rules + missing capabilities summary
- [instructions.md](../instructions.md) — quick reference
- [PROJECT.md](../../PROJECT.md) — living status
- [what-right-looks-like.md](../what-right-looks-like.md) — bar **I** (Cuphead UX)
