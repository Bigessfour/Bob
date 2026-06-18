#!/usr/bin/env bash
# Stdio MCP entrypoint for Cursor (bob-unity server → MCP for Unity).
set -euo pipefail

# GUI-launched Cursor may not inherit shell PATH; include common Homebrew locations.
export PATH="/opt/homebrew/bin:/usr/local/bin:${PATH-}"

UVX="${UVX:-uvx}"
if ! command -v "${UVX}" >/dev/null 2>&1; then
	echo "bob-unity MCP: uvx not found." >&2
	echo "Install uv: brew install uv  (or see https://docs.astral.sh/uv/)" >&2
	echo "Then open Unity → Window → MCP for Unity and complete the setup wizard." >&2
	exit 1
fi

# Unity Editor must be open with MCP for Unity bridge running (Window → MCP for Unity).
exec "${UVX}" --from mcpforunityserver mcp-for-unity --transport stdio
