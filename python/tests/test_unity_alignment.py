"""Guard alignment between Unity agent settings and trainer config."""

from __future__ import annotations

import json
import re
from pathlib import Path

# Expected ML-Agents settings for BobAgent.cs / BehaviorParameters in scene builder.
EXPECTED_BEHAVIOR_NAME = "Bob"
EXPECTED_BEHAVIOR_TYPE = "Default"
EXPECTED_BEHAVIOR_TYPE_ENUM = 0
EXPECTED_VECTOR_OBSERVATIONS = 13
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
    assert EXPECTED_VECTOR_OBSERVATIONS == 13
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

    builder = (
        repo_root / "Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs"
    ).read_text()
    assert "EnsureSpawnAndManager" in builder
    assert "WireBobToArena" in builder
    assert "HideLegacyCourtVisuals" in builder
    assert "TrainingBays" in builder
    assert "ApplyLabScenePreset" in builder
    assert "EnsureBobFace" in builder
    assert "EnsureBobVisual" in builder
    assert "BobVisualProfile" in builder
    assert "EnsureSingleBasketball" in builder
    assert "TrainingHoopDetail.UpgradeActiveHoop" in builder
    assert "BasketballProjectileSetup" in builder
    assert "BobWallHudBuilder.EnsureWallTrainingHud" in builder
    hud_idx = builder.index("BobWallHudBuilder.EnsureWallTrainingHud")
    save_idx = builder.index("SavePrefabFromInstance")
    assert hud_idx >= 0 and save_idx >= 0 and hud_idx < save_idx
    assert "EnsurePowerPathPulse" in builder
    assert "ApplyFromCli" in builder

    preset = (repo_root / "Assets/Scripts/ArcAcademyLabRenderPreset.cs").read_text()
    assert "ApplyLabViewPreset" in preset

    camera = (repo_root / "Assets/Scripts/ArcAcademyDemoCamera.cs").read_text()
    assert "ResetToLabHero" in camera
    assert "LabHero" in camera
    assert "ToggleLabTrainingCameras" in camera

    assert "LabCameraFieldOfView" in arena
    assert "LabBehindBobCameraPosition" in arena
    assert "GetHeroCameraPosition" in arena
    assert "11f, 3.5f, -2.2f" in arena
    assert (
        "PolishBobLabVisuals"
        in (
            repo_root / "Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs"
        ).read_text()
    )
    assert (
        "CreateArcLineMaterial"
        in (
            repo_root / "Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs"
        ).read_text()
    )
    assert (
        "0.32f, 0.32f, 0.1f"
        in (repo_root / "Assets/Scripts/BobFaceLayout.cs").read_text()
    )
    assert "LabHudWorldX" in arena
    assert "LabHudWorldZ" in arena
    assert "LabHudCanvasSize = new(800f, 850f)" in arena
    assert "NearBobHudCanvasSize = new(900f, 560f)" in arena
    assert "NearBobHudWorldPosition" in arena
    assert "WallSouthScale = new(22f, 4f, 1f)" in arena
    assert "WallSouthName" in arena
    assert "LabHudWallName = WallSouthName" in arena
    assert "ShowBudgetFlavorProps" in arena
    assert "BasketballPrefabPath" in arena

    assert (repo_root / "Assets/Scripts/BasketballProjectileSetup.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobShotArcPreview.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobWallTrainingHud.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobNearBobTrainingHud.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobAudioFeedback.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/BobAudioFeedbackBuilder.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/BobInferenceDemo.cs").is_file()
    assert (
        repo_root / "Assets/Tests/PlayMode/BobScoreIncrementPlayModeTest.cs"
    ).is_file()
    assert (repo_root / "Assets/Audio/sfx_score.wav").is_file()
    rewards = (repo_root / "Assets/Scripts/ArcAcademyRewards.cs").read_text()
    assert "MadeBasket = 8.0f" in rewards
    assert "BackboardSquareHit = 1.0f" in rewards
    assert "SwishBonus = 0f" in rewards
    assert "RimContactPenalty = 0f" in rewards
    assert (
        "NotifyBackboardSquareHit"
        in (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    )
    assert (
        "HoopTargetSquareHit"
        in (repo_root / "Assets/Scripts/HoopTargetSquareHit.cs").read_text()
    )
    assert (
        "EnsureTargetSquareHitZone"
        in (repo_root / "Assets/Scripts/TrainingHoopDetail.cs").read_text()
    )
    assert (
        "SquareHitMinApexRise"
        in (repo_root / "Assets/Scripts/ArcAcademyLayout.cs").read_text()
    )
    assert (
        "a make is a make" in rewards.lower()
        or "Any path through the hoop"
        in (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    )
    assert "NotifyRimContact" in (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    assert (
        "NotifyRimContact"
        in (repo_root / "Assets/Scripts/HoopRimContact.cs").read_text()
    )
    assert 'return "rim_out"' in (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    assert (
        "SwishSpeedThreshold = 18f"
        in (repo_root / "Assets/Scripts/ArcAcademyLayout.cs").read_text()
    )
    assert "PlayScore" in (repo_root / "Assets/Scripts/HoopScoreZone.cs").read_text()
    assert (
        "IsFallingThroughHoop"
        in (repo_root / "Assets/Scripts/HoopScoreZone.cs").read_text()
    )
    assert (
        "linearVelocity.y <= -minDownwardSpeed"
        in (repo_root / "Assets/Scripts/HoopScoreZone.cs").read_text()
    )
    assert (
        "BobAudioFeedbackBuilder.EnsureInScene"
        in (
            repo_root / "Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs"
        ).read_text()
    )
    assert (
        "Enable Inference Only"
        in (repo_root / "Assets/Scripts/Editor/BobInferenceDemo.cs").read_text()
    )
    assert (
        "RecordBasketballPoint"
        in (
            repo_root / "Assets/Tests/PlayMode/BobScoreIncrementPlayModeTest.cs"
        ).read_text()
    )
    assert (repo_root / "Assets/Scripts/BobScoreboardDisplay.cs").is_file()
    scoreboard_display = (
        repo_root / "Assets/Scripts/BobScoreboardDisplay.cs"
    ).read_text()
    assert "EpisodesLabel" in scoreboard_display
    assert "ApplyReadableTextStyle" in scoreboard_display
    assert "ApplyFloatHeroTextStyle" in scoreboard_display
    assert "ConfigureCanvasScaler" in scoreboard_display
    assert (repo_root / "Assets/Scripts/BobProceduralAnimator.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobFaceExpression.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobVisualProfile.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobVisualApplier.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobEyeFollow.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobFaceLayout.cs").is_file()
    face_layout = (repo_root / "Assets/Scripts/BobFaceLayout.cs").read_text()
    assert "LeftEyeLocalPosition" in face_layout
    assert "MouthSmileLocalPoints" in face_layout
    assert (repo_root / "Assets/Scripts/ArcAcademyPowerPathPulse.cs").is_file()
    assert (repo_root / "Assets/Scripts/ArcAcademyLabSceneCleanup.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/BobWallHudBuilder.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/BobNearBobHudBuilder.cs").is_file()
    wall_hud_layout = (repo_root / "Assets/Scripts/BobWallHudLayout.cs").read_text()
    assert "PlaceHudOnSouthWall" in wall_hud_layout
    assert "ApplyNearBobHudLayout" in wall_hud_layout
    assert "WallWestName" in wall_hud_layout
    assert "InverseParentScale" in wall_hud_layout
    assert "CompensateCanvasScale" in wall_hud_layout
    wall_hud_builder = (
        repo_root / "Assets/Scripts/Editor/BobWallHudBuilder.cs"
    ).read_text()
    assert "BobWallHudLayout.ApplyLabHudLayout" in wall_hud_builder
    assert "BobScoreboardDisplay.ConfigureCanvasScaler" in wall_hud_builder
    assert "GraphImage" in wall_hud_builder
    assert "Lab Console" in wall_hud_builder
    assert "ArcText" in wall_hud_builder
    assert "ApplyReadableTextStyle" in wall_hud_builder
    assert "CameraFacingBillboard" in wall_hud_builder
    near_bob_builder = (
        repo_root / "Assets/Scripts/Editor/BobNearBobHudBuilder.cs"
    ).read_text()
    assert "EnsureNearBobTrainingHud" in near_bob_builder
    assert "BobNearBobTrainingHud" in near_bob_builder
    assert "EpisodesText" in near_bob_builder
    assert (
        "FloatHeroFontSize" in near_bob_builder
        or "FloatHeroFontSize" in scoreboard_display
    )
    builder = (
        repo_root / "Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs"
    ).read_text()
    assert "BobNearBobHudBuilder.EnsureNearBobTrainingHud" in builder
    assert (repo_root / "Assets/Scripts/Editor/SimpleArenaTextureFactory.cs").is_file()

    builder = (
        repo_root / "Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs"
    ).read_text()
    assert "FindDeepChild(parent.transform, parts[1])" in builder
    assert "ArcAcademyLabSceneCleanup.HideLegacyClutter" in builder
    assert "EnsureBobEyeSphere" in builder
    assert "BobEyeFollow" in builder
    assert "LeftEye" in builder

    play_fix = (repo_root / "Assets/Scripts/ArcAcademyLabPlayFix.cs").read_text()
    assert "ArcAcademyLabSceneCleanup.EnsureLabCamera" in play_fix
    assert "BobWallHudLayout.ApplyActiveArenaLayout" in play_fix

    train_sh = (repo_root / "scripts/train.sh").read_text()
    assert "Bob/checkpoint.pt" in train_sh
    assert "--force" in train_sh

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
    assert (repo_root / "Assets/Scripts/BobShotActionLog.cs").is_file()
    assert (repo_root / "python/scripts/plot_training_progress.py").is_file()
    assert (repo_root / "python/scripts/review_training_run.py").is_file()

    agent_src = (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    assert "episodePeakArcQuality" in agent_src
    assert "BobShotActionLog.RecordLaunch" in agent_src
    assert "BobProceduralAnimator" in agent_src
    assert "ArcAcademyPowerPathPulse" in agent_src

    validator = (repo_root / "Assets/Scripts/Editor/BobSceneValidator.cs").read_text()
    assert "VerifySimpleArcAcademy" in validator
    assert "projectileBody must reference Basketball" in validator
    assert "Exactly one Basketball" in validator
    assert "BobWallTrainingHud" in validator
    assert "BobWallHudLayout" in validator
    assert "Wall_North must not contain LabTrainingHud" in validator
    assert "Wall_West must not contain LabTrainingHud" in validator
    assert "LabHudWorldZ" in validator
    assert "LabHudWorldX" in validator
    assert "Wall_South" in validator
    assert "CanvasScaler" in validator
    assert "headline text must use Outline" in validator
    assert "Prefab_SimpleArena must bake LabTrainingHud" in validator
    assert "CameraFacingBillboard for orbit readability" in validator
    assert "SpawnPadBranding must be inactive" in validator
    assert "IsLabCameraPosition" in validator
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
    # Note: "Bob::SimpleBasketball" lives in Prefab_Basketball.prefab; scene may reference via fileID/guid only.
    # Scene has "projectileBody" + "Basketball" (asserted above). Check prefab for script identifier.
    ball_prefab = (repo_root / "Assets/Prefabs/Prefab_Basketball.prefab").read_text()
    assert "Bob::SimpleBasketball" in ball_prefab or "Bob::BobShotArcPreview" in content
    assert "Bob::BobWallTrainingHud" in content or "LabTrainingHud" in content
    assert "HoopSuccess" in content or "m_TagString: HoopSuccess" in content


def test_bob_training_scene_yaml_alignment(repo_root: Path) -> None:
    """Offline mirror of BobSceneValidator checks on BobTraining.unity YAML."""
    scene_path = repo_root / SCENE_PATH
    assert scene_path.is_file(), f"Missing training scene: {SCENE_PATH}"
    content = scene_path.read_text()
    bob_prefab = (repo_root / "Assets/Prefabs/Prefab_Bob.prefab").read_text()
    ml_agents_blob = content + bob_prefab

    assert (
        "m_EditorClassIdentifier: Bob::BobAgent" in content
        or "Bob::BobAgent" in bob_prefab
    )
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
    is_simple_arc = "m_Name: SimpleArcAcademyArena" in content
    is_lab_showcase = is_simple_arc and "m_Name: TrainingBays" not in content
    if is_lab_showcase:
        assert "m_Name: Floor" in content
        assert (
            "Bob::SimpleArcArenaManager" in content
            or "SimpleArcAcademyArena" in content
        )
    else:
        assert "m_Name: CourtFloor" in content
    assert "m_EditorClassIdentifier: Bob::HoopScoreZone" in content
    assert "m_EditorClassIdentifier: Bob::ArcAcademyManager" in content
    assert "m_EditorClassIdentifier: Bob::MovableHoop" in content
    assert "m_Name: BallSpawnPoint" in content
    assert "m_EditorClassIdentifier: Bob::BobShootingInput" in ml_agents_blob
    assert "m_EditorClassIdentifier: Bob::ArcAcademyScorePopup" in content
    # Simple Arc Academy: visual net only; scoring uses HoopSuccess + HoopScoreZone (no HoopNetPhysics).
    if not is_simple_arc:
        assert "m_EditorClassIdentifier: Bob::HoopNetPhysics" in content
    assert "m_Name: SpawnPad" in content
    if not is_lab_showcase:
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
    assert (
        "GetRimOrange" in builder or "GetRimSilver" in builder or "GetRubber" in builder
    )
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
    assert "HoopSuccessName" in layout
    assert "RimScoreHeight" in layout
    assert "RimLocalOnHoopHead" in layout
    assert "StationaryHoopHeadLocalPosition" in layout
    assert "KeyHalfWidth = 2.44f" in layout
    assert "FreeThrowCircleRadius = 1.83f" in layout
    assert "HalfCourtLineWorldZ" in layout
    assert "KeyDepthFromBaseline = 5.79f" in layout
    markings = (
        repo_root / "Assets/Scripts/Editor/SimpleArcCourtMarkingsBuilder.cs"
    ).read_text()
    assert "ThreePointArc" in markings
    assert "CreateCourtLineMaterial" in markings
    assert "CreateKeyPaintMaterial" in markings
    assert "HalfCourtLine" in markings
    assert (repo_root / "Assets/Scripts/ArcAcademyManager.cs").is_file()
    assert (repo_root / "Assets/Scripts/MovableHoop.cs").is_file()
    assert (repo_root / "Assets/Scripts/HoopNetPhysics.cs").is_file()
    assert (repo_root / "Assets/Scripts/HoopVisualMaterials.cs").is_file()
    assert (repo_root / "Assets/Scripts/TrainingHoopDetail.cs").is_file()
    hoop_detail = (repo_root / "Assets/Scripts/TrainingHoopDetail.cs").read_text()
    assert "RimColliders" in hoop_detail
    assert "EnsureScoreTrigger" in hoop_detail
    assert "EnsureRimMaterial" in hoop_detail
    assert "HoopVisualMaterials" in hoop_detail
    assert "ConfigureRimColliders" in hoop_detail
    assert "FreezeStationaryAssembly" in hoop_detail
    movable = (repo_root / "Assets/Scripts/MovableHoop.cs").read_text()
    assert "SetStationaryForTraining" in movable
    assert "stationaryForTraining" in movable
    assert (repo_root / "Assets/Scripts/BobShootingInput.cs").is_file()
    assert (repo_root / "Assets/Scripts/ArcAcademyScorePopup.cs").is_file()
    assert (repo_root / "Assets/Scripts/BobTrainingStats.cs").is_file()
    scoreboard = (repo_root / "Assets/Scripts/BobTrainingScoreboard.cs").read_text()
    assert "BobScoreboardDisplay.EpisodesLabel" in scoreboard
    wall_hud = (repo_root / "Assets/Scripts/BobWallTrainingHud.cs").read_text()
    assert "Lab Console" in wall_hud
    assert "ArcText" in wall_hud
    near_bob_hud = (repo_root / "Assets/Scripts/BobNearBobTrainingHud.cs").read_text()
    assert "BobScoreboardDisplay.EpisodesLabel" in near_bob_hud
    assert (
        "FloatHeroFontSize" in near_bob_hud or "ApplyFloatHeroTextStyle" in near_bob_hud
    )
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
    assert "TrainingHoopDetail" in builder
    assert "physicsColliders: false" in builder
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
    assert (
        "HoopSuccess trigger missing on Rim" in validator
        or "HoopSuccess child missing" in validator
    )
    assert "CapsuleCollider" in validator
    material_factory = (
        repo_root / "Assets/Scripts/Editor/ArcAcademyMaterialFactory.cs"
    ).read_text()
    assert "GetRimOrange" in material_factory or "GetRimSilver" in material_factory
    assert "GetOpaqueNet" in material_factory or "GetTranslucentNet" in material_factory
    assert (
        "ArcAcademyRim.mat"
        in (repo_root / "Assets/Scripts/Editor/ArcAcademyMaterialPaths.cs").read_text()
    )
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
    assert "GetRimOrange" in builder or "GetRimSilver" in builder
    assert "GetOpaqueNet" in builder or "GetTranslucentNet" in builder
    assert "HoopSuccessName" in builder
    assert "CapsuleCollider" in builder
    assert (repo_root / "Assets/Scripts/BobCourtLayout.cs").is_file()


def test_bob_court_layout_in_agent(repo_root: Path) -> None:
    agent = (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    layout = (repo_root / "Assets/Scripts/ArcAcademyLayout.cs").read_text()
    assert "ArcAcademyLayout" in agent
    assert "RegisterMadeShot" in agent
    assert "CalculateArcQuality" in agent
    assert "ApplyLaunchDirectionRewards" in agent
    assert "LaunchRadicallyWrongFlatPenalty" in layout
    assert "ApplyFlightDirectionPenalties" in agent
    assert "NotifyEpisodeBegin" in agent
    assert "ResolveEpisodeAsMiss" in agent
    assert "MissProximityRewardScale" in layout
    assert "RimPlaneMissPenalty" in layout
    assert "ShotResolveMaxSteps" in layout
    assert "PerStepDistancePenaltyScale" in layout
    assert "ArcQualityRewardScale = 0f" in layout
    assert "MissProximityRewardScale = 0f" in layout
    assert "RimPlaneMissPenalty = 2.5f" in layout
    assert "LaunchTowardHoopRewardScale = 0.08f" in layout
    assert "LaunchUpwardRewardScale = 0.06f" in layout
    assert "LaunchArcAlignRewardScale = 0.05f" in layout
    assert "IdealLaunchFy = 4.9f" in layout
    assert "IdealSolverMatchRewardScale = 0.35f" in layout
    assert "LaunchPowerBandPenaltyScale = 0.04f" in layout
    assert "ResidualLateralScale = 1.0f" in layout
    assert "ResidualVerticalScale = 1.25f" in layout
    assert "ResidualForwardScale = 1.0f" in layout
    assert "ResidualMaxMagnitude = 1.0f" in layout
    assert (
        "BobCurriculum" in (repo_root / "Assets/Scripts/BobCurriculum.cs").read_text()
    )
    assert "GetCurriculumHoopDeltaZ" in layout
    assert "SquareHitMinApexRise = 1.1f" in layout
    assert "LaunchPowerBandPenaltyScale" in agent
    assert "IdealSolverMatchRewardScale" in agent
    assert "TrainingRangeScale" not in layout
    assert "TrainingImpulseRangeScale" not in agent
    assert "idealImpulse + residualWorld" in agent
    assert "ResidualMaxMagnitude" in agent
    assert "transform.rotation * localImpulse" in agent  # solver-fallback path
    assert "forwardBias = 6f" in agent
    assert "forwardBias = -6f" not in agent
    # Heuristic uses analytic high-arc swish solver (not a flat make-island push).
    assert "BobSwishLaunchSolver" in agent
    assert "TryGetIdealWorldImpulse" in agent
    assert "LeftControl" in agent
    assert "IdealFreeThrowKinematics" in agent
    assert (
        "PreferredLaunchAngleDegrees = 56f"
        in (repo_root / "Assets/Scripts/BobSwishLaunchSolver.cs").read_text()
    )
    assert (
        "TryComputeWorldImpulse"
        in (repo_root / "Assets/Scripts/BobSwishLaunchSolver.cs").read_text()
    )
    assert (
        "WorldResidualToActions"
        in (repo_root / "Assets/Scripts/BobSwishLaunchSolver.cs").read_text()
    )
    assert (
        "AimAboveRimMeters = 0.18f"
        in (repo_root / "Assets/Scripts/BobSwishLaunchSolver.cs").read_text()
    )
    assert (
        "AimPastRimMeters = 0.05f"
        in (repo_root / "Assets/Scripts/BobSwishLaunchSolver.cs").read_text()
    )
    assert (
        "DampingCompensation = 1.08f"
        in (repo_root / "Assets/Scripts/BobSwishLaunchSolver.cs").read_text()
    )
    assert (
        "LaunchAngleDegreesFromImpulse"
        in (repo_root / "Assets/Scripts/BobSwishLaunchSolver.cs").read_text()
    )
    assert "pureExpert" in agent
    assert "GetEffectiveLaunchAngleDegrees" in agent
    assert "IsDemonstrationRecording()" in agent
    assert (
        "EstimateFlightDuration"
        in (repo_root / "Assets/Scripts/BobShotArcPreview.cs").read_text()
    )
    assert (
        "IdealFreeThrowArc"
        in (repo_root / "Assets/Scripts/BobShotArcPreview.cs").read_text()
    )
    assert (
        "launch_angle_deg"
        in (repo_root / "Assets/Scripts/BobShotActionLog.cs").read_text()
    )
    assert "MakeIslandUp" not in agent
    assert "MakeIslandForward" not in agent
    assert "IsHeuristicDemoMode" in agent
    assert "IsHeuristicShootHeld" in agent
    assert "IsDemonstrationRecording" in agent
    assert "applyRimPlaneMissPenalty" in agent
    # Tier 1.6+: proximity gated off for rim-plane misses; unified past-plane helpers
    assert "!applyRimPlaneMissPenalty" in agent
    assert "IsPastRimPlane" in agent
    assert "rimHeight + 1.2f" not in agent
    # rim_miss only via penalty flag — not ResolveMissReason dual path
    assert 'return "rim_miss"' not in agent or agent.count('? "rim_miss"') >= 1
    assert 'return "rim_miss";' not in agent
    assert (repo_root / "Assets/Scripts/HoopScoreZone.cs").is_file()

    bob_prefab = (repo_root / "Assets/Prefabs/Prefab_Bob.prefab").read_text()
    arena_prefab = (repo_root / "Assets/Prefabs/Prefab_SimpleArena.prefab").read_text()
    scene = (repo_root / SCENE_PATH).read_text()
    for label, blob in (
        ("Prefab_Bob", bob_prefab),
        ("Prefab_SimpleArena", arena_prefab),
        ("BobTraining.unity", scene),
    ):
        assert (
            f"VectorObservationSize: {EXPECTED_VECTOR_OBSERVATIONS}" in blob
        ), f"{label} must serialize VectorObservationSize: {EXPECTED_VECTOR_OBSERVATIONS}"
        assert (
            "forwardBias: 6" in blob
        ), f"{label} must serialize forwardBias: 6 (local +Z)"
        assert (
            "forwardBias: -6" not in blob
        ), f"{label} must not keep world-era forwardBias: -6"


def test_plot_learning_dashboard_script_exists(repo_root: Path) -> None:
    script = repo_root / "python/scripts/plot_learning_dashboard.py"
    assert script.is_file()
    text = script.read_text()
    assert "episode_net_rl" in text or "end_reason" in text
    assert "evaluate_tier15_pass" in text or "--check-pass" in text
    assert "positive" in text.lower() or "economics" in text.lower()


def test_bc_config_and_demo_recorder_menu(repo_root: Path) -> None:
    bc = (repo_root / "config/bob_free_throw_bc.yaml").read_text()
    assert "behavioral_cloning:" in bc
    # Recorder DemonstrationName "bob_free_throw" → bobfreethrow.demo on disk
    assert "Assets/Demos/bobfreethrow.demo" in bc
    assert "strength: 0.5" in bc

    probe_bc = (repo_root / "config/bob_free_throw_probe_4k_bc.yaml").read_text()
    assert "behavioral_cloning:" in probe_bc
    assert "Assets/Demos/bobfreethrow.demo" in probe_bc
    assert "max_steps: 380000" in probe_bc

    menu = (
        repo_root / "Assets/Scripts/Editor/BobDemonstrationRecorderMenu.cs"
    ).read_text()
    assert "DemonstrationRecorder" in menu
    assert "Enable Demonstration Recorder" in menu

    assert (repo_root / "Assets/Demos/.gitkeep").is_file()
    assert (repo_root / "Assets/Demos/bobfreethrow.demo").is_file()
    assert (repo_root / "scripts/tensorboard.sh").is_file()


def test_release_checklist_script_exists(repo_root: Path) -> None:
    script = repo_root / "scripts/release-checklist.sh"
    assert script.is_file()
    text = script.read_text()
    assert "validate-scene.sh" in text
    assert "test_unity_alignment.py" in text
    assert "RELEASE_CHECKLIST_OK" in text


def test_build_standalone_script_exists(repo_root: Path) -> None:
    script = repo_root / "scripts/build-standalone.sh"
    assert script.is_file()
    assert "BobBuildCli.BuildStandaloneMacFromCli" in script.read_text()


def test_ai_review_ollama_config_exists(repo_root: Path) -> None:
    assert (repo_root / ".ai-review.yaml").is_file()
    assert (repo_root / ".ai-review-models.txt").is_file()
    cfg = (repo_root / ".ai-review.yaml").read_text()
    assert "OLLAMA" in cfg
    assert "qwen2.5-coder:7b" in cfg
    models = (repo_root / ".ai-review-models.txt").read_text()
    assert "qwen2.5-coder:7b" in models
    workflow = (repo_root / ".github/workflows/ai-review.yml").read_text()
    assert "actions/cache@v4" in workflow
    assert "ollama-cache" in workflow or "Cache Ollama" in workflow
    assert (repo_root / "scripts/setup-ai-review-ollama.sh").is_file()
    assert (repo_root / "prompts/ai-review/bob-summary.md").is_file()


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
    assert "PreparePlayCaptureView" in capture
    assert "BOB_CAPTURE_PLAY_FRAMES" in capture
    assert "SessionState" in capture


def test_bob_training_session_runner_batchmode_entry(repo_root: Path) -> None:
    runner = (repo_root / "Assets/Editor/BobTrainingSessionRunner.cs").read_text()
    assert "RunFromCli" in runner
    assert "BOB_TRAIN_SESSION_SECONDS" in runner
    assert "BOB_TRAIN_SESSION_DONE" in runner


def test_deploy_portfolio_script_exists(repo_root: Path) -> None:
    script = repo_root / "scripts/deploy-portfolio.sh"
    assert script.is_file()
    text = script.read_text()
    assert "AICO" in text
    assert "aws s3 sync" in text


def test_bob_training_scoreboard_wiring(repo_root: Path) -> None:
    agent = (repo_root / "Assets/Scripts/BobAgent.cs").read_text()
    assert "BobTrainingStats.Instance" in agent
    assert "GiveReward" in agent
    assert "BeginIteration" in agent
    assert "shotImpulseThisEpisode" in agent
    assert "if (!shotImpulseThisEpisode)" in agent

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
    assert "StatsRecorder" in stats
    assert "Environment/TowardHoop" in stats
    assert "GetRecentEndReasons" in stats

    hud = (repo_root / "Assets/Scripts/BobTrainingHUD.cs").read_text()
    assert "UnityEngine.UI" in hud
    assert "BobTrainingStats" in hud
    assert "GetRecentEndReasons" in hud
    assert "TextMeshProUGUI" not in hud

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
    assert "BOB_TRAINING_LOST" in monitor
    assert "trainingTimeScale" in monitor
    assert (repo_root / "Assets/Scripts/BobTrainingSessionFlags.cs").is_file()
    assert (repo_root / "Assets/Scripts/Editor/BobTrainingPlayModeGuard.cs").is_file()
    guard = (
        repo_root / "Assets/Scripts/Editor/BobTrainingPlayModeGuard.cs"
    ).read_text()
    assert "BOB_TRAINING_COMPILE_DURING_PLAY" in guard
    assert "BOB_TRAINING_END" in guard

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
    assert (
        "FixedExposure = 10.0f"
        in (repo_root / "Assets/Scripts/ArcAcademyLabLightingValues.cs").read_text()
    )
    assert (
        "LabBloomIntensity"
        in (repo_root / "Assets/Scripts/ArcAcademyLabLightingValues.cs").read_text()
    )
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
    assert (
        repo_root / "Assets/Scripts/Editor/SimpleFreeThrowSetupEditor.cs"
    ).read_text().count("ApplyFromCli") >= 1


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


def test_scene_builder_lab_showcase_default(repo_root: Path) -> None:
    """Portfolio rebuild uses LabShowcase and delegates lab visuals to Simple Arc."""
    layout = (repo_root / "Assets/Scripts/ArcAcademyLayout.cs").read_text()
    assert "enum VisualMode" in layout
    assert "LabShowcase" in layout
    assert "CurrentMode = VisualMode.LabShowcase" in layout

    builder = (repo_root / EDITOR_SCRIPTS[1]).read_text()
    assert "VisualMode.LabShowcase" in builder
    assert "SimpleArcAcademyArenaBuilder.ApplyAll()" in builder
    assert "CreateMinimalSpawnAnchor" in builder
    assert "Rebuild Arc Academy (Warehouse Legacy)" in builder
    assert "CreateWarehouseShell" in builder

    simple_arc = (
        repo_root / "Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs"
    ).read_text()
    assert "public static void ApplyAll()" in simple_arc
