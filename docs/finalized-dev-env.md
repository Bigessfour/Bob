# Bob — Finalized Development Environment

Canonical checklist before writing C# agent code. Complete every item, then start Week 1 implementation.

## Ready-to-Code Checklist

- [ ] Python 3.10.12 venv at `python/.venv` (`./scripts/setup-python.sh`)
- [ ] Cursor interpreter: `python/.venv/bin/python` (3.10.12)
- [ ] `.env` created from `.env.example`
- [ ] Docker `bob-train` image builds (`docker compose build train`)
- [ ] `docker compose run --rm train mlagents-learn --help` succeeds
- [ ] Unity 6 **6000.5.0f1** project at repo root (`Assets/` exists)
- [ ] `com.unity.ml-agents` installed in Package Manager
- [ ] CI green on `main` ([Actions](https://github.com/Bigessfour/Bob/actions))
- [ ] `pytest tests/ -v` passes locally ([testing-strategy.md](testing-strategy.md))
- [ ] Workspace extensions installed (see below)

## Locked Tool Versions

| Tool              | Version                        | Pinned in                                   |
| ----------------- | ------------------------------ | ------------------------------------------- |
| Python            | **3.10.12**                    | `.python-version`, CI, Dockerfile, Trunk    |
| Unity             | **6000.5.0f1**                 | `.vscode/settings.json`, `scripts/unity.sh` |
| ML-Agents (pip)   | **1.1.0**                      | `python/requirements.txt`                   |
| ML-Agents (Unity) | match 1.1.0                    | Package Manager                             |
| Terraform         | **1.5.7**                      | `.github/workflows/ci.yml`                  |
| Docker base       | `python:3.10.12-slim-bookworm` | `Dockerfile`                                |

> **Why 3.10.12?** `mlagents==1.1.0` requires Python 3.10.1–3.10.12. GitHub's default `3.10` resolves to 3.10.20 and fails CI.

## IDE Extensions (Required)

Install when Cursor prompts from [`.vscode/extensions.json`](../.vscode/extensions.json):

| Extension      | ID                                   |
| -------------- | ------------------------------------ |
| Python         | `ms-python.python`                   |
| C#             | `ms-dotnettools.csharp`              |
| C# Dev Kit     | `ms-dotnettools.csdevkit`            |
| Unity          | `visualstudiotoolsforunity.vstuc`    |
| Terraform      | `hashicorp.terraform`                |
| Docker         | `ms-azuretools.vscode-docker`        |
| YAML           | `redhat.vscode-yaml`                 |
| GitHub Actions | `github.vscode-github-actions`       |
| Trunk          | `trunk.io`                           |
| Dev Containers | `ms-vscode-remote.remote-containers` |
| Markdown       | `yzhang.markdown-all-in-one`         |

## Environment Variables

Copy and edit:

```bash
cp .env.example .env
```

| Variable             | Default                      | Purpose                   |
| -------------------- | ---------------------------- | ------------------------- |
| `UNITY_VERSION`      | `6000.5.0f1`                 | Unity Editor version      |
| `UNITY_PROJECT_PATH` | repo root                    | Unity project location    |
| `RUN_ID`             | `bob-v0`                     | ML-Agents training run ID |
| `CONFIG`             | `config/bob_free_throw.yaml` | Trainer config path       |

`./scripts/train.sh` auto-adds `--resume` when `results/<RUN_ID>` already exists. For a clean slate use `RUN_ID=bob-v1 ./scripts/train.sh` or `./scripts/train.sh --force`.

Loaded automatically via `.vscode/settings.json` (`python.envFile`, terminal env).

## Workflows by Task

| Task               | Command                                       | Where                  |
| ------------------ | --------------------------------------------- | ---------------------- |
| Create venv        | `./scripts/setup-python.sh`                   | Local (once)           |
| Train Bob          | `./scripts/train.sh`                          | Docker (Apple Silicon) |
| TensorBoard        | `tensorboard --logdir results`                | Local venv             |
| Reward plots       | `python python/scripts/plot_rewards.py`       | Local venv             |
| Unity batchmode    | `./scripts/unity.sh -batchmode -quit`         | Local                  |
| Terraform validate | `terraform validate` in `terraform/bootstrap` | Local                  |
| Lint all           | `trunk check`                                 | Local                  |
| Run tests          | `cd python && pytest tests/ -v`               | Local venv             |

## Apple Silicon Notes

`grpcio==1.48.2` (mlagents dependency) has no macOS arm64 wheel. On M-series Macs:

- **IDE / linting / TensorBoard:** local `python/.venv`
- **Training (`mlagents-learn`):** Docker with `network_mode: host` (see `docker-compose.yml`)

## AI Assistants

| Tool                                 | Use                                                                                   |
| ------------------------------------ | ------------------------------------------------------------------------------------- |
| Cursor + [`AGENTS.md`](../AGENTS.md) | Primary code agent                                                                    |
| Unity Tools extension                | Editor debug/attach                                                                   |
| `scripts/unity.sh`                   | CLI automation (builds, tests)                                                        |
| Unity MCP (`unity-mcp`)              | Live Editor MCP — [unity-mcp.md](unity-mcp.md); requires Editor open + bridge Running |

## GitHub

- **CI:** [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) — pytest + Terraform validate + tflint + Docker build
- **Secrets (Week 3):** `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` for deploy
- **Branch protection (recommended):** require CI checks on `main`

## Related Docs

- [cursor-setup.md](cursor-setup.md) — IDE configuration
- [setup-checklist.md](setup-checklist.md) — Unity Hub + Python install
- [unity-dev.md](unity-dev.md) — Unity CLI and ML-Agents package
- [project-plan.md](project-plan.md) — milestones
- [testing-strategy.md](testing-strategy.md) — pytest, CI, phased test plan
