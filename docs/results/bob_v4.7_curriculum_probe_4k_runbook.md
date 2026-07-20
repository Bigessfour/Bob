# bob-v4.7-curriculum — 4k distance curriculum + make-hunt BC

**RUN_ID:** `bob-v4.7-curriculum`  
**CONFIG:** `config/bob_free_throw_probe_4k_v47.yaml`  
**Init:** `--force` (13 obs; do not init from v4.6 11-obs checkpoints)  
**Chronicle:** [training-chronicle.md](../design/training-chronicle.md)

## Pre-flight

1. Play smoke: solver prior make rate (`scripts/solver_prior_eval.py`)
2. **Make-hunt demos:** Bob → Demo → **Enable Make-Hunt Demonstration Recorder** → ≥40 makes → copy to `bobfreethrow.demo`
3. Stop Play; compile idle

## Launch

```bash
RUN_ID=bob-v4.7-curriculum CONFIG=config/bob_free_throw_probe_4k_v47.yaml \
  ./scripts/train.sh --force
```

## Results

_(fill after run)_

| Metric           | Value | Gate                   |
| ---------------- | ----- | ---------------------- |
| Episodes         |       | ~4000                  |
| Success          |       | >5% stretch / trend up |
| Positive-miss    |       | ≤25%                   |
| rim_miss mean RL |       | < 0                    |
