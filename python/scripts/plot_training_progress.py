#!/usr/bin/env python3
"""Plot Bob training progress from summaries/bob_session.csv and ML-Agents rewards.

Usage:
    python scripts/plot_training_progress.py
    python scripts/plot_training_progress.py --run-id bob-v0 --output ../docs/results/training_progress.png
"""

from __future__ import annotations

import argparse
import csv
import sys
from pathlib import Path

from plot_rewards import find_training_log, load_rewards


def default_summaries_path() -> Path:
    return Path(__file__).resolve().parent.parent.parent / "summaries" / "bob_session.csv"


def load_session_log(csv_path: Path) -> dict[str, list[float | int]]:
    """Parse summaries/bob_session.csv into column lists."""
    columns: dict[str, list[float | int]] = {
        "iteration": [],
        "session_success_pct": [],
        "rolling_success_pct": [],
        "rolling_arc_quality": [],
        "net_rl": [],
    }

    with csv_path.open(newline="") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            columns["iteration"].append(int(row["iteration"]))
            columns["session_success_pct"].append(float(row["session_success_pct"]))
            columns["rolling_success_pct"].append(float(row["rolling_success_pct"]))
            columns["rolling_arc_quality"].append(float(row["rolling_arc_quality"]))
            columns["net_rl"].append(float(row["net_rl"]))

    return columns


def plot_training_progress(
    session: dict[str, list[float | int]] | None,
    reward_steps: list[int] | None,
    reward_values: list[float] | None,
    output: Path,
) -> None:
    """Render success-rate and optional reward curves."""
    import matplotlib.pyplot as plt

    has_session = session is not None and bool(session["iteration"])
    has_rewards = reward_steps is not None and reward_values is not None and bool(reward_steps)

    if not has_session and not has_rewards:
        raise ValueError("No session log or reward data to plot")

    panel_count = int(has_session) + int(has_rewards)
    fig, axes = plt.subplots(panel_count, 1, figsize=(10, 4 * panel_count), squeeze=False)
    axis_index = 0

    if has_session:
        ax = axes[axis_index, 0]
        iterations = session["iteration"]
        ax.plot(iterations, session["session_success_pct"], label="Session success %", linewidth=1.5)
        ax.plot(
            iterations,
            session["rolling_success_pct"],
            label="Rolling success %",
            linewidth=1.5,
        )
        ax.plot(
            iterations,
            session["rolling_arc_quality"],
            label="Rolling arc quality %",
            linewidth=1.2,
            linestyle="--",
        )
        ax.set_xlabel("Iteration")
        ax.set_ylabel("Percent")
        ax.set_title("Bob — In-Scene Success Metrics")
        ax.grid(True, alpha=0.3)
        ax.legend(loc="best")
        axis_index += 1

    if has_rewards:
        ax = axes[axis_index, 0]
        ax.plot(reward_steps, reward_values, color="#f97316", linewidth=1.5)
        ax.set_xlabel("Training Step")
        ax.set_ylabel("Mean Reward")
        ax.set_title("Bob — ML-Agents Reward Curve")
        ax.grid(True, alpha=0.3)

    fig.tight_layout()
    output.parent.mkdir(parents=True, exist_ok=True)
    fig.savefig(output, dpi=150)
    plt.close(fig)


def main() -> int:
    parser = argparse.ArgumentParser(description="Plot Bob training progress")
    parser.add_argument(
        "--session-log",
        type=Path,
        default=default_summaries_path(),
        help="Path to summaries/bob_session.csv",
    )
    parser.add_argument(
        "--results-dir",
        type=Path,
        default=Path(__file__).resolve().parent.parent.parent / "results",
        help="Path to ML-Agents results/ directory",
    )
    parser.add_argument("--run-id", type=str, default=None, help="Filter rewards by run ID")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("training_progress.png"),
        help="Output image path",
    )
    args = parser.parse_args()

    session = None
    if args.session_log.is_file():
        session = load_session_log(args.session_log)

    reward_steps: list[int] | None = None
    reward_values: list[float] | None = None
    reward_csv = find_training_log(args.results_dir, args.run_id)
    if reward_csv is not None:
        reward_steps, reward_values = load_rewards(reward_csv)

    if session is None and reward_csv is None:
        print(
            "No training data found. Run Play with ./scripts/train.sh connected, then retry.\n"
            f"  Session log: {args.session_log}\n"
            f"  Rewards: {args.results_dir}/**/TrainingRewards.csv",
            file=sys.stderr,
        )
        return 1

    plot_training_progress(session, reward_steps, reward_values, args.output)
    print(f"Saved plot to {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
