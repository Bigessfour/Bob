# Bob — ML Project Timeline (portfolio demo)

**Purpose:** Demonstrate visible learning progress across training runs — for README, portfolio site, and interviews.
**Live data:** `summaries/bob_session.csv`, `summaries/bob_shots.csv`, `docs/design/training-chronicle.md`
**Plots:** `./scripts/capture-ml-run.sh <run_id> <utc_start>`

---

## Story arc (what changed, what improved)

| Phase                | Run ID                  | Method change                                   | Success (session)         | Key insight                                             |
| -------------------- | ----------------------- | ----------------------------------------------- | ------------------------- | ------------------------------------------------------- |
| Baseline             | bob-v4                  | Tier 1 shot-resolved episodes                   | **0.50%**                 | Aim improved; makes almost never happen                 |
| Economics            | bob-v4.2–v4.4           | Rim_miss penalties; BC solver demos             | **~1.2%**                 | Positive-miss fixed; makes still flat                   |
| Residual hybrid      | bob-v4.6-residual       | Analytic solver + PPO residual c                | **2.08%**                 | Prior helps; demos still near-miss heavy                |
| **Breakthrough**     | **bob-v4.7-curriculum** | Retuned solver + make-hunt BC + hoop curriculum | **10.93%**                | Cleared **>5%** interim gate; rolling **20.8%**         |
| Extension            | bob-v4.7-ext            | Init v4.7; regulation FT only                   | **14.58%** / roll **25%** | Plateau; trailing-1k **11.5%** — portfolio capture done |
| **v4.8 tight prior** | bob-v4.8-tight-prior    | \|c\| max **1.0**, solver-match **0.35**        | **34.43%**                | **~2.5×** vs v4.7 plateau; trail-1k **34.5%**           |
| **v4.8 inference**   | Play (InferenceOnly)    | `Assets/Models/Bob.onnx`                        | **~31–35%** (validated)   | Matches training — not solver streak                    |

---

## Artifacts for demo

| Artifact                  | Path                                               | When to refresh                  |
| ------------------------- | -------------------------------------------------- | -------------------------------- |
| Multi-run success overlay | `docs/results/bob_ml_timeline_comparison.png`      | After each major run             |
| Per-run dashboard         | `docs/results/bob_v4.7_ext_learning_dashboard.png` | After run completes              |
| Session progress          | `docs/results/bob_v4.7_ext_training_progress.png`  | After run completes              |
| Chronological log         | [training-chronicle.md](training-chronicle.md)     | After every `./scripts/train.sh` |
| Checkpoints               | `results/<run_id>/Bob/*.onnx`                      | Auto from trainer                |

---

## Capture workflow (after every training run)

1. Note **UTC start** when Play connects (`BOB_TRAINING_OK`) — store in `docs/results/<run>_session.meta.json`.
2. Let trainer finish (or stop cleanly: Stop Play → Ctrl+C trainer).
3. Run:

```bash
./scripts/capture-ml-run.sh bob-v4.7-ext 2026-07-20T11:02:18
```

4. Append results row to [training-chronicle.md](training-chronicle.md).
5. Copy best PNGs into portfolio: `docs/portfolio-site/` (manual link update).

---

## Inference demo (no trainer)

Load `results/bob-v4.7-curriculum/Bob.onnx` (or latest ext checkpoint) → Behavior **Default** → Play. Scoreboard shows iterations, makes, rolling success — audience sees learning without TensorBoard.

---

## References

- [training-run-plan.md](training-run-plan.md) — interim >5% and demo-done 70%/1k bars
- [ml-training-recommendations.md](ml-training-recommendations.md) — Tier 1–3 method changes
- [what-finished-looks-like.md](../what-finished-looks-like.md) — product north star
