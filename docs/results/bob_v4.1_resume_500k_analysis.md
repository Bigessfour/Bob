# bob-v4.1 resume → 500k — analysis (2026-07-18)

**Run:** `RUN_ID=bob-v4.1 --resume` from step **102209** → **500000**
**Shots this resume:** **4060** (≈98 steps/shot)
**Plots:** [bob_v4.1_resume_500k_dashboard.png](bob_v4.1_resume_500k_dashboard.png) · [bob_v4.1_run_comparison.png](bob_v4.1_run_comparison.png)

## Verdict

Bob **is learning aim and arc**, not **makes**. Reward economics still **pay for near-misses**, so PPO parks on `rim_miss`.

| Gate                    | Result                                          |
| ----------------------- | ----------------------------------------------- |
| Interim >5% rolling     | **FAIL** — last 1k **1.0%**                     |
| Demo ≥70% / last 1k     | **FAIL** — **1.0%**                             |
| Make ≫ rim_miss mean RL | **PASS** — make **+7.97** vs rim_miss **+0.31** |
| Positive-miss ≤25%      | **FAIL** — **57%** (rose to **~68%** late)      |

## What improved (keep)

| Metric                 | Early (Q1) | Late (Q5) |
| ---------------------- | ---------- | --------- |
| Success                | 0.12%      | **~1.0%** |
| Mean `toward_hoop_dot` | 0.82       | **0.97**  |
| `rim_miss` share       | 41%        | **67%**   |
| Timeout / OOB          | high       | lower     |

Makes cluster near `a≈(0, 0.24, 0.58)`, `F≈(0.2, 7.8, −14)` — usable BC island later.

## What failed (fix)

- **`rim_miss` mean RL rose** Q1 **+0.07** → Q5 **+0.51** while the agent aimed better — classic **near-miss farming**.
- High-toward rim_misses (`toward≥0.98`) average **+0.53** RL — proximity + arc outweigh **−0.25** rim-plane penalty.
- Volume: **57% of misses** finish with **positive** net RL → policy prefers “good miss” over rare make.

Dashboard `--check-pass`: separation OK, **positive-miss FAIL**.

## Tier 1.6 reward change (implemented + review follow-ups)

| Change                         | Before             | After                                   | Why                                                    |
| ------------------------------ | ------------------ | --------------------------------------- | ------------------------------------------------------ |
| No proximity on rim_miss       | always paid        | **skipped**                             | Stop paying the farmed outcome                         |
| `MissProximityRewardScale`     | 0.35               | **0.20**                                | Softer “closer” for floor/timeout only                 |
| `RimPlaneMissPenalty`          | 0.25 → 0.45        | **1.25**                                | Exceeds max launch shaping (~0.50)                     |
| Launch toward / up / arc-align | 0.45 / 0.30 / 0.30 | **0.20 / 0.15 / 0.15**                  | Good-aim miss no longer net-positive from launch alone |
| `ArcQualityRewardScale`        | 0.02               | **0.01**                                | Less dense padding                                     |
| Unify rim_miss                 | dual label paths   | **single** (`applyRimPlaneMissPenalty`) | Penalty always matches reason                          |
| Past-plane timeout/settled     | proximity timeout  | **rim_miss + penalty**                  | Close high-arc farming gap                             |
| Rim height +1.2 gate           | required           | **removed**                             | High arcs past rim still rim_miss                      |

**Next train:** new run id (do **not** `--resume` old economics into a mixed policy):

```bash
RUN_ID=bob-v4.2 ./scripts/train.sh --force
```

**Pass checks (short ≥1k episodes):** positive-miss **≤25%**; rim_miss mean RL **&lt; 0** (or ≪ make/20); then extend toward **>5%**.

**Still next after contrast holds:** BC demos (`Assets/Demos/`) → `bob_free_throw_bc.yaml` — aim alone will not reach **70%/1k**.
