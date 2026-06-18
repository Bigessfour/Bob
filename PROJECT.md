# Bob — Free Throw RL Agent

> **North Star:** [What Right Looks Like](docs/what-right-looks-like.md) — canonical milestone + workflow diagrams. Read before planning or merging; every change must align with the current week and permanent quality bars.

**Status:** Week 1 — Basic Agent  
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio  
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + Terraform + GitHub Actions

---

## Current Milestone

**Week 1 — Setup + Basic Agent**

Unity project initialized at repo root with `BobAgent.cs`, ML-Agents 4.0.3 (Unity 6), and automated scene builder. Trainer listens on port 5004 — press Play in Unity to connect.

## Next Actions

- [ ] Press Play in `BobTraining` scene — confirm Bob logs to console
- [ ] Run `./scripts/train.sh` with Unity Play to start first training run
- [ ] Tune reward shaping (Week 2)

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
| Unity MCP (`bob-unity`)            | CoplayDev MCP for Unity — consult before scene/agent work; [docs/unity-mcp.md](docs/unity-mcp.md) |
| Unity project                      | `Assets/`, `ProjectSettings/`, `Packages/` at repo root                                           |
| ML-Agents                          | `com.unity.ml-agents` 4.0.3 + `com.unity.ai.inference` 2.2.1                                      |
| WebGL deploy pipeline              | Week 3                                                                                            |

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
