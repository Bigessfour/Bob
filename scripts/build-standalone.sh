#!/usr/bin/env bash
# Build macOS standalone player for Bob (Apple Silicon native when Unity Editor is installed).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LOG_FILE="${REPO_ROOT}/logs/unity-build.log"
mkdir -p "${REPO_ROOT}/logs" "${REPO_ROOT}/builds/macos"

echo "=== Bob macOS standalone build ==="
echo "Note: Close the Unity Editor before batchmode build."

"${REPO_ROOT}/scripts/unity.sh" -batchmode -quit -nographics \
	-logFile "${LOG_FILE}" \
	-executeMethod BobBuildCli.BuildStandaloneMacFromCli

if ! grep -q "BOB_BUILD_OK:" "${LOG_FILE}"; then
	echo "Build failed. See ${LOG_FILE}"
	exit 1
fi

grep "BOB_BUILD_OK:" "${LOG_FILE}"
