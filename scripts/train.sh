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

echo "Starting trainer (Unity Editor must be open with BobTraining scene)..."
echo ""
echo "Training handshake order:"
echo "  1. Stop Play if already running (inference fallback blocks reconnect)."
echo "  2. Wait until this trainer prints it is waiting for a connection."
echo "  3. Press Play in Unity."
echo "  4. Confirm Editor console shows training steps (not inference fallback)."
echo ""
# --service-ports publishes 5004:5004 from docker-compose.yml (required on Docker Desktop Mac).
docker compose run --rm --service-ports train \
	mlagents-learn "${CONFIG}" --run-id="${RUN_ID}" "${EXTRA_ARGS[@]}"
