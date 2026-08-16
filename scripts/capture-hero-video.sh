#!/usr/bin/env bash
# Silent batchmode GIF/play capture. For the classmate talk track (solver wow
# then InferenceOnly ONNX, spoken labels), use QuickTime and follow
# docs/showcase-capture.md — do not treat this script as the showcase video.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LABEL="${1:-bob-hero-capture}"

echo "=== Bob hero capture (${LABEL}) ==="
echo "Batchmode GIF only. Classmate video: docs/showcase-capture.md"
echo "Uses capture-progress.sh --play (Editor closed required for batchmode)."

"${REPO_ROOT}/scripts/capture-progress.sh" --play "${LABEL}"

echo "Capture complete. Check docs/progress/ for output + meta.json."
