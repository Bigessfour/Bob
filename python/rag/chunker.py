"""Split repository text files into overlapping chunks for vector indexing."""

from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path

from rag.settings import CHUNK_OVERLAP, CHUNK_SIZE, REPO_ROOT, TEXT_EXTENSIONS

_HEADING_RE = re.compile(r"^(#{1,6}\s+.+)$", re.MULTILINE)
_CS_MEMBER_RE = re.compile(
    r"^(\s*(?:public|private|protected|internal|static|override|async|\s)+"
    r"(?:class|struct|interface|enum|void|bool|int|float|string|[\w<>,\[\]]+)\s+\w+)",
    re.MULTILINE,
)


@dataclass(frozen=True)
class TextChunk:
    text: str
    rel_path: str
    start_line: int
    end_line: int
    kind: str


def is_indexable_file(path: Path) -> bool:
    if path.suffix.lower() not in TEXT_EXTENSIONS:
        return False
    if path.name in {"packages-lock.json", "ProjectSettings.asset"}:
        return False
    return True


def _line_number_at(text: str, index: int) -> int:
    return text.count("\n", 0, index) + 1


def _split_on_boundaries(text: str, pattern: re.Pattern[str]) -> list[tuple[int, str]]:
    matches = list(pattern.finditer(text))
    if not matches:
        return [(0, text)]

    parts: list[tuple[int, str]] = []
    for i, match in enumerate(matches):
        start = match.start()
        end = matches[i + 1].start() if i + 1 < len(matches) else len(text)
        parts.append((start, text[start:end].strip()))
    return parts


def chunk_file(path: Path) -> list[TextChunk]:
    rel_path = path.relative_to(REPO_ROOT).as_posix()
    suffix = path.suffix.lower()
    text = path.read_text(encoding="utf-8", errors="replace")

    if suffix == ".md" or suffix == ".mdc":
        sections = _split_on_boundaries(text, _HEADING_RE)
    elif suffix == ".cs":
        sections = _split_on_boundaries(text, _CS_MEMBER_RE)
    else:
        sections = [(0, text)]

    kind = "doc" if suffix in {".md", ".mdc"} else "code"
    if rel_path.startswith("config/"):
        kind = "config"

    chunks: list[TextChunk] = []
    for start_offset, section in sections:
        chunks.extend(_chunk_file_section(rel_path, text, section, start_offset, kind))
    return chunks


def _chunk_file_section(
    rel_path: str,
    full_text: str,
    section: str,
    start_offset: int,
    kind: str,
) -> list[TextChunk]:
    if not section.strip():
        return []

    chunks: list[TextChunk] = []
    step = max(CHUNK_SIZE - CHUNK_OVERLAP, 1)
    for offset in range(0, len(section), step):
        piece = section[offset : offset + CHUNK_SIZE].strip()
        if not piece:
            continue
        line_start = _line_number_at(full_text, start_offset + offset)
        line_end = _line_number_at(
            full_text, start_offset + offset + min(len(piece), len(section) - offset)
        )
        chunks.append(
            TextChunk(
                text=piece,
                rel_path=rel_path,
                start_line=line_start,
                end_line=line_end,
                kind=kind,
            )
        )
        if offset + CHUNK_SIZE >= len(section):
            break
    return chunks
