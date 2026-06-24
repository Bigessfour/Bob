from __future__ import annotations

from pathlib import Path

import pytest
from rag.chunker import chunk_file, is_indexable_file
from rag.settings import REPO_ROOT

# Small, stable corpus for Chroma embed/search tests (avoids full-repo index_all in CI).
MINI_CORPUS = [
    "Assets/Scripts/BobAgent.cs",
    "config/bob_free_throw.yaml",
    "AGENTS.md",
]


@pytest.fixture
def isolated_rag_dir(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> Path:
    """Per-test Chroma store under tmp_path; respects dynamic BOB_RAG_DATA_DIR."""
    rag_dir = tmp_path / "rag-data"
    monkeypatch.setenv("BOB_RAG_DATA_DIR", str(rag_dir))
    from rag.store import reset_chroma_store

    reset_chroma_store()
    return rag_dir


def test_is_indexable_file_accepts_cs_and_md() -> None:
    assert is_indexable_file(REPO_ROOT / "Assets/Scripts/BobAgent.cs")
    assert is_indexable_file(REPO_ROOT / "AGENTS.md")
    assert not is_indexable_file(REPO_ROOT / "docs/progress/001/capture.png")


def test_chunk_file_produces_metadata() -> None:
    path = REPO_ROOT / "config" / "bob_free_throw.yaml"
    chunks = chunk_file(path)
    assert chunks
    assert chunks[0].rel_path == "config/bob_free_throw.yaml"
    assert chunks[0].kind == "config"
    assert chunks[0].start_line >= 1


@pytest.mark.rag
def test_iter_indexable_files_includes_agent_and_docs() -> None:
    from rag.indexer import iter_indexable_files

    files = {p.relative_to(REPO_ROOT).as_posix() for p in iter_indexable_files()}
    assert "Assets/Scripts/BobAgent.cs" in files
    assert "AGENTS.md" in files
    assert "docs/unity-dev.md" in files


@pytest.mark.rag
def test_index_and_search_round_trip(isolated_rag_dir: Path) -> None:
    from rag.indexer import index_paths
    from rag.search import search
    from rag.store import collection_stats

    result = index_paths(MINI_CORPUS)
    assert result["files"] == len(MINI_CORPUS)
    assert result["chunks"] > 0

    stats = collection_stats()
    assert stats["chunk_count"] > 0
    assert str(isolated_rag_dir) in stats["chroma_path"]

    hits = search("BobAgent class Agent behaviors Bob PPO free throw", top_k=5)
    assert hits
    paths = {hit.path for hit in hits}
    assert any("BobAgent" in p or "bob_free_throw" in p for p in paths)


@pytest.mark.rag
def test_partial_reindex_updates_single_file(isolated_rag_dir: Path) -> None:
    from rag.indexer import index_paths
    from rag.search import search
    from rag.store import get_collection, read_manifest

    index_paths(["Assets/Scripts/BobAgent.cs", "AGENTS.md"])
    assert get_collection().count() > 0

    result = index_paths(["AGENTS.md"])
    assert result["files"] == 1
    assert result["chunks"] > 0

    manifest = read_manifest()
    assert manifest.get("last_mode") == "partial"
    assert "AGENTS.md" in manifest.get("last_paths", [])

    hits = search("BobAgent ML-Agents Behavior Name", top_k=5)
    assert any("BobAgent" in hit.path for hit in hits)
    assert get_collection().count() > 0


@pytest.mark.rag
def test_index_all_mini_roots(isolated_rag_dir: Path) -> None:
    """Full index pipeline on a tiny root list (reset + batch embed), not the whole repo."""
    from rag.indexer import index_all
    from rag.search import search
    from rag.store import read_manifest

    result = index_all(roots=["config", "AGENTS.md"])
    assert result["files"] >= 2
    assert result["chunks"] > 0

    manifest = read_manifest()
    assert manifest.get("last_mode") == "full"
    assert manifest.get("chunk_count", 0) > 0

    hits = search("behaviors Bob trainer yaml", top_k=3)
    assert hits
    assert any("bob_free_throw" in hit.path or "AGENTS" in hit.path for hit in hits)
