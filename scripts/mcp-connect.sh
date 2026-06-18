#!/usr/bin/env bash
# Ensure Unity MCP HTTP server is running for Cursor (unityMCP → :8080/mcp).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="/opt/homebrew/bin:/usr/local/bin:${PATH-}"

if lsof -iTCP:8080 -sTCP:LISTEN >/dev/null 2>&1; then
	echo "unityMCP HTTP server already listening on 127.0.0.1:8080"
else
	echo "Starting unityMCP HTTP server..."
	"${REPO_ROOT}/scripts/unity-mcp-http.sh" &
	sleep 3
fi

if lsof -iTCP:8080 -sTCP:LISTEN >/dev/null 2>&1; then
	echo "OK: http://127.0.0.1:8080/mcp"
	echo "In Unity: Bob → MCP → Configure Project Connection (or Window → MCP for Unity → Start Bridge)"
else
	echo "FAIL: HTTP server did not start on port 8080" >&2
	exit 1
fi
