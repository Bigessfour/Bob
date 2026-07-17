# Bob — Free Throw RL Agent

> **North Star:** [What Right Looks Like](docs/what-right-looks-like.md) (workflow) · [What Finished Looks Like](docs/what-finished-looks-like.md) (product) · [Visual vision](docs/design/visual-vision.md) (look)

**Status:** Phase 3–4 — polish on `feature/dual-hud-scoreboard` ([PR #10](https://github.com/Bigessfour/Bob/pull/10)); **learning blocked on ML Tier 1** ([ml-training-recommendations.md](docs/design/ml-training-recommendations.md))
**Branch:** `feature/dual-hud-scoreboard`
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + GitHub Actions (+ Terraform CI validate only; **no AWS hosting**)

---

## Current Milestone

**Phase 3 — Learning demo (blocked on reward redesign)**

MVP loop + Arc Academy Lab + dual HUD are implemented. **bob-v2/v3** show high arc, **0% makes** — implement [ml-training-recommendations.md](docs/design/ml-training-recommendations.md) Tier 1, then **`RUN_ID=bob-v4`**. Behavior Name **`Bob`**, **8** obs, **3** actions until obs tier ships.

**Product definition:** [docs/what-finished-looks-like.md](docs/what-finished-looks-like.md)

## Build Status (2026-07-14)

| Area               | Status                                                                                          |
| ------------------ | ----------------------------------------------------------------------------------------------- |
| Unity scene        | `BobTraining.unity` — dual HUD: `NearBobTrainingHud` + Wall_South lab console                   |
| Scene validation   | `./scripts/validate-scene.sh` → **VALIDATE_PASS**                                               |
| Offline regression | `pytest tests/test_unity_alignment.py` — **35/35**                                              |
| Phase 3 training   | **bob-v2** plot done; **bob-v3** partial (~40k steps, 0% makes) — **use bob-v4 after ML fixes** |
| ML evaluation      | [docs/design/ml-training-recommendations.md](docs/design/ml-training-recommendations.md)        |
| Progress gallery   | Hero `023-dual-hud-hero` + GIF `023-training-gif`                                               |

## Next Actions

**Live checklist:** [docs/bob-done-tracker.md](docs/bob-done-tracker.md)
**14-day priority stack:** [docs/planning/next-14-days.md](docs/planning/next-14-days.md)

| P   | Action                                                | Status             |
| --- | ----------------------------------------------------- | ------------------ |
| 1   | ML Tier 1 in `BobAgent.cs` → `RUN_ID=bob-v4`          | Not started (code) |
| 2   | `build-standalone.sh` + Recorder + hero video scripts | Not started        |
| 3   | `BobGameStateMachine.cs` + juice pass                 | Partial            |
| 4   | `release-checklist.sh` + CI extension                 | Not started        |
| 5   | Cursor workflow (rules, RAG, skills)                  | Partial            |

1. Implement **ML Tier 1** in `BobAgent.cs` (shot-resolved episode, miss proximity)
2. **`RUN_ID=bob-v4`** train + refresh `docs/results/training_progress.png`
3. Merge [PR #10](https://github.com/Bigessfour/Bob/pull/10) → `main`
4. README + `docs/portfolio-site/` synced with latest artifacts

## Links

| Resource                | Location                                                                                                       |
| ----------------------- | -------------------------------------------------------------------------------------------------------------- |
| Portfolio write-up      | [`docs/portfolio-site/index.html`](docs/portfolio-site/index.html) (in-repo; link from README)                 |
| CI workflow             | [`.github/workflows/ci.yml`](.github/workflows/ci.yml) · [AI Review (Ollama)](.github/workflows/ai-review.yml) |
| Testing strategy        | [`docs/testing-strategy.md`](docs/testing-strategy.md)                                                         |
| Unity dev guide         | [`docs/unity-dev.md`](docs/unity-dev.md)                                                                       |
| Unity MCP (Editor)      | [`docs/unity-mcp.md`](docs/unity-mcp.md)                                                                       |
| Build progress gallery  | [`docs/progress/`](docs/progress/)                                                                             |
| **Product north star**  | [`docs/what-finished-looks-like.md`](docs/what-finished-looks-like.md)                                         |
| **Visual vision**       | [`docs/design/visual-vision.md`](docs/design/visual-vision.md)                                                 |
| Repository RAG          | [`docs/rag.md`](docs/rag.md)                                                                                   |
| Terraform (CI only)     | [`terraform/README.md`](terraform/README.md) — fmt/validate in CI; **not used for hosting**                    |
| Agent rules             | [`AGENTS.md`](AGENTS.md)                                                                                       |
| Setup guide             | [`docs/setup-checklist.md`](docs/setup-checklist.md)                                                           |
| Project plan            | [`docs/project-plan.md`](docs/project-plan.md)                                                                 |
| **North Star diagrams** | [`docs/what-right-looks-like.md`](docs/what-right-looks-like.md)                                               |

## DevOps Status

| Component                       | Status                                                                                              |
| ------------------------------- | --------------------------------------------------------------------------------------------------- |
| Terraform (CI validate only)    | `terraform/` fmt/validate in CI — **no AWS deploy for Bob**                                         |
| GitHub Actions CI               | pytest + Terraform validate + tflint + Docker build                                                 |
| Docker training image           | Built locally (`bob-train:latest`)                                                                  |
| Baseline pytest suite           | `python/tests/` — config + plot_rewards + unity alignment + RAG                                     |
| Repository RAG (ChromaDB + MCP) | `python/rag/` — query before code edits; `./scripts/rag-index.sh`                                   |
| Unity MCP (`unity-mcp`)         | Official Unity MCP bridge — consult before scene/agent work; [docs/unity-mcp.md](docs/unity-mcp.md) |
| Unity project                   | `Assets/`, `ProjectSettings/`, `Packages/` at repo root                                             |
| ML-Agents                       | `com.unity.ml-agents` 4.0.3 + `com.unity.ai.inference` 2.2.1                                        |
| Portfolio                       | Static HTML in `docs/portfolio-site/` + README links (no hosted deploy)                             |

## Update Log

| Date       | Update                                                                                                                     |
| ---------- | -------------------------------------------------------------------------------------------------------------------------- |
| 2026-06-18 | Initial repo scaffold pushed to `main`                                                                                     |
| 2026-06-18 | DevOps foundations added (Terraform, CI, Docker, Cursor config)                                                            |
| 2026-06-18 | Dev environment locked (Python 3.10.12, CI green)                                                                          |
| 2026-06-18 | Baseline testing strategy + pytest suite added                                                                             |
| 2026-06-18 | Unity project + BobAgent + training scene builder added                                                                    |
| 2026-06-18 | Progress screenshot workflow — `docs/progress/` gallery + `capture-progress.sh`                                            |
| 2026-06-18 | Repository RAG — ChromaDB index, `bob-rag` MCP, Cursor hooks                                                               |
| 2026-06-22 | **PR #7** — Simple Arc Academy, basketball projectile, wall HUD, Bob charisma, power pulse                                 |
| 2026-06-22 | Session CSV log + `plot_training_progress.py`; portfolio site scaffold                                                     |
| 2026-06-18 | Unity MCP — agent consultation rules, bob-rag integration                                                                  |
| 2026-06-18 | North Star — `docs/what-right-looks-like.md` pinned in PROJECT, AGENTS, project-plan                                       |
| 2026-06-18 | **PR #3** — HDRP Arc Academy photoreal rebuild; WebGL removed (HDRP incompatible)                                          |
| 2026-07-14 | ML evaluation — [ml-training-recommendations.md](docs/design/ml-training-recommendations.md); bob-v4 plan; dual HUD PR #10 |
| 2026-07-14 | [next-14-days.md](docs/planning/next-14-days.md) — priority stack; **AWS hosting removed from scope**                      |
