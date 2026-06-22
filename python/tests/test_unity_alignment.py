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
MCP_CONFIG = Path(".cursor/mcp.json")
UNITY_MCP_SCRIPT = Path("scripts/unity-mcp.sh")
BOB_MCP_TOOLS = Path("Assets/Scripts/Editor/BobUnityMcpTools.cs")
SCENE_EDITOR_ASMDEF = Path("Assets/Scripts/Editor/Bob.SceneEditor.asmdef")
MANIFEST = Path("Packages/manifest.json")
EDITOR_ASMDEF = Path("Assets/Editor/Bob.Editor.asmdef")
VALIDATE_SCENE_SCRIPT = Path("scripts/validate-scene.sh")


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


def test_unity_mcp_cursor_config(repo_root: Path) -> None:
    """Cursor MCP uses official Unity relay script, not CoplayDev HTTP."""
    config = json.loads((repo_root / MCP_CONFIG).read_text())
    servers = config["mcpServers"]
    assert "unity-mcp" in servers
    assert "unityMCP" not in servers
    unity = servers["unity-mcp"]
    assert unity["command"].endswith("unity-mcp.sh")
    script = (repo_root / UNITY_MCP_SCRIPT).read_text()
    assert "--mcp" in script
    assert ".unity/relay" in script


def test_manifest_uses_official_unity_mcp(repo_root: Path) -> None:
    manifest = json.loads((repo_root / MANIFEST).read_text())
    deps = manifest["dependencies"]
    assert "com.coplaydev.unity-mcp" not in deps
    assert "com.unity.ai.assistant" in deps


def test_bob_unity_mcp_tools_registered(repo_root: Path) -> None:
    source = (repo_root / BOB_MCP_TOOLS).read_text()
    assert "[McpTool(" in source
    assert "bob_setup_simple_arena" in source
    assert "bob_open_training_scene" in source
    assert 'ScenePath = "Assets/Scenes/BobTraining.unity"' in source


def test_scene_editor_references_unity_mcp(repo_root: Path) -> None:
    asmdef = json.loads((repo_root / SCENE_EDITOR_ASMDEF).read_text())
    assert "Unity.AI.MCP.Editor" in asmdef["references"]


def test_legacy_coplay_mcp_bootstrap_removed(repo_root: Path) -> None:
    assert not (repo_root / "Assets/Editor/Mcp/BobMcpBootstrap.cs").exists()
    assert not (repo_root / "scripts/unity-mcp-http.sh").exists()
    assert not (repo_root / "scripts/mcp-connect.sh").exists()


def test_bob_editor_asmdef_exists(repo_root: Path) -> None:
    asmdef_path = repo_root / EDITOR_ASMDEF
    assert asmdef_path.is_file()
    asmdef = json.loads(asmdef_path.read_text())
    assert asmdef["name"] == "Bob.Editor"
    assert "Bob" in asmdef["references"]
    assert "Unity.ML-Agents" in asmdef["references"]


def test_bob_runtime_asmdef_references_hdrp(repo_root: Path) -> None:
    asmdef_path = repo_root / "Assets/Scripts/Bob.asmdef"
    asmdef = json.loads(asmdef_path.read_text())
    assert asmdef["name"] == "Bob"
    assert "Unity.RenderPipelines.Core.Runtime" in asmdef["references"]
    assert "Unity.RenderPipelines.HighDefinition.Runtime" in asmdef["references"]


def test_validate_scene_script_wires_cli_methods(repo_root: Path) -> None:
    script = (repo_root / VALIDATE_SCENE_SCRIPT).read_text()
    assert "ArcAcademyHdrpSetup.EnsureHdrpFromCli" in script
    assert "BobTrainingSceneBuilder.CreateTrainingSceneFromCli" in script
    assert "SimpleArcAcademyArenaBuilder.ApplyFromCli" in script
    assert "BobSceneValidator.VerifyFromCli" in script
    assert "VALIDATE_PASS" in script


def test_simple_arc_academy_wiring(repo_root: Path) -> None:
    """Offline mirror of VerifySimpleArcAcademy + builder constants."""
    arena = (repo_root / "Assets/Scripts/SimpleArcAcademyArena.cs").read_text()
    assert 'RootName = "SimpleArcAcademyArena"' in arena
    assert 'SpawnPointName = "SpawnPoint"' in arena
    assert 'BobPrefabPath = "Assets/Prefabs/Prefab_Bob.prefab"' in arena
    assert 'GoalBudgetSurplusName = "Goal_BudgetSurplus"' in arena

    builder = (repo_root / "Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs").read_text()
    assert "EnsureSpawnAndManager" in builder
    assert "WireBobToArena" in builder
    assert "HideLegacyCourtVisuals" in builder
    assert "TrainingBays" in builder
    assert "ApplyLabScenePreset" in builder
    assert "EnsureBobFace" in builder
    assert "EnsureSingleBasketball" in builder
    assert "BasketballProjectileSetup" in builder
    assert "BobWallHudBuilder.EnsureWallTrainingHud" in builder
    assert "EnsurePowerPathPulse" in builder
    assert "Mat_Wall_Tile_White" in builder
    assert "ApplyFromCli" in builder

    preset = (repo_root / "Assets/Scripts/ArcAcademyLabRenderPreset.cs").read_text()
    assert "ApplyLabViewPreset" in preset

    camera = (repo_root / "Assets/Scripts/ArcAcademyDemoCamera.cs").read_text()
    assert "ResetToLabHero" in camera
    assert "LabHero" in camera

    assert "LabCameraFieldOfView" in arena
    assert "ShowBudgetFlavorProps" in arena
    assert "BasketballPrefabPath" in arena

    assert (repo_root / "Assets/Scripts/BasketballProjectileSetup.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobShotArcPreview.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobWallTrainingHud.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobProceduralAnimator.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobFaceExpression.cs").is_file()
    assert (repo_root / "Assets/Scripts/ArcAcademyPowerPathPulse.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/BobWallHudBuilder.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/SimpleArenaTextureFactory.cs").is_file()

    manager = (repo_root / "Assets/Scripts/SimpleArcArenaManager.cs").read_text()
    assert "GetBobSpawnPosition" in manager
    assert "ResetEpisode" in manager

    arc_mgr = (repo_root / "Assets/Scripts/ArcAcademyManager.cs").read_text()
    assert "SimpleArcArenaManager.Instance" in arc_mgr
    assert "BobSpeechBubble" in arc_mgr
    assert "BobFaceExpression" in arc_mgr

    stats = (repo_root / "Assets/Scripts/BobTrainingStats.cs").read_text()
    assert "FlushEpisodeArcQuality" in stats
    assert "RollingAverageArcQuality" in stats
    assert "BobTrainingSessionLog" in stats

    assert (repo_root / "Assets/Scripts/BobTrainingSessionLog.cs").is_file()
    assert (repo_root / "python/scripts/plot_training_progress.py").is_file()

    agent_src = (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    assert "episodePeakArcQuality" in agent_src
    assert "BobProceduralAnimator" in agent_src
    assert "ArcAcademyPowerPathPulse" in agent_src

    validator = (repo_root / "Assets/Scripts/Editor/BobSceneValidator.cs").read_text()
    assert "VerifySimpleArcAcademy" in validator
    assert "projectileBody must reference Basketball" in validator
    assert "Exactly one Basketball" in validator
    assert "BobWallTrainingHud" in validator
    assert "SimpleArcAcademyArena.BobPrefabPath" in validator

    assert (repo_root / "Assets/Prefabs/Prefab_Bob.prefab").is_file()
    assert (repo_root / "Assets/Prefabs/Prefab_SimpleArena.prefab").is_file()
    assert (repo_root / "Assets/AssistantCustomInstructions.txt").is_file()


def test_bob_training_scene_simple_arena_yaml(repo_root: Path) -> None:
    content = (repo_root / SCENE_PATH).read_text()
    assert "m_Name: SimpleArcAcademyArena" in content
    assert "m_Name: SpawnPoint" in content
    assert "Bob::SimpleArcArenaManager" in content
    assert "m_Name: Basketball" in content or "value: Basketball" in content
    assert "projectileBody:" in content or "propertyPath: projectileBody" in content
    assert "Bob::SimpleBasketball" in content or "Bob::BobShotArcPreview" in content
    assert "Bob::BobWallTrainingHud" in content or "LabTrainingHud" in content


def test_bob_training_scene_yaml_alignment(repo_root: Path) -> None:
    """Offline mirror of BobSceneValidator checks on BobTraining.unity YAML."""
    scene_path = repo_root / SCENE_PATH
    assert scene_path.is_file(), f"Missing training scene: {SCENE_PATH}"
    content = scene_path.read_text()
    bob_prefab = (repo_root / "Assets/Prefabs/Prefab_Bob.prefab").read_text()
    ml_agents_blob = content + bob_prefab

    assert "m_EditorClassIdentifier: Bob::BobAgent" in content or "Bob::BobAgent" in bob_prefab
    assert f"m_BehaviorName: {EXPECTED_BEHAVIOR_NAME}" in ml_agents_blob
    assert f"m_BehaviorType: {EXPECTED_BEHAVIOR_TYPE_ENUM}" in ml_agents_blob
    assert f"VectorObservationSize: {EXPECTED_VECTOR_OBSERVATIONS}" in ml_agents_blob
    assert f"m_NumContinuousActions: {EXPECTED_CONTINUOUS_ACTIONS}" in ml_agents_blob
    hoop_wired = re.search(
        r"^\s*hoop: \{fileID: [1-9]\d*", ml_agents_blob, re.MULTILINE
    ) or re.search(
        r"propertyPath: hoop\s*\n\s*value:\s*\n\s*objectReference: \{fileID: [1-9]\d*",
        content,
    )
    assert hoop_wired, "Bob hoop reference must be set in scene or Prefab_Bob"
    assert "m_Name: TrainingArena" in content
    assert "m_Name: CourtFloor" in content
    assert "m_EditorClassIdentifier: Bob::HoopScoreZone" in content
    assert "m_EditorClassIdentifier: Bob::ArcAcademyManager" in content
    assert "m_EditorClassIdentifier: Bob::MovableHoop" in content
    assert "m_Name: BallSpawnPoint" in content
    assert "m_EditorClassIdentifier: Bob::BobShootingInput" in ml_agents_blob
    assert "m_EditorClassIdentifier: Bob::ArcAcademyScorePopup" in content
    assert "m_EditorClassIdentifier: Bob::HoopNetPhysics" in content
    assert "m_Name: SpawnPad" in content
    assert "m_Name: DistanceMarkings" in content
    assert "m_Name: TrainingBays" in content
    assert "m_Name: MountainWindow" in content
    assert "m_Name: SpawnPadBranding" in content
    assert "m_Name: ReflectionProbe_Window" in content
    assert "m_Name: FloorDecals" in content
    assert "m_Name: TrajectoryVisuals" in content
    assert "m_EditorClassIdentifier: Bob::ArcTrajectoryVisual" in content


def test_arc_academy_visual_scripts_exist(repo_root: Path) -> None:
    assert (repo_root / "Assets/Scripts/ArcTrajectoryVisual.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/ArcAcademyMaterialFactory.cs").is_file()
    assert (repo_root / "docs/design/arc-academy-reference.jpg").is_file()


def test_arc_academy_visual_builder_wiring(repo_root: Path) -> None:
    builder = (repo_root / EDITOR_SCRIPTS[1]).read_text()
    assert "CreateMountainWindow" in builder
    assert "CreateTrajectoryVisuals" in builder
    assert "ArcAcademyPortableHoopBuilder" in builder
    assert "CreateBayDivider" in builder
    assert "SpawnPadBranding" in builder
    assert "GetActiveBackboardGlass" in builder
    assert "HoopSwishVfx" in builder
    assert "CreateHdrpVolume" in builder
    assert "CreateAdaptiveProbeVolume" in builder
    assert "CreateReflectionProbes" in builder or "ReflectionProbe_Window" in builder
    assert "GetGlossyFloor" in builder
    assert "GetMetal" in builder
    assert "GetGlass" in builder
    assert "GetRubber" in builder
    assert "CreateFloorDecals" in builder
    assert "PrimitiveType.Cube" in builder
    assert "ArcAcademyMaterialFactory" in builder
    assert "GetMatteWall" in builder or "CreateHdrpLit" in builder
    assert "ArcTrajectoryVisual" in builder

    validator = (repo_root / EDITOR_SCRIPTS[0]).read_text()
    assert "MountainWindow" in validator
    assert "TrajectoryVisuals" in validator
    assert "SpawnPadBranding" in validator
    assert "DecorativeHoopMarker" in validator
    assert "RoboticLauncherVisual" in validator
    assert "PortableHoopStand" in validator
    assert "HoopSwishVfx" in validator
    assert "HdrpVolume" in validator
    assert "AdaptiveProbeVolume" in validator
    assert "ArcTrajectoryVisual" in validator


def test_arc_academy_layout_and_scripts_exist(repo_root: Path) -> None:
    layout = (repo_root / "Assets/Scripts/ArcAcademyLayout.cs").read_text()
    assert "TrainingBayCount = 8" in layout
    assert "BuildTrajectoryArcTargets" in layout
    assert "BallSpawnPointName" in layout
    assert "HoopRootDefaultPosition = new(0f, 0f, -5.5f)" in layout
    assert (repo_root / "Assets/Scripts/ArcAcademyManager.cs").is_file()
    assert (repo_root / "Assets/Scripts/MovableHoop.cs").is_file()
    assert (repo_root / "Assets/Scripts/HoopNetPhysics.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobShootingInput.cs").is_file()
    assert (repo_root / "Assets/Scripts/ArcAcademyScorePopup.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobTrainingStats.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobTrainingScoreboard.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobPhysicsLayers.cs").is_file()
    assert (repo_root / "Assets/Scripts/SpawnPadPulse.cs").is_file()
    assert (repo_root / "Assets/Scripts/CameraFacingBillboard.cs").is_file()
    assert (repo_root / "Assets/Scripts/DecorativeHoopMarker.cs").is_file()
    assert (repo_root / "Assets/Scripts/RoboticLauncherVisual.cs").is_file()
    assert (repo_root / "Assets/Scripts/HoopSwishVfx.cs").is_file()
    launcher = (repo_root / "Assets/Scripts/RoboticLauncherVisual.cs").read_text()
    assert "Update" in launcher
    assert "LauncherArm" in launcher
    assert (repo_root / "Assets/Scripts/Editor/ArcAcademyHdrpSetup.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/ArcAcademyShaderGraphSetup.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/ArcAcademyMaterialPaths.cs").is_file()
    factory = (
        repo_root / "Assets/Scripts/Editor/ArcAcademyMaterialFactory.cs"
    ).read_text()
    assert "GetActiveBackboardGlass" in factory


def test_arc_academy_builder_wiring(repo_root: Path) -> None:
    builder = (repo_root / EDITOR_SCRIPTS[1]).read_text()
    assert "ArcAcademyLayout" in builder
    assert "ArcAcademyManager" in builder
    assert "MovableHoop" in builder
    assert "WireReferences" in builder
    assert "BallSpawnPoint" in builder
    assert "HoopNetPhysics" in builder
    assert "BobShootingInput" in builder
    assert "BobEntranceController" in builder
    assert "ArcAcademyDemoCamera" in builder
    assert (
        "CreateBasketballMaterial"
        in (
            repo_root / "Assets/Scripts/Editor/ArcAcademyMaterialFactory.cs"
        ).read_text()
    )
    assert "ArcAcademyScorePopup" in builder
    assert "BobTrainingStats" in builder
    assert "BobTrainingScoreboard" in builder
    assert "BobPhysicsLayerSetup" in builder
    assert "ApplyTrainingPhysicsLayers" in builder
    assert "SpawnPadPulse" in builder
    assert "CameraFacingBillboard" in builder
    assert "ConfigureRevoluteJoint" in builder
    assert "DistanceMarkings" in builder
    assert "SpawnPad" in builder
    assert "TrainingBays" in builder
    assert "CreateTrainingBays" in builder

    validator = (repo_root / EDITOR_SCRIPTS[0]).read_text()
    assert "ArcAcademyManager" in validator
    assert "MovableHoop" in validator
    assert "HoopScoreZone" in validator
    assert "BallSpawnPoint" in validator
    assert "SpawnPad" in validator
    assert "DistanceMarkings" in validator
    assert "TrainingBays" in validator
    assert "BobEntranceController" in validator
    assert "ArcAcademyDemoCamera" in validator


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
    assert "NotifyEpisodeBegin" in agent
    assert (repo_root / "Assets/Scripts/HoopScoreZone.cs").is_file()


def test_capture_progress_script_wires_hdrp_setup(repo_root: Path) -> None:
    script = (repo_root / "scripts/capture-progress.sh").read_text()
    assert "ArcAcademyHdrpSetup.EnsureHdrpFromCli" in script
    assert "BobProgressCapture.CaptureFromCli" in script
    assert "BobProgressCapture.CapturePlayModeFromCli" in script
    assert "--play" in script


def test_progress_capture_play_mode_entry_point(repo_root: Path) -> None:
    capture = (repo_root / "Assets/Editor/BobProgressCapture.cs").read_text()
    assert "CapturePlayModeFromCli" in capture
    assert "PlayCaptureSession" in capture
    assert "BOB_CAPTURE_PLAY_FRAMES" in capture
    assert "SessionState" in capture


def test_bob_training_scoreboard_wiring(repo_root: Path) -> None:
    agent = (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    assert "BobTrainingStats.Instance" in agent
    assert "GiveReward" in agent
    assert "BeginIteration" in agent

    manager = (repo_root / "Assets/Scripts/ArcAcademyManager.cs").read_text()
    assert "RecordBasketballPoint" in manager

    tag_manager = (repo_root / "ProjectSettings/TagManager.asset").read_text()
    assert "Bob" in tag_manager
    assert "TrainingArena" in tag_manager
    assert "Decoration" in tag_manager


def test_visual_vision_doc_exists(repo_root: Path) -> None:
    vision = repo_root / "docs/design/visual-vision.md"
    assert vision.is_file()
    text = vision.read_text()
    assert "Arc Academy Lab" in text
    assert "ai-warehouse-lab-reference.png" in text
    assert "Phase 1" in text
    assert (repo_root / "docs/design/ai-warehouse-lab-reference.png").is_file()


def test_what_finished_looks_like_doc_exists(repo_root: Path) -> None:
    product = repo_root / "docs/what-finished-looks-like.md"
    assert product.is_file()
    text = product.read_text()
    assert "BobTrainingStats" in text
    assert "BobTrainingSuccessGraph" in text
    assert "TotalIterations" in text
    assert "BasketballPoints" in text
    assert "TotalRewards" in text
    assert "TotalPenalties" in text
    assert "SessionSuccessRate" in text


def test_success_graph_wiring(repo_root: Path) -> None:
    stats = (repo_root / "Assets/Scripts/BobTrainingStats.cs").read_text()
    assert "SessionSuccessRate" in stats
    assert "RollingSuccessRate" in stats
    assert "BeginIteration" in stats

    graph = (repo_root / "Assets/Scripts/BobTrainingSuccessGraph.cs").read_text()
    assert "BobTrainingStats" in graph

    builder = (
        repo_root / "Assets/Scripts/Editor/BobTrainingSceneBuilder.cs"
    ).read_text()
    assert "BobTrainingSuccessGraph" in builder

    validator = (repo_root / "Assets/Scripts/Editor/BobSceneValidator.cs").read_text()
    assert "BobTrainingSuccessGraph" in validator


def test_training_connection_monitor_wiring(repo_root: Path) -> None:
    monitor = (repo_root / "Assets/Scripts/BobTrainingConnectionMonitor.cs").read_text()
    assert "IsCommunicatorOn" in monitor
    assert "BOB_TRAINING_WARN" in monitor
    assert "trainingTimeScale" in monitor

    builder = (
        repo_root / "Assets/Scripts/Editor/BobTrainingSceneBuilder.cs"
    ).read_text()
    assert "BobTrainingConnectionMonitor" in builder

    scoreboard = (repo_root / "Assets/Scripts/BobTrainingScoreboard.cs").read_text()
    assert "BobTrainingConnectionMonitor.Instance" in scoreboard


def test_yaml_training_ops(repo_root: Path) -> None:
    import yaml

    config = yaml.safe_load((repo_root / "config/bob_free_throw.yaml").read_text())
    bob = config["behaviors"]["Bob"]
    assert bob["hyperparameters"]["beta_schedule"] == "linear"
    assert bob["hyperparameters"]["epsilon_schedule"] == "linear"
    assert bob["summary_freq"] == 5000
    assert config["engine_settings"]["time_scale"] == 20
    assert "train_model" not in config["checkpoint_settings"]


def test_hdrp_lab_volume_defaults(repo_root: Path) -> None:
    setup = (repo_root / "Assets/Scripts/Editor/ArcAcademyHdrpSetup.cs").read_text()
    preset = (repo_root / "Assets/Scripts/ArcAcademyLabRenderPreset.cs").read_text()
    assert "ApplyLabVolumePolish" in setup
    assert "ArcAcademyLabRenderPreset" in setup
    assert "ApplyMinimalTrainerVolume" in preset
    assert "EnforceSingleDirectionalShadow" in preset
    assert "FixedExposure = 10.0f" in (repo_root / "Assets/Scripts/ArcAcademyLabLightingValues.cs").read_text()
    assert "bloom.active = false" in preset


def test_simple_free_throw_minimal_trainer(repo_root: Path) -> None:
    setup = (repo_root / "Assets/Scripts/SimpleFreeThrowSetup.cs").read_text()
    agent = (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    validator = (repo_root / "Assets/Scripts/Editor/BobSceneValidator.cs").read_text()
    assert "SimpleFreeThrowSetup" in setup
    assert "ApplyMinimalTrainerVolumeInScene" in setup
    assert "ConfigureProjectileLauncher" in agent
    assert "projectileBody" in agent
    assert "VerifyMinimal" in validator
    assert (repo_root / "Assets/Scripts/SimpleBasketball.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/SimpleFreeThrowSetupEditor.cs").read_text().count("ApplyFromCli") >= 1


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
