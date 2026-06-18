from __future__ import annotations

import shutil
import sys
from pathlib import Path

import pytest
import yaml

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
PYTHON_ROOT = REPO_ROOT / "python"
if str(PYTHON_ROOT) not in sys.path:
    sys.path.insert(0, str(PYTHON_ROOT))

CONFIG_PATH = REPO_ROOT / "config" / "bob_free_throw.yaml"


@pytest.fixture
def repo_root() -> Path:
    return REPO_ROOT


@pytest.fixture
def config_path() -> Path:
    return CONFIG_PATH


@pytest.fixture
def trainer_config(config_path: Path) -> dict:
    with config_path.open() as f:
        return yaml.safe_load(f)


def mlagents_available() -> bool:
    return shutil.which("mlagents-learn") is not None


def pytest_configure(config: pytest.Config) -> None:
    config.addinivalue_line(
        "markers",
        "requires_mlagents: needs mlagents-learn on PATH",
    )


def pytest_collection_modifyitems(
    config: pytest.Config, items: list[pytest.Item]
) -> None:
    if mlagents_available():
        return
    skip = pytest.mark.skip(
        reason="mlagents-learn not installed (use CI or Docker on Apple Silicon)"
    )
    for item in items:
        if "requires_mlagents" in item.keywords:
            item.add_marker(skip)
