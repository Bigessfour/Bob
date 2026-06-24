# Abnormal Logs Digest — 2026-06-23

Scanned `Logs/` (Unity Editor, batch CLI, trainer, MCP traces). Grouped by **Fix** vs **Ignore** per [ai-warehouse-ops.md](../../docs/design/ai-warehouse-ops.md).

Unity MCP was **unavailable** this turn (`Named pipe socket file not found` — relay shut down after client disconnect). Findings are from log files only.

---

## Fix — Bob product / training

### 1. HDRP cascade shadow spam (critical noise + possible visual bug)

| Field       | Value                                                                                       |
| ----------- | ------------------------------------------------------------------------------------------- |
| **Log**     | `Logs/unity-editor-launch.log`                                                              |
| **Count**   | **272,611** occurrences                                                                     |
| **Message** | `Cascade Shadow atlasing has failed, only one directional light can cast shadows at a time` |
| **When**    | Play mode / editor render loop (2026-06-22 session)                                         |

**Likely cause:** More than one enabled `LightType.Directional` has `shadows != None` before `ArcAcademyLabRenderPreset.EnforceSingleDirectionalShadow()` runs, or a light is re-enabled afterward (e.g. `LightingRig` children, scene defaults, or editor scene view lights).

**Existing mitigation:** `ArcAcademyLabRenderPreset.EnforceSingleDirectionalShadow()` (only `Sun` or first directional casts).

**Proposed fix:**

1. Call `EnforceSingleDirectionalShadow()` earlier — e.g. `[RuntimeInitializeOnLoadMethod]` before first frame, and in `ArcAcademyLabPlayFix.Awake` **before** volume tweaks.
2. Audit scene/prefab for extra directionals with shadows on (`LabKeyFill`, `WarehouseLight_Center`, etc.).
3. Add validator check: fail if `>1` directional has `LightShadows.Soft/Hard`.

---

### 2. Training disconnect + trainer crash (train-gate)

| Field        | Value                                                                                                                         |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------- |
| **Log**      | `Logs/train-gate-test.log`                                                                                                    |
| **Sequence** | Training to step **15,000** → `Communicator has exited` → worker restart → `UnityTimeOutException` → crash on checkpoint save |

```
[WARNING] Restarting worker[0] after 'Communicator has exited.'
mlagents_envs.exception.UnityTimeOutException: The Unity environment took too long to respond.
...
ModuleNotFoundError: No module named 'onnxscript'
```

**Likely causes:**

- **Primary:** Play mode stopped or Unity hung during worker restart (see [ai-warehouse-ops.md](../../docs/design/ai-warehouse-ops.md) stability rules).
- **Secondary:** Docker image runs **PyTorch 2.12.1** (`train-gate-test.log` line 90) but `python/requirements.txt` caps `torch<=2.8.0`. Newer torch ONNX export pulls `onnxscript`, which is not installed — crash masks the real disconnect.

**Proposed fix:**

1. **Ops:** Rebuild `bob-train` image from pinned `requirements.txt`; verify `torch --version` ≤ 2.8 inside container.
2. **Optional:** Add `onnxscript` to requirements if we intentionally upgrade torch later.
3. **Unity:** Ensure `BobTrainingPlayModeGuard` / session flags log `BOB_TRAINING_LOST` / `BOB_TRAINING_END` when Play exits during training (grep found `BOB_TRAINING_OK` in launch log but no `LOST`/`END` markers).
4. **Workflow:** Stop Play before script saves; single Play press after `Listening on port 5004`.

---

### 3. Volume SSR `MissingReferenceException` on Play enter

| Field     | Value                                                                                                   |
| --------- | ------------------------------------------------------------------------------------------------------- |
| **Log**   | `Logs/unity-capture.log` (~line 700)                                                                    |
| **Stack** | `ArcAcademyLabRenderPreset.ApplyLabViewPreset()` → `volume.profile` → `ScreenSpaceReflection` destroyed |

```
MissingReferenceException: The object of type 'UnityEngine.Rendering.HighDefinition.ScreenSpaceReflection' has been destroyed but you are still trying to access it.
  at ArcAcademyLabRenderPreset.ApplyLabViewPreset () ... Line: 354
  at ArcAcademyLabPlayFix.Awake () ... Line: 13
```

**Likely cause:** `Volume.profile` getter clones/instantiates profile components; a stale or shared profile reference is destroyed mid-access during play-mode capture or first Awake.

**Proposed fix:**

1. In `ApplyLabViewPreset`, duplicate profile once at start: `volume.profile = Object.Instantiate(volume.profile)` (if not already unique).
2. Null-guard / try-get each override; skip SSR block if `TryGet` fails.
3. Defer preset apply to `Start` or `yield return null` if race with volume lifecycle in batchmode capture.

---

### 4. Arena builder CLI — `BubbleText` missing `TextMesh` (may be fixed)

| Field       | Value                                                                                        |
| ----------- | -------------------------------------------------------------------------------------------- |
| **Log**     | `Logs/simple-arena-setup.log` (2026-06-23 12:54)                                             |
| **Message** | `MissingComponentException: There is no 'TextMesh' attached to the "BubbleText" game object` |
| **Method**  | `SimpleArcAcademyArenaBuilder.EnsureSpeechBubbleText`                                        |

**Status:** Current source (`SimpleArcAcademyArenaBuilder.cs` ~927–941) adds defensive `RemoveMonoBehavioursWithMissingScript`, null-check, and `AddComponent<TextMesh>()` — likely **fixed in working tree** but **not re-verified** in logs after fix.

**Proposed fix:**

1. Re-run `./scripts/unity.sh -executeMethod SimpleArcAcademyArenaBuilder.ApplyFromCli` and confirm no exception + `VALIDATE_PASS`.
2. If still failing, destroy and recreate `BubbleText` child instead of reusing corrupt prefab state.

---

## Fix — DevOps / MCP / gateway

### 5. MCP relay port conflict

| Field       | Value                                                            |
| ----------- | ---------------------------------------------------------------- | --------------------------------------------- |
| **Logs**    | `Logs/Editor.log`, `Logs/unity-capture.log`, `Logs/traces.jsonl` |
| **Message** | `[ERROR] [relay] Server error                                    | Failed to start server. Is port 9003 in use?` |
| **Also**    | `connection.lost`, relay auto-shutdown after 180s inactivity     |

**Proposed fix:**

1. Kill stale relay: `pkill -f relay_mac_arm64` or restart Unity MCP bridge.
2. Ensure only one Cursor/grok MCP client connects at a time.
3. Approve pending client in **Edit → Project Settings → AI → Unity MCP** (`Validation: Pending` for grok-shell seen in Editor.log).

---

### 6. AI Gateway partial setup

| Field       | Value                                                                                                          |
| ----------- | -------------------------------------------------------------------------------------------------------------- |
| **Log**     | `Logs/Editor.log`                                                                                              |
| **Message** | `BOB_AI_GATEWAY_PARTIAL: env set for Codex → xAI ... Open Project Settings → AI → Gateway to save credentials` |

**Proposed fix:** Run `./scripts/setup-unity-ai-gateway.sh` and save credentials in Unity UI per [unity-ai-gateway.md](../../docs/unity-ai-gateway.md).

---

## Ignore (noise)

| Message                                                  | Log                       | Notes                                                 |
| -------------------------------------------------------- | ------------------------- | ----------------------------------------------------- |
| `[Licensing::Client] Error: Code 404`                    | Editor, validate, capture | No matching entitlements; Editor still runs           |
| `TouchGestureResponder was not found`                    | Editor.log                | macOS Unity editor noise                              |
| `Shader Hidden/ChartRasterizerHardware is not supported` | Editor.log                | GPU lacks conservative rasterization; HDRP probe only |
| `CS0618 FindObjectOfType obsolete`                       | `unity-hdrp-setup.log`    | Cleanup when touching those files                     |

---

## Healthy signals (same scan)

| Signal                                             | Log                                       |
| -------------------------------------------------- | ----------------------------------------- |
| `VALIDATE_PASS: Simple Arc Academy arena is ready` | `unity-validate.log`                      |
| `BOB_TRAINING_OK: Python trainer connected`        | `unity-editor-launch.log`                 |
| `Registered Communicator in Agent`                 | `Editor.log` (today)                      |
| Trainer connected, steps 5k–15k                    | `train-gate-test.log` (before disconnect) |

---

## Suggested fix order (PR-sized slices)

| Priority | Item                                                   | Effort | Impact                                        |
| -------- | ------------------------------------------------------ | ------ | --------------------------------------------- |
| P0       | Pin/rebuild Docker torch + verify train-gate completes | Small  | Prevents trainer crash on disconnect recovery |
| P0       | SSR volume guard in `ArcAcademyLabRenderPreset`        | Small  | Fixes Play/capture exception                  |
| P1       | Directional shadow enforcement timing + validator      | Medium | Stops 272k console spam; cleaner HDRP         |
| P1       | Re-run arena builder CLI; confirm BubbleText fix       | Small  | Unblocks batch scene rebuild                  |
| P2       | MCP relay port hygiene doc / script                    | Small  | Agent workflow reliability                    |
| P3       | Replace obsolete `FindObjectOfType` calls              | Small  | Compile warning cleanup                       |

## Fixes Implemented (2026-06-23 follow-up)

Permanent code changes applied per this digest using batch CLI verification (Unity MCP relay unavailable at time of fix):

- **SSR / MissingReference (P0)**: Added profile clone `Object.Instantiate(volume.profile)` + early Enforce in `ApplyLabViewPreset`, `ApplyMinimalTrainerVolumeInScene`, and capture exposure paths in [ArcAcademyLabRenderPreset.cs](../../Assets/Scripts/ArcAcademyLabRenderPreset.cs) + [BobProgressCapture.cs](../../Assets/Editor/BobProgressCapture.cs). Guards against destroyed overrides.
- **Cascade shadow spam (P1)**: Added `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)] EarlyShadowEnforcement()` + shadow caster count==1 validation in simple arena path. Calls now earlier in play/preset paths. See `EnforceSingleDirectionalShadow` and validator.
- **Docker torch (P0)**: Explicit `pip install "torch<=2.8.0"` before reqs in Dockerfile + post-build version print + train.sh verification of torch cap. Prevents onnxscript 2.12 pulls.
- **BubbleText / builder (P1)**: Defensive RemoveMono + AddComponent + font fallback already present in EnsureSpeechBubbleText; re-ran `ApplyFromCli` + `validate-scene.sh` → **VALIDATE_PASS** (0 TextMesh errors).
- New validator check enforces <=1 shadow caster.

**Re-verify commands succeeded**:
- `./scripts/validate-scene.sh` → VALIDATE_PASS (includes new shadow check).
- No new BubbleText / SSR / cascade errors in fresh batch logs.
- Historical spam remains in old Editor.log (safe to rm Logs/Editor*.log to clean console view).

MCP relay / gateway remain user-side (restart Unity, approve in Project Settings → AI → Unity MCP; run gateway setup script).

Training disconnect root (Communicator + workflow) covered by existing `BobTraining*Guard` + monitor logs + train.sh docs. Run long `RUN_ID=...` to confirm stability post-torch rebuild.

---

## Commands to re-verify after fixes

```bash
# Arena + validate (Editor closed)
./scripts/unity.sh -executeMethod SimpleArcAcademyArenaBuilder.ApplyFromCli
./scripts/unity.sh -executeMethod BobSceneValidator.ValidateFromCli

# Training gate (Editor open, BobTraining scene)
./scripts/train-gate.sh   # or existing train-gate-test path

# Shadow spam: Play once, then:
rg -c "Cascade Shadow atlasing has failed" Logs/Editor.log
# Expect: 0 (or near-zero during first frame only)
```
