#!/usr/bin/env bash
# Pre-merge release checklist for Bob (local gate; CI runs pytest/terraform/docker separately).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}"

echo "=== Bob release checklist ==="
FAIL=0

run_step() {
	local name="$1"
	shift
	echo ""
	echo ">> ${name}"
	if "$@"; then
		echo "   OK: ${name}"
	else
		echo "   FAIL: ${name}" >&2
		FAIL=1
	fi
}

run_step "Scene validation (batchmode)" bash ./scripts/validate-scene.sh
run_step "Unity alignment pytest" bash -c 'cd python && pytest tests/test_unity_alignment.py -q'
run_step "Core pytest (non-RAG)" bash -c 'cd python && pytest tests/ -q -m "not rag"'

if [[ -f docs/results/training_progress.png ]]; then
	echo ""
	echo ">> training_progress.png present"
else
	echo ""
	echo ">> WARN: docs/results/training_progress.png missing (refresh after bob-v4 train)"
fi

if [[ -d docs/portfolio-site ]]; then
	echo ">> portfolio-site scaffold present"
else
	echo ">> FAIL: docs/portfolio-site missing" >&2
	FAIL=1
fi

echo ""
if [[ ${FAIL} -eq 0 ]]; then
	echo "RELEASE_CHECKLIST_OK"
	exit 0
fi

echo "RELEASE_CHECKLIST_FAIL"
exit 1
