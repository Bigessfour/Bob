# Bob — Project Plan

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

- [ ] Unity project at repo root
- [ ] ML-Agents package installed
- [ ] Python venv and `mlagents` verified
- [ ] Basic court scene (floor, hoop, ball)
- [ ] Bob agent with `Agent` subclass, observations, actions
- [ ] Placeholder reward function (made basket = +1)
- [ ] First training run completes without errors

### Week 2 — Training & Iteration

- [ ] Tune reward shaping (approach, release angle, make/miss)
- [ ] Record training progress GIFs
- [ ] Iterate on observation space and action space
- [ ] Document hyperparameters in `config/`

### Week 3 — Polish + Deployment + Documentation

- [ ] Visual polish (materials, camera, simple UI)
- [ ] WebGL build
- [ ] Terraform AWS hosting (S3 + CloudFront or Amplify)
- [ ] README demo link, technical write-up in `docs/`
- [ ] Optional: GitHub Actions CI smoke test

## Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Unity location | Repo root | Standard Unity layout; ML-Agents expects `Assets/` at project root |
| Trainer | PPO | Default ML-Agents algorithm; good for continuous control |
| Behavior name | `Bob` | Matches agent character and config YAML |
| Python version | 3.10 | ML-Agents compatibility |

## References

- [Unity ML-Agents Documentation](https://docs.unity3d.com/Packages/com.unity.ml-agents@latest)
- [ML-Agents GitHub](https://github.com/Unity-Technologies/ml-agents)
