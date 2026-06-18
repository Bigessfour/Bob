#!/usr/bin/env bash
# Stdio MCP fallback for CoplayDev MCP for Unity (not the default Cursor path).
#
# Default Bob + Cursor setup uses HTTP in .cursor/mcp.json:
#   "unityMCP": { "url": "http://127.0.0.1:8080/mcp" }
# Match Unity Editor transport to HTTP (Window → MCP for Unity → Auto-Setup).
#
# Use this script only if you intentionally switch both Unity and .cursor/mcp.json to stdio.
set -euo pipefail

export PATH="/opt/homebrew/bin:/usr/local/bin:${PATH-}"

UVX="${UVX:-uvx}"
if ! command -v "${UVX}" >/dev/null 2>&1; then
	echo "unityMCP stdio fallback: uvx not found." >&2
	echo "Install uv: brew install uv  (or see https://docs.astral.sh/uv/)" >&2
	echo "Prefer HTTP: open Unity → Window → MCP for Unity → Auto-Setup." >&2
	exit 1
fi

exec "${UVX}" --from mcpforunityserver mcp-for-unity --transport stdio
