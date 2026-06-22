# Bob — Free Throw RL Agent

> **North Star:** [What Right Looks Like](docs/what-right-looks-like.md) (workflow) · [What Finished Looks Like](docs/what-finished-looks-like.md) (product) · [Visual vision](docs/design/visual-vision.md) (look)

**Status:** Week 1–2 — Simple Arc Academy Lab + Phase 1.5 basketball ([PR #7](https://github.com/Bigessfour/Bob/pull/7))  
**Branch:** `feature/simple-arc-academy`  
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio  
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + Terraform + GitHub Actions

---

## Current Milestone

**Week 1 — Setup + Basic Agent**

Foundations on **`main`** plus **Arc Academy Lab** visual direction (AI Warehouse–inspired training room, basketball theme). **Primary visual target:** [`docs/design/visual-vision.md`](docs/design/visual-vision.md) + [`docs/design/ai-warehouse-lab-reference.png`](docs/design/ai-warehouse-lab-reference.png). **Stretch reference:** [`docs/design/arc-academy-reference.jpg`](docs/design/arc-academy-reference.jpg). One active scoring hoop, in-scene scoreboards, physics layers for decoration. Behavior Name **`Bob`**, **8** obs, **3** actions unchanged.

**Week 1 exit criterion:** first end-to-end training run; scoreboard and success graph update each iteration in Play.

**Product definition:** [docs/what-finished-looks-like.md](docs/what-finished-looks-like.md)

## Build Status (2026-06-22)

| Area               | Status                                                                                     |
| ------------------ | ------------------------------------------------------------------------------------------ |
| Unity scene        | `BobTraining.unity` — Simple Arc Academy Lab (sideline camera, wall HUD)                  |
| Visual north star  | [`docs/design/visual-vision.md`](docs/design/visual-vision.md) — Phase 2 largely complete  |
| Render pipeline    | HDRP 17 — flat/lab materials in default mode; **no WebGL** (portfolio Week 3)              |
| Scene validation   | `./scripts/validate-scene.sh` → **VALIDATE_PASS**                                          |
| Offline regression | `pytest tests/test_unity_alignment.py` — **32/32**                                         |
| Primary reference  | [`docs/design/ai-warehouse-lab-reference.png`](docs/design/ai-warehouse-lab-reference.png) |
| Stretch reference  | [`docs/design/arc-academy-reference.jpg`](docs/design/arc-academy-reference.jpg)           |
| Training runtime   | 1 Bob launcher + 1 basketball; wall HUD + session CSV log                                  |
| First training run | **Gate remaining** — `./scripts/train.sh` → Play → `BOB_TRAINING_OK` in console            |
| Hoop + ball physics | Segmented rim colliders, visual net, single-shot impulse — `TrainingHoopDetail`            |
| Progress gallery   | [`docs/progress/`](docs/progress/) — through `017-arc-academy-lab-ux-v1`                   |

## Next Actions

1. **Week 1 gate** — `./scripts/train.sh`, Play; confirm `BOB_TRAINING_OK` and wall HUD updates
2. **Merge PR #7** — Simple Arc Academy + basketball + lab UX polish
3. **Phase 3** — extended training run; `plot_training_progress.py` → `docs/results/`
4. **Phase 4** — Terraform apply + sync `docs/portfolio-site/`

## Links

| Resource                | Location                                                               |
| ----------------------- | ---------------------------------------------------------------------- |
| Live demo               | _Coming soon — CloudFront portfolio site (static; not WebGL)_          |
| CI workflow             | [`.github/workflows/ci.yml`](.github/workflows/ci.yml)                 |
| Testing strategy        | [`docs/testing-strategy.md`](docs/testing-strategy.md)                 |
| Unity dev guide         | [`docs/unity-dev.md`](docs/unity-dev.md)                               |
| Unity MCP (Editor)      | [`docs/unity-mcp.md`](docs/unity-mcp.md)                               |
| Build progress gallery  | [`docs/progress/`](docs/progress/)                                     |
| **Product north star**  | [`docs/what-finished-looks-like.md`](docs/what-finished-looks-like.md) |
| **Visual vision**       | [`docs/design/visual-vision.md`](docs/design/visual-vision.md)         |
| Repository RAG          | [`docs/rag.md`](docs/rag.md)                                           |
| Terraform               | [`terraform/README.md`](terraform/README.md)                           |
| Agent rules             | [`AGENTS.md`](AGENTS.md)                                               |
| Setup guide             | [`docs/setup-checklist.md`](docs/setup-checklist.md)                   |
| Project plan            | [`docs/project-plan.md`](docs/project-plan.md)                         |
| **North Star diagrams** | [`docs/what-right-looks-like.md`](docs/what-right-looks-like.md)       |

## DevOps Status

| Component                          | Status                                                                                              |
| ---------------------------------- | --------------------------------------------------------------------------------------------------- |
| Terraform bootstrap (state bucket) | Scaffolded — not yet applied                                                                        |
| Terraform dev (S3 + CloudFront)    | Scaffolded — not yet applied                                                                        |
| GitHub Actions CI                  | pytest + Terraform validate + tflint + Docker build                                                 |
| Docker training image              | Built locally (`bob-train:latest`)                                                                  |
| Baseline pytest suite              | `python/tests/` — config + plot_rewards + unity alignment + RAG                                     |
| Repository RAG (ChromaDB + MCP)    | `python/rag/` — query before code edits; `./scripts/rag-index.sh`                                   |
| Unity MCP (`unity-mcp`)            | Official Unity MCP bridge — consult before scene/agent work; [docs/unity-mcp.md](docs/unity-mcp.md) |
| Unity project                      | `Assets/`, `ProjectSettings/`, `Packages/` at repo root                                             |
| ML-Agents                          | `com.unity.ml-agents` 4.0.3 + `com.unity.ai.inference` 2.2.1                                        |
| Portfolio site deploy              | Week 3 — S3 + CloudFront static HTML (GIFs, gallery, write-up)                                      |

## Update Log

| Date       | Update                                                                                                     |
| ---------- | ---------------------------------------------------------------------------------------------------------- |
| 2026-06-18 | Initial repo scaffold pushed to `main`                                                                     |
| 2026-06-18 | DevOps foundations added (Terraform, CI, Docker, Cursor config)                                            |
| 2026-06-18 | Dev environment locked (Python 3.10.12, CI green)                                                          |
| 2026-06-18 | Baseline testing strategy + pytest suite added                                                             |
| 2026-06-18 | Unity project + BobAgent + training scene builder added                                                    |
| 2026-06-18 | Progress screenshot workflow — `docs/progress/` gallery + `capture-progress.sh`                            |
| 2026-06-18 | Repository RAG — ChromaDB index, `bob-rag` MCP, Cursor hooks                                               |
| 2026-06-22 | **PR #7** — Simple Arc Academy, basketball projectile, wall HUD, Bob charisma, power pulse |
| 2026-06-22 | Session CSV log + `plot_training_progress.py`; portfolio site scaffold                     |
| 2026-06-18 | Unity MCP — agent consultation rules, bob-rag integration                                                  |
| 2026-06-18 | North Star — `docs/what-right-looks-like.md` pinned in PROJECT, AGENTS, project-plan                       |
| 2026-06-18 | **PR #3** — HDRP Arc Academy photoreal rebuild; WebGL removed (HDRP incompatible)                          |
| 2026-06-19 | Product north star — `docs/what-finished-looks-like.md`; success graph + scoreboard metrics                |
