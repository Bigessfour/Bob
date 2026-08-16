# Bob ONNX models

Canonical classmate / inference model:

`Assets/Models/Bob.onnx` — **bob-v4.8-tight-prior** (trailing-1k **34.5%**).

```bash
# After a new training run, replace the checked-in file:
cp results/bob-v4.8-tight-prior/Bob.onnx Assets/Models/Bob.onnx
```

Then in the Editor (Play **stopped**):

1. **Bob → Demo → Prepare Classmate Showcase (Inference ONNX)**
2. Console: `BOB_INFERENCE_OK: BehaviorType=InferenceOnly`
3. HUD chip must read **INFERENCE**, not **SOLVER**

Do **not** demo `bob-v3` (0% makes) or a HeuristicOnly 50/50 streak as “the trained policy.”
Solver wow is a separate menu: **Bob → Demo → Prepare Solver Wow**.
