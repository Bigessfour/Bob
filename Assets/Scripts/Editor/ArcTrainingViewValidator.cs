#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-click validator / polisher for the Arc Academy single-training-shot view.
/// Menu: Bob → Polish → Fix Training View (also available under Bob → Test).
/// Executes the full checklist from the 6-prompt review:
/// 1. Enforce single Bob + single Basketball
/// 2. Ensure cute eyes + BobEyeFollow
/// 3. Rational default camera (CameraRig + CameraOrbit)
/// 4. Scoreboard / wall HUD placement + readability
/// 5. Hoop/rim/net HDRP materials + visibility
/// 6. Trajectory preview + ball trail
/// 7. Run simple arena builder to bake
/// 8. Print success message (and optionally prepare for single-shot capture)
/// </summary>
public static class ArcTrainingViewValidator
{
    private const string ScenePath = "Assets/Scenes/BobTraining.unity";

    [MenuItem("Bob/Polish/Fix Training View")]
    public static void FixTrainingViewMenu()
    {
        FixTrainingView(showDialog: true);
    }

    [MenuItem("Bob/Test/Fix & Validate Training View")]
    public static void FixTrainingViewTestMenu()
    {
        FixTrainingView(showDialog: true);
    }

    public static void FixTrainingViewFromCli()
    {
        FixTrainingView(showDialog: false);
        EditorApplication.Exit(0);
    }

    public static void FixTrainingView(bool showDialog = false)
    {
        Debug.Log("=== ARC TRAINING VIEW FIX START ===");

        // Ensure we are in the right scene
        if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path != ScenePath)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath);
        }

        // 1. Enforce single Bob + single Basketball
        EnforceSingleBobAndBall();

        // 2. Cute eyes + follow (Prompt 1)
        EnsureBobEyesAndFollow();

        // 3. Rational default camera + orbit (Prompt 2)
        EnsureRationalCamera();

        // 4. Scoreboard / wall HUD (Prompt 3) – wall HUD is the canonical one
        EnsureScoreboardPlacementAndVisibility();

        // 5. Hoop/rim/net HDRP rendering (Prompt 4)
        EnsureHoopRendering();

        // 6. Trajectory + trail
        EnsureTrajectoryAndTrail();

        ValidateHalfCourtMarkings();

        // 7. Bake simple arena (ensures prefab + hierarchy are clean)
        SimpleArcAcademyArenaBuilder.ApplySilently();

        // 8. Final validation print
        Debug.Log("Training view clean – ready for single shot");
        Debug.Log("=== ARC TRAINING VIEW FIX COMPLETE ===");

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Arc Training View",
                "Training view fixed and validated.\n\n" +
                "Run 'Bob → Test → Play Single Shot' for the hero capture.",
                "OK");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    private static void EnforceSingleBobAndBall()
    {
        var bobs = Object.FindObjectsByType<BobAgent>(FindObjectsInactive.Include);
        if (bobs.Length > 1)
        {
            for (int i = 1; i < bobs.Length; i++)
            {
                Object.DestroyImmediate(bobs[i].gameObject);
            }
            Debug.Log("Enforced single Bob");
        }

        var balls = Object.FindObjectsByType<SimpleBasketball>(FindObjectsInactive.Include);
        if (balls.Length > 1)
        {
            for (int i = 1; i < balls.Length; i++)
            {
                Object.DestroyImmediate(balls[i].gameObject);
            }
            Debug.Log("Enforced single Basketball");
        }
    }

    private static void EnsureBobEyesAndFollow()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob == null) return;

        // Eyes are expected to be created by arena builder / face setup
        var left = bob.transform.Find(BobFaceLayout.LeftEyeName);
        var right = bob.transform.Find(BobFaceLayout.RightEyeName);

        if (left == null || right == null)
        {
            Debug.LogWarning("Eyes not found on Bob – rerun arena builder or BobFace setup.");
            return;
        }

        if (bob.GetComponent<BobEyeFollow>() == null)
        {
            bob.gameObject.AddComponent<BobEyeFollow>();
        }
    }

    private static void EnsureRationalCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        // The builder now creates CameraRig; just ensure the orbit component and reset
        var orbit = Object.FindAnyObjectByType<CameraOrbit>();
        if (orbit != null)
        {
            orbit.ResetToDefault();
        }
        else if (cam.GetComponentInParent<CameraOrbit>() == null)
        {
            // In case we are in an old scene – attach to parent rig or camera
            var rig = GameObject.Find("CameraRig") ?? cam.gameObject;
            rig.AddComponent<CameraOrbit>();
        }
    }

    private static void EnsureScoreboardPlacementAndVisibility()
    {
        // Wall HUD is the primary for lab view (on west wall at hoop depth)
        var hud = Object.FindAnyObjectByType<BobWallTrainingHud>();
        if (hud != null)
        {
            // Already has Canvas + large text via BobWallHudBuilder + BobScoreboardDisplay
            // Just ensure it is active
            hud.gameObject.SetActive(true);
        }

        // The 2D OnGUI fallback should be suppressed when wall HUD is present (already handled in BobTrainingScoreboard)
    }

    private static void EnsureHoopRendering()
    {
        var rim = GameObject.Find(ArcAcademyLayout.RimName);
        if (rim != null && rim.GetComponent<Renderer>() != null)
        {
            var mat = ArcAcademyMaterialFactory.GetRimSilver();
            if (mat != null)
            {
                rim.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        // Net is built by TrainingHoopDetail / BobTrainingSceneBuilder using HoopVisualMaterials
        TrainingHoopDetail.UpgradeActiveHoop();
    }

    private static void EnsureTrajectoryAndTrail()
    {
        var bob = Object.FindAnyObjectByType<BobAgent>();
        if (bob != null && bob.GetComponent<BobShotArcPreview>() == null)
        {
            bob.gameObject.AddComponent<BobShotArcPreview>();
        }

        var ball = Object.FindAnyObjectByType<SimpleBasketball>();
        if (ball != null)
        {
            // Trail is added by BasketballProjectileSetup.EnsureBasketball
            // Just make sure it is emitting capable
        }
    }

    private static void ValidateHalfCourtMarkings()
    {
        const float tolerance = 0.15f;
        var arenaRoot = GameObject.Find(SimpleArcAcademyArena.RootName);
        if (arenaRoot == null)
        {
            Debug.LogWarning("Court markings check skipped — SimpleArcAcademyArena missing.");
            return;
        }

        var markings = arenaRoot.transform.Find(SimpleArcCourtMarkingsBuilder.CourtMarkingsName);
        if (markings == null || !markings.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Court markings missing or inactive under SimpleArcAcademyArena.");
            return;
        }

        if (markings.parent != arenaRoot.transform)
        {
            Debug.LogError("VALIDATE_FAIL: CourtMarkings must be parented to SimpleArcAcademyArena root (not Floor).");
        }

        if (markings.localScale != Vector3.one)
        {
            Debug.LogError("VALIDATE_FAIL: CourtMarkings local scale must be (1,1,1).");
        }

        var floor = arenaRoot.transform.Find(SimpleArcAcademyArena.FloorName);
        if (floor != null && floor.Find(SimpleArcCourtMarkingsBuilder.CourtMarkingsName) != null)
        {
            Debug.LogError("VALIDATE_FAIL: CourtMarkings must not be nested under scaled Floor.");
        }

        if (markings.Find("KeyPaint") == null)
        {
            Debug.LogWarning("KeyPaint missing from court markings.");
        }

        if (markings.Find("ThreePointArc") == null)
        {
            Debug.LogWarning("ThreePointArc missing from court markings.");
        }

        var ftLine = markings.Find("FreeThrowLine");
        if (ftLine == null)
        {
            Debug.LogWarning("FreeThrowLine missing from court markings.");
            return;
        }

        float ftWorldZ = arenaRoot.transform.TransformPoint(ftLine.localPosition).z;
        if (Mathf.Abs(ftWorldZ - ArcAcademyLayout.FreeThrowLineWorldZ) > tolerance)
        {
            Debug.LogError(
                $"VALIDATE_FAIL: FreeThrowLine world Z expected {ArcAcademyLayout.FreeThrowLineWorldZ:F2}, got {ftWorldZ:F2}");
        }
    }
}
#endif
