from __future__ import annotations

import sys
from pathlib import Path
from unittest.mock import MagicMock, patch

# Allow importing from python/scripts/
sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "scripts"))
from plot_rewards import (
    find_training_log,
    load_rewards,
)
from plot_rewards import main as plot_main  # noqa: E402
from plot_rewards import (
    plot_rewards,
)


def test_find_training_log_missing_dir(tmp_path: Path) -> None:
    assert find_training_log(tmp_path / "nonexistent", None) is None  # nosec B101


def test_find_training_log_finds_csv(tmp_path: Path) -> None:
    run_dir = tmp_path / "bob-v0" / "Bob"
    run_dir.mkdir(parents=True)
    csv_file = run_dir / "TrainingRewards.csv"
    csv_file.write_text("Step,Value\n1000,0.5\n")

    found = find_training_log(tmp_path, "bob-v0")
    assert found == csv_file  # nosec B101


def test_find_training_log_no_match_for_run_id(tmp_path: Path) -> None:
    run_dir = tmp_path / "other-run" / "Bob"
    run_dir.mkdir(parents=True)
    (run_dir / "TrainingRewards.csv").write_text("Step,Value\n1000,0.5\n")

    assert find_training_log(tmp_path, "bob-v0") is None  # nosec B101


def test_load_rewards_parses_csv(tmp_path: Path) -> None:
    csv_file = tmp_path / "TrainingRewards.csv"
    csv_file.write_text("Step,Value\n1000,0.5\n2000,1.2\n3000,2.0\n")

    steps, rewards = load_rewards(csv_file)
    assert steps == [1000, 2000, 3000]  # nosec B101
    assert rewards == [0.5, 1.2, 2.0]  # nosec B101


def test_plot_rewards_writes_file(tmp_path: Path) -> None:
    output = tmp_path / "reward_curve.png"
    mock_plt = MagicMock()
    mock_fig = MagicMock()
    mock_plt.figure.return_value = mock_fig

    with patch.dict("sys.modules", {"matplotlib.pyplot": mock_plt}):
        plot_rewards([100, 200], [0.1, 0.5], output)

    mock_plt.savefig.assert_called_once()
    mock_plt.close.assert_called_once()


def test_main_returns_error_on_no_csv(tmp_path: Path, monkeypatch, capsys) -> None:
    monkeypatch.setattr(
        sys, "argv", ["plot_rewards.py", "--results-dir", str(tmp_path)]
    )
    ret = plot_main()
    captured = capsys.readouterr()
    assert ret == 1
    assert "No TrainingRewards.csv" in captured.err


def test_main_prints_no_data(tmp_path: Path, monkeypatch, capsys) -> None:
    run_dir = tmp_path / "bob-v0" / "Bob"
    run_dir.mkdir(parents=True)
    (run_dir / "TrainingRewards.csv").write_text("Step,Value\n")
    monkeypatch.setattr(
        sys,
        "argv",
        ["plot_rewards.py", "--results-dir", str(tmp_path), "--run-id", "bob-v0"],
    )
    ret = plot_main()
    captured = capsys.readouterr()
    assert ret == 1
    assert "No data" in captured.err
