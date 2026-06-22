# Bob — Project Plan

> **North Star:** [What Right Looks Like](what-right-looks-like.md) (workflow) · [What Finished Looks Like](what-finished-looks-like.md) (product) · [Visual vision](design/visual-vision.md) (look)

## Vision

Bob is an **orange cube agent** that **shoots at one basketball hoop**, learning through **PPO iterations**. **In-scene scoreboards** show iterations, **score** (made baskets), cumulative **RL rewards** and **penalties**, and a **success-rate graph** tracks improvement over time. Visual style: **AI Warehouse lab** ([visual-vision.md](design/visual-vision.md)).

## Core Scope (MVP)

- Clean training lab + one active hoop in Unity
- Bob learns free-throw targeting via ML-Agents (`Behavior Name: Bob`)
- In-scene scoreboard + success graph (not TensorBoard as audience UI)
- Training GIFs + portfolio static site (Week 3)
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
- [x] Python venv and `mlagents` verified (Docker `bob-train` on Apple Silicon; local venv skips grpcio on arm64)
- [x] CI green (Python 3.10.12 + Terraform validate)
- [x] Baseline pytest suite (`python/tests/` — 16 tests incl. Arc Academy visual guards)
- [x] Docker build in CI
- [x] Training scene builder + validator CLI (`./scripts/validate-scene.sh` → VALIDATE_PASS)
- [x] Bob agent with `Agent` subclass — Behavior Name `Bob`, 8 obs, 3 actions, gravity shot
- [x] Arc Academy scene builder + validator (`./scripts/validate-scene.sh` → VALIDATE_PASS)
- [x] In-scene training scoreboard (`BobTrainingStats`: iterations, score, rewards, penalties)
- [x] Success-rate graph (`BobTrainingSuccessGraph`)
- [x] Product north star — [`docs/what-finished-looks-like.md`](what-finished-looks-like.md)
- [x] Visual vision — [`docs/design/visual-vision.md`](design/visual-vision.md)
- [ ] **First training run completes without errors** ← **current gate** (`BOB_TRAINING_OK`)

### Week 2 — Training & Iteration

- [ ] **Prove training loop** — `./scripts/train.sh`, Play after trainer listens; wall HUD updates each episode
- [x] **Phase 1.5** — separate basketball projectile (launcher cube + ball)
- [x] **Arc Academy Lab visuals** — corner room, wall HUD, Bob charisma ([visual-vision.md](design/visual-vision.md) Phase 2)
- [x] Progress screenshot history in `docs/progress/`
- [ ] Record training progress GIFs (play-mode capture + Recorder/ffmpeg)
- [x] Document hyperparameters in `config/`
- [x] Session metrics export + `plot_training_progress.py`

### Week 3 — Polish + Deployment + Documentation

- [ ] Portfolio polish — lab hero GIF + optional warehouse stretch still
- [ ] Terraform bootstrap applied (`terraform/bootstrap`)
- [ ] Terraform dev stack applied (`terraform/environments/dev`)
- [ ] Portfolio site synced to S3 + CloudFront invalidation (`docs/portfolio-site/` static HTML)
- [ ] README demo link updated with CloudFront URL
- [ ] Technical write-up in `docs/`
- [x] GitHub Actions CI smoke test (Python + Terraform validate)
- [x] Portfolio site scaffold (`docs/portfolio-site/index.html`)

## Key Design Decisions

| Decision        | Choice                                                                    | Rationale                                                          |
| --------------- | ------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| Unity location  | Repo root                                                                 | Standard Unity layout; ML-Agents expects `Assets/` at project root |
| Trainer         | PPO                                                                       | Default ML-Agents algorithm; good for continuous control           |
| Behavior name   | `Bob`                                                                     | Matches agent character and config YAML                            |
| Python version  | 3.10                                                                      | ML-Agents compatibility                                            |
| Terraform state | S3 remote backend                                                         | Production-style DevOps; bootstrap creates state bucket            |
| Render pipeline | HDRP 17 — **Lab mode** flat materials default; warehouse optional stretch |
| Static hosting  | S3 + CloudFront OAC — portfolio site, not Unity WebGL                     |
| CI              | GitHub Actions                                                            | pytest + Terraform validate + tflint + Docker build                |

## DevOps Milestones

### Infrastructure (Week 3)

- [ ] Apply `terraform/bootstrap` — state bucket + DynamoDB lock
- [ ] Configure `backend.tf` from bootstrap outputs
- [ ] Apply `terraform/environments/dev` — site bucket + CloudFront
- [ ] Deploy portfolio site via `aws s3 sync docs/portfolio-site/`
- [ ] Update `PROJECT.md` and README with live demo URL

### CI/CD

- [x] Baseline CI: pytest + Terraform fmt/validate/tflint + Docker build
- [ ] Automated S3 deploy + CloudFront invalidation on merge to `main` (portfolio site)

## Testing

See [testing-strategy.md](testing-strategy.md) for the full phased plan.

## References

- [**What finished looks like**](what-finished-looks-like.md) — product definition (agent, hoop, scoreboard, graph)
- [**Visual vision**](design/visual-vision.md) — Arc Academy Lab look + visual workflow
- [What Right Looks Like](what-right-looks-like.md) — canonical milestone + workflow diagrams
- [Unity ML-Agents Documentation](https://docs.unity3d.com/Packages/com.unity.ml-agents@latest)
- [ML-Agents GitHub](https://github.com/Unity-Technologies/ml-agents)
