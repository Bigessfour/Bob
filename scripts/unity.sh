#!/usr/bin/env bash
# Unity Editor CLI wrapper for Bob (batchmode / headless tasks).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_VERSION="${UNITY_VERSION:-6000.5.0f1}"
UNITY_BIN="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="${UNITY_PROJECT_PATH:-${REPO_ROOT}}"

if [[ ! -x ${UNITY_BIN} ]]; then
	echo "Unity not found at: ${UNITY_BIN}"
	echo "Set UNITY_VERSION or install Unity ${UNITY_VERSION} via Unity Hub."
	exit 1
fi

if [[ ! -d "${PROJECT_PATH}/Assets" ]]; then
	echo "Unity project not found at: ${PROJECT_PATH}"
	echo "Create a 3D project at the repo root, or set UNITY_PROJECT_PATH."
	exit 1
fi

exec "${UNITY_BIN}" \
	-projectPath "${PROJECT_PATH}" \
	"$@"
