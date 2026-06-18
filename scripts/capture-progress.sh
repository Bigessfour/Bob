#!/usr/bin/env bash
# Capture a progress screenshot from the BobTraining scene into docs/progress/.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LABEL="${1:-snapshot}"
LOG_FILE="${REPO_ROOT}/logs/unity-capture.log"

export BOB_CAPTURE_LABEL="${LABEL}"
BOB_CAPTURE_GIT_SHA="$(git -C "${REPO_ROOT}" rev-parse --short HEAD 2>/dev/null || true)"
export BOB_CAPTURE_GIT_SHA

mkdir -p "${REPO_ROOT}/logs" "${REPO_ROOT}/docs/progress"

echo "Ensuring HDRP pipeline and material library..."
"${REPO_ROOT}/scripts/unity.sh" -batchmode -quit -nographics \
	-logFile "${REPO_ROOT}/logs/unity-capture-hdrp.log" \
	-executeMethod ArcAcademyHdrpSetup.EnsureHdrpFromCli

echo "Capturing progress screenshot (GPU required)..."
"${REPO_ROOT}/scripts/unity.sh" -batchmode -quit \
	-logFile "${LOG_FILE}" \
	-executeMethod BobProgressCapture.CaptureFromCli

if ! grep -q "CAPTURE_OK:" "${LOG_FILE}"; then
	echo "Progress capture failed. See ${LOG_FILE}"
	exit 1
fi

grep "CAPTURE_OK:" "${LOG_FILE}" | tail -1
