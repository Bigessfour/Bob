from __future__ import annotations

import sys
from pathlib import Path
from unittest.mock import MagicMock, patch

sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "scripts"))
from plot_training_progress import (  # noqa: E402
    load_session_log,
    main as plot_progress_main,
    plot_training_progress,
)


def test_load_session_log_parses_csv(tmp_path: Path) -> None:
    csv_file = tmp_path / "bob_session.csv"
    csv_file.write_text(
        "timestamp,iteration,scored,basketball_points,session_success_pct,"
        "rolling_success_pct,rolling_arc_quality,net_rl\n"
        "2026-06-22T00:00:00Z,1,0,0,0.00,0.00,12.50,0.000\n"
        "2026-06-22T00:00:01Z,2,1,1,50.00,50.00,25.00,1.250\n"
    )

    session = load_session_log(csv_file)
    assert session["iteration"] == [1, 2]  # nosec B101
    assert session["session_success_pct"] == [0.0, 50.0]  # nosec B101
    assert session["rolling_arc_quality"] == [12.5, 25.0]  # nosec B101


def test_plot_training_progress_writes_file(tmp_path: Path) -> None:
    output = tmp_path / "training_progress.png"
    session = {
        "iteration": [1, 2, 3],
        "session_success_pct": [0.0, 50.0, 66.7],
        "rolling_success_pct": [0.0, 50.0, 66.7],
        "rolling_arc_quality": [10.0, 20.0, 30.0],
        "net_rl": [0.0, 1.0, 2.0],
    }

    mock_plt = MagicMock()
    mock_fig = MagicMock()
    mock_axes = MagicMock()
    mock_plt.subplots.return_value = (mock_fig, mock_axes)

    with patch.dict("sys.modules", {"matplotlib.pyplot": mock_plt}):
        plot_training_progress(session, None, None, output)

    mock_fig.savefig.assert_called_once()
    mock_plt.close.assert_called_once_with(mock_fig)


def test_main_missing_data_returns_error(tmp_path: Path, monkeypatch, capsys) -> None:
    monkeypatch.setattr(
        sys,
        "argv",
        [
            "plot_training_progress.py",
            "--session-log",
            str(tmp_path / "missing.csv"),
            "--results-dir",
            str(tmp_path / "results"),
        ],
    )
    exit_code = plot_progress_main()
    captured = capsys.readouterr()
    assert exit_code == 1  # nosec B101
    assert "No training data found" in captured.err  # nosec B101
