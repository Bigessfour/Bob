"""ChromaDB vector store for Bob repository RAG."""

from __future__ import annotations

import json
import shutil
from datetime import datetime, timezone
from typing import Any

import chromadb
from chromadb.config import Settings as ChromaSettings
from rag.settings import (
    COLLECTION_NAME,
    get_chroma_path,
    get_manifest_path,
    get_rag_data_dir,
)

_client: chromadb.PersistentClient | None = None
_client_path: str | None = None


def _utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat()


def reset_chroma_store() -> None:
    """Remove on-disk Chroma data so full rebuilds do not leave stale HNSW segments."""
    global _client, _client_path
    _client = None
    _client_path = None

    chroma_path = get_chroma_path()
    if chroma_path.exists():
        shutil.rmtree(chroma_path)
    chroma_path.mkdir(parents=True, exist_ok=True)


def get_client() -> chromadb.PersistentClient:
    global _client, _client_path

    chroma_path = str(get_chroma_path())
    if _client is not None and _client_path != chroma_path:
        _client = None

    if _client is None:
        get_rag_data_dir().mkdir(parents=True, exist_ok=True)
        _client = chromadb.PersistentClient(
            path=chroma_path,
            settings=ChromaSettings(anonymized_telemetry=False),
        )
        _client_path = chroma_path

    return _client


def get_collection(client: chromadb.PersistentClient | None = None):
    client = client or get_client()
    return client.get_or_create_collection(
        name=COLLECTION_NAME,
        metadata={"description": "Bob Unity ML-Agents repository knowledge"},
    )


def write_manifest(payload: dict[str, Any]) -> None:
    get_rag_data_dir().mkdir(parents=True, exist_ok=True)
    get_manifest_path().write_text(json.dumps(payload, indent=2), encoding="utf-8")


def read_manifest() -> dict[str, Any]:
    manifest_path = get_manifest_path()
    if not manifest_path.exists():
        return {}
    return json.loads(manifest_path.read_text(encoding="utf-8"))


def collection_stats() -> dict[str, Any]:
    manifest = read_manifest()
    try:
        collection = get_collection()
        count = collection.count()
    except Exception:
        count = 0
    return {
        "collection": COLLECTION_NAME,
        "chroma_path": str(get_chroma_path()),
        "chunk_count": count,
        "last_indexed_at": manifest.get("last_indexed_at"),
        "indexed_files": manifest.get("indexed_files", 0),
    }


def touch_manifest(
    *, indexed_files: int, mode: str, paths: list[str] | None = None
) -> None:
    manifest = read_manifest()
    manifest.update(
        {
            "last_indexed_at": _utc_now(),
            "indexed_files": indexed_files,
            "last_mode": mode,
            "last_paths": paths or manifest.get("last_paths", []),
            "chunk_count": get_collection().count(),
        }
    )
    write_manifest(manifest)
