#!/usr/bin/env bash
# Bootstrap RAG index on agent session start if empty.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PYTHON="${REPO_ROOT}/python/.venv/bin/python"

if [[ ! -x ${PYTHON} ]]; then
	printf '%s\n' '{"additional_context":"Bob RAG is not installed. Run ./scripts/setup-python.sh && ./scripts/rag-setup.sh && ./scripts/rag-index.sh"}'
	exit 0
fi

chunk_count="$(
	cd "${REPO_ROOT}/python" && "${PYTHON}" -c "from rag.store import collection_stats; print(collection_stats().get('chunk_count', 0))"
)" || chunk_count="0"

if [[ ${chunk_count} == "0" ]]; then
	(cd "${REPO_ROOT}/python" && "${PYTHON}" scripts/rag_index.py >/dev/null 2>&1) || true
fi

printf '%s\n' '{"additional_context":"Bob North Star: read docs/what-right-looks-like.md before planning or scoping — align with current week milestone and PR/CI workflow. Bob RAG active. Before any code edit, call MCP tool rag_query (or run python/scripts/rag_query.py). After significant method changes, call rag_index_paths or rely on the stop hook to refresh the index. Unity MCP (bob-unity): before any Unity task (Assets/, ProjectSettings/, scene/agent work), consult bob-unity MCP tools with Unity Editor open — see docs/unity-mcp.md."}'
exit 0
