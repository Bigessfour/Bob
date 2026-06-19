#!/usr/bin/env bash
# Run ML-Agents training via Docker (recommended on Apple Silicon).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}"

RUN_ID="${RUN_ID:-bob-v0}"
CONFIG="${CONFIG:-config/bob_free_throw.yaml}"
RESULTS_DIR="results/${RUN_ID}"

TRAIN_CMD=(mlagents-learn "${CONFIG}" --run-id="${RUN_ID}")

# ML-Agents refuses to start when results/<run-id> already exists unless --resume or --force.
if [[ -d ${RESULTS_DIR} ]]; then
	has_resume_or_force=false
	for arg in "$@"; do
		case "${arg}" in
		--resume | --force) has_resume_or_force=true ;;
		esac
	done
	if [[ ${has_resume_or_force} == false ]]; then
		echo "Resuming existing run '${RUN_ID}' (${RESULTS_DIR} found)."
		echo "  Fresh run:  RUN_ID=bob-v1 ./scripts/train.sh"
		echo "  Overwrite:  ./scripts/train.sh --force"
		echo ""
		TRAIN_CMD+=(--resume)
	fi
fi

if (("$#")); then
	TRAIN_CMD+=("$@")
fi

mkdir -p results summaries

echo "Clearing stale trainer containers (port 5004)..."
docker compose down --remove-orphans 2>/dev/null || true

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
echo "If port 5004 is busy (stale container): docker compose down && docker container prune -f"
echo ""
# --service-ports publishes 5004:5004 from docker-compose.yml (required on Docker Desktop Mac).
docker compose run --rm --service-ports train "${TRAIN_CMD[@]}"
