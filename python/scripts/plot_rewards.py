#!/usr/bin/env python3
"""Plot ML-Agents training reward curves from results/ directory.

Usage:
    python scripts/plot_rewards.py
    python scripts/plot_rewards.py --run-id bob-v0
    python scripts/plot_rewards.py --results-dir ../results --output reward_curve.png
"""

from __future__ import annotations

import argparse
import csv
import sys
from pathlib import Path


def find_training_log(results_dir: Path, run_id: str | None) -> Path | None:
    """Locate the first TrainingRewards.csv under results/."""
    if not results_dir.exists():
        return None

    candidates = sorted(results_dir.glob("**/TrainingRewards.csv"))
    if run_id:
        candidates = [p for p in candidates if run_id in str(p)]

    return candidates[0] if candidates else None


def load_rewards(csv_path: Path) -> tuple[list[int], list[float]]:
    """Parse ML-Agents TrainingRewards.csv into step and reward lists."""
    steps: list[int] = []
    rewards: list[float] = []

    with csv_path.open(newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            steps.append(int(row["Step"]))
            rewards.append(float(row["Value"]))

    return steps, rewards


def plot_rewards(steps: list[int], rewards: list[float], output: Path) -> None:
    """Render reward curve with matplotlib."""
    import matplotlib.pyplot as plt

    plt.figure(figsize=(10, 5))
    plt.plot(steps, rewards, linewidth=1.5)
    plt.xlabel("Training Step")
    plt.ylabel("Mean Reward")
    plt.title("Bob — Training Reward Curve")
    plt.grid(True, alpha=0.3)
    plt.tight_layout()
    plt.savefig(output, dpi=150)
    plt.close()
    print(f"Saved plot to {output}")


def main() -> int:
    parser = argparse.ArgumentParser(description="Plot ML-Agents training rewards")
    parser.add_argument(
        "--results-dir",
        type=Path,
        default=Path(__file__).resolve().parent.parent.parent / "results",
        help="Path to ML-Agents results/ directory",
    )
    parser.add_argument("--run-id", type=str, default=None, help="Filter by run ID")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("reward_curve.png"),
        help="Output image path",
    )
    args = parser.parse_args()

    csv_path = find_training_log(args.results_dir, args.run_id)
    if csv_path is None:
        print(
            f"No TrainingRewards.csv found in {args.results_dir}. "
            "Run training first: mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0",
            file=sys.stderr,
        )
        return 1

    steps, rewards = load_rewards(csv_path)
    if not steps:
        print(f"No data in {csv_path}", file=sys.stderr)
        return 1

    plot_rewards(steps, rewards, args.output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
