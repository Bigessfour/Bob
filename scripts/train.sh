#!/usr/bin/env bash
# Run ML-Agents training via Docker (recommended on Apple Silicon).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}"

RUN_ID="${RUN_ID:-bob-v0}"
CONFIG="${CONFIG:-config/bob_free_throw.yaml}"
EXTRA_ARGS=("$@")

mkdir -p results summaries

echo "Building bob-train image if needed..."
docker compose build train

echo "Starting trainer (Unity Editor must be open with training scene)..."
echo "Press Play in Unity when prompted."
docker compose run --rm train \
	mlagents-learn "${CONFIG}" --run-id="${RUN_ID}" "${EXTRA_ARGS[@]}"
