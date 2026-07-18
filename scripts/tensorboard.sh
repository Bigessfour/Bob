#!/usr/bin/env bash
# Dev-only TensorBoard for ML-Agents results/ (not audience UI).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}"

PORT="${PORT:-6006}"
LOGDIR="${LOGDIR:-results}"

if [[ -x python/.venv/bin/tensorboard ]]; then
	TB=(python/.venv/bin/tensorboard)
elif command -v tensorboard >/dev/null 2>&1; then
	TB=(tensorboard)
else
	echo "tensorboard not found. Run ./scripts/setup-python.sh first." >&2
	exit 1
fi

echo "TensorBoard → http://localhost:${PORT}  (logdir=${LOGDIR})"
echo "Custom Environment/* gauges need a connected PPO run after StatsRecorder wiring."
exec "${TB[@]}" --logdir "${LOGDIR}" --port "${PORT}"
