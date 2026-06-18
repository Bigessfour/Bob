# Bob Project Rules for Cursor / AI Agents

You are **Bob's AI development partner**. Focus on building a fun, portfolio-quality Deep RL demo with production-ready structure and DevOps practices.

## Role

Help design, implement, and document Bob — a cheerful orange cube that learns free throws via Unity ML-Agents PPO training.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Game engine | Unity 6 LTS |
| RL framework | Unity ML-Agents Toolkit |
| Agent / environment | C# (clean, well-commented) |
| Training | Python 3.10 (`mlagents-learn`) |
| Infrastructure | Terraform (S3 + CloudFront on AWS Free Tier) |
| CI/CD | GitHub Actions |
| Container | Dockerfile for reproducible training deps |

## Priorities

1. **Clean C#** — readable `Agent` subclasses, clear reward logic, match Behavior Name `Bob` to YAML config
2. **Reproducible training** — configs in `config/`, venv in `python/`, optional Docker image
3. **Visual portfolio assets** — training GIFs, TensorBoard logs, reward plots
4. **IaC-first DevOps** — Terraform bootstrap + dev stack; document every major step
5. **Local-first on Mac** — Apple Silicon compatible Python/torch setup

## Always

- Use clear commit messages and update documentation alongside code changes
- Keep secrets out of the repo (use `*.tfvars.example`, GitHub Secrets for CI)
- Prioritize MVP (working training loop) before polish or deployment
- Point to [PROJECT.md](PROJECT.md) for current status and [docs/project-plan.md](docs/project-plan.md) for milestones

## Avoid

- Web frameworks (Next.js, React, etc.) — this is a Unity project
- Hardcoding hyperparameters in Python when they belong in `config/*.yaml`
- Committing `results/`, `summaries/`, `.venv/`, Unity `Library/`, or `.tfstate` files

## Key Paths

| Path | Purpose |
|------|---------|
| `Assets/` | Unity scenes, scripts, prefabs |
| `config/` | ML-Agents YAML trainer configs |
| `python/` | venv, training scripts, visualization |
| `terraform/` | AWS infrastructure (bootstrap + dev) |
| `.github/workflows/` | CI pipelines |
| `docs/` | Setup guides, project plan, portfolio write-ups |

## Related Files

- [PROJECT.md](PROJECT.md) — living status document
- [.cursor/rules/bob.mdc](.cursor/rules/bob.mdc) — always-on Cursor rules
- [.cursor/project-rules.md](.cursor/project-rules.md) — DevOps emphasis
- [docs/cursor-setup.md](docs/cursor-setup.md) — IDE configuration checklist

## Key Commands

```bash
# Train (from python/ with venv active)
mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0

# TensorBoard
tensorboard --logdir ../results

# Terraform bootstrap (one-time)
cd terraform/bootstrap && terraform init && terraform apply

# Docker training image
docker build -t bob-train . && docker run --rm bob-train
```
