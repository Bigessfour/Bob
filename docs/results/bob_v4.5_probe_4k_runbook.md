# bob-v4.5 — ABORTED (invalid)

**Status:** Stopped by operator — **invalid run**.

## Why

`TrainingRangeScale = 0.65` moved Bob **off the free-throw line** toward the hoop. That is not a free-throw task and must not be used for learning claims or comparisons.

Partial steps (~55k) and any shots from this window are **discarded** for analysis.

## Reverted

- `TrainingRangeScale` / spawn Z override / impulse range scale removed
- Spawn restored to regulation `FreeThrowLineWorldZ`
- Power-band (`IdealLaunchFy = 8.5`) kept

## Valid next levers (Bob stays on FT line)

1. **Quality make demos** at regulation — record confirmed makes only → BC
2. **Richer observations** (`vy`, speed, rim distance) — Tier 3
3. **Larger temporary rim / score trigger** for discoverability (still FT line)
4. Not: move spawn closer; not: another identical regulation PPO 4k without a new prior
