# Classmate showcase — video vs blog

You do **not** need classmates to install Unity 6 HDRP. Ship **one 3–4 minute video** plus the in-repo blog at [`docs/portfolio-site/story.html`](portfolio-site/story.html).

## Which one to use

| Format | Use when | Stop when |
| --- | --- | --- |
| **Video** | Live Code Platoon demo, instructor review, “press play” energy | 3–4 minutes, two labeled modes, one dashboard still |
| **Blog** (`story.html`) | Repo README, Cloud Resume, people who will never open Unity | One page, honest 0.5% → 34.5% arc, solver vs PPO labeled |

Do both. The blog is the source of truth; the video is the same script spoken.

## Video recipe (Mac, no extra packages)

1. Unity: open `BobTraining.unity`. **Stop Play.**
2. **Bob → Demo → Prepare Solver Wow (Heuristic c≈0)**. HUD chip must say **SOLVER**.
3. QuickTime Player → File → New Screen Recording → record the Game view at 1080p.
4. Press Play. Capture **3–4 makes**. Say: *“This is the analytic prior — physics I wrote, not the neural net.”*
5. Stop Play.
6. **Bob → Demo → Prepare Classmate Showcase (Inference ONNX)**. Console: `BOB_INFERENCE_OK`. HUD chip **INFERENCE · Bob**.
7. Press Play. Capture **~20 shots**. Say: *“This is the v4.8 ONNX policy. Residuals are non-zero. About one in three goes in.”*
8. Cut to `docs/results/bob_v4_8-tight-prior_learning_dashboard.png` (or `docs/portfolio-site/ml-timeline.png`) for 20 seconds.
9. Export `docs/progress/027-classmate-showcase/bob-showcase.mp4` (git-lfs or stream to YouTube/unlisted and link it — do not commit a huge MP4 if the repo is already heavy).

Spoken outline (keep it):

1. Problem — sparse make, cube at the line, one hoop.
2. Solver wow — prove scoring is real (ball falls through the cylinder).
3. What PPO actually learns — 3 residual actions around the 56° prior.
4. Training history — 0.5% → 2% → 11% → **34.5%**.
5. Inference — Play matches training. Not 100%. That is the point.

## 90-second classroom version (no Unity in the room)

Open [`docs/portfolio-site/story.html`](portfolio-site/story.html) on the projector.

1. **15s — started here:** “First honest run after we fixed episode termination: 5 makes in 1,001 shots. 0.5%.”
2. **25s — method:** “We stopped asking PPO to invent a free throw. A 56° ballistic solver proposes the shot. The net learns a 3-D residual. A make only counts if the ball falls through the rim.”
3. **20s — table:** Point at 0.50 → 2.08 → 10.93 → **34.5%**. “The graph moved when the residual band got tighter, not when we added juice.”
4. **20s — two demos:** “The 52-swish clip is the solver. The ONNX policy is about one in three. We mixed those up once and wrote it down.”
5. **10s — close:** “Residual RL around a controller — same pattern as robotics. We did not chase 70%.”

If you have Unity on the podium, swap step 4 for 20 live solver shots + 20 inference shots (HUD chip visible). Still no live `train.sh`.


## Do not

- Live-train (`./scripts/train.sh`) in front of people.
- Show a 50/50 streak and call it inference (that was HeuristicOnly).
- Claim 70% — that bar was deferred on purpose.

## Editor helpers already in the repo

| Menu | Meaning |
| --- | --- |
| `Bob → Demo → Prepare Classmate Showcase` | InferenceOnly + `Assets/Models/Bob.onnx`, recorder off |
| `Bob → Demo → Prepare Solver Wow` | HeuristicOnly, labeled SOLVER |
| `Bob → Demo → Restore Training Behavior` | Default, for `./scripts/train.sh` |

`./scripts/capture-hero-video.sh` is a **batchmode GIF** helper. Use it for a silent loop; use QuickTime for the talk track.
