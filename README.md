# Bob the Free Throw Champion

![CI](https://github.com/Bigessfour/Bob/actions/workflows/ci.yml/badge.svg)

A fun Deep Reinforcement Learning demo where **Bob** — a cheerful orange cube — learns to shoot perfect free throws in a 3D basketball court. Inspired by AI Warehouse training videos, this project showcases an entertaining learning curve with visual training progress, ideal for a portfolio piece.

**Live demo:** Portfolio site scaffold at [`docs/portfolio-site/`](docs/portfolio-site/) (local / GitHub Pages–ready). **AWS deploy is not run against the AICO account** — Terraform remains CI-validated scaffold only until a separate personal/portfolio AWS profile is configured.

**Project status:** See [PROJECT.md](PROJECT.md) | **Product:** [docs/what-finished-looks-like.md](docs/what-finished-looks-like.md) | **Visual:** [docs/design/visual-vision.md](docs/design/visual-vision.md) | **Workflow:** [docs/what-right-looks-like.md](docs/what-right-looks-like.md) | **Agents:** [AGENTS.md](AGENTS.md)

---

## Vision

Bob is an **orange cube** that learns to **shoot at one basketball hoop** through PPO training. Each made basket is **+1 score** on the **in-scene scoreboard**; a **success-rate graph** shows improvement over iterations. Cumulative **RL rewards** and **penalties** track the learning signal separately. Visual target: clean **AI Warehouse–style lab** ([docs/design/visual-vision.md](docs/design/visual-vision.md)).

**Finished product:** [docs/what-finished-looks-like.md](docs/what-finished-looks-like.md)

## MVP Scope

- Clean training lab + one active hoop (Unity 6 LTS, HDRP Editor)
- Bob agent (`Behavior Name: Bob`) — iterative shots toward hoop
- In-scene scoreboard + success graph (`BobTrainingStats`)
- `./scripts/train.sh` + Play for ML-Agents training
- Portfolio static site on AWS (Week 3)

## Stretch Goals

- Variable difficulty (distance, defenders, wind)
- Multi-shot routines or mini-games
- Leaderboard / visitor interaction

## Tech Stack

| Layer               | Technology                       |
| ------------------- | -------------------------------- |
| Engine              | Unity 6 LTS (Personal)           |
| RL Framework        | Unity ML-Agents Toolkit          |
| Agent / Environment | C#                               |
| Training            | Python 3.10 + `mlagents`         |
| Hosting             | Terraform + AWS (portfolio site) |
| Tooling             | GitHub, Cursor                   |

## Repository Structure

```text
bob/
├── Assets/                    # Unity project (created via Unity Hub)
├── ProjectSettings/
├── Packages/
├── config/                    # ML-Agents YAML trainer configs
├── python/                    # Training venv and scripts
├── docs/                      # Planning, diagrams, results, North Star
├── terraform/                 # AWS IaC (bootstrap + dev S3/CloudFront)
├── .github/workflows/         # CI (Python + Terraform validate)
├── .cursor/rules/             # Cursor agent context
├── AGENTS.md                  # AI agent instructions
├── PROJECT.md                 # Living status document
├── Dockerfile                 # Reproducible training image
├── README.md
├── LICENSE
└── .gitignore
```

## Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/Bigessfour/Bob.git
cd Bob
```

### 2. Set up Unity

1. Install [Unity Hub](https://unity.com/download) and **Unity 6 LTS** (6000.x) with **HDRP** (included with 3D HDRP template or add High Definition RP package)
2. Open this repo as the Unity project (HDRP pipeline configured via `./scripts/validate-scene.sh`)
3. In Package Manager, confirm `com.unity.ml-agents` (match version to `python/requirements.txt`)

See [docs/setup-checklist.md](docs/setup-checklist.md) for the full M5 Mac checklist.

### 3. Set up Python training environment

```bash
cd python
python3.10 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
```

### 4. Train Bob (after Unity scene is ready)

With the Unity Editor open and the training scene loaded:

```bash
mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0
```

Press **Play** in the Unity Editor when prompted.

## DevOps

### Terraform (AWS Portfolio Hosting)

Two-layer IaC with S3 remote state:

1. **Bootstrap** (one-time): `cd terraform/bootstrap && terraform init && terraform apply`
2. **Dev stack**: Copy `backend.tf.example` → `backend.tf`, then `terraform apply` in `terraform/environments/dev/`

See [terraform/README.md](terraform/README.md) for full apply order and portfolio deploy commands.

### CI

GitHub Actions ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)) runs on every push to `main`:

- Python 3.10 dependency install + `mlagents-learn --help`
- Terraform `fmt -check` and `validate` (bootstrap + dev)

### Docker

Reproducible training dependencies (Unity Editor still runs on host):

```bash
docker build -t bob-train .
docker run --rm bob-train
```

### IDE Setup

Workspace settings in [`.vscode/`](.vscode/). See [docs/cursor-setup.md](docs/cursor-setup.md) for extension and interpreter setup.

## Timeline (Part-Time)

| Week | Focus                                           |
| ---- | ----------------------------------------------- |
| 1    | Setup, basic agent, court scene                 |
| 2    | Training iteration, reward tuning, capture GIFs |
| 3    | Polish, portfolio site deploy, documentation    |

See [docs/project-plan.md](docs/project-plan.md) for full scope details.

## License

MIT — see [LICENSE](LICENSE).
