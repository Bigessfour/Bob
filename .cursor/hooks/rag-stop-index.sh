#!/usr/bin/env bash
# Re-index files touched during the agent turn (working tree changes).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PYTHON="${REPO_ROOT}/python/.venv/bin/python"

if [[ ! -x ${PYTHON} ]]; then
	exit 0
fi

cd "${REPO_ROOT}"

mapfile -t changed < <(
	{
		git diff --name-only HEAD 2>/dev/null || true
		git diff --name-only --cached 2>/dev/null || true
		git ls-files --others --exclude-standard 2>/dev/null || true
	} | sort -u | grep -E '\.(cs|py|md|yaml|yml|tf|sh|mdc|asmdef)$' || true
)

if [[ ${#changed[@]} -eq 0 ]]; then
	exit 0
fi

(cd "${REPO_ROOT}/python" && "${PYTHON}" scripts/rag_index.py --paths "${changed[@]}" >/dev/null 2>&1) || true
exit 0
