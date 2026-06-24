"""RAG paths and indexing configuration for the Bob repo."""

from __future__ import annotations

import os
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
_default_rag_dir = REPO_ROOT / "python" / ".rag"
COLLECTION_NAME = "bob_repo"


def get_rag_data_dir() -> Path:
    """Resolve RAG data dir from env on each call (tests set BOB_RAG_DATA_DIR per case)."""
    return Path(os.environ.get("BOB_RAG_DATA_DIR", str(_default_rag_dir)))


def get_chroma_path() -> Path:
    return get_rag_data_dir() / "chroma"


def get_manifest_path() -> Path:
    return get_rag_data_dir() / "manifest.json"

CHUNK_SIZE = 1200
CHUNK_OVERLAP = 150
MAX_FILE_BYTES = 256_000
DEFAULT_TOP_K = 6

# Relative paths from repo root to index (globs resolved at index time).
INDEX_ROOTS: tuple[str, ...] = (
    "AGENTS.md",
    "PROJECT.md",
    "README.md",
    "Assets/Scripts",
    "Assets/Editor",
    "config",
    "docs",
    "python/rag",
    "python/scripts",
    "scripts",
    "terraform",
    ".cursor/rules",
    ".cursor/project-rules.md",
)

TEXT_EXTENSIONS: frozenset[str] = frozenset(
    {
        ".cs",
        ".py",
        ".md",
        ".yaml",
        ".yml",
        ".tf",
        ".sh",
        ".json",
        ".mdc",
        ".asmdef",
    }
)

SKIP_DIR_NAMES: frozenset[str] = frozenset(
    {
        ".git",
        ".venv",
        "Library",
        "Temp",
        "Logs",
        "logs",
        "results",
        "summaries",
        "Build",
        "Builds",
        "node_modules",
        "__pycache__",
        ".rag",
        ".terraform",
        ".pytest_cache",
    }
)

SKIP_FILE_NAMES: frozenset[str] = frozenset(
    {
        "packages-lock.json",
        "manifest.json",
    }
)
