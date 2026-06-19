using UnityEngine;

/// <summary>
/// Play-mode safety net: re-applies lab HDRP preset if the scene still has warehouse light stacks.
/// </summary>
[DefaultExecutionOrder(-200)]
public class ArcAcademyLabPlayFix : MonoBehaviour
{
    private void Awake()
    {
        // ArcAcademyManager also applies the lab preset; keep this as a fallback if manager is missing.
        ArcAcademyLabRenderPreset.ApplyAll();
    }
}
