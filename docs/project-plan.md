# Bob — Project Plan

> **North Star:** [What Right Looks Like](what-right-looks-like.md) — milestone flowchart and repo/workflow compass. Cross-check every planned task against both diagrams before adding scope.

## Vision

A fun, visual Deep Reinforcement Learning demo where Bob (cheerful orange cube) learns to shoot perfect free throws. Inspired by AI Warehouse videos — entertaining training progress, great for portfolio.

## Core Scope (MVP)

- 3D basketball court environment in Unity
- Bob (orange cube agent) learns free throw mechanics
- Clear learning curve with training videos/GIFs
- WebGL demo hosted on AWS (Free Tier)
- Professional GitHub repo + write-up

## Stretch (Nice-to-Have)

- Variable difficulty (distance, defenders, wind)
- Multi-shot routines or mini-games
- Leaderboard / visitor interaction

## Tech Stack

- Unity 6 LTS (Personal)
- ML-Agents Toolkit
- C# + Python 3.10
- Terraform for AWS hosting
- GitHub + Cursor

## Timeline (Realistic Part-Time)

### Week 1 — Setup + Basic Agent

- [x] Unity project at repo root
- [x] ML-Agents package installed (`com.unity.ml-agents` 4.0.3 — required for Unity 6)
- [ ] Python venv and `mlagents` verified (Docker on Apple Silicon)
- [x] CI green (Python 3.10.12 + Terraform validate)
- [x] Baseline pytest suite (`python/tests/`)
- [x] Docker build in CI
- [x] Basic court scene builder (`Bob/Create Training Scene`) — free-throw half-court, hoop, score zone
- [x] Bob agent with `Agent` subclass, observations, actions
- [ ] First training run completes without errors

### Week 2 — Training & Iteration

- [ ] Tune reward shaping (approach, release angle, make/miss)
- [x] Progress screenshot history in `docs/progress/` (edit-mode; CLI + Editor menu)
- [ ] Record training progress GIFs (Week 2 — play-mode capture + Recorder/ffmpeg)
- [ ] Iterate on observation space and action space
- [ ] Document hyperparameters in `config/`

### Week 3 — Polish + Deployment + Documentation

- [ ] Visual polish (materials, camera, simple UI)
- [ ] WebGL build
- [ ] Terraform bootstrap applied (`terraform/bootstrap`)
- [ ] Terraform dev stack applied (`terraform/environments/dev`)
- [ ] WebGL build synced to S3 + CloudFront invalidation
- [ ] README demo link updated with CloudFront URL
- [ ] Technical write-up in `docs/`
- [x] GitHub Actions CI smoke test (Python + Terraform validate)
- [ ] Full Unity WebGL build in CI (game-ci) — stretch

## Key Design Decisions

| Decision        | Choice              | Rationale                                                          |
| --------------- | ------------------- | ------------------------------------------------------------------ |
| Unity location  | Repo root           | Standard Unity layout; ML-Agents expects `Assets/` at project root |
| Trainer         | PPO                 | Default ML-Agents algorithm; good for continuous control           |
| Behavior name   | `Bob`               | Matches agent character and config YAML                            |
| Python version  | 3.10                | ML-Agents compatibility                                            |
| Terraform state | S3 remote backend   | Production-style DevOps; bootstrap creates state bucket            |
| Static hosting  | S3 + CloudFront OAC | HTTPS CDN; no public S3 ACL                                        |
| CI              | GitHub Actions      | pytest + Terraform validate + tflint + Docker build                |

## DevOps Milestones

### Infrastructure (Week 3)

- [ ] Apply `terraform/bootstrap` — state bucket + DynamoDB lock
- [ ] Configure `backend.tf` from bootstrap outputs
- [ ] Apply `terraform/environments/dev` — site bucket + CloudFront
- [ ] Deploy WebGL build via `aws s3 sync`
- [ ] Update `PROJECT.md` and README with live demo URL

### CI/CD

- [x] Baseline CI: pytest + Terraform fmt/validate/tflint + Docker build
- [ ] Unity WebGL build pipeline (game-ci)
- [ ] Automated S3 deploy + CloudFront invalidation on merge to `main`

## Testing

See [testing-strategy.md](testing-strategy.md) for the full phased plan.

## References

- [What Right Looks Like](what-right-looks-like.md) — canonical milestone + workflow diagrams (North Star)
- [Unity ML-Agents Documentation](https://docs.unity3d.com/Packages/com.unity.ml-agents@latest)
- [ML-Agents GitHub](https://github.com/Unity-Technologies/ml-agents)
