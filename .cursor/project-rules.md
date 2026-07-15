# Bob — DevOps Project Rules

Supplement to [`.cursor/rules/bob.mdc`](rules/bob.mdc) and [`AGENTS.md`](../AGENTS.md).

**North Star:** [docs/what-right-looks-like.md](../docs/what-right-looks-like.md) — align CI and release work with the milestone flowchart and PR workflow diagram.

**Hosting:** Bob is **not** deployed to AWS. Do not plan `terraform apply`, S3 sync, or CloudFront for this project.

## Infrastructure as Code (CI only)

- `terraform/` exists for **GitHub Actions fmt/validate/tflint** — not for provisioning Bob hosting
- Never commit `terraform.tfvars` with real values; use `terraform.tfvars.example`
- Do not add AWS deploy steps to CI unless the user explicitly changes hosting strategy

## Documentation for Portfolio

- Document every major step in `docs/` for the portfolio write-up
- Update [`PROJECT.md`](../PROJECT.md) status when milestones complete
- Capture training progress (GIFs, success plots) — re-capture after **bob-v4** policy lands
- Portfolio deliverable: in-repo [`docs/portfolio-site/`](../docs/portfolio-site/) + README links
- **ML changes:** follow [ml-training-recommendations.md](../docs/design/ml-training-recommendations.md); update [bob-done-tracker.md](../docs/bob-done-tracker.md) when gates move

## Reproducibility

- Training hyperparameters live in `config/*.yaml`, not hardcoded in Python
- Python deps pinned in `python/requirements.txt`; venv at `python/.venv`
- Docker image (`Dockerfile`) provides an alternative reproducible training environment
- CI validates Python deps and Terraform on every push to `main`

## Git and Security

- Clear, descriptive commit messages focused on _why_
- No secrets, API keys, or `.tfstate` files in the repo
- Use GitHub Secrets for CI credentials only if future deploy pipeline is scoped

## CI/CD Progression

1. **Now:** Python smoke test + Terraform fmt/validate + Docker build
2. **Next:** `scripts/release-checklist.sh` + optional macOS build/capture smoke (see [next-14-days.md](../docs/planning/next-14-days.md))
