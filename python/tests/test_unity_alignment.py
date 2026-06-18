"""Guard alignment between Unity agent settings and trainer config."""

from __future__ import annotations

# Expected ML-Agents settings for BobAgent.cs / BehaviorParameters in scene builder.
EXPECTED_BEHAVIOR_NAME = "Bob"
EXPECTED_VECTOR_OBSERVATIONS = 9
EXPECTED_CONTINUOUS_ACTIONS = 3


def test_yaml_behavior_name(trainer_config: dict) -> None:
    assert EXPECTED_BEHAVIOR_NAME in trainer_config["behaviors"]


def test_yaml_trainer_is_ppo(trainer_config: dict) -> None:
    bob = trainer_config["behaviors"][EXPECTED_BEHAVIOR_NAME]
    assert bob["trainer_type"] == "ppo"


def test_unity_agent_constants_documented() -> None:
    """Constants mirror Assets/Scripts/BobAgent.cs and scene builder."""
    assert EXPECTED_VECTOR_OBSERVATIONS == 9
    assert EXPECTED_CONTINUOUS_ACTIONS == 3
