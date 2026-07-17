#!/usr/bin/env bash
# Pull the pinned AI Review Ollama model if not already present (local Mac).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MODEL_FILE="${REPO_ROOT}/.ai-review-models.txt"
MODEL="$(grep -v '^#' "${MODEL_FILE}" | grep -v '^[[:space:]]*$' | head -1 | tr -d '[:space:]')"

if [[ -z ${MODEL} ]]; then
	echo "No model pinned in ${MODEL_FILE}" >&2
	exit 1
fi

if ! command -v ollama >/dev/null 2>&1; then
	echo "Install Ollama from https://ollama.com then re-run this script." >&2
	exit 1
fi

echo "=== Bob AI Review — Ollama model: ${MODEL} ==="

if ollama show "${MODEL}" >/dev/null 2>&1; then
	echo "Model already present (cached in ~/.ollama/models)."
else
	echo "Pulling model (one-time download)..."
	ollama pull "${MODEL}"
fi

echo "OK. Run reviews locally with: pip install xai-review && ai-review run-summary"
echo "Or trigger GitHub Actions: AI Review workflow."
