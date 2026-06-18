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

| File                      | Coverage                                                                  |
| ------------------------- | ------------------------------------------------------------------------- |
| `test_env.py`             | YAML config parse, Bob behavior schema, `mlagents-learn --help` (CI only) |
| `test_plot_rewards.py`    | `find_training_log`, `load_rewards`, plot output                          |
| `test_unity_alignment.py` | Unity/trainer alignment, scene YAML, asmdef layout, MCP pref keys         |

### Infrastructure (CI)

- `pytest` on Ubuntu with Python 3.10.12
- `terraform fmt -check`, `validate`, `tflint` on bootstrap + dev
- `docker build` + `mlagents-learn --help` in container

### Local linting

```bash
trunk check          # includes tflint, ruff, actionlint, etc.
docker compose build train
```

## Phase 2 — Unity Tests (Week 2+)

### Phase 1 offline guards (implemented — run in CI today)

`test_unity_alignment.py` catches regressions without Unity Test Framework:

| Guard                                           | What breaks if it fails                                                                          |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| `test_bob_training_scene_yaml_alignment`        | `BobTraining.unity` lost Behavior Name `Bob`, BehaviorType `0`, 8 obs, 3 actions, or hoop wiring |
| `test_scene_builder_constants_match_validator`  | `BobTrainingSceneBuilder` / `BobSceneValidator` drift on ML-Agents constants or scene path       |
| `test_editor_scripts_live_under_scripts_editor` | Scene builder/validator moved back to legacy `Assets/Editor/` paths                              |
| `test_mcp_asmdef_layout`                        | `Bob.Mcp.asmdef` missing, wrong references, or not Editor-only                                   |
| `test_bob_editor_asmdef_exists`                 | `Bob.Editor.asmdef` references drift                                                             |
| `test_mcp_bootstrap_pref_keys`                  | `BobMcpBootstrap` MCP EditorPref key strings changed                                             |
| `test_validate_scene_script_wires_cli_methods`  | `./scripts/validate-scene.sh` no longer calls CLI entry points                                   |
| `test_arc_academy_layout_and_scripts_exist`     | Arc Academy runtime scripts removed from repo                                                    |
| `test_arc_academy_builder_wiring`               | Scene builder/validator lost Arc Academy manager, hoop, spawn pad wiring                         |
| `test_arc_academy_visual_scripts_exist`         | Arc trajectory visual, material factory, or design reference removed                             |
| `test_arc_academy_visual_builder_wiring`        | Visual build methods (mountain window, decorative hoops, arcs) removed from builder/validator    |

Manual / batchmode (not in CI yet): `./scripts/validate-scene.sh` rebuilds the scene and runs `BobSceneValidator.VerifyFromCli` in Unity.

### EditMode tests (deferred)

- [ ] Install `com.unity.test-framework` via Package Manager
- [ ] Create `Assets/Tests/EditMode/BobAgentTests.cs`
- [ ] Test reward calculation, observation collection, action application
- [ ] Add EditMode tests for `BobTrainingSceneBuilder` output and `BobSceneValidator` pass/fail paths
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
