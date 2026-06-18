#!/usr/bin/env python3
"""CLI: semantic search over the Bob repository RAG index."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from rag.search import format_hits, search
from rag.store import collection_stats


def main() -> int:
    parser = argparse.ArgumentParser(description="Query Bob repo RAG index")
    parser.add_argument("query", nargs="?", default="", help="Natural language query")
    parser.add_argument(
        "--query", "-q", dest="query_flag", help="Query string (alternative)"
    )
    parser.add_argument("--top-k", type=int, default=6)
    parser.add_argument("--json", action="store_true", help="Emit JSON hits")
    parser.add_argument(
        "--file", help="Target file path — prepends file context to query"
    )
    args = parser.parse_args()

    query = (args.query_flag or args.query or "").strip()
    if args.file:
        file_hint = f"File: {args.file}"
        query = f"{file_hint}. {query}" if query else file_hint
    if not query:
        parser.error("query is required")

    hits = search(query, top_k=args.top_k)
    if args.json:
        print(
            json.dumps(
                {
                    "query": query,
                    "hits": [h.to_dict() for h in hits],
                    "stats": collection_stats(),
                },
                indent=2,
            )
        )
    else:
        print(format_hits(hits))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
