# Python Training Environment

ML-Agents training runs from this directory. Use Python 3.10 for compatibility with `mlagents==1.1.0`.

## Setup (M5 Mac)

```bash
cd python
python3.10 -m venv .venv
source .venv/bin/activate
pip install --upgrade pip
pip install -r requirements.txt
```

## Verify Installation

```bash
mlagents-learn --help
```

## Train Bob

With the Unity Editor open and a training scene loaded (Behavior Name = `Bob`):

```bash
mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0
```

Press **Play** in Unity when the trainer prompts for a connection.

## TensorBoard

Monitor training metrics after a run starts writing to `results/`:

```bash
tensorboard --logdir ../results
```

Open http://localhost:6006 in your browser.

## Reward Plots

Generate a reward curve PNG from training logs:

```bash
python scripts/plot_rewards.py --run-id bob-v0 --output ../docs/reward_curve.png
```

Requires a completed training run with `TrainingRewards.csv` in `results/`.

## Docker Alternative

From the repo root (reproducible Python/trainer deps; Unity Editor still runs on host):

```bash
docker build -t bob-train .
docker run --rm bob-train
docker run --rm -v "$(pwd)/results:/app/results" bob-train \
  mlagents-learn config/bob_free_throw.yaml --run-id=bob-v0
```

## Apple Silicon Notes

If `torch` fails to install, install it separately first:

```bash
pip install torch --index-url https://download.pytorch.org/whl/cpu
pip install -r requirements.txt
```

Training outputs (`results/`, `summaries/`) are written to the repo root and are gitignored.

See [docs/setup-checklist.md](../docs/setup-checklist.md) for the full local setup guide.
