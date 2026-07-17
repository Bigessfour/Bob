#!/usr/bin/env python3
"""Summarize Bob training session + per-shot action logs for post-run review.

Usage:
    python scripts/review_training_run.py
    python scripts/review_training_run.py --since 2026-07-17T22:40:00
    python scripts/review_training_run.py --shots ../summaries/bob_shots.csv
"""

from __future__ import annotations

import argparse
import csv
import statistics
import sys
from collections import Counter
from pathlib import Path


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent


def default_session_path() -> Path:
    return repo_root() / "summaries" / "bob_session.csv"


def default_shots_path() -> Path:
    return repo_root() / "summaries" / "bob_shots.csv"


def load_rows(path: Path, since: str | None) -> list[dict[str, str]]:
    if not path.is_file():
        return []
    rows: list[dict[str, str]] = []
    with path.open(newline="") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            ts = row.get("timestamp", "")
            if since is not None and ts < since:
                continue
            rows.append(row)
    return rows


def summarize_session(rows: list[dict[str, str]]) -> None:
    if not rows:
        print("Session CSV: no rows (or none after --since).")
        return

    n = len(rows)
    makes = sum(int(r.get("scored", "0") or 0) for r in rows)
    arcs = [
        float(r["rolling_arc_quality"]) for r in rows if r.get("rolling_arc_quality")
    ]
    nets = [float(r["net_rl"]) for r in rows if r.get("net_rl")]
    print("=== Session (bob_session.csv) ===")
    print(f"Episodes: {n}")
    print(f"Makes: {makes}  ({100.0 * makes / n:.2f}% success)")
    if arcs:
        print(
            f"Rolling arc quality (last): {arcs[-1]:.1f}%  mean: {statistics.mean(arcs):.1f}%"
        )
    if nets:
        print(f"Net RL (last): {nets[-1]:.2f}  mean: {statistics.mean(nets):.2f}")
    print(f"First ts: {rows[0].get('timestamp', '?')}")
    print(f"Last ts:  {rows[-1].get('timestamp', '?')}")


def summarize_shots(rows: list[dict[str, str]]) -> None:
    if not rows:
        print(
            "\nShots CSV: none yet. Play after this change to write summaries/bob_shots.csv"
        )
        print("  (Console also logs BOB_SHOT: lines each launch.)")
        return

    n = len(rows)
    makes = sum(int(r.get("scored", "0") or 0) for r in rows)
    training = sum(int(r.get("training_connected", "0") or 0) for r in rows)
    toward = [float(r["toward_hoop_dot"]) for r in rows if r.get("toward_hoop_dot")]
    reasons = Counter(r.get("end_reason", "unknown") for r in rows)
    away = sum(1 for t in toward if t < 0)
    print("\n=== Shots (bob_shots.csv) ===")
    print(f"Shots logged: {n}")
    print(f"Training-connected launches: {training}/{n}")
    print(f"Makes: {makes}")
    if toward:
        print(
            f"Toward-hoop dot: mean={statistics.mean(toward):+.3f}  "
            f"away={away}/{n} ({100.0 * away / n:.0f}% sideways/back)"
        )
    print("End reasons:")
    for reason, count in reasons.most_common():
        print(f"  {reason}: {count}")

    print("\nLast 8 launches:")
    for r in rows[-8:]:
        print(
            f"  ep={r.get('iteration')} a=({r.get('ax')},{r.get('ay')},{r.get('az')}) "
            f"toward={r.get('toward_hoop_dot')} end={r.get('end_reason')} "
            f"train={r.get('training_connected')} scored={r.get('scored')}"
        )


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Review Bob training session + shot action logs"
    )
    parser.add_argument("--session", type=Path, default=default_session_path())
    parser.add_argument("--shots", type=Path, default=default_shots_path())
    parser.add_argument(
        "--since",
        default=None,
        help="ISO timestamp lower bound (e.g. 2026-07-17T22:40:00)",
    )
    args = parser.parse_args()

    session_rows = load_rows(args.session, args.since)
    shot_rows = load_rows(args.shots, args.since)
    summarize_session(session_rows)
    summarize_shots(shot_rows)

    if session_rows and makes_zero(session_rows):
        print("\nNote: 0% makes is expected early in bob-v4 exploration.")
        print(
            "Sporadic directions = random PPO actions in world XYZ until policy learns."
        )
        print(
            "If Console shows BOB_TRAINING_WARN / Communicator exited: re-handshake train.sh → Play."
        )

    return 0


def makes_zero(rows: list[dict[str, str]]) -> bool:
    return all(int(r.get("scored", "0") or 0) == 0 for r in rows)


if __name__ == "__main__":
    sys.exit(main())
