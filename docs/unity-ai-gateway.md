# Unity AI Gateway — Grok (xAI) BYOM

Bob routes **Unity AI Assistant** prompts through **xAI Grok** using your own API key (BYOM), bypassing Unity AI credits.

Unity AI Assistant `2.12.0-pre.2` is listed in [`Packages/manifest.json`](../Packages/manifest.json). Unity’s AI Gateway supports **Codex**, **Claude Code**, **Gemini**, and **Cursor** agents — not a first-party Grok agent. Bob configures the **bundled Codex agent** against xAI’s OpenAI-compatible API (`https://api.x.ai/v1`).

| Layer          | Bob choice                           |
| -------------- | ------------------------------------ |
| Unity package  | `com.unity.ai.assistant`             |
| Gateway agent  | **Codex** (Unity-bundled)            |
| Model provider | **xAI Grok** (`grok-4.3` default)    |
| Secrets        | macOS Keychain / `.env` (gitignored) |

## Prerequisites

- Unity 6 project open with `com.unity.ai.assistant` resolved
- Unity organization seat with **AI Gateway** access (see [Unity docs](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.12/manual/integration/ai-gateway-get-started.html))
- xAI API key in macOS Passwords or `.env`

## One-time setup

### 1. Retrieve API key (macOS)

```bash
# Primary Keychain entry (xAI API Key / Grok CLI)
security find-generic-password -s "XAI_API_KEY" -a "xai" -w

# Alternate entry name
security find-generic-password -s "XAI_API_KEY" -a "XAI_API_KEY" -w
```

Or copy from **Passwords** → search `xAI` / `XAI_API_KEY` → paste when prompted.

### 2. Run Bob setup script

```bash
chmod +x scripts/setup-unity-ai-gateway.sh
./scripts/setup-unity-ai-gateway.sh
```

This script:

- Syncs `XAI_API_KEY`, `OPENAI_API_KEY`, `OPENAI_BASE_URL`, and `GROK_MODEL` into `.env` (never committed)
- Writes [`.codex/config.toml`](../.codex/config.toml) for the Codex agent
- Tests connectivity: `GET https://api.x.ai/v1/models`

Options:

```bash
./scripts/setup-unity-ai-gateway.sh --test-api          # API check only
./scripts/setup-unity-ai-gateway.sh --sync-env          # .env only
./scripts/setup-unity-ai-gateway.sh --configure-codex   # .codex/config.toml only
```

### 3. Unity Editor

1. Open Bob in Unity 6
2. **Bob → AI Gateway → Apply Grok BYOM Settings** (or wait for auto-apply on load)
3. **Project Settings → AI → Gateway**
   - Agent: **Codex**
   - Env: `OPENAI_API_KEY`, `OPENAI_BASE_URL=https://api.x.ai/v1`
4. **Window → AI Assistant** → agent selector → **Codex** → model **grok-4.3**

`Assets/Editor/BobAiGatewayBootstrap.cs` reads `.env` / environment and pushes credentials into Gateway preferences without writing secrets into `Assets/` or `ProjectSettings/`.

## Environment variables

See [`.env.example`](../.env.example):

| Variable                 | Purpose                                   |
| ------------------------ | ----------------------------------------- |
| `XAI_API_KEY`            | xAI API key (primary)                     |
| `OPENAI_API_KEY`         | Same key for Codex OpenAI-compatible wire |
| `OPENAI_BASE_URL`        | `https://api.x.ai/v1`                     |
| `GROK_MODEL`             | Default model (`grok-4.3`)                |
| `UNITY_AI_GATEWAY_AGENT` | Gateway agent id (`codex`)                |

## Verify Grok connection

```bash
./scripts/setup-unity-ai-gateway.sh --test-api
```

Expected: `OK: xAI API connected` with a Grok model sample list.

In Unity, send a short prompt in **Assistant** with **Codex** selected. Console should show `BOB_AI_GATEWAY_OK` on load.

## Relationship to Cursor MCP

| Tool                             | Role                                                                                    |
| -------------------------------- | --------------------------------------------------------------------------------------- |
| **Cursor + `unity-mcp`**         | Primary agent bridge (official Unity relay → Editor) — see [unity-mcp.md](unity-mcp.md) |
| **Unity AI Assistant + Gateway** | In-Editor Assistant with BYOM Grok via Codex                                            |

Both can coexist. Bob’s Cursor workflow uses **`unity-mcp`**; Unity Assistant BYOM is for in-Editor prompts without Unity credits.

## Security

- Never commit `.env`, raw API keys, or `~/.grok/auth.json`
- `.gitignore` already excludes `.env`
- Gateway stores keychain-backed credentials via Unity Relay (machine-local)

## Troubleshooting

| Symptom                              | Fix                                                             |
| ------------------------------------ | --------------------------------------------------------------- |
| Keychain command fails               | Use Passwords app; confirm service `XAI_API_KEY`, account `xai` |
| `BOB_AI_GATEWAY: key not set`        | Run `./scripts/setup-unity-ai-gateway.sh --sync-env`            |
| Gateway banner “credentials missing” | **Project Settings → AI → Gateway** → Codex → Save env vars     |
| Assistant shows Unity agent only     | Enable Codex provider; select Codex in agent dropdown           |
| No AI Gateway tab                    | Confirm Unity seat + link project to organization               |

## Related

- [unity-mcp.md](unity-mcp.md) — Official Unity MCP for Cursor
- [unity-dev.md](unity-dev.md) — Unity CLI and packages
- [AGENTS.md](../AGENTS.md) — agent workflow rules
