#if UNITY_EDITOR
/// <summary>
/// Editor alias for <see cref="ArcAcademyLabLightingValues"/> (scene builder).
/// </summary>
public static class ArcAcademyLabLighting
{
    public const float SunLux = ArcAcademyLabLightingValues.SunLux;
    public const float FillDirectionalLux = ArcAcademyLabLightingValues.FillDirectionalLux;
    public const float CeilingStripLumen = ArcAcademyLabLightingValues.CeilingStripLumen;
    public const float WindowFillLumen = ArcAcademyLabLightingValues.WindowFillLumen;
    public const float CenterPointLumen = ArcAcademyLabLightingValues.CenterPointLumen;
    public const float BobSpotLumen = ArcAcademyLabLightingValues.BobSpotLumen;
    public const float SpawnPadPointLumen = ArcAcademyLabLightingValues.SpawnPadPointLumen;
    public const int MinCeilingStripLights = ArcAcademyLabLightingValues.MaxActiveCeilingStrips;
}
#endif
