#!/usr/bin/env bash
# Official Unity MCP relay for Cursor (stdio → ~/.unity/relay → Unity Editor bridge).
# Requires Unity Editor open on THIS repo root (folder with Assets/ + config/ + python/);
# approve Cursor under Edit → Project Settings → AI → Unity MCP.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
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

# Bind MCP to this repo only. Opening a non-repo folder in Hub (empty Assets/, no
# BobTraining.unity) yields an empty project and zero Unity MCP tools.
if [[ ! -f "${REPO_ROOT}/Assets/Scenes/BobTraining.unity" ]]; then
	echo "unity-mcp: ${REPO_ROOT} is not the Bob Unity repo (missing Assets/Scenes/BobTraining.unity)." >&2
	exit 1
fi

# Soft warning when Editor is clearly on a different project path (tools stay empty).
if command -v pgrep >/dev/null 2>&1; then
	unity_paths="$(pgrep -lf 'Unity.app/Contents/MacOS/Unity' 2>/dev/null | grep -o '\-projectpath [^ ]*' | sed 's/-projectpath //' || true)"
	if [[ -n ${unity_paths} ]] && ! printf '%s\n' "${unity_paths}" | grep -Fxq "${REPO_ROOT}"; then
		echo "unity-mcp: warning — Unity is open on a different path:" >&2
		printf '  %s\n' ${unity_paths} >&2
		echo "unity-mcp: open this folder in Unity Hub instead: ${REPO_ROOT}" >&2
	fi
fi

exec "${EXEC}" --mcp --project-path "${REPO_ROOT}" --name "Bob" "$@"
