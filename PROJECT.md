# Bob — Free Throw RL Agent

> **North Star:** [What Right Looks Like](docs/what-right-looks-like.md) — canonical milestone + workflow diagrams. Read before planning or merging; every change must align with the current week and permanent quality bars.

**Status:** Week 1 — Basic Agent (training loop pending)  
**Branch:** `feature/week1-rag-mcp-north-star` — Arc Academy MVP implemented locally, not yet committed  
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio  
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + Terraform + GitHub Actions

---

## Current Milestone

**Week 1 — Setup + Basic Agent**

Foundations are in place: Unity 6 project, ML-Agents 4.0.3, CI green, RAG + Unity MCP, scene builder/validator CLI, and **Arc Academy MVP** (warehouse court, **fixed regulation hoop**, static training bays, spawn platform). Behavior Name **`Bob`**, **8** vector observations, **3** continuous actions — unchanged for trainer compatibility. Hoop/spawn randomization is **off by default** until curriculum Phase 2.

**Week 1 exit criterion still open:** no successful end-to-end training run yet (`results/` empty; prior smoke timed out waiting for Play).

## Build Status (2026-06-18)

| Area                | Status                                                                          |
| ------------------- | ------------------------------------------------------------------------------- |
| Unity scene         | `BobTraining.unity` — full Arc Academy visual build (Example.jpg target)          |
| Scene validation    | `./scripts/validate-scene.sh` → **VALIDATE_PASS**                               |
| Offline regression  | `pytest tests/test_unity_alignment.py` — **16/16**                              |
| Design reference    | [`docs/design/arc-academy-reference.jpg`](docs/design/arc-academy-reference.jpg) |
| Arc Academy runtime | Fixed main hoop + bays, decorative hoops, trajectory arcs, mountain window      |
| First training run  | **Not done** — trainer needs Unity Play after "Listening on port 5004"          |
| Git                 | Arc Academy changes **uncommitted** on `feature/week1-rag-mcp-north-star`       |

## Next Actions

1. **Commit + PR** — Arc Academy MVP on `feature/arc-academy-mvp` (or extend current feature branch) → green CI → merge per [North Star](docs/what-right-looks-like.md)
2. **First training run (Week 1 gate)** — `./scripts/train.sh --timeout-wait=120 --time-scale=20 --force`, then Press Play when trainer listens; confirm `results/bob-v0/` event files
3. **Week 2** — tune reward shaping, TensorBoard review, training GIFs (after loop is proven)

## Links

| Resource                | Location                                                         |
| ----------------------- | ---------------------------------------------------------------- |
| Live demo               | _Coming soon — CloudFront URL after Week 3 deploy_               |
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
| WebGL deploy pipeline              | Week 3                                                                                            |

## Update Log

| Date       | Update                                                                                                 |
| ---------- | ------------------------------------------------------------------------------------------------------ |
| 2026-06-18 | Initial repo scaffold pushed to `main`                                                                 |
| 2026-06-18 | DevOps foundations added (Terraform, CI, Docker, Cursor config)                                        |
| 2026-06-18 | Dev environment locked (Python 3.10.12, CI green)                                                      |
| 2026-06-18 | Baseline testing strategy + pytest suite added                                                         |
| 2026-06-18 | Unity project + BobAgent + training scene builder added                                                |
| 2026-06-18 | Progress screenshot workflow — `docs/progress/` gallery + `capture-progress.sh`                        |
| 2026-06-18 | Repository RAG — ChromaDB index, `bob-rag` MCP, Cursor hooks                                           |
| 2026-06-18 | Unity MCP — `com.coplaydev.unity-mcp`, `bob-unity` MCP, agent consultation rules                       |
| 2026-06-18 | North Star — `docs/what-right-looks-like.md` pinned in PROJECT, AGENTS, project-plan                   |
| 2026-06-18 | Arc Academy MVP — warehouse arena, movable hoop, spawn randomization, arc rewards (local, uncommitted) |
| 2026-06-18 | Scene validator + 14 pytest alignment guards; progress capture `004-arc-academy-mvp`                   |
