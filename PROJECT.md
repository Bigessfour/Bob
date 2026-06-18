# Bob — Free Throw RL Agent

**Status:** Week 1 — Basic Agent  
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio  
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + Terraform + GitHub Actions

---

## Current Milestone

**Week 1 — Setup + Basic Agent**

Unity project initialized at repo root with `BobAgent.cs`, ML-Agents 3.0.0, and automated scene builder. Next: first training run.

## Next Actions

- [ ] Open project in Unity Hub → **Bob → Create Training Scene** (if scene not built via CLI)
- [ ] Press Play — confirm Bob logs to console
- [ ] Run `./scripts/train.sh` with Unity Play to connect trainer
- [ ] Tune reward shaping (Week 2)

## Links

| Resource | Location |
|----------|----------|
| Live demo | _Coming soon — CloudFront URL after Week 3 deploy_ |
| CI workflow | [`.github/workflows/ci.yml`](.github/workflows/ci.yml) |
| Testing strategy | [`docs/testing-strategy.md`](docs/testing-strategy.md) |
| Unity dev guide | [`docs/unity-dev.md`](docs/unity-dev.md) |
| Terraform | [`terraform/README.md`](terraform/README.md) |
| Agent rules | [`AGENTS.md`](AGENTS.md) |
| Setup guide | [`docs/setup-checklist.md`](docs/setup-checklist.md) |
| Project plan | [`docs/project-plan.md`](docs/project-plan.md) |

## DevOps Status

| Component | Status |
|-----------|--------|
| Terraform bootstrap (state bucket) | Scaffolded — not yet applied |
| Terraform dev (S3 + CloudFront) | Scaffolded — not yet applied |
| GitHub Actions CI | pytest + Terraform validate + tflint + Docker build |
| Docker training image | Built locally (`bob-train:latest`) |
| Baseline pytest suite | `python/tests/` — config + plot_rewards + unity alignment |
| Unity project | `Assets/`, `ProjectSettings/`, `Packages/` at repo root |
| ML-Agents | `com.unity.ml-agents` 3.0.0 |
| WebGL deploy pipeline | Week 3 |

## Update Log

| Date | Update |
|------|--------|
| 2026-06-18 | Initial repo scaffold pushed to `main` |
| 2026-06-18 | DevOps foundations added (Terraform, CI, Docker, Cursor config) |
| 2026-06-18 | Dev environment locked (Python 3.10.12, CI green) |
| 2026-06-18 | Baseline testing strategy + pytest suite added |
| 2026-06-18 | Unity project + BobAgent + training scene builder added |
