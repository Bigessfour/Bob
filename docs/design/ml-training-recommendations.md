# Bob — ML Training Recommendations

**Last updated:** 2026-07-14
**Audience:** Future agents implementing learning improvements
**Basis:** E2E evaluation of `BobAgent`, reward shaping, PPO config, and bob-v2/v3 session data
**References:** [ML-Agents Training](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Training-ML-Agents.html) · [Imitation Learning](https://github.com/Unity-Technologies/ml-agents/blob/release-0.14.0/docs/Training-Imitation-Learning.md) · [Environment Design](https://github.com/Unity-Technologies/ml-agents/blob/release_2_verified_docs/docs/Learning-Environment-Design-Agents.md)

---

## Executive summary

Bob’s **infrastructure** (PPO handshake, scoring, HUD, CSV) works. **Learning** does not: recent runs show **~67–74% arc quality** with **0% makes** and **net RL trending negative** (~−163 after ~65 episodes). More PPO steps on the current reward design is unlikely to fix this — the same pattern appears in [Unity projectile tasks that failed until miss proximity + short episodes were added](https://discussions.unity.com/t/ml-agents-agent-not-converging-for-simple-projectile-to-target-task-in-unity/1596784).

**Next training run:** implement **Tier 1** below, then `RUN_ID=bob-v4` (not more bob-v3).

---

## E2E pipeline (current)

```text
config/bob_free_throw.yaml (PPO)
  → ./scripts/train.sh (Docker, port 5004)
  → Unity Play @ 20× (BehaviorType Default)
  → BobAgent: 8 obs → policy → single impulse
  → physics → HoopScoreZone (sparse +3 make)
  → BobTrainingStats → HUD + summaries/bob_session.csv
```

| Stage          | Status           | Issue                                          |
| -------------- | ---------------- | ---------------------------------------------- |
| PPO config     | OK               | No BC/GAIL/curriculum                          |
| Observations   | Thin             | Missing `vy`, speed, shot phase                |
| Actions        | 3D world impulse | Not Bob-local; breaks under spawn jitter       |
| Launch prior   | Weak             | Biases only; no expert/BC demos                |
| Dense rewards  | Misaligned       | Per-step penalties dominate sparse makes       |
| Episode length | Too long         | Ends on OOB only; bounces accumulate penalties |
| Success metric | Misleading       | High arc ≠ makes                               |

---

## Diagnosis

### 1. Initial trajectory — Bob does not start knowing free throws

**What helps today:** neutral actions `c≈[0,0,0]` → impulse `(0, +4, -6)` via `verticalBias` / `forwardBias` in `BobAgent.cs`, plus launch-direction shaping in `ApplyLaunchDirectionRewards`.

**What breaks the prior:**

| Gap                  | Detail                                                                                                   |
| -------------------- | -------------------------------------------------------------------------------------------------------- |
| World vs local force | Impulse applied in world XYZ; Bob rotates per spawn (`GetSpawnFacingRotation`) but force does not follow |
| No imitation prior   | No `.demo` file; YAML has no `behavioral_cloning` block                                                  |
| No analytic seed     | No parabolic `v0` solver from spawn → rim for demos or heuristic warm-start                              |

**ML-Agents guidance:** For sparse rewards, pre-train with **Behavioral Cloning** or **GAIL** from demonstrations ([overview](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/ML-Agents-Overview.html)).

### 2. Reward / punishment — realistic physics, wrong economics

Typical episode reward ledger:

| Signal                    | When                          | Magnitude            |
| ------------------------- | ----------------------------- | -------------------- |
| Launch toward hoop        | Once                          | up to ~+0.45         |
| Launch upward / arc align | Once                          | up to ~+0.6          |
| Arc quality               | **Every step** while tracking | up to ~+0.1/step     |
| Distance penalty          | **Every step**                | −0.002 × xzDist/step |
| Made basket               | Rare                          | +3.0                 |
| OOB                       | End                           | −0.5                 |

After one shot the ball can bounce until OOB (**100–400+ steps** at 20×). Net RL drifts **very negative** while arc metrics look good. PPO learns “long episodes bad” faster than “make = great.”

**ML-Agents guidance:** Per-step penalties should pair with **early `EndEpisode()`** on goal or miss ([environment design](https://github.com/Unity-Technologies/ml-agents/blob/release_2_verified_docs/docs/Learning-Environment-Design-Agents.md)).

### 3. bob-v3 run outcome (2026-07-14)

Partial checkpoints saved under `results/bob-v3/Bob/` (~9k–41k steps). Runs aborted with `Communicator has exited` / `UnityTimeOutException` when Play toggled or scripts recompiled during training. **Do not resume bob-v3 for learning claims** — implement bob-v4 reward fixes first.

---

## Implementation tiers (priority order)

### Tier 1 — Highest leverage (bob-v4 blockers)

Implement in `BobAgent.cs` before the next extended train:

1. **End episode when shot resolves** — After impulse, end when:
   - ball crosses rim plane (miss or make already handled), **or**
   - ball hits floor / settles, **or**
   - `MaxStep` ~60–90 (~3 s sim @ 20×)
2. **Terminal miss proximity reward** — On episode end without make:
   ```csharp
   float proximity = 1f - Mathf.Clamp01(distanceToRim / maxDist);
   GiveReward(proximity * missProximityScale);  // e.g. 0.5–1.0
   EndEpisode();
   ```
3. **Remove or gate per-step `-0.002 * xzDist`** — Apply once at shot end, or only while airborne for first ~1.5 s; never unbounded post-bounce.

**Success criteria for bob-v4:** rolling success **>5%** over 30+ min uninterrupted Play; net RL trend **up** when makes occur; plot refreshed.

### Tier 2 — “Bob knows what a free throw is”

4. **Behavioral cloning** — Record 30–50 expert shots (parabolic `v0` spawn→rim or tuned heuristic); add to `config/bob_free_throw.yaml`:
   ```yaml
   behavioral_cloning:
     demo_path: Assets/Demos/bob_free_throw.demo
     strength: 0.5
     steps: 100000
   ```
5. **Bob-local impulse** — `impulse = transform.rotation * localImpulse` so jittered spawns stay coherent.
6. **Optional curriculum** — `environment_parameters` in YAML: start closer/shorter rim height; widen as success rises.

### Tier 3 — Trainer tuning & observations

7. **Observations** — Add `vy`, speed magnitude, normalized distance to rim (may require validator + alignment test updates if obs count changes).
8. **`time_horizon: 64`** — OK if episodes are ~30–80 steps after Tier 1; otherwise try 128.
9. **Training stability** — No script saves, MCP bakes, or Play toggles during an active run ([ai-warehouse-ops.md](ai-warehouse-ops.md#training-stability-prevent-crashes)).

---

## Safe training workflow (mandatory)

```bash
# 1. Stop Play; wait for Unity compile idle
# 2. Start trainer
RUN_ID=bob-v4 ./scripts/train.sh --force   # or --resume after first checkpoint

# 3. Press Play ONCE after "Listening on port 5004"
# 4. Do NOT edit Assets/Scripts or run Unity MCP bakes until stopping training
# 5. Stop Play, then Ctrl+C trainer
```

**Never** run `./scripts/train.sh --force` while Unity is in Play from a prior session without stopping first.

---

## Files to touch (when implementing)

| File                                   | Changes                                         |
| -------------------------------------- | ----------------------------------------------- |
| `Assets/Scripts/BobAgent.cs`           | Episode end, miss proximity, local impulse, obs |
| `Assets/Scripts/ArcAcademyLayout.cs`   | Reward scale constants for bob-v4               |
| `config/bob_free_throw.yaml`           | Optional BC block, curriculum                   |
| `Assets/Demos/`                        | Expert `.demo` recordings (new)                 |
| `python/tests/test_unity_alignment.py` | Guards if obs count or reward strings change    |
| `docs/results/training_progress.png`   | Refresh after bob-v4                            |

---

## Related

- [ai-warehouse-ops.md](ai-warehouse-ops.md) — handshake + stability
- [bob-done-tracker.md](../bob-done-tracker.md) — live checklist
- [what-finished-looks-like.md](../what-finished-looks-like.md) — product Phase 3 gates
- [results/README.md](../results/README.md) — plot generation
