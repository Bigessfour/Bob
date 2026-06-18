from __future__ import annotations

import subprocess

import pytest
import yaml


def test_config_parses(config_path) -> None:
    with config_path.open() as f:
        config = yaml.safe_load(f)
    assert isinstance(config, dict)


def test_config_has_bob_behavior(trainer_config: dict) -> None:
    assert "behaviors" in trainer_config
    assert "Bob" in trainer_config["behaviors"]
    bob = trainer_config["behaviors"]["Bob"]
    assert bob["trainer_type"] == "ppo"


def test_config_required_sections(trainer_config: dict) -> None:
    for section in ("behaviors", "engine_settings", "checkpoint_settings"):
        assert section in trainer_config, f"Missing section: {section}"


def test_config_behavior_name_matches_project(trainer_config: dict) -> None:
    assert "Bob" in trainer_config["behaviors"]


@pytest.mark.requires_mlagents
def test_mlagents_learn_help() -> None:
    result = subprocess.run(
        ["mlagents-learn", "--help"],
        capture_output=True,
        text=True,
    )
    assert result.returncode == 0
    assert "usage:" in result.stdout.lower() or "mlagents" in result.stdout.lower()
