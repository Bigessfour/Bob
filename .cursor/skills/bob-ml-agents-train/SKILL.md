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
  - docs/bob-done-tracker.md
---

# Bob ML-Agents Training

## Read first

1. [docs/design/ml-training-recommendations.md](../../../docs/design/ml-training-recommendations.md)
2. [docs/bob-done-tracker.md](../../../docs/bob-done-tracker.md)
3. `rag_query` on **bob-rag** before editing reward/obs code

## Non-negotiables

| Constant       | Value                                                                   |
| -------------- | ----------------------------------------------------------------------- |
| Behavior Name  | `Bob`                                                                   |
| Observations   | 8 (until Tier 3 — update validator + alignment tests)                   |
| Actions        | 3 continuous                                                            |
| Config         | `config/bob_free_throw.yaml` only — no Python hyperparameter hardcoding |
| Success metric | **BasketballPoints / TotalIterations** — not arc quality alone          |

## Current learning gate (bob-v4)

**Do not** resume bob-v2/v3 expecting visible learning (0% makes, high arc, net RL negative).

Implement **Tier 1** before extended train:

1. `EndEpisode()` when shot resolves (rim pass, floor, or `MaxStep` ~60–90)
2. Terminal **miss proximity** reward at episode end
3. **Gate** per-step `-0.002 × xzDist` — no unbounded post-bounce accumulation

Then:

```bash
RUN_ID=bob-v4 ./scripts/train.sh --force
# Play ONCE after "Listening on port 5004" — no C# edits until training stops
```

**Pass:** rolling success **>5%** over 30+ min @ 20×; refresh `docs/results/training_progress.png`.

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
