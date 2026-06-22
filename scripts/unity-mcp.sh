#!/usr/bin/env bash
# Official Unity MCP relay for Cursor (stdio → ~/.unity/relay → Unity Editor bridge).
# Requires Unity Editor open on this project; approve Cursor under Edit → Project Settings → AI → Unity MCP.
set -euo pipefail

RELAY_ROOT="${HOME}/.unity/relay"
UNAME="$(uname -s)"
ARCH="$(uname -m)"

if [[ ${UNAME} == "Darwin" ]]; then
	if [[ ${ARCH} == "arm64" ]]; then
		EXEC="${RELAY_ROOT}/relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64"
	else
		EXEC="${RELAY_ROOT}/relay_mac_x64.app/Contents/MacOS/relay_mac_x64"
	fi
elif [[ ${UNAME} == MINGW* || ${UNAME} == MSYS* || ${UNAME} == CYGWIN* ]]; then
	EXEC="${RELAY_ROOT}/relay_win.exe"
else
	EXEC="${RELAY_ROOT}/relay_linux"
fi

if [[ ! -x ${EXEC} ]]; then
	echo "unity-mcp: relay not found at ${EXEC}" >&2
	echo "Open this project in Unity 6 once — the relay installs to ~/.unity/relay on Editor startup." >&2
	echo "Then Edit → Project Settings → AI → Unity MCP → Start bridge and approve Cursor." >&2
	exit 1
fi

exec "${EXEC}" --mcp "$@"
