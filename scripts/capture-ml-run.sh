#!/usr/bin/env bash
# Post-training capture: dashboards, timeline comparison, session summary.
# Usage:
#   ./scripts/capture-ml-run.sh bob-v4.7-ext 2026-07-20T11:02:18
#   ./scripts/capture-ml-run.sh   # reads docs/results/<run_id>_session.meta.json if present
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}/python"

RUN_ID="${1:-bob-v4.7-ext}"
SINCE="${2-}"

META="${REPO_ROOT}/docs/results/${RUN_ID//./_}_session.meta.json"
if [[ -z ${SINCE} && -f ${META} ]]; then
	SINCE="$(${PYTHON:-python3} -c "import json; print(json.load(open('${META}'))['started_at_utc'])")"
fi

if [[ -z ${SINCE} ]]; then
	echo "Usage: $0 <run_id> <since_utc>"
	echo "  or create docs/results/<run_id>_session.meta.json with started_at_utc"
	exit 1
fi

if [[ ! -d .venv ]]; then
	echo "Run ./scripts/setup-python.sh first"
	exit 1
fi

PYTHON="${REPO_ROOT}/python/.venv/bin/python3.10"
if [[ ! -x ${PYTHON} ]]; then
	PYTHON="${REPO_ROOT}/python/.venv/bin/python3"
fi

# shellcheck disable=SC1091
source .venv/bin/activate 2>/dev/null || true

${PYTHON} -m pip install -q matplotlib 2>/dev/null || true

SAFE="${RUN_ID//./_}"
DASH="../docs/results/${SAFE}_learning_dashboard.png"
PROG="../docs/results/${SAFE}_training_progress.png"
COMPARE="../docs/results/bob_ml_timeline_comparison.png"

echo "=== Capture ${RUN_ID} since ${SINCE} ==="

${PYTHON} scripts/plot_learning_dashboard.py \
	--since "${SINCE}" \
	--title "${RUN_ID}" \
	--output "${DASH}" \
	--check-pass || true

${PYTHON} scripts/plot_training_progress.py \
	--since "${SINCE}" \
	--output "${PROG}" 2>/dev/null ||
	${PYTHON} scripts/plot_training_progress.py --output "${PROG}"

${PYTHON} scripts/plot_run_comparison.py \
	--window "bob-v4:2026-07-17T22:59:00" \
	--window "bob-v4.6-residual:2026-07-20T00:30:00" \
	--window "bob-v4.7-curriculum:2026-07-20T03:06:00" \
	--window "${RUN_ID}:${SINCE}" \
	--output "${COMPARE}" \
	--check-demo-bar || true

${PYTHON} scripts/review_training_run.py --since "${SINCE}" | tail -30

echo ""
echo "Artifacts:"
echo "  ${DASH}"
echo "  ${PROG}"
echo "  ${COMPARE}"
echo "  summaries/bob_session.csv (live during run)"
echo "  results/${RUN_ID}/ (checkpoints, TensorBoard)"
echo ""
echo "Update docs/design/training-chronicle.md and docs/design/ml-project-timeline.md"
