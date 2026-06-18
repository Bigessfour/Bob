#!/usr/bin/env bash
# Rebuild the Bob repository RAG index (ChromaDB).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VENV="${REPO_ROOT}/python/.venv"

if [[ ! -x "${VENV}/bin/python" ]]; then
	echo "Python venv missing. Run ./scripts/setup-python.sh && ./scripts/rag-setup.sh"
	exit 1
fi

cd "${REPO_ROOT}/python"
exec "${VENV}/bin/python" scripts/rag_index.py "$@"
