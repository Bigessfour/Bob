#!/usr/bin/env bash
# Inject RAG context before code-editing tool calls.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PYTHON="${REPO_ROOT}/python/.venv/bin/python"
INPUT="$(cat)"

if [[ ! -x ${PYTHON} ]]; then
	printf '%s\n' '{"permission":"allow","agent_message":"RAG unavailable. Run ./scripts/rag-setup.sh && ./scripts/rag-index.sh, then call rag_query before editing code."}'
	exit 0
fi

if ! command -v jq >/dev/null 2>&1; then
	printf '%s\n' '{"permission":"allow","agent_message":"Install jq for automatic RAG injection hooks, or manually call rag_query before code edits."}'
	exit 0
fi

tool_name="$(echo "${INPUT}" | jq -r '.tool_name // empty')"
path="$(echo "${INPUT}" | jq -r '.tool_input.path // .tool_input.target_notebook // empty')"
old_string="$(echo "${INPUT}" | jq -r '.tool_input.old_string // empty' | head -c 200)"

query="Repository patterns and constraints for ${path:-code change}"
if [[ -n ${old_string} ]]; then
	query="${query}. Related code: ${old_string}"
fi

context="$(
	cd "${REPO_ROOT}/python" &&
		"${PYTHON}" scripts/rag_query.py --file "${path:-Assets/}" -q "${query}" 2>/dev/null || true
)"

if [[ -z ${context} ]]; then
	context="RAG query failed. Call MCP rag_query manually before editing ${path:-files}."
fi

agent_message="**Mandatory RAG context before ${tool_name}**\n\n${context}\n\nIf you have not called rag_query this turn, do so now and align the edit with retrieved patterns."

jq -n --arg msg "${agent_message}" '{permission:"allow", agent_message:$msg}'
exit 0
