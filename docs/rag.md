# Bob Repository RAG

Local **Retrieval-Augmented Generation** over Bob source, docs, and config. Powers the **`bob-rag`** MCP server and Cursor hooks so agents query repo context before editing code.

## Stack

| Component          | Choice                                                     |
| ------------------ | ---------------------------------------------------------- |
| Embeddings + store | [ChromaDB](https://www.trychroma.com/) (persistent, local) |
| MCP transport      | Python `mcp` FastMCP stdio server                          |
| Index location     | `python/.rag/chroma/` (gitignored)                         |

## Setup

```bash
./scripts/setup-python.sh
./scripts/rag-setup.sh
./scripts/rag-index.sh
```

Restart Cursor so [`.cursor/mcp.json`](../.cursor/mcp.json) loads the **`bob-rag`** server.

## Agent workflow

### Before code edits

1. MCP: `rag_query` with task + target file path
2. Align implementation with retrieved patterns (behavior names, reward logic, CLI conventions)

### After significant changes per turn

1. MCP: `rag_index_paths` with touched files, or `./scripts/rag-index.sh --paths ...`
2. Stop hook (`.cursor/hooks/rag-stop-index.sh`) also re-indexes changed text files at turn end

## CLI

```bash
# Full rebuild
./scripts/rag-index.sh

# Partial update
./scripts/rag-index.sh --paths Assets/Scripts/BobAgent.cs docs/unity-dev.md

# Query
cd python && source .venv/bin/activate
python scripts/rag_query.py -q "Bob training scene validator CLI" --top-k 5
python scripts/rag_query.py -q "reward shaping" --json
```

## MCP tools (`bob-rag`)

| Tool              | Purpose                                               |
| ----------------- | ----------------------------------------------------- |
| `rag_query`       | Semantic search — **call before every code edit**     |
| `rag_index_paths` | Re-index specific files after significant development |
| `rag_index_full`  | Full rebuild                                          |
| `rag_status`      | Chunk count, last indexed timestamp                   |

## Indexed paths

Defined in [`python/rag/settings.py`](../python/rag/settings.py):

- `AGENTS.md`, `PROJECT.md`, `README.md`
- `Assets/Scripts/`, `Assets/Editor/`
- `config/`, `docs/`, `scripts/`, `terraform/`
- `python/rag/`, `python/scripts/`
- `.cursor/rules/`, `.cursor/project-rules.md`

Skips binaries, `Library/`, `.venv/`, `results/`, files over 256 KB.

## Week 2 extension (play-mode captures)

When adding play-mode screenshot capture, index new Editor methods immediately via `rag_index_paths` so agents see the full API surface—not stub comments.

## Troubleshooting

| Issue                                         | Fix                                                                                                                                                                                                    |
| --------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Empty query results                           | Run `./scripts/rag-index.sh`                                                                                                                                                                           |
| Query hangs or MCP `Connection closed`        | Corrupt Chroma index — full rebuild: `./scripts/rag-index.sh` (wipes `python/.rag/chroma/`). If rebuild hangs, `mv python/.rag/chroma python/.rag/chroma.bak && mkdir python/.rag/chroma` then re-run. |
| MCP server fails to start                     | `./scripts/rag-setup.sh`; confirm `python/.venv/bin/python` exists                                                                                                                                     |
| Hooks warn RAG unavailable                    | Install jq (`brew install jq`) for pre-code injection                                                                                                                                                  |
| Stale context after edits                     | `./scripts/rag-index.sh --paths <files>` or wait for stop hook                                                                                                                                         |
| `Failed to send telemetry event` stderr noise | Harmless Chroma/posthog version mismatch; queries still work                                                                                                                                           |
