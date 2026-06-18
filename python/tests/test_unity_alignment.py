"""Guard alignment between Unity agent settings and trainer config."""

from __future__ import annotations

import json
import re
from pathlib import Path

# Expected ML-Agents settings for BobAgent.cs / BehaviorParameters in scene builder.
EXPECTED_BEHAVIOR_NAME = "Bob"
EXPECTED_BEHAVIOR_TYPE = "Default"
EXPECTED_BEHAVIOR_TYPE_ENUM = 0
EXPECTED_VECTOR_OBSERVATIONS = 8
EXPECTED_CONTINUOUS_ACTIONS = 3

SCENE_PATH = Path("Assets/Scenes/BobTraining.unity")
EDITOR_SCRIPTS = (
    Path("Assets/Scripts/Editor/BobSceneValidator.cs"),
    Path("Assets/Scripts/Editor/BobTrainingSceneBuilder.cs"),
)
LEGACY_EDITOR_SCRIPTS = (
    Path("Assets/Editor/BobSceneValidator.cs"),
    Path("Assets/Editor/BobTrainingSceneBuilder.cs"),
)
MCP_BOOTSTRAP = Path("Assets/Editor/Mcp/BobMcpBootstrap.cs")
MCP_ASMDEF = Path("Assets/Editor/Mcp/Bob.Mcp.asmdef")
EDITOR_ASMDEF = Path("Assets/Editor/Bob.Editor.asmdef")
VALIDATE_SCENE_SCRIPT = Path("scripts/validate-scene.sh")

MCP_PREF_KEYS = (
    "MCPForUnity.AutoStartOnLoad",
    "MCPForUnity.LockCursorConfig",
    "MCPForUnity.ClientProjectDir",
)


def test_yaml_behavior_name(trainer_config: dict) -> None:
    assert EXPECTED_BEHAVIOR_NAME in trainer_config["behaviors"]


def test_yaml_trainer_is_ppo(trainer_config: dict) -> None:
    bob = trainer_config["behaviors"][EXPECTED_BEHAVIOR_NAME]
    assert bob["trainer_type"] == "ppo"


def test_unity_agent_constants_documented() -> None:
    """Constants mirror Assets/Scripts/BobAgent.cs and scene builder."""
    assert EXPECTED_BEHAVIOR_TYPE == "Default"
    assert EXPECTED_VECTOR_OBSERVATIONS == 8
    assert EXPECTED_CONTINUOUS_ACTIONS == 3


def test_editor_scripts_live_under_scripts_editor(repo_root: Path) -> None:
    """Scene builder/validator moved out of Assets/Editor/ root."""
    for path in EDITOR_SCRIPTS:
        assert (repo_root / path).is_file(), f"Missing editor script: {path}"
    for path in LEGACY_EDITOR_SCRIPTS:
        assert not (repo_root / path).exists(), f"Legacy path still present: {path}"


def test_mcp_asmdef_layout(repo_root: Path) -> None:
    assert (repo_root / MCP_BOOTSTRAP).is_file()
    asmdef_path = repo_root / MCP_ASMDEF
    assert asmdef_path.is_file()
    asmdef = json.loads(asmdef_path.read_text())
    assert asmdef["name"] == "Bob.Mcp"
    assert "MCPForUnity.Editor" in asmdef["references"]
    assert asmdef["includePlatforms"] == ["Editor"]


def test_bob_editor_asmdef_exists(repo_root: Path) -> None:
    asmdef_path = repo_root / EDITOR_ASMDEF
    assert asmdef_path.is_file()
    asmdef = json.loads(asmdef_path.read_text())
    assert asmdef["name"] == "Bob.Editor"
    assert "Bob" in asmdef["references"]
    assert "Unity.ML-Agents" in asmdef["references"]


def test_mcp_bootstrap_pref_keys(repo_root: Path) -> None:
    """BobMcpBootstrap mirrors MCPForUnity EditorPrefKeys (offline string guard)."""
    source = (repo_root / MCP_BOOTSTRAP).read_text()
    for key in MCP_PREF_KEYS:
        assert key in source, f"MCP pref key missing from bootstrap: {key}"
    assert 'ScenePath = "Assets/Scenes/BobTraining.unity"' in source


def test_validate_scene_script_wires_cli_methods(repo_root: Path) -> None:
    script = (repo_root / VALIDATE_SCENE_SCRIPT).read_text()
    assert "ArcAcademyHdrpSetup.EnsureHdrpFromCli" in script
    assert "BobTrainingSceneBuilder.CreateTrainingSceneFromCli" in script
    assert "BobSceneValidator.VerifyFromCli" in script
    assert "VALIDATE_PASS" in script


def test_bob_training_scene_yaml_alignment(repo_root: Path) -> None:
    """Offline mirror of BobSceneValidator checks on BobTraining.unity YAML."""
    scene_path = repo_root / SCENE_PATH
    assert scene_path.is_file(), f"Missing training scene: {SCENE_PATH}"
    content = scene_path.read_text()

    assert "m_EditorClassIdentifier: Bob::BobAgent" in content
    assert f"m_BehaviorName: {EXPECTED_BEHAVIOR_NAME}" in content
    assert f"m_BehaviorType: {EXPECTED_BEHAVIOR_TYPE_ENUM}" in content
    assert f"VectorObservationSize: {EXPECTED_VECTOR_OBSERVATIONS}" in content
    assert f"m_NumContinuousActions: {EXPECTED_CONTINUOUS_ACTIONS}" in content
    assert re.search(r"^\s*hoop: \{fileID: [1-9]\d*", content, re.MULTILINE)
    assert "m_Name: TrainingArena" in content
    assert "m_Name: CourtFloor" in content
    assert "m_EditorClassIdentifier: Bob::HoopScoreZone" in content
    assert "m_EditorClassIdentifier: Bob::ArcAcademyManager" in content
    assert "m_EditorClassIdentifier: Bob::MovableHoop" in content
    assert "m_Name: SpawnPad" in content
    assert "m_Name: DistanceMarkings" in content
    assert "m_Name: TrainingBays" in content
    assert "m_Name: MountainWindow" in content
    assert "m_Name: DecorativeHoops" in content
    assert "m_Name: TrajectoryVisuals" in content
    assert "m_EditorClassIdentifier: Bob::ArcTrajectoryVisual" in content


def test_arc_academy_visual_scripts_exist(repo_root: Path) -> None:
    assert (repo_root / "Assets/Scripts/ArcTrajectoryVisual.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/ArcAcademyMaterialFactory.cs").is_file()
    assert (repo_root / "docs/design/arc-academy-reference.jpg").is_file()


def test_arc_academy_visual_builder_wiring(repo_root: Path) -> None:
    builder = (repo_root / EDITOR_SCRIPTS[1]).read_text()
    assert "CreateMountainWindow" in builder
    assert "CreateDecorativeHoops" in builder
    assert "CreateTrajectoryVisuals" in builder
    assert "CreateRoboticLauncherArm" in builder
    assert "CreateHdrpVolume" in builder
    assert "CreateAdaptiveProbeVolume" in builder
    assert "CreateReflectionProbe" in builder
    assert "ArcAcademyMaterialFactory" in builder
    assert "CreateHdrpLit" in builder or "CreateGlassBackboard" in builder
    assert "ArcTrajectoryVisual" in builder

    validator = (repo_root / EDITOR_SCRIPTS[0]).read_text()
    assert "MountainWindow" in validator
    assert "TrajectoryVisuals" in validator
    assert "DecorativeHoops" in validator
    assert "DecorativeHoopMarker" in validator
    assert "RoboticLauncherVisual" in validator
    assert "HdrpVolume" in validator
    assert "AdaptiveProbeVolume" in validator
    assert "ArcTrajectoryVisual" in validator


def test_arc_academy_layout_and_scripts_exist(repo_root: Path) -> None:
    layout = (repo_root / "Assets/Scripts/ArcAcademyLayout.cs").read_text()
    assert "TrainingBayCount = 8" in layout
    assert (repo_root / "Assets/Scripts/ArcAcademyManager.cs").is_file()
    assert (repo_root / "Assets/Scripts/MovableHoop.cs").is_file()
    assert (repo_root / "Assets/Scripts/DecorativeHoopMarker.cs").is_file()
    assert (repo_root / "Assets/Scripts/RoboticLauncherVisual.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/ArcAcademyHdrpSetup.cs").is_file()


def test_arc_academy_builder_wiring(repo_root: Path) -> None:
    builder = (repo_root / EDITOR_SCRIPTS[1]).read_text()
    assert "ArcAcademyLayout" in builder
    assert "ArcAcademyManager" in builder
    assert "MovableHoop" in builder
    assert "WireReferences" in builder
    assert "DistanceMarkings" in builder
    assert "SpawnPad" in builder
    assert "TrainingBays" in builder
    assert "CreateTrainingBays" in builder

    validator = (repo_root / EDITOR_SCRIPTS[0]).read_text()
    assert "ArcAcademyManager" in validator
    assert "MovableHoop" in validator
    assert "SpawnPad" in validator
    assert "DistanceMarkings" in validator
    assert "TrainingBays" in validator


def test_bob_court_layout_referenced_in_builder(repo_root: Path) -> None:
    builder = (repo_root / EDITOR_SCRIPTS[1]).read_text()
    assert "ArcAcademyLayout" in builder
    assert "TrainingArena" in builder or "ArcAcademyLayout.ArenaName" in builder
    assert "HoopScoreZone" in builder
    assert (repo_root / "Assets/Scripts/BobCourtLayout.cs").is_file()


def test_bob_court_layout_in_agent(repo_root: Path) -> None:
    agent = (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    assert "ArcAcademyLayout" in agent
    assert "RegisterMadeShot" in agent
    assert "CalculateArcQuality" in agent
    assert "PrepareEpisode" in agent
    assert (repo_root / "Assets/Scripts/HoopScoreZone.cs").is_file()


def test_scene_builder_constants_match_validator(repo_root: Path) -> None:
    """Scene builder hardcodes the same ML-Agents values BobSceneValidator asserts."""
    builder = (repo_root / EDITOR_SCRIPTS[1]).read_text()
    assert f'BehaviorName = "{EXPECTED_BEHAVIOR_NAME}"' in builder
    assert "BehaviorType.Default" in builder
    assert f"VectorObservationSize = {EXPECTED_VECTOR_OBSERVATIONS}" in builder
    assert f"ActionSpec.MakeContinuous({EXPECTED_CONTINUOUS_ACTIONS})" in builder
    assert f'ScenePath = "{SCENE_PATH.as_posix()}"' in builder

    validator = (repo_root / EDITOR_SCRIPTS[0]).read_text()
    assert f'BehaviorName != "{EXPECTED_BEHAVIOR_NAME}"' in validator
    assert f"VectorObservationSize != {EXPECTED_VECTOR_OBSERVATIONS}" in validator
    assert f"NumContinuousActions != {EXPECTED_CONTINUOUS_ACTIONS}" in validator

