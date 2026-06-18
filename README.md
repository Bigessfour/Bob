# Bob the Free Throw Champion

A fun Deep Reinforcement Learning demo where **Bob** — a cheerful orange cube — learns to shoot perfect free throws in a 3D basketball court. Inspired by AI Warehouse training videos, this project showcases an entertaining learning curve with visual training progress, ideal for a portfolio piece.

**Live demo:** _Coming soon — WebGL build hosted on AWS Free Tier_

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
├── terraform/                 # AWS WebGL hosting (Week 3)
├── .github/workflows/         # CI/CD (added later)
├── .cursor/rules/             # Cursor agent context
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

## Timeline (Part-Time)

| Week | Focus |
|------|-------|
| 1 | Setup, basic agent, court scene |
| 2 | Training iteration, reward tuning, capture GIFs |
| 3 | Polish, WebGL build, AWS deploy, documentation |

See [docs/project-plan.md](docs/project-plan.md) for full scope details.

## License

MIT — see [LICENSE](LICENSE).
