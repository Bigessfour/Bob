"""Bob repository RAG package.

Public helpers are exposed lazily so ``import rag.chunker`` does not require chromadb.
"""

from __future__ import annotations

from typing import Any

__all__ = [
    "collection_stats",
    "index_all",
    "index_paths",
    "search",
    "search_formatted",
]


def __getattr__(name: str) -> Any:
    if name in ("index_all", "index_paths"):
        from rag.indexer import index_all, index_paths

        return index_all if name == "index_all" else index_paths
    if name in ("search", "search_formatted"):
        from rag.search import search, search_formatted

        return search if name == "search" else search_formatted
    if name == "collection_stats":
        from rag.store import collection_stats

        return collection_stats
    raise AttributeError(f"module {__name__!r} has no attribute {name!r}")
