# Bob — Testing Strategy

Lightweight, growable testing foundation for reproducibility and CI regression. Mirrors portfolio DevOps practices (local-first + GitHub Actions) adapted for Unity + Python RL.

## Principles

1. **Test what matters early** — env setup, configs, agent behaviors, training stability (not pixel-perfect visuals initially)
2. **Local-first + CI** — fast local runs; GitHub Actions for regression on every push
3. **Grow with features** — smoke → unit → integration → E2E as Bob matures
4. **Coverage goal** — 70%+ on new C#/Python code; prioritize reward functions, observations, actions

## Tool Matrix

| Layer                | Tool                                      | Phase               |
| -------------------- | ----------------------------------------- | ------------------- |
| Python smoke/unit    | `pytest` + `pytest-cov`                   | 1 (now)             |
| Config validation    | YAML schema checks in pytest              | 1 (now)             |
| Terraform            | `terraform validate` + `tflint`           | 1 (now)             |
| Docker               | `docker build` + `mlagents-learn --help`  | 1 (now)             |
| Unity C#             | Unity Test Framework (NUnit)              | 2 (after `Assets/`) |
| Training integration | Short episode runs, reward trends         | 3 (Week 2)          |
| E2E / Visual         | WebGL smoke, training GIFs                | 3 (Week 3)          |
| Metrics              | TensorBoard assertions, `plot_rewards.py` | 3 (optional)        |

## Apple Silicon Caveat

`mlagents==1.1.0` depends on `grpcio==1.48.2`, which has **no macOS arm64 wheel**. On M-series Macs:

- **Local venv:** config and `plot_rewards` unit tests run; `requires_mlagents` tests are skipped
- **Full mlagents tests:** run in **CI (Ubuntu)** and **Docker** (`bob-train` image)

## Phase 1 — Baseline (Implemented)

### Python

```bash
cd python
source .venv/bin/activate
pip install -r requirements-dev.txt
pytest tests/ -v --cov=scripts --cov-report=term-missing
```

Tests in `python/tests/`:

| File                   | Coverage                                                                  |
| ---------------------- | ------------------------------------------------------------------------- |
| `test_env.py`          | YAML config parse, Bob behavior schema, `mlagents-learn --help` (CI only) |
| `test_plot_rewards.py` | `find_training_log`, `load_rewards`, plot output                          |

### Infrastructure (CI)

- `pytest` on Ubuntu with Python 3.10.12
- `terraform fmt -check`, `validate`, `tflint` on bootstrap + dev
- `docker build` + `mlagents-learn --help` in container

### Local linting

```bash
trunk check          # includes tflint, ruff, actionlint, etc.
docker compose build train
```

## Phase 2 — Unity Tests (After `Assets/` Exists)

- [ ] Install `com.unity.test-framework` via Package Manager
- [ ] Create `Assets/Tests/EditMode/BobAgentTests.cs`
- [ ] Test reward calculation, observation collection, action application
- [ ] Run: `./scripts/unity.sh -batchmode -runTests -testPlatform editmode -quit`

## Phase 3 — Training & E2E (Week 2–3)

- [ ] Short training integration run (check `results/` for checkpoints)
- [ ] Post-run script: mean reward trend, success rate thresholds
- [ ] WebGL build smoke test in browser
- [ ] Optional: game-ci Unity builder in GitHub Actions

## CI Jobs

| Job                | What it validates                       |
| ------------------ | --------------------------------------- |
| Python Tests       | pytest suite + mlagents on Ubuntu       |
| Terraform Validate | fmt, validate, tflint                   |
| Docker Build Test  | Dockerfile builds; mlagents-learn works |

See [`.github/workflows/ci.yml`](../.github/workflows/ci.yml).

## Related Docs

- [what-right-looks-like.md](what-right-looks-like.md) — **North Star** milestone + workflow diagrams
- [finalized-dev-env.md](finalized-dev-env.md) — environment setup
- [project-plan.md](project-plan.md) — milestones
- [PROJECT.md](../PROJECT.md) — living status
