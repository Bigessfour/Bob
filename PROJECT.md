# Bob — Free Throw RL Agent

> **North Star:** [What Right Looks Like](docs/what-right-looks-like.md) (workflow) · [What Finished Looks Like](docs/what-finished-looks-like.md) (product) · [Visual vision](docs/design/visual-vision.md) (look)

**Status:** **Portfolio complete (v4.8)** — visible learning **0.5% → 34.5%**; inference validated **~35%** in Play
**Branch:** `feature/bob-v4.1-local-impulse-sign` (PR pending merge)
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + GitHub Actions (+ Terraform CI validate only; **no AWS hosting**)

---

## Current Milestone

**Phase 3–4 — Portfolio demo complete**

MVP loop + Arc Academy Lab + dual HUD + **bob-v4.8 PPO** (`Assets/Models/Bob.onnx`). Behavior Name **`Bob`**, **13** obs, **3** actions (residual hybrid). See [ml-project-timeline.md](docs/design/ml-project-timeline.md).

**Product definition:** [docs/what-finished-looks-like.md](docs/what-finished-looks-like.md)

## Build Status (2026-07-20)

| Area               | Status                                                                    |
| ------------------ | ------------------------------------------------------------------------- |
| Unity scene        | `BobTraining.unity` — dual HUD + v4.8 trained policy                      |
| Scene validation   | `./scripts/validate-scene.sh` → **VALIDATE_PASS**                         |
| Offline regression | `pytest tests/test_unity_alignment.py` — **40/40**                        |
| PPO training peak  | **bob-v4.8-tight-prior** — **34.5%** trailing-1k; inference Play **~35%** |
| Portfolio          | `docs/portfolio-site/` + progress captures `025`/`026` + ML timeline PNGs |
| ML chronicle       | [docs/design/training-chronicle.md](docs/design/training-chronicle.md)    |

## Next Actions (vNext — optional)

**Live checklist:** [docs/bob-done-tracker.md](docs/bob-done-tracker.md)

| P   | Action                                               | Status       |
| --- | ---------------------------------------------------- | ------------ |
| 1   | Merge feature branch → `main`                        | This PR      |
| 2   | Record 5–8 min portfolio video (`docs/demo-package`) | Optional     |
| 3   | v4.9 tighter residual toward 70%/1k                  | Deferred     |
| 4   | Standalone build + Recorder hero video               | Deferred     |
| 5   | AWS / CloudFront hosting                             | Out of scope |

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
