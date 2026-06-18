"""MCP server exposing Bob repository RAG tools to Cursor agents."""

from __future__ import annotations

import json

from mcp.server.fastmcp import FastMCP
from rag.indexer import index_all, index_paths
from rag.search import format_hits, search
from rag.store import collection_stats

mcp = FastMCP(
    "bob-rag",
    instructions=(
        "Bob repository RAG. Agents MUST call rag_query before any code edit "
        "(Write, StrReplace, or file creation). After developing significant "
        "methods or workflows, call rag_index_paths or rag_index_full to refresh context."
    ),
)


@mcp.tool()
def rag_query(query: str, top_k: int = 6) -> str:
    """Retrieve relevant Bob repo context (code, docs, config) for a task or file.

    Call this before every code change to ground decisions in existing patterns.
    """
    hits = search(query, top_k=top_k)
    return format_hits(hits)


@mcp.tool()
def rag_index_full() -> str:
    """Rebuild the entire repository RAG index from scratch."""
    result = index_all()
    stats = collection_stats()
    return json.dumps({"result": result, "stats": stats}, indent=2)


@mcp.tool()
def rag_index_paths(paths: list[str]) -> str:
    """Re-index specific repo-relative files after significant code changes.

    Example paths: ['Assets/Scripts/BobAgent.cs', 'docs/unity-dev.md']
    """
    result = index_paths(paths)
    stats = collection_stats()
    return json.dumps({"result": result, "stats": stats, "paths": paths}, indent=2)


@mcp.tool()
def rag_status() -> str:
    """Return RAG index health (chunk count, last indexed time)."""
    return json.dumps(collection_stats(), indent=2)


def main() -> None:
    mcp.run()


if __name__ == "__main__":
    main()
