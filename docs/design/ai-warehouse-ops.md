# AI Warehouse — Operational Patterns for Bob

**Sources:** [AIWarehouse/AIWarehouse](https://github.com/AIWarehouse/AIWarehouse) (official), [DQN-Labs/aiw_soccer](https://github.com/DQN-Labs/aiw_soccer) (community config reference), Bob logs (`logs/Editor.log`, `logs/unity-*.log`).

Bob’s **visual** target is documented in [visual-vision.md](visual-vision.md). This doc captures **how AI Warehouse runs training** and what Bob emulates.

---

## What AI Warehouse says publicly

From the official README:

> _We use Unity and ML-Agents; most of the time the learning algo is **PPO**. The actual meat of the learning happens because of the **observations and reward function** we set up._

Their full project code is **private**; the public repo only ships a sample `.onnx`. Bob must own observations, rewards, and in-scene progress UI.

---

## Patterns Bob adopts

| Pattern                    | AI Warehouse style                       | Bob implementation                                                                                  |
| -------------------------- | ---------------------------------------- | --------------------------------------------------------------------------------------------------- |
| **Algorithm**              | PPO default                              | `trainer_type: ppo` in `config/bob_free_throw.yaml`                                                 |
| **Behavior name**          | Per-agent YAML key                       | `Bob` — must match Unity `BehaviorParameters`                                                       |
| **Observations + rewards** | Primary learning lever                   | `BobAgent.cs` (8 obs, 3 actions) + `ArcAcademyRewards`                                              |
| **Fast training**          | Sped-up sim in videos                    | `engine_settings.time_scale: 20` + `BobTrainingConnectionMonitor` applies 20× when trainer connects |
| **Exploration decay**      | Linear β/ε schedules (community configs) | `beta_schedule: linear`, `epsilon_schedule: linear` in YAML                                         |
| **Progress UI**            | Wall counters / readable HUD             | `BobTrainingStats` + scoreboard + success graph (not TensorBoard for audience)                      |
| **Decision frequency**     | Every step                               | `DecisionRequester.DecisionPeriod = 1`                                                              |
| **Training handshake**     | Trainer first, then Play                 | `./scripts/train.sh` → `Listening on port 5004` → Unity Play                                        |

---

## Community config reference (aiw_soccer)

`ai_config/FootballAgent.yaml` uses:

- `batch_size: 1024`, `buffer_size: 10240` (same order of magnitude as Bob)
- `learning_rate: 3.0e-4`, `gamma: 0.99`
- `time_horizon: 1024` (Bob uses `64` — shorter episodes for single-shot free throws)
- `summary_freq: 25000` (Bob uses `5000` for faster Week 1 feedback)

Bob stays **single-agent PPO** (not POca/multi-agent like soccer).

---

## Log anomalies reviewed (2026-06-19)

| Log signal                                                                | Severity     | Action                                                                                                                              |
| ------------------------------------------------------------------------- | ------------ | ----------------------------------------------------------------------------------------------------------------------------------- |
| `Couldn't connect to trainer on port 5004 … inference instead`            | **Fix**      | `BobTrainingConnectionMonitor` + scoreboard status line; `train.sh` auto `docker compose down`                                      |
| `FileNotFoundError: … checkpoint.pt` on resume                            | **Fix**      | `./scripts/train.sh --force` or `RUN_ID=bob-v1 ./scripts/train.sh`; `train.sh` now auto-`--force` when checkpoint missing           |
| `ModuleNotFoundError: no module named 'onnxscript'`                       | **Fix**      | Rebuild Docker after `torch<=2.8.0` pin in `python/requirements.txt`; avoid PyTorch 2.9+ with mlagents 1.1.0                        |
| `The referenced script on this Behaviour (Game Object 'Bob') is missing!` | **Fix**      | Run `./scripts/validate-scene.sh` (repairs `VrShootInputPlaceholder` on `Prefab_Bob`); or remove missing script on Bob in Inspector |
| Ball flies erratically / multiple launches per shot                       | **Fix**      | One impulse per episode in `BobAgent`; visual-only net + segmented `RimColliders` via `TrainingHoopDetail`                          |
| Rim detached from backboard / hoop bobbing up and down                    | **Fix**      | `TrainingHoopDetail.FreezeStationaryAssembly` — reparent `HoopHead`, disable arm, snap rim to backboard                             |
| Batchmode Burst segfault after scene build                                | **Mitigate** | `validate-scene.sh` passes `-disableBurstCompilation`                                                                               |
| `[WARNING] --train option deprecated`                                     | Low          | Removed `train_model` from `checkpoint_settings`                                                                                    |
| Unity Licensing 404 / NoSubscription AI                                   | Ignore       | Batchmode + no Unity AI subscription — does not block training                                                                      |
| MCP WebSocket connection failed                                           | Ignore       | Bridge not running — expected when Editor closed                                                                                    |
| HDRP material upgrader skip                                               | Ignore       | Package shaders; Bob uses `Assets/Materials/HDRP/` + Bob menu upgrade                                                               |

---

## Recommended training workflow (Bob)

```bash
# 1. Trainer (resumes bob-v0 only when results/bob-v0/Bob/checkpoint.pt exists)
./scripts/train.sh

# 2. Unity: BobTraining scene, Play STOPPED until trainer listens

# 3. Press Play when terminal shows: Listening on port 5004

# 4. Confirm scoreboard shows: "Training (PPO)" in green (not inference fallback)
```

If inference fallback appears: **Stop Play** → confirm trainer listening → **Play** again.

---

## Related

- [what-finished-looks-like.md](../what-finished-looks-like.md) — product definition
- [visual-vision.md](visual-vision.md) — lab look
- [unity-dev.md](../unity-dev.md) — HDRP + port 5004 troubleshooting
