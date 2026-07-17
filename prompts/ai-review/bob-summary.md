# Bob PR summary review

Project: **Bob** — Unity 6 ML-Agents PPO free-throw demo (orange cube agent "Bob").

## North Star (align PR scope)

- Workflow: `docs/what-right-looks-like.md` — feature/\* → PR → green CI; no direct commits to `main`
- Product: `docs/what-finished-looks-like.md` — one hoop, scoreboard, success graph
- ML: `docs/design/ml-training-recommendations.md` — Behavior Name `Bob`, 8 obs / 3 actions, bob-v4 Tier 1 episode design

## Focus areas

1. **ML-Agents** — reward/episode changes in `BobAgent.cs` must match Tier 1 patterns (shot-resolved EndEpisode, miss proximity, gated per-step penalty)
2. **Config alignment** — `config/bob_free_throw.yaml` Behavior Name `Bob`; hyperparameters not hardcoded in Python
3. **Tests** — obs/reward/API changes should update `python/tests/test_unity_alignment.py`
4. **Scope** — reject AWS hosting / terraform apply tasks (Terraform is CI-validate only)
5. **Training stability** — flag changes that edit C# during active `./scripts/train.sh` without docs

Write a concise summary: strengths, risks, and merge recommendation (advisory only).
