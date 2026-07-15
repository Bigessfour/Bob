#!/usr/bin/env bash
# Reminder checklist before Unity MCP work on Bob. Run from repo root.
set -euo pipefail

echo "=== Bob Unity MCP preflight ==="
echo "1. Unity Editor open on Bob project"
echo "2. Edit → Project Settings → AI → Unity MCP → bridge Running"
echo "3. Cursor approved under Connected Clients"
echo "4. Stop Play before saving Assets/Scripts (compile during Play breaks training)"
echo ""
echo "If training is active: STOP — do not MCP-bake until trainer disconnected."
echo "Validate after scene work: ./scripts/validate-scene.sh"
echo "Docs: docs/unity-mcp.md"
