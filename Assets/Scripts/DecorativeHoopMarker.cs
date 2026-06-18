using UnityEngine;

/// <summary>
/// Marks a hoop assembly as decorative (no scoring). The single active hoop uses MovableHoop + HoopScoreZone instead.
/// </summary>
public class DecorativeHoopMarker : MonoBehaviour
{
    [SerializeField] private bool isScoring;

    public bool IsScoring => isScoring;
}
