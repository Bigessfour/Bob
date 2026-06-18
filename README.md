# Bob the Free Throw Champion

![CI](https://github.com/Bigessfour/Bob/actions/workflows/ci.yml/badge.svg)

A fun Deep Reinforcement Learning demo where **Bob** — a cheerful orange cube — learns to shoot perfect free throws in a 3D basketball court. Inspired by AI Warehouse training videos, this project showcases an entertaining learning curve with visual training progress, ideal for a portfolio piece.

**Live demo:** _Coming soon — WebGL build hosted on AWS Free Tier_

**Project status:** See [PROJECT.md](PROJECT.md) | **Agent context:** See [AGENTS.md](AGENTS.md) | **Dev setup:** See [docs/finalized-dev-env.md](docs/finalized-dev-env.md) | **Testing:** See [docs/testing-strategy.md](docs/testing-strategy.md)

---

## Vision

Bob starts with clumsy throws and gradually masters free-throw mechanics through PPO training in Unity ML-Agents. The goal is a polished, shareable demo: watch Bob improve shot by shot, with training GIFs and a playable WebGL build.

## MVP Scope

- 3D basketball court environment in Unity 6 LTS
- Bob (orange cube agent) learns free-throw mechanics via ML-Agents
- Clear learning curve with training videos/GIFs
- WebGL demo hosted on AWS (Free Tier)
- Professional GitHub repo and technical write-up

## Stretch Goals

- Variable difficulty (distance, defenders, wind)
- Multi-shot routines or mini-games
- Leaderboard / visitor interaction

## Tech Stack

| Layer | Technology |
|-------|------------|
| Engine | Unity 6 LTS (Personal) |
| RL Framework | Unity ML-Agents Toolkit |
| Agent / Environment | C# |
| Training | Python 3.10 + `mlagents` |
| Hosting | Terraform + AWS (WebGL) |
| Tooling | GitHub, Cursor |

## Repository Structure

```text
bob/
├── Assets/                    # Unity project (created via Unity Hub)
├── ProjectSettings/
├── Packages/
├── config/                    # ML-Agents YAML trainer configs
├── python/                    # Training venv and scripts
├── docs/                      # Planning, diagrams, results
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

1. Install [Unity Hub](https://unity.com/download) and **Unity 6 LTS** (6000.x) with **WebGL Build Support**
2. Create a new **3D (URP or Built-in)** project at the repo root
3. In Package Manager, add `com.unity.ml-agents` (match version to `python/requirements.txt`)

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

### Terraform (AWS WebGL Hosting)

Two-layer IaC with S3 remote state:

1. **Bootstrap** (one-time): `cd terraform/bootstrap && terraform init && terraform apply`
2. **Dev stack**: Copy `backend.tf.example` → `backend.tf`, then `terraform apply` in `terraform/environments/dev/`

See [terraform/README.md](terraform/README.md) for full apply order and WebGL deploy commands.

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

| Week | Focus |
|------|-------|
| 1 | Setup, basic agent, court scene |
| 2 | Training iteration, reward tuning, capture GIFs |
| 3 | Polish, WebGL build, AWS deploy, documentation |

See [docs/project-plan.md](docs/project-plan.md) for full scope details.

## License

MIT — see [LICENSE](LICENSE).
