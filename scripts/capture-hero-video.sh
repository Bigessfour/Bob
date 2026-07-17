#!/usr/bin/env bash
# Capture hero training GIF/video via play-mode progress capture pipeline.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LABEL="${1:-bob-hero-capture}"

echo "=== Bob hero capture (${LABEL}) ==="
echo "Uses capture-progress.sh --play (Editor closed required for batchmode)."

"${REPO_ROOT}/scripts/capture-progress.sh" --play "${LABEL}"

echo "Capture complete. Check docs/progress/ for output + meta.json."
