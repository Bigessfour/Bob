---
name: bob-unity-mcp
description: >-
  Operate Bob's Unity 6 project via official unity-mcp bridge — scene hierarchy,
  Bob/hoop wiring, Behavior Parameters, polish bakes, console verification.
  Use before editing Assets/, scenes, prefabs, BobAgent, arena builders, or
  running validate-scene; not during active ./scripts/train.sh Play sessions.
paths:
  - Assets/**
  - ProjectSettings/**
  - Packages/manifest.json
  - docs/unity-mcp.md
---

# Bob Unity MCP

Bob uses **official Unity MCP** (`com.unity.ai.assistant`) — server id **`unity-mcp`** in Cursor MCP settings.

## Preflight

1. Unity Editor open on this repo
2. **Edit → Project Settings → AI → Unity MCP** — bridge **Running**
3. Cursor approved under Connected Clients
4. **Stop Play** before saving scripts (compile during Play kills training)

```bash
.cursor/skills/bob-unity-mcp/scripts/preflight-unity-mcp.sh
```

If bridge unavailable: batchmode only (`./scripts/validate-scene.sh`) and end with **Further development required**.

## Resource-first workflow

```
1. Editor state   → Unity_ManageEditor (Action: GetState) — not Playing during script saves
2. Active scene   → Unity_ManageScene (Action: GetActive)
3. Find targets   → Unity_ManageGameObject (action: find) — Bob, Hoop, Basketball, NearBobTrainingHud
4. Act            → Unity_ManageGameObject, Unity_RunCommand, bob_* custom tools
5. Verify         → Unity_ReadConsole (Error/Warning), Unity_Camera_Capture or validate-scene
```

**Always** call `GetMcpTools` before invoking tools — do not guess parameter shapes.

## Bob custom MCP tools

| Tool                      | Use                       |
| ------------------------- | ------------------------- |
| `bob_open_training_scene` | Open `BobTraining.unity`  |
| `bob_setup_simple_arena`  | Rebuild Arc Academy arena |

See [Assets/Scripts/Editor/BobUnityMcpTools.cs](../../../Assets/Scripts/Editor/BobUnityMcpTools.cs).

## Bob invariants (validator-enforced)

- Behavior Name **`Bob`**, BehaviorType **Default** for training
- **8** obs, **3** continuous actions
- Exactly **one** `HoopScoreZone`, **one** wired `SimpleBasketball`
- `BobTrainingStats` present for HUD/CSV

After hierarchy changes:

```bash
./scripts/validate-scene.sh
cd python && pytest tests/test_unity_alignment.py -q
```

## Training + MCP conflict

**Never** MCP-bake scenes or save `Assets/Scripts/**` while `./scripts/train.sh` is connected and Unity is in **Play** — causes `Communicator has exited`.

Order: Stop Play → compile idle → train → Play once → train completes → then MCP polish.

## Polish path

Menu equivalents (or MCP `Unity_RunCommand`):

- **Bob → Polish → Fix Bob Lab Visuals** — dual HUD + audio
- Rebuild wall + near-Bob via `BobWallHudBuilder` / `BobNearBobHudBuilder`

See [reference.md](reference.md) for tool name mapping (official Unity MCP vs legacy docs).
