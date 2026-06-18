# Bob — Free Throw RL Agent

**Status:** Planning / Setup  
**Goal:** Fun Deep RL demo + DevOps showcase for Cloud Resume Challenge portfolio  
**Tech:** Unity 6 LTS + ML-Agents + Python 3.10 + Terraform + GitHub Actions

---

## Current Milestone

**Week 1 — Setup + Basic Agent**

Repo scaffold and DevOps foundations are in place. Next: Unity project at repo root, ML-Agents package, Python venv, first court scene.

## Next Actions

- [ ] Install Unity Hub + Unity 6 LTS (WebGL module)
- [ ] Create 3D project at repo root
- [ ] Add `com.unity.ml-agents` in Package Manager
- [ ] Create Python venv: `cd python && python3.10 -m venv .venv`
- [ ] Build basic court scene and Bob `Agent` script

## Links

| Resource | Location |
|----------|----------|
| Live demo | _Coming soon — CloudFront URL after Week 3 deploy_ |
| CI workflow | [`.github/workflows/ci.yml`](.github/workflows/ci.yml) |
| Terraform | [`terraform/README.md`](terraform/README.md) |
| Agent rules | [`AGENTS.md`](AGENTS.md) |
| Setup guide | [`docs/setup-checklist.md`](docs/setup-checklist.md) |
| Project plan | [`docs/project-plan.md`](docs/project-plan.md) |

## DevOps Status

| Component | Status |
|-----------|--------|
| Terraform bootstrap (state bucket) | Scaffolded — not yet applied |
| Terraform dev (S3 + CloudFront) | Scaffolded — not yet applied |
| GitHub Actions CI | Placeholder (Python + Terraform validate) |
| Docker training image | Dockerfile ready |
| WebGL deploy pipeline | Week 3 |

## Update Log

| Date | Update |
|------|--------|
| 2026-06-18 | Initial repo scaffold pushed to `main` |
| 2026-06-18 | DevOps foundations added (Terraform, CI, Docker, Cursor config) |
