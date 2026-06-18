#!/usr/bin/env bash
# Install RAG dependencies (ChromaDB + MCP) into python/.venv.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VENV="${REPO_ROOT}/python/.venv"

if [[ ! -x "${VENV}/bin/python" ]]; then
	echo "Python venv missing. Run ./scripts/setup-python.sh first."
	exit 1
fi

"${VENV}/bin/python" -m pip install -r "${REPO_ROOT}/python/requirements-rag.txt"
echo "RAG dependencies installed."
echo "Next: ./scripts/rag-index.sh"
