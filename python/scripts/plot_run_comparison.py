#!/usr/bin/env python3
"""Overlay rolling success % across timestamp windows (multi-run compare).

CSVs do not store run_id — pass explicit windows:

    label:since[:until]

Examples:
    python scripts/plot_run_comparison.py \\
      --window bob-v4:2026-07-17T22:59:00:2026-07-18T02:00:00 \\
      --window bob-v4.1:2026-07-18T17:16:43 \\
      --output ../docs/results/run_comparison.png

    # Trailing-1k success for demo-done bar (70%)
    python scripts/plot_run_comparison.py --window bob-v4.1:2026-07-18T17:16:43 --check-demo-bar
"""

from __future__ import annotations

import argparse
import csv
import sys
from pathlib import Path


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent


def parse_window(spec: str) -> tuple[str, str, str | None]:
    """Parse label:since or label:since:until."""
    parts = spec.split(":")
    if len(parts) < 2:
        raise argparse.ArgumentTypeError(
            f"Bad --window '{spec}' (want label:since or label:since:until)"
        )
    label = parts[0]
    # ISO timestamps contain colons — rejoin remainder carefully.
    # Formats:
    #   label:2026-07-18T17:16:43
    #   label:2026-07-18T17:16:43:2026-07-18T20:00:00
    rest = spec[len(label) + 1 :]
    if "T" not in rest:
        raise argparse.ArgumentTypeError(
            f"Bad --window '{spec}' (missing ISO timestamp)"
        )
    # Split on second ISO date if present (pattern ...:YYYY-)
    until = None
    since = rest
    marker = ":20"  # next year-prefixed until after a full since
    # Find a second timestamp starting with YYYY- after the first complete ISO blob
    # Heuristic: since is first 19+ chars of ISO; if another ":YYYY-MM-DD" appears after index 19, it's until
    for i in range(19, len(rest) - 10):
        if rest[i] == ":" and len(rest) > i + 11 and rest[i + 1 : i + 5].isdigit():
            since = rest[:i]
            until = rest[i + 1 :]
            break
    return label, since, until


def load_shots(
    path: Path, since: str | None, until: str | None
) -> list[dict[str, str]]:
    if not path.is_file():
        return []
    rows: list[dict[str, str]] = []
    with path.open(newline="") as handle:
        for row in csv.DictReader(handle):
            ts = row.get("timestamp", "")
            if since is not None and ts < since:
                continue
            if until is not None and ts >= until:
                continue
            if row.get("training_connected", "1") != "1":
                continue
            rows.append(row)
    return rows


def rolling_success(
    rows: list[dict[str, str]], window: int = 50
) -> tuple[list[int], list[float]]:
    xs: list[int] = []
    ys: list[float] = []
    for i, row in enumerate(rows):
        a = max(0, i - window + 1)
        chunk = rows[a : i + 1]
        makes = sum(int(r["scored"]) for r in chunk)
        xs.append(i + 1)
        ys.append(100.0 * makes / len(chunk))
    return xs, ys


def trailing_success(rows: list[dict[str, str]], n: int = 1000) -> float | None:
    if not rows:
        return None
    tail = rows[-n:] if len(rows) >= n else rows
    return 100.0 * sum(int(r["scored"]) for r in tail) / len(tail)


def plot_comparison(
    series: list[tuple[str, list[int], list[float], float | None]],
    output: Path,
    demo_bar: float,
    interim_bar: float,
) -> None:
    import matplotlib.pyplot as plt

    fig, ax = plt.subplots(figsize=(10, 5))
    for label, xs, ys, trail in series:
        trail_txt = (
            f" (last{min(1000, len(xs))}={trail:.1f}%)" if trail is not None else ""
        )
        ax.plot(xs, ys, label=f"{label}{trail_txt}", linewidth=1.6)

    ax.axhline(
        interim_bar,
        color="0.45",
        linestyle="--",
        linewidth=1,
        label=f"interim {interim_bar:g}%",
    )
    ax.axhline(
        demo_bar,
        color="0.2",
        linestyle=":",
        linewidth=1.2,
        label=f"demo-done {demo_bar:g}%",
    )
    ax.set_xlabel("Episode index within window (connected)")
    ax.set_ylabel("Rolling success % (window=50)")
    ax.set_title("Bob run comparison — rolling success")
    ax.set_ylim(0, max(100, demo_bar + 5))
    ax.legend(loc="upper left")
    ax.grid(True, alpha=0.25)
    output.parent.mkdir(parents=True, exist_ok=True)
    fig.tight_layout()
    fig.savefig(output, dpi=120)
    plt.close(fig)


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Compare Bob runs via timestamp windows"
    )
    parser.add_argument(
        "--shots",
        type=Path,
        default=repo_root() / "summaries" / "bob_shots.csv",
    )
    parser.add_argument(
        "--window",
        action="append",
        default=[],
        metavar="label:since[:until]",
        help="Repeatable run window (ISO timestamps)",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=repo_root() / "docs" / "results" / "run_comparison.png",
    )
    parser.add_argument(
        "--demo-bar",
        type=float,
        default=70.0,
        help="Official demo-done success %% over last 1k (default 70)",
    )
    parser.add_argument(
        "--interim-bar",
        type=float,
        default=5.0,
        help="Interim learning gate %% (default 5)",
    )
    parser.add_argument(
        "--check-demo-bar",
        action="store_true",
        help="Exit 0 only if each window's trailing-1k success >= --demo-bar",
    )
    args = parser.parse_args()

    if not args.window:
        print("Provide at least one --window label:since[:until]", file=sys.stderr)
        return 1

    series: list[tuple[str, list[int], list[float], float | None]] = []
    print("Window summaries:")
    all_pass = True
    for spec in args.window:
        label, since, until = parse_window(spec)
        rows = load_shots(args.shots, since, until)
        if not rows:
            print(f"  {label}: no connected rows (since={since} until={until})")
            all_pass = False
            continue
        xs, ys = rolling_success(rows)
        trail = trailing_success(rows, 1000)
        series.append((label, xs, ys, trail))
        print(
            f"  {label}: n={len(rows)} makes={sum(int(r['scored']) for r in rows)} "
            f"session={100.0 * sum(int(r['scored']) for r in rows) / len(rows):.2f}% "
            f"last_{min(1000, len(rows))}={trail:.2f}%"
            if trail is not None
            else f"  {label}: n={len(rows)}"
        )
        if args.check_demo_bar and (
            trail is None or trail < args.demo_bar or len(rows) < 1000
        ):
            all_pass = False
            need = f">={args.demo_bar}% over last 1000"
            print(f"    FAIL demo-done ({need}; have n={len(rows)} trail={trail})")
        elif args.check_demo_bar:
            print(f"    OK demo-done (>= {args.demo_bar}% over last 1000)")

    if not series:
        print("No data to plot.", file=sys.stderr)
        return 1

    plot_comparison(series, args.output, args.demo_bar, args.interim_bar)
    print(f"Saved {args.output}")

    if args.check_demo_bar:
        print("PASS" if all_pass else "FAIL")
        return 0 if all_pass else 2
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
