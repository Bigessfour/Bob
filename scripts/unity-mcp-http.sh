#!/usr/bin/env bash
# Start the CoplayDev MCP for Unity HTTP server (Cursor connects to :8080/mcp).
# Unity Editor must also have Window → MCP for Unity → Start Bridge running.
set -euo pipefail

export PATH="/opt/homebrew/bin:/usr/local/bin:${PATH-}"

UVX="${UVX:-uvx}"
if ! command -v "${UVX}" >/dev/null 2>&1; then
	echo "unityMCP HTTP: uvx not found. Install: brew install uv" >&2
	exit 1
fi

exec "${UVX}" --from mcpforunityserver mcp-for-unity \
	--transport http \
	--http-host 127.0.0.1 \
	--http-port 8080
