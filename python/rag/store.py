"""ChromaDB vector store for Bob repository RAG."""

from __future__ import annotations

import json
from datetime import datetime, timezone
from typing import Any

import chromadb
from chromadb.config import Settings as ChromaSettings
from rag.settings import CHROMA_PATH, COLLECTION_NAME, MANIFEST_PATH, RAG_DATA_DIR


def _utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat()


def get_client() -> chromadb.PersistentClient:
    RAG_DATA_DIR.mkdir(parents=True, exist_ok=True)
    return chromadb.PersistentClient(
        path=str(CHROMA_PATH),
        settings=ChromaSettings(anonymized_telemetry=False),
    )


def get_collection(client: chromadb.PersistentClient | None = None):
    client = client or get_client()
    return client.get_or_create_collection(
        name=COLLECTION_NAME,
        metadata={"description": "Bob Unity ML-Agents repository knowledge"},
    )


def write_manifest(payload: dict[str, Any]) -> None:
    RAG_DATA_DIR.mkdir(parents=True, exist_ok=True)
    MANIFEST_PATH.write_text(json.dumps(payload, indent=2), encoding="utf-8")


def read_manifest() -> dict[str, Any]:
    if not MANIFEST_PATH.exists():
        return {}
    return json.loads(MANIFEST_PATH.read_text(encoding="utf-8"))


def collection_stats() -> dict[str, Any]:
    manifest = read_manifest()
    try:
        collection = get_collection()
        count = collection.count()
    except Exception:
        count = 0
    return {
        "collection": COLLECTION_NAME,
        "chroma_path": str(CHROMA_PATH),
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
