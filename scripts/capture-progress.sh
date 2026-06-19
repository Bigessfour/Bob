#!/usr/bin/env bash
# Capture a progress screenshot from the BobTraining scene into docs/progress/.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PLAY_MODE=0
LABEL=""

for arg in "$@"; do
	case "${arg}" in
	--play) PLAY_MODE=1 ;;
	-h | --help)
		cat <<'EOF'
Usage: ./scripts/capture-progress.sh [--play] <milestone-label>

  --play   Enter Play mode, wait for HDRP settle, then capture (GPU required)
  default  Edit-mode Main Camera capture

Environment:
  BOB_CAPTURE_PLAY_FRAMES  Physics frames to wait in play mode (default: 120)
  BOB_CAPTURE_WIDTH        Override width (default: 1280)
  BOB_CAPTURE_HEIGHT       Override height (default: 720)

Note: Close the Unity Editor before running — batchmode cannot open a project
that is already open in another instance.
EOF
		exit 0
		;;
	-*)
		echo "Unknown option: ${arg}" >&2
		exit 1
		;;
	*)
		if [[ -n ${LABEL} ]]; then
			echo "Unexpected extra argument: ${arg}" >&2
			exit 1
		fi
		LABEL="${arg}"
		;;
	esac
done

LABEL="${LABEL:-snapshot}"
LOG_FILE="${REPO_ROOT}/logs/unity-capture.log"

export BOB_CAPTURE_LABEL="${LABEL}"
BOB_CAPTURE_GIT_SHA="$(git -C "${REPO_ROOT}" rev-parse --short HEAD 2>/dev/null || true)"
export BOB_CAPTURE_GIT_SHA

mkdir -p "${REPO_ROOT}/logs" "${REPO_ROOT}/docs/progress"

if [[ ${PLAY_MODE} -eq 1 ]]; then
	CAPTURE_METHOD="BobProgressCapture.CapturePlayModeFromCli"
	MODE_NAME="play"
else
	CAPTURE_METHOD="BobProgressCapture.CaptureFromCli"
	MODE_NAME="edit"
fi

echo "Ensuring HDRP pipeline and material library..."
"${REPO_ROOT}/scripts/unity.sh" -batchmode -quit -nographics \
	-logFile "${REPO_ROOT}/logs/unity-capture-hdrp.log" \
	-executeMethod ArcAcademyHdrpSetup.EnsureHdrpFromCli

echo "Capturing progress screenshot (${MODE_NAME} mode, GPU required)..."
if [[ ${PLAY_MODE} -eq 1 ]]; then
	# Play mode is async (EnterPlaymode → wait frames → capture → Exit). Do not pass -quit
	# or Unity shuts down before EnteredPlayMode fires.
	"${REPO_ROOT}/scripts/unity.sh" -batchmode \
		-logFile "${LOG_FILE}" \
		-executeMethod "${CAPTURE_METHOD}"
else
	"${REPO_ROOT}/scripts/unity.sh" -batchmode -quit \
		-logFile "${LOG_FILE}" \
		-executeMethod "${CAPTURE_METHOD}"
fi

if ! grep -q "CAPTURE_OK:" "${LOG_FILE}"; then
	echo "Progress capture failed. See ${LOG_FILE}"
	exit 1
fi

grep "CAPTURE_OK:" "${LOG_FILE}" | tail -1
