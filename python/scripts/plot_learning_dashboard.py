#!/usr/bin/env python3
"""Multi-panel learning dashboard from bob_session.csv + bob_shots.csv.

Includes Tier 1.5 diagnostic panels: economics by end_reason, positive-net-miss
trend, and make-signature fy/fz histograms.

Usage:
    python scripts/plot_learning_dashboard.py
    python scripts/plot_learning_dashboard.py --since 2026-07-17T22:59:00 \\
        --output ../docs/results/bob_v4.1_learning_dashboard.png
"""

from __future__ import annotations

import argparse
import csv
from collections import Counter, defaultdict
from pathlib import Path


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent


def load_csv(path: Path, since: str | None) -> list[dict[str, str]]:
    if not path.is_file():
        return []
    rows: list[dict[str, str]] = []
    with path.open(newline="") as handle:
        for row in csv.DictReader(handle):
            if since is not None and row.get("timestamp", "") < since:
                continue
            rows.append(row)
    return rows


def _f(row: dict[str, str], key: str) -> float:
    return float(row[key])


def _i(row: dict[str, str], key: str) -> int:
    return int(row[key])


def plot_dashboard(
    session: list[dict[str, str]],
    shots: list[dict[str, str]],
    output: Path,
    title: str,
) -> dict:
    import matplotlib.pyplot as plt

    if not shots and not session:
        raise ValueError("No session or shot rows to plot")

    train_shots = [r for r in shots if r.get("training_connected", "1") == "1"] or shots
    n = len(train_shots)
    makes = sum(_i(r, "scored") for r in train_shots)
    toward = [_f(r, "toward_hoop_dot") for r in train_shots if r.get("toward_hoop_dot")]
    reasons = Counter(r.get("end_reason", "unknown") for r in train_shots)

    iters = [_i(r, "iteration") for r in session]
    succ = [_f(r, "session_success_pct") for r in session]
    roll = [_f(r, "rolling_success_pct") for r in session]
    arc = [_f(r, "rolling_arc_quality") for r in session]

    window = 50
    ep = [_i(r, "iteration") for r in train_shots]
    scored = [_i(r, "scored") for r in train_shots]
    nets = [_f(r, "episode_net_rl") for r in train_shots if r.get("episode_net_rl")]
    roll_toward: list[float] = []
    roll_make_pct: list[float] = []
    pos_miss_frac: list[float] = []
    for i in range(n):
        a = max(0, i - window + 1)
        chunk = train_shots[a : i + 1]
        chunk_t = toward[a : i + 1] if toward else [0.0]
        chunk_s = scored[a : i + 1]
        roll_toward.append(sum(chunk_t) / len(chunk_t))
        roll_make_pct.append(100.0 * sum(chunk_s) / len(chunk_s))
        misses = [r for r in chunk if _i(r, "scored") == 0]
        if misses and all(r.get("episode_net_rl") for r in misses):
            pos = sum(1 for r in misses if _f(r, "episode_net_rl") > 0)
            pos_miss_frac.append(100.0 * pos / len(misses))
        else:
            pos_miss_frac.append(0.0)

    reason_order = ["make", "rim_miss", "timeout", "floor", "oob", "settled", "miss"]
    bucket = 100
    buckets: list[dict] = []
    for start in range(0, n, bucket):
        chunk = train_shots[start : start + bucket]
        if not chunk:
            continue
        c = Counter(r.get("end_reason", "unknown") for r in chunk)
        buckets.append(
            {
                "label": f"{start + 1}-{start + len(chunk)}",
                **{k: c.get(k, 0) for k in reason_order},
            }
        )

    by_reason: dict[str, list[float]] = defaultdict(list)
    for r in train_shots:
        if r.get("episode_net_rl"):
            by_reason[r.get("end_reason", "unknown")].append(_f(r, "episode_net_rl"))

    make_shots = [r for r in train_shots if _i(r, "scored") == 1]
    make_fy = [_f(r, "fy") for r in make_shots if r.get("fy")]
    make_fz = [_f(r, "fz") for r in make_shots if r.get("fz")]
    miss_fy = [_f(r, "fy") for r in train_shots if _i(r, "scored") == 0 and r.get("fy")]
    miss_fz = [_f(r, "fz") for r in train_shots if _i(r, "scored") == 0 and r.get("fz")]

    fig, axes = plt.subplots(3, 2, figsize=(14, 12), constrained_layout=True)
    fig.suptitle(title, fontsize=13)

    ax = axes[0, 0]
    if iters:
        ax.plot(iters, succ, label="Session success %", color="#2a9d8f", linewidth=1.5)
        ax.plot(
            iters, roll, label="Rolling success % (24)", color="#e9c46a", linewidth=1.2
        )
    ax.axhline(5, color="#e76f51", linestyle="--", linewidth=1, label="Gate 5%")
    ax.set_ylabel("Success %")
    ax.set_xlabel("Episode")
    ax.legend(loc="upper left", fontsize=8)
    ax.grid(True, alpha=0.3)
    ax.set_title("Basketball makes / episodes (north-star)")

    ax = axes[0, 1]
    if ep:
        ax.plot(ep, roll_make_pct, label=f"Make % (w={window})", color="#2a9d8f")
        ax.plot(
            ep,
            [t * 100 for t in roll_toward],
            label=f"Toward-hoop % (w={window})",
            color="#457b9d",
            alpha=0.9,
        )
        ax.plot(
            ep,
            pos_miss_frac,
            label=f"Positive-net miss % (w={window})",
            color="#e76f51",
        )
    if iters:
        ax.plot(iters, arc, label="Arc quality %", color="#a8dadc", alpha=0.75)
    ax.set_ylabel("%")
    ax.set_xlabel("Episode")
    ax.legend(loc="lower right", fontsize=7)
    ax.grid(True, alpha=0.3)
    ax.set_title("Aim vs makes vs positive-miss economics")

    ax = axes[1, 0]
    if buckets:
        labels = [b["label"] for b in buckets]
        bottom = [0] * len(buckets)
        colors = {
            "make": "#2a9d8f",
            "rim_miss": "#e9c46a",
            "timeout": "#f4a261",
            "floor": "#e76f51",
            "oob": "#6d6875",
            "settled": "#9b5de5",
            "miss": "#adb5bd",
        }
        for key in reason_order:
            vals = [b.get(key, 0) for b in buckets]
            if sum(vals) == 0:
                continue
            ax.bar(
                labels,
                vals,
                bottom=bottom,
                label=key,
                color=colors.get(key, "#999"),
                width=0.85,
            )
            bottom = [b + v for b, v in zip(bottom, vals)]
        ax.tick_params(axis="x", labelrotation=25)
    ax.set_ylabel("Shots per bucket")
    ax.set_xlabel("Episode bucket")
    ax.legend(loc="upper right", ncol=2, fontsize=7)
    ax.set_title("How episodes end")
    ax.grid(True, axis="y", alpha=0.3)

    ax = axes[1, 1]
    econ_order = [k for k in reason_order if k in by_reason]
    means = [sum(by_reason[k]) / len(by_reason[k]) for k in econ_order]
    colors_bar = [
        "#2a9d8f" if k == "make" else "#e9c46a" if k == "rim_miss" else "#adb5bd"
        for k in econ_order
    ]
    ax.bar(econ_order, means, color=colors_bar)
    ax.axhline(0, color="black", linewidth=0.8)
    ax.set_ylabel("Mean episode net RL")
    ax.set_xlabel("end_reason")
    ax.set_title("Economics: mean net RL by end_reason (Tier 1.5 gate)")
    ax.tick_params(axis="x", labelrotation=20)
    ax.grid(True, axis="y", alpha=0.3)
    for i, m in enumerate(means):
        ax.text(
            i, m, f"{m:+.2f}", ha="center", va="bottom" if m >= 0 else "top", fontsize=8
        )

    ax = axes[2, 0]
    if miss_fy:
        ax.hist(
            miss_fy, bins=30, alpha=0.45, color="#adb5bd", label="miss fy", density=True
        )
    if make_fy:
        ax.hist(
            make_fy,
            bins=min(12, max(3, len(make_fy))),
            alpha=0.85,
            color="#2a9d8f",
            label="make fy",
            density=True,
        )
    ax.set_xlabel("Impulse fy")
    ax.set_ylabel("Density")
    ax.set_title("Make signature — vertical impulse (fy)")
    ax.legend(fontsize=8)
    ax.grid(True, alpha=0.3)

    ax = axes[2, 1]
    if miss_fz:
        ax.hist(
            miss_fz, bins=30, alpha=0.45, color="#adb5bd", label="miss fz", density=True
        )
    if make_fz:
        ax.hist(
            make_fz,
            bins=min(12, max(3, len(make_fz))),
            alpha=0.85,
            color="#2a9d8f",
            label="make fz",
            density=True,
        )
    ax.set_xlabel("Impulse fz")
    ax.set_ylabel("Density")
    ax.set_title("Make signature — forward impulse (fz)")
    ax.legend(fontsize=8)
    ax.grid(True, alpha=0.3)

    output.parent.mkdir(parents=True, exist_ok=True)
    fig.savefig(output, dpi=150)
    plt.close(fig)
    print(f"Saved plot to {output}")

    pos_misses = 0
    miss_n = 0
    for r in train_shots:
        if _i(r, "scored") == 0 and r.get("episode_net_rl"):
            miss_n += 1
            if _f(r, "episode_net_rl") > 0:
                pos_misses += 1

    return {
        "episodes": n,
        "makes": makes,
        "success_pct": round(100.0 * makes / max(n, 1), 2),
        "toward_mean": round(sum(toward) / len(toward), 3) if toward else 0.0,
        "positive_miss_pct": (
            round(100.0 * pos_misses / max(miss_n, 1), 1) if miss_n else 0.0
        ),
        "reasons": dict(reasons),
        "econ_means": {k: round(sum(v) / len(v), 3) for k, v in by_reason.items()},
        "make_episodes": [
            _i(r, "iteration") for r in train_shots if _i(r, "scored") == 1
        ],
        "mean_net": round(sum(nets) / len(nets), 3) if nets else 0.0,
    }


def evaluate_tier15_pass(
    summary: dict,
    *,
    min_episodes: int = 100,
    rim_miss_max_mean: float = 0.5,
    positive_miss_max_pct: float = 25.0,
    bob_v4_baseline_positive_miss_pct: float = 44.0,
) -> tuple[bool, list[str]]:
    """Tier 1.5 contrast pass checks from ml-training-recommendations.md."""
    lines: list[str] = []
    econ = summary.get("econ_means") or {}
    make_mean = econ.get("make")
    rim_mean = econ.get("rim_miss")
    pos_miss = float(summary.get("positive_miss_pct") or 0.0)
    n = int(summary.get("episodes") or 0)

    if n < min_episodes:
        lines.append(f"FAIL sample: episodes={n} (need >= {min_episodes})")
    else:
        lines.append(f"OK sample: episodes={n}")

    if make_mean is None:
        lines.append("FAIL economics: no make episodes yet (need at least one make)")
    else:
        lines.append(f"OK make mean net RL={make_mean:+.3f}")

    if rim_mean is None:
        lines.append("FAIL economics: no rim_miss episodes")
    else:
        if rim_mean < rim_miss_max_mean:
            lines.append(
                f"OK rim_miss mean net RL={rim_mean:+.3f} (< {rim_miss_max_mean})"
            )
        else:
            lines.append(
                f"FAIL rim_miss mean net RL={rim_mean:+.3f} "
                f"(want < {rim_miss_max_mean}; bob-v4 was ~+1.69)"
            )
        if make_mean is not None and rim_mean < make_mean:
            lines.append(
                f"OK separation: rim_miss ({rim_mean:+.3f}) ≪ make ({make_mean:+.3f})"
            )
        elif make_mean is not None:
            lines.append(
                f"FAIL separation: rim_miss ({rim_mean:+.3f}) not ≪ make ({make_mean:+.3f})"
            )

    if pos_miss <= positive_miss_max_pct:
        lines.append(
            f"OK positive-miss={pos_miss:.1f}% "
            f"(<= {positive_miss_max_pct}%; bob-v4 baseline ~{bob_v4_baseline_positive_miss_pct}%)"
        )
    else:
        lines.append(
            f"FAIL positive-miss={pos_miss:.1f}% "
            f"(want <= {positive_miss_max_pct}%; bob-v4 was ~{bob_v4_baseline_positive_miss_pct}%)"
        )

    passed = n >= min_episodes and all(line.startswith("OK ") for line in lines)
    return passed, lines


def main() -> int:
    parser = argparse.ArgumentParser(description="Plot Bob learning dashboard")
    parser.add_argument(
        "--session",
        type=Path,
        default=repo_root() / "summaries" / "bob_session.csv",
    )
    parser.add_argument(
        "--shots",
        type=Path,
        default=repo_root() / "summaries" / "bob_shots.csv",
    )
    parser.add_argument("--since", default=None)
    parser.add_argument(
        "--output",
        type=Path,
        default=repo_root() / "docs" / "results" / "bob_v4.1_learning_dashboard.png",
    )
    parser.add_argument(
        "--title",
        default="Bob learning dashboard (Tier 1.5 diagnostics)",
    )
    parser.add_argument(
        "--check-pass",
        action="store_true",
        help="Exit 0 only if Tier 1.5 contrast pass checks succeed",
    )
    parser.add_argument(
        "--min-episodes",
        type=int,
        default=100,
        help="Minimum connected PPO episodes for --check-pass (default 100)",
    )
    args = parser.parse_args()

    session = load_csv(args.session, args.since)
    shots = load_csv(args.shots, args.since)
    if not session and not shots:
        print(
            "No rows found. Run a connected PPO session first.",
            file=__import__("sys").stderr,
        )
        return 1

    summary = plot_dashboard(session, shots, args.output, args.title)
    print(
        f"Episodes={summary['episodes']} makes={summary['makes']} "
        f"success={summary['success_pct']}% toward={summary['toward_mean']:+.3f} "
        f"positive_miss={summary['positive_miss_pct']}%"
    )
    print(f"Economics means: {summary['econ_means']}")

    if not args.check_pass:
        return 0

    passed, lines = evaluate_tier15_pass(summary, min_episodes=args.min_episodes)
    print("\nTier 1.5 contrast pass checks:")
    for line in lines:
        print(f"  {line}")
    print("PASS" if passed else "FAIL")
    return 0 if passed else 2


if __name__ == "__main__":
    raise SystemExit(main())
