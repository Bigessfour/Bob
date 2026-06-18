#!/usr/bin/env bash
# Remind agents to consult bob-unity MCP before Unity-related code edits.
set -euo pipefail

INPUT="$(cat)"

if ! command -v jq >/dev/null 2>&1; then
	printf '%s\n' '{"permission":"allow","agent_message":"Install jq for Unity MCP pre-edit hooks, or manually consult bob-unity MCP before Unity edits (see docs/unity-mcp.md)."}'
	exit 0
fi

tool_name="$(echo "${INPUT}" | jq -r '.tool_name // empty')"
path="$(echo "${INPUT}" | jq -r '.tool_input.path // .tool_input.target_notebook // empty')"

is_unity=false
case "${path}" in
Assets/* | Packages/* | ProjectSettings/* | *.unity | *.prefab | *.asset | *.asmdef)
	is_unity=true
	;;
scripts/unity.sh | scripts/capture-progress.sh)
	is_unity=true
	;;
*) ;;
esac

if [[ ${is_unity} != "true" ]]; then
	printf '%s\n' '{"permission":"allow"}'
	exit 0
fi

agent_message="$(
	cat <<EOF
**Mandatory Unity MCP consultation before ${tool_name} on \`${path}\`**

Before implementing this Unity change, call **bob-unity** MCP tools to inspect live Editor state and use correct parameter schemas:

1. \`manage_scene\` — \`action: get_active\` and/or \`get_hierarchy\` for scene context
2. \`find_gameobjects\` — locate Bob, hoop, ball, and other targets before hierarchy edits
3. \`manage_components\` — read/set Behavior Parameters, Rigidbody, colliders; Behavior Name must be \`Bob\` (matches \`config/bob_free_throw.yaml\`)
4. \`read_console\` — verify no errors after changes

**Prerequisites:** Unity Editor open on this project; **Window → MCP for Unity** bridge connected (green status).
Read tool schemas from bob-unity MCP descriptors — do not guess parameter shapes.
Full workflow: docs/unity-mcp.md

If bob-unity is unavailable, use batchmode CLI fallbacks (\`./scripts/unity.sh -executeMethod ...\`) where possible and end the turn with **Further development required**.
EOF
)"

jq -n --arg msg "${agent_message}" '{permission:"allow", agent_message:$msg}'
exit 0
