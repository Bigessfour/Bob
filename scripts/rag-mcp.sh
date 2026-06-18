#!/usr/bin/env bash
# Stdio MCP entrypoint for Cursor (bob-rag server).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VENV="${REPO_ROOT}/python/.venv"

if [[ ! -x "${VENV}/bin/python" ]]; then
	echo "bob-rag MCP: python venv missing at ${VENV}" >&2
	echo "Run: ./scripts/setup-python.sh && ./scripts/rag-setup.sh" >&2
	exit 1
fi

cd "${REPO_ROOT}/python"
exec "${VENV}/bin/python" -m rag.mcp_server
