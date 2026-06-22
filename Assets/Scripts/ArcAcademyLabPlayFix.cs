using UnityEngine;

/// <summary>
/// Play-mode safety net: re-applies lab HDRP preset if the scene still has warehouse light stacks.
/// </summary>
[DefaultExecutionOrder(-200)]
public class ArcAcademyLabPlayFix : MonoBehaviour
{
    private void Awake()
    {
        if (SimpleArcAcademyArena.IsLabViewActive)
        {
            ArcAcademyLabRenderPreset.ApplyLabViewPreset();
            ArcAcademyLabSceneCleanup.HideLegacyClutter();
            TrainingHoopDetail.UpgradeActiveHoop();
        }
        else
        {
            ArcAcademyLabRenderPreset.ApplyAll();
        }
    }

    private void Start()
    {
        if (SimpleArcAcademyArena.IsLabViewActive)
        {
            ArcAcademyLabSceneCleanup.EnsureLabCamera();
        }
    }
}
