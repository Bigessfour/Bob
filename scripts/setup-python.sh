#!/usr/bin/env bash
# Create Python 3.10.12 venv for IDE/linting. On Apple Silicon, use Docker for mlagents-learn.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PYTHON_DIR="$REPO_ROOT/python"
VENV="$PYTHON_DIR/.venv"

cd "$PYTHON_DIR"

if ! command -v uv >/dev/null 2>&1; then
  echo "uv is required. Install: brew install uv"
  exit 1
fi

echo "Installing Python 3.10.12 via uv..."
uv python install 3.10.12

echo "Creating venv at python/.venv..."
rm -rf "$VENV"
uv venv --python 3.10.12 "$VENV"
# shellcheck disable=SC1091
source "$VENV/bin/activate"

python -m ensurepip --upgrade
python -m pip install --upgrade pip wheel
python -m pip install "setuptools==65.5.0"

ARCH="$(uname -m)"
if [[ "$ARCH" == "arm64" ]]; then
  echo ""
  echo "Apple Silicon detected: grpcio 1.48.2 has no macOS arm64 wheel."
  echo "Installing IDE/linting deps locally; use Docker for mlagents-learn (see scripts/train.sh)."
  python -m pip install \
    "numpy>=1.23.5,<1.24.0" \
    "matplotlib>=3.7,<3.9" \
    "tensorboard>=2.14" \
    "protobuf>=3.6,<3.21"
else
  python -m pip install -r requirements.txt
fi

echo ""
echo "Venv ready: $VENV/bin/python ($("$VENV/bin/python" --version))"
echo "Select interpreter in Cursor: $VENV/bin/python"
