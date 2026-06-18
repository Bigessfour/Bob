"""Bob repository RAG package."""

from rag.indexer import index_all, index_paths
from rag.search import search, search_formatted
from rag.store import collection_stats

__all__ = [
    "collection_stats",
    "index_all",
    "index_paths",
    "search",
    "search_formatted",
]
