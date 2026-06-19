"""Index repository files into the Bob RAG vector store."""

from __future__ import annotations

from pathlib import Path

from rag.chunker import TextChunk, chunk_file, is_indexable_file
from rag.settings import (
    INDEX_ROOTS,
    MAX_FILE_BYTES,
    REPO_ROOT,
    SKIP_DIR_NAMES,
    SKIP_FILE_NAMES,
)
from rag.store import COLLECTION_NAME, get_client, get_collection, touch_manifest


def iter_indexable_files(roots: list[str] | None = None) -> list[Path]:
    selected_roots = roots or list(INDEX_ROOTS)
    files: list[Path] = []

    for root in selected_roots:
        path = REPO_ROOT / root
        if path.is_file():
            if _should_index(path):
                files.append(path)
            continue
        if not path.is_dir():
            continue
        for candidate in sorted(path.rglob("*")):
            if candidate.is_file() and _should_index(candidate):
                files.append(candidate)

    return sorted(set(files))


def _should_index(path: Path) -> bool:
    if path.name in SKIP_FILE_NAMES:
        return False
    if not is_indexable_file(path):
        return False
    if any(part in SKIP_DIR_NAMES for part in path.parts):
        return False
    try:
        if path.stat().st_size > MAX_FILE_BYTES:
            return False
    except OSError:
        return False
    return True


def _chunk_id(chunk: TextChunk) -> str:
    return f"{chunk.rel_path}:{chunk.start_line}:{chunk.end_line}:{hash(chunk.text) & 0xFFFFFFFF:08x}"


def index_files(paths: list[Path], *, mode: str = "partial") -> dict[str, int]:
    collection = get_collection()
    chunks: list[TextChunk] = []
    indexed_paths: list[str] = []

    for path in paths:
        rel = path.relative_to(REPO_ROOT).as_posix()
        indexed_paths.append(rel)
        _delete_path_chunks(collection, rel)
        chunks.extend(chunk_file(path))

    if not chunks:
        touch_manifest(indexed_files=len(paths), mode=mode, paths=indexed_paths)
        return {"files": len(paths), "chunks": 0}

    collection.add(
        ids=[_chunk_id(c) for c in chunks],
        documents=[c.text for c in chunks],
        metadatas=[
            {
                "path": c.rel_path,
                "start_line": c.start_line,
                "end_line": c.end_line,
                "kind": c.kind,
            }
            for c in chunks
        ],
    )

    touch_manifest(indexed_files=len(paths), mode=mode, paths=indexed_paths)
    return {"files": len(paths), "chunks": len(chunks)}


def index_all() -> dict[str, int]:
    files = iter_indexable_files()
    client = get_client()
    if any(c.name == COLLECTION_NAME for c in client.list_collections()):
        client.delete_collection(COLLECTION_NAME)

    chunks: list[TextChunk] = []
    for path in files:
        chunks.extend(chunk_file(path))

    collection = get_collection()
    if not chunks:
        touch_manifest(indexed_files=0, mode="full", paths=[])
        return {"files": 0, "chunks": 0}

    collection.add(
        ids=[_chunk_id(c) for c in chunks],
        documents=[c.text for c in chunks],
        metadatas=[
            {
                "path": c.rel_path,
                "start_line": c.start_line,
                "end_line": c.end_line,
                "kind": c.kind,
            }
            for c in chunks
        ],
    )

    touch_manifest(
        indexed_files=len(files),
        mode="full",
        paths=[p.relative_to(REPO_ROOT).as_posix() for p in files],
    )
    return {"files": len(files), "chunks": len(chunks)}


def index_paths(relative_paths: list[str]) -> dict[str, int]:
    resolved: list[Path] = []
    for rel in relative_paths:
        path = (REPO_ROOT / rel).resolve()
        if not path.is_relative_to(REPO_ROOT):
            continue
        if path.is_file() and _should_index(path):
            resolved.append(path)
    return index_files(resolved, mode="partial")


def _delete_path_chunks(collection, rel_path: str) -> None:
    collection.delete(where={"path": rel_path})
