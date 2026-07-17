#!/usr/bin/env bash
# Run AI Review locally via Ollama (requires Python 3.11 venv — separate from mlagents 3.10).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VENV="${REPO_ROOT}/python/.venv-ai-review"
PR_NUMBER="${1-}"

if [[ ! -x "${VENV}/bin/ai-review" ]]; then
	echo "Setting up ai-review venv (Python 3.11)..."
	/opt/homebrew/bin/python3.11 -m venv "${VENV}"
	"${VENV}/bin/pip" install -q 'xai-review>=0.71.0'
fi

if ! curl -sf http://127.0.0.1:11434/api/tags >/dev/null; then
	echo "Start Ollama (open app or: ollama serve)" >&2
	exit 1
fi

"${REPO_ROOT}/scripts/setup-ai-review-ollama.sh"

if [[ -z ${PR_NUMBER} ]]; then
	echo "Usage: $0 <pull-request-number> [--dry-run]" >&2
	echo "Example: $0 11 --dry-run" >&2
	exit 1
fi

DRY_RUN=false
if [[ ${2-} == "--dry-run" ]]; then
	DRY_RUN=true
fi

OWNER="$(gh repo view --json owner -q .owner.login)"
REPO="$(gh repo view --json name -q .name)"
TOKEN="$(gh auth token)"

cd "${REPO_ROOT}"
export LLM__PROVIDER=OLLAMA
export LLM__META__MODEL=qwen2.5-coder:7b
export LLM__HTTP_CLIENT__API_URL=http://127.0.0.1:11434
export VCS__PROVIDER=GITHUB
export VCS__PIPELINE__OWNER="${OWNER}"
export VCS__PIPELINE__REPO="${REPO}"
export VCS__PIPELINE__PULL_NUMBER="${PR_NUMBER}"
export VCS__HTTP_CLIENT__API_URL=https://api.github.com
export VCS__HTTP_CLIENT__API_TOKEN="${TOKEN}"
export REVIEW__DRY_RUN="${DRY_RUN}"

echo "=== ai-review run-summary PR #${PR_NUMBER} (dry_run=${DRY_RUN}) ==="
"${VENV}/bin/ai-review" run-summary
