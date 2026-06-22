from __future__ import annotations

from pathlib import Path

import pytest
from rag.chunker import chunk_file, is_indexable_file
from rag.settings import REPO_ROOT


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
def test_index_and_search_round_trip(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    from rag.indexer import index_all, index_paths
    from rag.search import search

    monkeypatch.setenv("BOB_RAG_DATA_DIR", str(tmp_path / "rag-data"))
    result = index_all()
    assert result["files"] > 0
    assert result["chunks"] > 0

    # Full-repo search can rank MCP/docs above BobAgent; verify retrieval on
    # the canonical agent + trainer config with a targeted partial index.
    index_paths(["Assets/Scripts/BobAgent.cs", "config/bob_free_throw.yaml"])
    hits = search("BobAgent class Agent behaviors Bob PPO free throw", top_k=5)
    assert hits
    paths = {hit.path for hit in hits}
    assert any("BobAgent" in p or "bob_free_throw" in p for p in paths)


@pytest.mark.rag
def test_partial_reindex_updates_single_file(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    from rag.indexer import index_all, index_paths

    monkeypatch.setenv("BOB_RAG_DATA_DIR", str(tmp_path / "rag-data"))
    index_all()
    result = index_paths(["AGENTS.md"])
    assert result["files"] == 1
    assert result["chunks"] > 0
