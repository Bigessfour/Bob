# Project Overview

- **Game Title:** Bob — Deep RL Free Throw Training Lab
- **High-Level Concept:** An orange cube agent ("Bob") learns to shoot free throws at a single hoop via ML-Agents (PPO). The training room should read like an AI Warehouse YouTube video: a clean, bright lab with white tile walls, a dark grid floor, one hoop, Bob, and wall-mounted scoreboards.
- **Players:** Single-agent RL (no human player); audience = portfolio/learning-video viewers.
- **Inspiration / Reference Games:** [AI Warehouse](https://www.youtube.com/@AIWarehouse) training videos (see attached reference image: white grid-tile walls, dark grid floor, black wall counters with white digits, cube agents with eyes).
- **Tone / Art Direction:** Clean, friendly, readable "lab" — readability over photorealism.
- **Target Platform:** StandaloneOSX (Editor-first workflow).
- **Screen Orientation / Resolution:** Landscape, 16:9.
- **Render Pipeline:** HDRP (`HDRenderPipelineAsset`).

# E2E Evaluation — Current State vs. AI Warehouse Target

The training loop, single-hoop scoring, Bob's face/feedback, and a wall-mounted HUD (`SimpleArcAcademyArena/Wall_West/LabTrainingHud`) all exist and the major legacy groups (`WarehouseShell`, `CourtFloor`, `CourtMarkings`, `TrainingBays`, `Boundaries`, `FloorDecals`) are hidden in Edit mode. However, four concrete gaps prevent the clean view + UX the reference image shows. These were verified by direct scene inspection.

| #   | Gap                                                                                                                                                                        | Evidence (verified)                                                                                                                                                                                                                                                                                                                                                                                                                                   | Severity |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- |
| 1   | **Duplicated rim/net geometry around Bob's rim** — this is the "decorative basketball areas around the outside of Bob's rim."                                              | `TrainingArena/Hoop/.../Rim` subtree contains **106 `NetStrand_*`** (expected 12) and **108 `RimSeg_*`** (expected 12) — ~9 accumulated copies piled around the rim.                                                                                                                                                                                                                                                                                  | Critical |
| 2   | **Camera flips to the wrong side on Play** — points low at the floor, scoreboard ends up behind the camera.                                                                | `CameraOrbit.ComputeSphericalFromOffset` (L171) uses `Atan2(offset.x, offset.z)` with no 180° correction while `ApplyOrbitTransform` (L148) rebuilds with `Vector3.back`. On the first `Update`, the camera jumps from the correct `(13, 3.2, -3.5)` to `(-13, 3.2, -5.5)` facing East. (Confirmed in a prior Play-mode probe: `camPos=(-13,3.2,-5.5)`, ray hits `Wall_West`.)                                                                        | Critical |
| 3   | **No runtime guarantee the lab stays clean** — decorations are hidden only because the scene file persists `activeSelf=False`; the runtime safety net does not cover them. | `ArcAcademyLabPlayFix.Awake` → `ArcAcademyLabSceneCleanup.HideLegacyClutter()` only re-hides branding/signage/labels (L18–23). It never touches `TrainingBays`, `BayHoop`, `PortableHoopStand`, `RoboticLauncher`, or `DecorativeHoopMarker`. The robust stripping logic lives only in the **editor** builder. `Bay_1..Bay_8` each have `activeSelf=True` under an inactive parent, so any reactivation re-shows all clutter with no runtime re-hide. | High     |
| 4   | **Screen-space HUD overlays compete with the wall panels** + **all key lights are off.**                                                                                   | `Main Camera` still carries `ArcAcademyDemoUi`, `BobTrainingScoreboard`, `BobTrainingSuccessGraph` (screen overlays). `TrainingArena/LightingRig` children `LabKeyFill`, `WarehouseLight_Center`, `BobRimLight` are all `activeSelf=False` — lab currently relies on sky/ambient only, which can read flat/dark vs. AI Warehouse's soft even lighting.                                                                                                | Medium   |

**Approved decisions (from user):**

1. Legacy decorations → **Delete from BobTraining.unity AND add a runtime guard** so they can never reappear.
2. Rim/net duplication → **Fix the cause (DestroyImmediate in edit mode) AND strip the existing 100+ duplicates** to a single clean rim/net.
3. Screen HUD → **Hidden in lab mode**; wall panels are the hero.
4. Camera bug → **Fix it.**

# Game Mechanics

## Core Gameplay Loop

Unchanged. Episode begins → Bob aims + shoots (3 continuous actions, 8 observations, PPO) → ball travels → through hoop = +1 basketball point + RL reward + `EndEpisode`; miss/OOB = penalty + `EndEpisode` → scoreboards/graph update → repeat. **No changes to `BobAgent`, observation/action counts, Behavior Name `Bob`, reward shaping, or `config/bob_free_throw.yaml`.**

## Controls and Input Methods

No gameplay controls (autonomous RL). Camera convenience keys in `CameraOrbit` remain (F1 reset, right-drag orbit within clamps). Legacy Input Manager is unchanged.

# UI

Target framing matches the reference image: a clean room with **wall-mounted black scoreboard panels** showing iterations / score / cumulative RL / success-rate graph, fed by `BobTrainingStats`.

```
+--------------------------------------------------------------+
|  WALL_WEST (camera faces this wall)                          |
|   [ LabTrainingHud Panel ]   <- iterations, score, net RL,   |
|   [ Success-rate graph    ]      success %, arc quality      |
|                                                              |
|        (hoop + backboard)              (no bay clutter)      |
|     [Bob orange cube w/ eyes]   o  basketball                |
|   ============ dark grid floor ============                  |
+--------------------------------------------------------------+
   (no screen-space overlay HUD in lab mode)
```

- **Wall HUD:** `SimpleArcAcademyArena/Wall_West/LabTrainingHud/Canvas/Panel` (+ `GraphImage`, `GraphLegendText`) — already present and active; remains the hero.
- **Screen HUD:** `ArcAcademyDemoUi`, `BobTrainingScoreboard`, `BobTrainingSuccessGraph` on `Main Camera` — suppressed when `SimpleArcAcademyArena.IsLabViewActive`.

# Key Asset & Context

Scripts to modify (all confirmed to exist):

- **`Assets/Scripts/CameraOrbit.cs`** — fix the spherical/`Vector3.back` mismatch.
  - `ComputeSphericalFromOffset` (L160–173): change `outYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;` → `... - 180f;`
  - Verified reconstruction: with the `-180f` correction, `lookAt + Euler(pitch,yaw,0)*(Vector3.back*dist)` reproduces `(13,3.2,-3.5)` and 4 random test positions with ~0 error.
- **`Assets/Scripts/TrainingHoopDetail.cs`** — stop net/collider accumulation and clean existing duplicates.
  - `EnsureVisualNet` (~L396) calls `ClearChildren(netRoot)` then creates 12 strands + 2 rings; `ConfigureRimColliders` (~L144) rebuilds `RimColliders`. The clears use deferred `Object.Destroy`, so in Edit mode old children survive the same-call rebuild → accumulation each builder run / Play.
  - Make all clears edit-mode-safe: use `Object.DestroyImmediate` when `!Application.isPlaying` (mirror the existing `Application.isPlaying ? Destroy : DestroyImmediate` pattern already used elsewhere in this file). Audit `ClearChildren`, `StripNetPhysicsColliders` (L477), strand/ring/collider `Object.Destroy` calls (L417, L435, L450, L473, L486, L491).
  - Add a one-time idempotency: before building, fully clear any pre-existing `Net`, `RimColliders`, `NetStrand_*`, `RimSeg_*` so a single clean set always results.
- **`Assets/Scripts/ArcAcademyLabSceneCleanup.cs`** — extend the **runtime** guard (`HideLegacyClutter`, L11–24, already called from `ArcAcademyLabPlayFix.Awake` with `[DefaultExecutionOrder(-200)]`).
  - Also `SetActive(false)` on `TrainingArena/TrainingBays`.
  - Deactivate every `DecorativeHoopMarker`, `PortableHoopStand`, and `RoboticLauncher` that is **not** a child of the single active scoring hoop (mirror the editor `HideExtraDecorativeHoops`/`StripBackgroundHoopDecorations` logic, but runtime-safe and `SetActive` only — no `Destroy`).
- **`Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs`** — for the **delete** path: add edit-time permanent removal (`DestroyImmediate`) of the legacy decorative roots in `BobTraining.unity` (`TrainingBays`, `WarehouseShell`, `CourtFloor`, `CourtMarkings`, `DistanceMarkings`, `Boundaries`, `FloorDecals`, `DecorativeHoops`, any non-active `PortableHoopStand`/`DecorativeHoopMarker`). Guarded so it only deletes when the lab arena is the active view, leaving the `_Backup` scene untouched.
- **Screen-HUD suppression** — `Assets/Scripts/BobTrainingScoreboard.cs`, `BobTrainingSuccessGraph.cs`, `ArcAcademyDemoUi.cs`: in lab mode, suppress `OnGUI`/draw when `SimpleArcAcademyArena.IsLabViewActive` (single early-return guard each).
- **Lighting (verify/optional)** — `Assets/Scripts/ArcAcademyLabRenderPreset.cs` (`ApplyLabViewPreset`): ensure a soft key/fill is active in lab mode if the current sky/ambient-only setup reads flat or dark after the other fixes.

Reference constants: `Assets/Scripts/ArcAcademyLayout.cs` (`TrainingBaysName`, `PortableHoopStandName`, `DecorativeHoopsName`, `HoopName`, `RimName`, etc.).

Validation: `Assets/Scripts/BobSceneValidator.cs` (`VerifyFromCli`) — must still report exactly one `HoopScoreZone` and pass.

# Implementation Steps

### Step 1 — Fix the camera-framing bug

- **Description:** In `CameraOrbit.cs`, subtract `180f` from the yaw computed in `ComputeSphericalFromOffset` so `ApplyOrbitTransform`'s `Vector3.back` reconstruction lands on `defaultPosition`. Verify `ResetToDefault` and the orbit yaw clamps still keep Bob + hoop + west-wall scoreboard framed.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (independent file)

### Step 2 — Stop rim/net accumulation + clean existing duplicates

- **Description:** In `TrainingHoopDetail.cs`, make every net/collider clear use `DestroyImmediate` when not playing; add a pre-build full strip of existing `Net`/`RimColliders`/`NetStrand_*`/`RimSeg_*` so the result is exactly 12 strands + 12 rim segments + 1 net + 1 collider group. Run the hoop upgrade once to collapse the current 106/108 duplicates to clean counts.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (independent file)

### Step 3 — Runtime guard for decorations

- **Description:** Extend `ArcAcademyLabSceneCleanup.HideLegacyClutter` to also hide `TrainingBays` and all non-active `DecorativeHoopMarker`/`PortableHoopStand`/`RoboticLauncher` at runtime (SetActive only). Confirm it runs via `ArcAcademyLabPlayFix.Awake` on every Play.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (independent file)

### Step 4 — Permanently delete legacy decorative geometry from BobTraining.unity

- **Description:** Add edit-time deletion (in `SimpleArcAcademyArenaBuilder`) of the legacy decorative roots and stray decorative hoops from `BobTraining.unity`, guarded to the lab view so `_Backup` is unaffected. Re-save the scene.
- **Assigned role:** developer
- **Dependencies:** Depends on Step 3 (keep the runtime guard as the safety net even after deletion).
- **Parallelizable:** No

### Step 5 — Hide screen-space HUD in lab mode

- **Description:** Add an `IsLabViewActive` early-return guard to `BobTrainingScoreboard`, `BobTrainingSuccessGraph`, and `ArcAcademyDemoUi` so only the wall panels render in lab mode.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** Yes (independent files)

### Step 6 — Lighting/readability verification (conditional)

- **Description:** After Steps 1–5, evaluate the Play view. If it reads flat/dark, ensure `ArcAcademyLabRenderPreset.ApplyLabViewPreset` enables a soft key/fill (Sun + gentle fill, minimal bloom, SSR off) to match the AI Warehouse soft-even-light look. Only change lighting if needed.
- **Assigned role:** developer
- **Dependencies:** Depends on Steps 1–5
- **Parallelizable:** No

### Step 7 — Validate + visual confirm

- **Description:** Run `BobSceneValidator.VerifyFromCli` (exactly one `HoopScoreZone`, behavior name `Bob`). Enter Play, capture the Game View, and visually compare against the reference image: clean rim (no duplicate nets), no bay clutter, correct east-sideline framing, wall scoreboard visible, no screen overlay.
- **Assigned role:** developer
- **Dependencies:** Depends on Steps 1–6
- **Parallelizable:** No

# Verification & Testing

- **Rim cleanliness:** Re-inspect `Rim` subtree → exactly `NetStrand` = 12, `RimSeg` = 12, `Net` groups = 1, `RimColliders` = 1. Press Play several times → counts stay stable (no re-accumulation).
- **Camera framing (Play):** `Main Camera` stays at ≈ `(13, 3.2, -3.5)`, FOV 52°, forward ray hits the hoop/Bob region (not the floor or the back of `Wall_West`); the `Wall_West` `LabTrainingHud` is visible in frame.
- **Decoration removal:** `TrainingBays`/`BayHoop`/`PortableHoopStand`/`RoboticLauncher` are gone from `BobTraining.unity`; pressing Play never re-shows them; `_Backup` scene still contains the complex view.
- **Single hoop:** `BobSceneValidator` → `VALIDATE_PASS`, exactly one active `HoopScoreZone`; Behavior Name remains `Bob`; 8 obs / 3 actions unchanged.
- **HUD:** In Play (lab mode), no screen-space overlay; only the wall panel + graph render and update each episode.
- **Visual A/B:** Play-mode Game View screenshot compared to `docs/design/ai-warehouse-lab-reference.png` and the attached reference — clean rim, bright even lighting, one hoop, wall scoreboard, friendly Bob.
- **Regression:** Training loop still runs (`mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0` + Play → training steps), no new console errors.

# Notes / Out of Scope

- No changes to `BobAgent`, observations/actions, reward shaping, or `bob_free_throw.yaml`.
- No second scoring hoop; Behavior Name stays `Bob`.
- Photoreal warehouse remains a stretch (kept only in `_Backup`).
- Wall-panel visual restyling beyond what exists is Phase 2 polish, not required for this clean-up pass.
