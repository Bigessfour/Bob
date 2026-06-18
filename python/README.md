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

## Apple Silicon Notes

If `torch` fails to install, install it separately first:

```bash
pip install torch --index-url https://download.pytorch.org/whl/cpu
pip install -r requirements.txt
```

Training outputs (`results/`, `summaries/`) are written to the repo root and are gitignored.

See [docs/setup-checklist.md](../docs/setup-checklist.md) for the full local setup guide.
