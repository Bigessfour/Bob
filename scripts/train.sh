#!/usr/bin/env bash
# Run ML-Agents training via Docker (recommended on Apple Silicon).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}"

RUN_ID="${RUN_ID:-bob-v0}"
CONFIG="${CONFIG:-config/bob_free_throw.yaml}"
RESULTS_DIR="results/${RUN_ID}"

TRAIN_CMD=(mlagents-learn "${CONFIG}" --run-id="${RUN_ID}")
CHECKPOINT="${RESULTS_DIR}/Bob/checkpoint.pt"

# ML-Agents refuses to start when results/<run-id> already exists unless --resume or --force.
has_resume_or_force=false
for arg in "$@"; do
	case "${arg}" in
	--resume | --force) has_resume_or_force=true ;;
	esac
done

if [[ -d ${RESULTS_DIR} && ${has_resume_or_force} == false ]]; then
	if [[ -f ${CHECKPOINT} ]]; then
		echo "Resuming existing run '${RUN_ID}' (${CHECKPOINT} found)."
		echo "  Fresh run:  RUN_ID=bob-v1 ./scripts/train.sh"
		echo "  Overwrite:  ./scripts/train.sh --force"
		echo ""
		TRAIN_CMD+=(--resume)
	else
		echo "Incomplete run '${RUN_ID}' (${RESULTS_DIR} exists but no checkpoint.pt)."
		echo "  Starting fresh with --force (avoids checkpoint.pt / onnxscript resume errors)."
		echo "  Or use:  RUN_ID=bob-v1 ./scripts/train.sh"
		echo ""
		TRAIN_CMD+=(--force)
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
echo "Verifying pinned torch in image (should be <=2.8.0):"
docker run --rm bob-train python -c "
import torch, sys
print('TRAIN_TORCH_VERSION:', torch.__version__)
if not any(x in torch.__version__ for x in ('2.1','2.2','2.3','2.4','2.5','2.6','2.7','2.8')):
    print('WARNING: torch may exceed cap; onnxscript errors possible. Rebuild with --no-cache.')
" 2>&1 | cat || true

echo "Starting trainer (Unity Editor must be open with BobTraining scene)..."
echo ""
echo "Training handshake order:"
echo "  1. Stop Play if already running (inference fallback blocks reconnect)."
echo "  2. Wait until this trainer prints it is waiting for a connection."
echo "  3. Wait for Unity compile spinner to finish (no script edits in progress)."
echo "  4. Press Play in Unity ONCE — do not toggle Play while trainer is running."
echo "  5. Confirm Editor console: BOB_TRAINING_OK (not BOB_TRAINING_LOST / compile-during-play)."
echo ""
echo "Stability (avoid trainer crash):"
echo "  - Do NOT save/edit Assets/Scripts while Play is running (forces domain reload → Communicator has exited)."
echo "  - Stop Play before Ctrl+C on trainer, or trainer may hit UnityTimeOutException on restart."
echo "  - If trainer shows 'Worker 0 exceeded restarts': stop trainer, Stop Play, fix compile, train.sh again."
echo ""
echo "If port 5004 is busy (stale container): docker compose down && docker container prune -f"
echo ""
# --service-ports publishes 5004:5004 from docker-compose.yml (required on Docker Desktop Mac).
docker compose run --rm --service-ports train "${TRAIN_CMD[@]}"
