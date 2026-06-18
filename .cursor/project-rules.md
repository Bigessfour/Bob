# Bob — DevOps Project Rules

Supplement to [`.cursor/rules/bob.mdc`](rules/bob.mdc) and [`AGENTS.md`](../AGENTS.md).

**North Star:** [docs/what-right-looks-like.md](../docs/what-right-looks-like.md) — align infrastructure and CI work with the milestone flowchart and PR workflow diagram.

## Infrastructure as Code

- **All AWS resources** must be defined in `terraform/` — no manual console-only setup without matching IaC
- Apply order: `terraform/bootstrap/` first (state bucket), then `terraform/environments/dev/`
- Never commit `terraform.tfvars` with real values; use `terraform.tfvars.example`
- Document bootstrap outputs and backend configuration steps in `terraform/README.md`

## Documentation for Portfolio

- Document every major step in `docs/` for the portfolio write-up
- Update [`PROJECT.md`](../PROJECT.md) status when milestones complete
- Capture training progress (GIFs, TensorBoard screenshots, reward plots) in Week 2

## Reproducibility

- Training hyperparameters live in `config/*.yaml`, not hardcoded in Python
- Python deps pinned in `python/requirements.txt`; venv at `python/.venv`
- Docker image (`Dockerfile`) provides an alternative reproducible training environment
- CI validates Python deps and Terraform on every push to `main`

## Git and Security

- Clear, descriptive commit messages focused on _why_
- No secrets, API keys, or `.tfstate` files in the repo
- Use GitHub Secrets for CI credentials when deploy pipeline is added (Week 3)

## CI/CD Progression

1. **Now:** Python smoke test + Terraform fmt/validate
2. **Week 3:** Portfolio static site deploy (S3 sync + CloudFront invalidation)
