#!/usr/bin/env python3
"""CLI: rebuild or partially update the Bob repository RAG index."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

# Allow `python scripts/rag_index.py` from python/ directory.
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from rag.indexer import index_all, index_paths
from rag.store import collection_stats


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Index Bob repo files into ChromaDB RAG store"
    )
    parser.add_argument(
        "--paths",
        nargs="*",
        help="Relative repo paths to re-index (default: full rebuild)",
    )
    parser.add_argument("--json", action="store_true", help="Emit JSON result")
    args = parser.parse_args()

    if args.paths:
        result = index_paths(args.paths)
        mode = "partial"
    else:
        result = index_all()
        mode = "full"

    payload = {"mode": mode, **result, "stats": collection_stats()}
    if args.json:
        print(json.dumps(payload, indent=2))
    else:
        print(
            f"RAG index {mode}: {result['files']} files, {result['chunks']} chunks "
            f"(total chunks: {payload['stats']['chunk_count']})"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
