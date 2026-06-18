"""Semantic search over the Bob RAG index."""

from __future__ import annotations

from dataclasses import asdict, dataclass
from typing import Any

from rag.settings import DEFAULT_TOP_K
from rag.store import collection_stats, get_collection


@dataclass(frozen=True)
class SearchHit:
    path: str
    start_line: int
    end_line: int
    kind: str
    score: float
    text: str

    def to_dict(self) -> dict[str, Any]:
        return asdict(self)


def search(query: str, top_k: int = DEFAULT_TOP_K) -> list[SearchHit]:
    query = query.strip()
    if not query:
        return []

    collection = get_collection()
    if collection.count() == 0:
        return []

    result = collection.query(
        query_texts=[query], n_results=min(top_k, collection.count())
    )
    documents = result.get("documents", [[]])[0]
    metadatas = result.get("metadatas", [[]])[0]
    distances = result.get("distances", [[]])[0]

    hits: list[SearchHit] = []
    for doc, meta, distance in zip(documents, metadatas, distances, strict=False):
        if not doc or not meta:
            continue
        hits.append(
            SearchHit(
                path=str(meta.get("path", "")),
                start_line=int(meta.get("start_line", 0)),
                end_line=int(meta.get("end_line", 0)),
                kind=str(meta.get("kind", "")),
                score=float(1.0 / (1.0 + distance)),
                text=doc,
            )
        )
    return hits


def format_hits(hits: list[SearchHit]) -> str:
    if not hits:
        stats = collection_stats()
        if stats.get("chunk_count", 0) == 0:
            return (
                "RAG index is empty. Run: ./scripts/rag-index.sh\n"
                "Then query again before editing code."
            )
        return "No relevant repository context found for that query."

    lines = ["## RAG context", ""]
    for i, hit in enumerate(hits, start=1):
        lines.append(
            f"### [{i}] `{hit.path}` L{hit.start_line}-L{hit.end_line} ({hit.kind}, score={hit.score:.3f})"
        )
        lines.append("")
        lines.append("```")
        lines.append(hit.text.strip())
        lines.append("```")
        lines.append("")
    return "\n".join(lines).strip()


def search_formatted(query: str, top_k: int = DEFAULT_TOP_K) -> str:
    return format_hits(search(query, top_k=top_k))
