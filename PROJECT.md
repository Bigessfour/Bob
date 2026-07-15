# Bob — Free Throw RL Agent

> **North Star:** [What Right Looks Like](docs/what-right-looks-like.md) (workflow) · [What Finished Looks Like](docs/what-finished-looks-like.md) (product) · [Visual vision](docs/design/visual-vision.md) (look)

**Status:** Phase 3–4 finish — dual HUD + commercial polish path ([docs/bob-done-tracker.md](docs/bob-done-tracker.md))  
**Branch:** `feature/dual-hud-scoreboard`  
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio  
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + Terraform + GitHub Actions

---

## Current Milestone

**Phase 3 — Learning demo + readable scoreboards**

MVP training loop and Arc Academy Lab are on **`main`**. Current work: **near-Bob floating hero board** + Wall_South RL console, bob-v3 visible learning, audio juice, training GIF, portfolio publish. Behavior Name **`Bob`**, **8** obs, **3** actions unchanged.

**Product definition:** [docs/what-finished-looks-like.md](docs/what-finished-looks-like.md)

## Build Status (2026-07-14)

| Area                | Status                                                                                          |
| ------------------- | ----------------------------------------------------------------------------------------------- |
| Unity scene         | `BobTraining.unity` — dual HUD: `NearBobTrainingHud` + Wall_South lab console                   |
| Scene rebuild       | **Bob → Polish → Fix Bob Lab Visuals** rebuilds wall + near-Bob boards                          |
| Visual north star   | [`docs/design/visual-vision.md`](docs/design/visual-vision.md) — float primary, wall graph/RL   |
| Render pipeline     | HDRP 17 — flat/lab materials; **no WebGL**                                                      |
| Scene validation    | `./scripts/validate-scene.sh` → **VALIDATE_PASS** (re-run after HUD bake)                       |
| Offline regression  | `pytest tests/test_unity_alignment.py` — **35/35**                                              |
| Training runtime    | 1 Bob launcher + 1 basketball; dual HUD + session CSV; `BobTrainingSessionRunner` batchmode     |
| Phase 3 training    | **bob-v2** done; **bob-v3** extended run + plot refresh in progress                             |
| Hoop + ball physics | Segmented rim colliders, visual net, single-shot impulse — `TrainingHoopDetail`                 |
| Progress gallery    | [`docs/progress/`](docs/progress/) — through `022-lab-hero-v2`; `docs/TrainingView_Success.png` |

## Next Actions

**Live checklist:** [docs/bob-done-tracker.md](docs/bob-done-tracker.md)

1. Merge dual-HUD PR → `main`
2. **bob-v3** extended train + refresh `docs/results/training_progress.png` + inference `.onnx` demo
3. Audio juice + training GIF + portfolio CloudFront (non-AICO profile)

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

| Date       | Update                                                                                      |
| ---------- | ------------------------------------------------------------------------------------------- |
| 2026-06-18 | Initial repo scaffold pushed to `main`                                                      |
| 2026-06-18 | DevOps foundations added (Terraform, CI, Docker, Cursor config)                             |
| 2026-06-18 | Dev environment locked (Python 3.10.12, CI green)                                           |
| 2026-06-18 | Baseline testing strategy + pytest suite added                                              |
| 2026-06-18 | Unity project + BobAgent + training scene builder added                                     |
| 2026-06-18 | Progress screenshot workflow — `docs/progress/` gallery + `capture-progress.sh`             |
| 2026-06-18 | Repository RAG — ChromaDB index, `bob-rag` MCP, Cursor hooks                                |
| 2026-06-22 | **PR #7** — Simple Arc Academy, basketball projectile, wall HUD, Bob charisma, power pulse  |
| 2026-06-22 | Session CSV log + `plot_training_progress.py`; portfolio site scaffold                      |
| 2026-06-18 | Unity MCP — agent consultation rules, bob-rag integration                                   |
| 2026-06-18 | North Star — `docs/what-right-looks-like.md` pinned in PROJECT, AGENTS, project-plan        |
| 2026-06-18 | **PR #3** — HDRP Arc Academy photoreal rebuild; WebGL removed (HDRP incompatible)           |
| 2026-06-19 | Product north star — `docs/what-finished-looks-like.md`; success graph + scoreboard metrics |
