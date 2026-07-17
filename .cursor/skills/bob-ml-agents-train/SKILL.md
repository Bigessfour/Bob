---
name: bob-ml-agents-train
description: >-
  Train and evaluate Bob's Unity ML-Agents PPO free-throw loop — handshake,
  reward shaping, bob-v4 runs, session CSV/plots. Use when changing BobAgent
  rewards, config/bob_free_throw.yaml, running ./scripts/train.sh, diagnosing
  learning (success rate, arc vs makes), or implementing ml-training-recommendations Tier 1/2/3.
paths:
  - Assets/Scripts/BobAgent.cs
  - Assets/Scripts/ArcAcademyLayout.cs
  - Assets/Scripts/ArcAcademyRewards.cs
  - config/*.yaml
  - python/scripts/plot_training_progress.py
  - docs/design/ml-training-recommendations.md
  - docs/planning/next-14-days.md
  - docs/bob-done-tracker.md
---

# Bob ML-Agents Training

## Read first

1. [docs/design/ml-training-recommendations.md](../../../docs/design/ml-training-recommendations.md)
2. [docs/planning/next-14-days.md](../../../docs/planning/next-14-days.md) — priority stack + status
3. [docs/bob-done-tracker.md](../../../docs/bob-done-tracker.md)
4. `rag_query` on **bob-rag** before editing reward/obs code

## Non-negotiables

| Constant       | Value                                                                   |
| -------------- | ----------------------------------------------------------------------- |
| Behavior Name  | `Bob`                                                                   |
| Observations   | 8 (until Tier 3 — update validator + alignment tests)                   |
| Actions        | 3 continuous                                                            |
| Config         | `config/bob_free_throw.yaml` only — no Python hyperparameter hardcoding |
| Success metric | **BasketballPoints / TotalIterations** — not arc quality alone          |

## Current learning gate (bob-v4.1)

**Do not** resume bob-v2/v3 expecting visible learning. Tier 1 episode design is shipped; bob-v4 showed **0.5% makes** with profitable rim_misses.

Implement **Tier 1.5** before extended train — [ml-training-recommendations.md](../../../docs/design/ml-training-recommendations.md):

1. `MadeBasket=7`, `ArcQualityRewardScale=0.02`, `MissProximityRewardScale=0.35`, `RimPlaneMissPenalty`
2. Bob-local impulse: `transform.rotation * localImpulse`
3. Short `RUN_ID=bob-v4.1` validation → diagnostic dashboard pass checks

Then:

```bash
RUN_ID=bob-v4.1 ./scripts/train.sh --force
# Play ONCE after "Listening on port 5004" — no C# edits until training stops
```

**Pass:** rim_miss mean net ≪ make; positive-miss % drops; then rolling success **>5%** over 30+ min @ 20×.

## Safe training workflow

```bash
# Preflight
.cursor/skills/bob-ml-agents-train/scripts/check-training-handshake.sh

# 1. Stop Play; Unity compile idle
# 2. Trainer
./scripts/train.sh                    # or RUN_ID=bob-v4 ./scripts/train.sh --force
# 3. Play once → BOB_TRAINING_OK in console
# 4. No script saves / Unity MCP bakes during run
# 5. Stop Play, then Ctrl+C trainer
```

**Failure signatures:** `Communicator has exited`, `UnityTimeOutException`, `Worker 0 exceeded restarts` — see [ai-warehouse-ops.md](../../../docs/design/ai-warehouse-ops.md).

## After code changes

```bash
cd python && pytest tests/test_unity_alignment.py -q
./scripts/rag-index.sh --paths Assets/Scripts/BobAgent.cs config/bob_free_throw.yaml
python scripts/plot_training_progress.py --output ../docs/results/training_progress.png
```

Update **bob-done-tracker.md** when gates move.

## Tier 2 (after Tier 1 proves makes)

- Behavioral cloning: `Assets/Demos/bob_free_throw.demo` + YAML `behavioral_cloning` block
- Bob-local impulse: `transform.rotation * localImpulse`
- Optional curriculum in `environment_parameters`

See [reference.md](reference.md) for reward ledger and observation gaps.
