#!/usr/bin/env python3
r"""Evaluate solver-prior quality from bob_shots.csv (residual ≈ 0 proxy).

Usage:
  cd python && .venv/bin/python scripts/solver_prior_eval.py \\
    --since 2026-07-20T02:14:45 --residual-max 0.35
"""

from __future__ import annotations

import argparse
import csv
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
SHOTS = REPO / "summaries" / "bob_shots.csv"


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Solver prior / low-residual shot stats"
    )
    parser.add_argument("--since", default="", help="ISO timestamp lower bound")
    parser.add_argument("--until", default="", help="ISO timestamp upper bound")
    parser.add_argument(
        "--residual-max",
        type=float,
        default=0.35,
        help="Max |ax|,|ay|,|az| to treat as solver-prior / c≈0",
    )
    parser.add_argument("--training-only", action="store_true")
    args = parser.parse_args()

    if not SHOTS.is_file():
        raise SystemExit(f"Missing {SHOTS}")

    rows: list[dict[str, str]] = []
    with SHOTS.open(newline="") as f:
        for row in csv.DictReader(f):
            ts = row.get("timestamp", "")
            if args.since and ts < args.since:
                continue
            if args.until and ts > args.until:
                continue
            if args.training_only and row.get("training_connected") != "1":
                continue
            rows.append(row)

    if not rows:
        print("No shots in window.")
        return

    low_residual: list[dict[str, str]] = []
    for row in rows:
        try:
            ax, ay, az = float(row["ax"]), float(row["ay"]), float(row["az"])
        except (KeyError, ValueError):
            continue
        if max(abs(ax), abs(ay), abs(az)) <= args.residual_max:
            low_residual.append(row)

    def rate(subset: list[dict[str, str]]) -> tuple[int, int, float]:
        makes = sum(1 for r in subset if r.get("scored") == "1")
        n = len(subset)
        return makes, n, (100.0 * makes / n if n else 0.0)

    all_m, all_n, all_pct = rate(rows)
    pri_m, pri_n, pri_pct = rate(low_residual)

    print(f"Window shots: {all_n}  makes: {all_m}  success: {all_pct:.2f}%")
    print(
        f"Low-residual (|a|≤{args.residual_max}): {pri_n}  makes: {pri_m}  "
        f"success: {pri_pct:.2f}%"
    )
    print("Gate: low-residual success ≥ 10% before extended PPO; ≥ 5% minimum prior.")

    reasons: dict[str, int] = {}
    for row in low_residual:
        reason = row.get("end_reason", "?")
        reasons[reason] = reasons.get(reason, 0) + 1
    if reasons:
        print("Low-residual end_reason mix:")
        for reason, count in sorted(reasons.items(), key=lambda x: -x[1]):
            print(f"  {reason}: {count}")


if __name__ == "__main__":
    main()
