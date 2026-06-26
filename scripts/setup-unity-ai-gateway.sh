#!/usr/bin/env bash
# Configure Unity AI Assistant / AI Gateway to use xAI Grok as BYOM (via Codex agent).
# Never commits secrets — reads from macOS Keychain or existing .env.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ENV_FILE="${REPO_ROOT}/.env"
ENV_EXAMPLE="${REPO_ROOT}/.env.example"
CODEX_CONFIG_DIR="${REPO_ROOT}/.codex"
CODEX_CONFIG="${CODEX_CONFIG_DIR}/config.toml"
# Documented for manual Unity prefs / gateway relay setup (not read by this script yet).
# shellcheck disable=SC2034
UNITY_PREFS="${HOME}/Library/Preferences/com.unity3d.UnityEditor5.x.plist"
# shellcheck disable=SC2034
GATEWAY_PREFS_KEY="Unity.AI.Gateway.Relay.envVars.preferences"

XAI_BASE_URL="${XAI_BASE_URL:-https://api.x.ai/v1}"
GROK_MODEL="${GROK_MODEL:-grok-4.3}"
UNITY_GATEWAY_AGENT="${UNITY_GATEWAY_AGENT:-codex}"

usage() {
	cat <<'EOF'
Usage: ./scripts/setup-unity-ai-gateway.sh [--test-api] [--sync-env] [--configure-codex]

Configures Bob to route Unity AI Assistant prompts through xAI Grok (BYOM).

Key retrieval (macOS Passwords / Keychain):
  security find-generic-password -s "XAI_API_KEY" -a "xai" -w

If the key is missing, add it in Passwords or export XAI_API_KEY before running.

Options:
  --test-api         Call xAI /v1/models and print status (no secret output)
  --sync-env         Write XAI_API_KEY into .env (gitignored) from Keychain
  --configure-codex  Write project .codex/config.toml for Unity bundled Codex agent
  (default)          sync-env + configure-codex + gateway plist hint
EOF
}

load_xai_api_key() {
	if [[ -n ${XAI_API_KEY-} ]]; then
		return 0
	fi

	if [[ -f ${ENV_FILE} ]]; then
		# shellcheck disable=SC1090
		set -a
		source "${ENV_FILE}"
		set +a
		if [[ -n ${XAI_API_KEY-} ]]; then
			return 0
		fi
	fi

	local key=""
	key="$(security find-generic-password -s "XAI_API_KEY" -a "xai" -w 2>/dev/null || true)"
	if [[ -z ${key} ]]; then
		key="$(security find-generic-password -s "XAI_API_KEY" -a "XAI_API_KEY" -w 2>/dev/null || true)"
	fi

	if [[ -n ${key} ]]; then
		export XAI_API_KEY="${key}"
		return 0
	fi

	echo "ERROR: XAI_API_KEY not found."
	echo '  Keychain: security find-generic-password -s "XAI_API_KEY" -a "xai" -w'
	echo "  Or copy from Passwords app → paste into .env as XAI_API_KEY=..."
	return 1
}

sync_env_file() {
	load_xai_api_key
	if [[ ! -f ${ENV_FILE} ]]; then
		cp "${ENV_EXAMPLE}" "${ENV_FILE}"
	fi

	if grep -q '^XAI_API_KEY=' "${ENV_FILE}"; then
		# Preserve existing .env value; only fill when empty.
		local current=""
		current="$(grep '^XAI_API_KEY=' "${ENV_FILE}" | cut -d= -f2-)"
		if [[ -z ${current} && -n ${XAI_API_KEY-} ]]; then
			sed -i '' "s|^XAI_API_KEY=.*|XAI_API_KEY=${XAI_API_KEY}|" "${ENV_FILE}"
		fi
	else
		echo "XAI_API_KEY=${XAI_API_KEY}" >>"${ENV_FILE}"
	fi

	upsert_env_var "XAI_BASE_URL" "${XAI_BASE_URL}"
	upsert_env_var "GROK_MODEL" "${GROK_MODEL}"
	upsert_env_var "UNITY_AI_GATEWAY_AGENT" "${UNITY_GATEWAY_AGENT}"
	upsert_env_var "OPENAI_BASE_URL" "${XAI_BASE_URL}"
	upsert_env_var "OPENAI_API_KEY" "${XAI_API_KEY}"

	echo "OK: synced ${ENV_FILE} (XAI_API_KEY not printed)"
}

upsert_env_var() {
	local name="$1"
	local value="$2"
	if grep -q "^${name}=" "${ENV_FILE}"; then
		sed -i '' "s|^${name}=.*|${name}=${value}|" "${ENV_FILE}"
	else
		echo "${name}=${value}" >>"${ENV_FILE}"
	fi
}

write_codex_config() {
	load_xai_api_key
	mkdir -p "${CODEX_CONFIG_DIR}"
	cat >"${CODEX_CONFIG}" <<EOF
# Bob — Unity AI Gateway BYOM (xAI Grok via OpenAI-compatible Codex agent)
# Secrets: OPENAI_API_KEY / XAI_API_KEY from .env or Keychain (never commit)

model_provider = "xai"
model = "${GROK_MODEL}"

[model_providers.xai]
name = "xAI Grok"
base_url = "${XAI_BASE_URL}"
env_key = "OPENAI_API_KEY"
wire_api = "chat"
EOF
	echo "OK: wrote ${CODEX_CONFIG}"
}

test_xai_api() {
	load_xai_api_key
	local http_code=""
	local body_file=""
	body_file="$(mktemp)"
	http_code="$(
		curl -sS -o "${body_file}" -w "%{http_code}" \
			-H "Authorization: Bearer ${XAI_API_KEY}" \
			-H "Content-Type: application/json" \
			"${XAI_BASE_URL}/models"
	)"

	if [[ ${http_code} != "200" ]]; then
		echo "FAIL: xAI API returned HTTP ${http_code}"
		head -c 400 "${body_file}" || true
		echo
		rm -f "${body_file}"
		return 1
	fi

	local model_count=""
	model_count="$(
		python3 - "${body_file}" <<'PY'
import json, sys
with open(sys.argv[1]) as f:
    data = json.load(f)
models = data.get("data") or []
print(len(models))
grok = [m.get("id") for m in models if "grok" in (m.get("id") or "").lower()]
print("sample:", ", ".join(grok[:5]) if grok else "none")
PY
	)"
	echo "OK: xAI API connected — ${model_count}"
	rm -f "${body_file}"
	return 0
}

print_gateway_instructions() {
	cat <<EOF

Unity Editor (one-time, with project open):
  1. Project Settings → AI → Gateway
  2. Agent Type: Codex
  3. Environment variables:
       OPENAI_API_KEY  = (same as XAI_API_KEY — loaded from Keychain/.env)
       OPENAI_BASE_URL = ${XAI_BASE_URL}
  4. Enable Codex provider; disable Unity credits agent as default
  5. Assistant window → agent selector → Codex → model ${GROK_MODEL}

Bob auto-applies gateway env on Editor load via Assets/Editor/BobAiGatewayBootstrap.cs
when XAI_API_KEY or OPENAI_API_KEY is set in the environment.

Docs: docs/unity-ai-gateway.md
EOF
}

DO_TEST=0
DO_SYNC=0
DO_CODEX=0
DO_ALL=1

for arg in "$@"; do
	case "${arg}" in
	--test-api)
		DO_TEST=1
		DO_ALL=0
		;;
	--sync-env)
		DO_SYNC=1
		DO_ALL=0
		;;
	--configure-codex)
		DO_CODEX=1
		DO_ALL=0
		;;
	-h | --help)
		usage
		exit 0
		;;
	*)
		echo "Unknown option: ${arg}"
		usage
		exit 1
		;;
	esac
done

if [[ ${DO_ALL} -eq 1 || ${DO_SYNC} -eq 1 ]]; then
	sync_env_file
fi
if [[ ${DO_ALL} -eq 1 || ${DO_CODEX} -eq 1 ]]; then
	write_codex_config
fi
if [[ ${DO_ALL} -eq 1 || ${DO_TEST} -eq 1 ]]; then
	test_xai_api
fi

if [[ ${DO_ALL} -eq 1 ]]; then
	print_gateway_instructions
fi
