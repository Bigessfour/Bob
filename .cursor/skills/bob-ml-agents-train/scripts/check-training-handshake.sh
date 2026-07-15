#!/usr/bin/env bash
# Preflight before Bob ML-Agents training. Run from repo root.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../../../../" && pwd)"
cd "${REPO_ROOT}"

echo "=== Bob training preflight ==="

if docker info >/dev/null 2>&1; then
	echo "Docker: OK"
else
	echo "Docker: NOT RUNNING — start Docker Desktop, then retry"
	exit 1
fi

if lsof -i :5004 >/dev/null 2>&1; then
	echo "Port 5004: IN USE (stale trainer?)"
	echo "  Fix: docker compose down --remove-orphans"
	lsof -i :5004 | head -3
else
	echo "Port 5004: free"
fi

echo ""
echo "Next: ./scripts/train.sh (or RUN_ID=bob-v4 ./scripts/train.sh --force)"
echo "      Wait for Listening on port 5004 → Unity Play ONCE → BOB_TRAINING_OK"
echo "      Do NOT edit C# during training."
