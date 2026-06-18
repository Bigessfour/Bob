# Bob — Free Throw RL Agent

> **North Star:** [What Right Looks Like](docs/what-right-looks-like.md) — canonical milestone + workflow diagrams. Read before planning or merging; every change must align with the current week and permanent quality bars.

**Status:** Week 1 — Basic Agent + HDRP Arc Academy ([PR #3](https://github.com/Bigessfour/Bob/pull/3))  
**Branch:** `feature/hdrp-arc-academy-visual`  
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio  
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + Terraform + GitHub Actions

---

## Current Milestone

**Week 1 — Setup + Basic Agent**

Foundations on **`main`** plus **HDRP Arc Academy photoreal pass** on feature branch: warehouse interior, 8 training bays with robotic launcher placeholders, single active scoring hoop, Volume/APV lighting. Visual target: [`docs/design/arc-academy-reference.jpg`](docs/design/arc-academy-reference.jpg). Behavior Name **`Bob`**, **8** obs, **3** actions unchanged.

**Week 1 exit criterion:** first end-to-end training run (handshake proven locally; extend run for TensorBoard).

## Build Status (2026-06-18)

| Area                | Status                                                                           |
| ------------------- | -------------------------------------------------------------------------------- |
| Unity scene         | `BobTraining.unity` — HDRP Arc Academy rebuild (Example.jpg target)              |
| Render pipeline     | HDRP 17 + Volume (Bloom/SSR) + APV — **no WebGL** (portfolio static site Week 3) |
| Scene validation    | `./scripts/validate-scene.sh` → **VALIDATE_PASS**                                  |
| Offline regression  | `pytest tests/test_unity_alignment.py` — **16/16**                                   |
| Design reference    | [`docs/design/arc-academy-reference.jpg`](docs/design/arc-academy-reference.jpg)   |
| Arc Academy runtime | 8 bays, 1 active hoop, robotic launcher visuals, trajectory arcs                     |
| First training run  | Use `./scripts/train.sh` → Press Play after port 5004 (`--service-ports` required)   |
| Progress gallery    | [`docs/progress/006-arc-academy-hdrp-v1/`](docs/progress/)                          |
| Git                 | **PR #3** — HDRP Arc Academy + WebGL removal                                       |

## Next Actions

1. **Open Unity** → `Assets/Scenes/BobTraining.unity` — confirm Arc Academy layout vs design reference
2. **First training run (Week 1 gate)** — `./scripts/train.sh --timeout-wait=120 --time-scale=20 --force`, Press Play after port 5004
3. **Week 2** — reward tuning, TensorBoard, training GIFs (after loop proven)

## Links

| Resource                | Location                                                         |
| ----------------------- | ---------------------------------------------------------------- |
| Live demo               | _Coming soon — CloudFront portfolio site (static; not WebGL)_        |
| CI workflow             | [`.github/workflows/ci.yml`](.github/workflows/ci.yml)           |
| Testing strategy        | [`docs/testing-strategy.md`](docs/testing-strategy.md)           |
| Unity dev guide         | [`docs/unity-dev.md`](docs/unity-dev.md)                         |
| Unity MCP (Editor)      | [`docs/unity-mcp.md`](docs/unity-mcp.md)                         |
| Build progress gallery  | [`docs/progress/`](docs/progress/)                               |
| Repository RAG          | [`docs/rag.md`](docs/rag.md)                                     |
| Terraform               | [`terraform/README.md`](terraform/README.md)                     |
| Agent rules             | [`AGENTS.md`](AGENTS.md)                                         |
| Setup guide             | [`docs/setup-checklist.md`](docs/setup-checklist.md)             |
| Project plan            | [`docs/project-plan.md`](docs/project-plan.md)                   |
| **North Star diagrams** | [`docs/what-right-looks-like.md`](docs/what-right-looks-like.md) |

## DevOps Status

| Component                          | Status                                                                                            |
| ---------------------------------- | ------------------------------------------------------------------------------------------------- |
| Terraform bootstrap (state bucket) | Scaffolded — not yet applied                                                                      |
| Terraform dev (S3 + CloudFront)    | Scaffolded — not yet applied                                                                      |
| GitHub Actions CI                  | pytest + Terraform validate + tflint + Docker build                                               |
| Docker training image              | Built locally (`bob-train:latest`)                                                                |
| Baseline pytest suite              | `python/tests/` — config + plot_rewards + unity alignment + RAG                                   |
| Repository RAG (ChromaDB + MCP)    | `python/rag/` — query before code edits; `./scripts/rag-index.sh`                                 |
| Unity MCP (`unityMCP`)             | CoplayDev MCP for Unity — consult before scene/agent work; [docs/unity-mcp.md](docs/unity-mcp.md) |
| Unity project                      | `Assets/`, `ProjectSettings/`, `Packages/` at repo root                                           |
| ML-Agents                          | `com.unity.ml-agents` 4.0.3 + `com.unity.ai.inference` 2.2.1                                      |
| Portfolio site deploy              | Week 3 — S3 + CloudFront static HTML (GIFs, gallery, write-up)                                  |

## Update Log

| Date       | Update                                                                               |
| ---------- | ------------------------------------------------------------------------------------ |
| 2026-06-18 | Initial repo scaffold pushed to `main`                                               |
| 2026-06-18 | DevOps foundations added (Terraform, CI, Docker, Cursor config)                      |
| 2026-06-18 | Dev environment locked (Python 3.10.12, CI green)                                    |
| 2026-06-18 | Baseline testing strategy + pytest suite added                                       |
| 2026-06-18 | Unity project + BobAgent + training scene builder added                              |
| 2026-06-18 | Progress screenshot workflow — `docs/progress/` gallery + `capture-progress.sh`      |
| 2026-06-18 | Repository RAG — ChromaDB index, `bob-rag` MCP, Cursor hooks                         |
| 2026-06-18 | Unity MCP — `com.coplaydev.unity-mcp`, `bob-unity` MCP, agent consultation rules     |
| 2026-06-18 | North Star — `docs/what-right-looks-like.md` pinned in PROJECT, AGENTS, project-plan |
| 2026-06-18 | **PR #3** — HDRP Arc Academy photoreal rebuild; WebGL removed (HDRP incompatible)    |
