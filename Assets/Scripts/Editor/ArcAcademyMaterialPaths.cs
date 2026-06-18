#if UNITY_EDITOR
/// <summary>
/// Asset paths for Arc Academy HDRP material library (Shader Graph + .mat presets).
/// </summary>
public static class ArcAcademyMaterialPaths
{
    public const string ShadersFolder = "Assets/Shaders/ArcAcademy";
    public const string MaterialsFolder = "Assets/Materials/HDRP";
    public const string TexturesFolder = "Assets/Textures/ArcAcademy";

    public const string GlossyFloorMat = MaterialsFolder + "/ArcAcademyGlossyFloor.mat";
    public const string MatteWallMat = MaterialsFolder + "/ArcAcademyMatteWall.mat";
    public const string MetalMat = MaterialsFolder + "/ArcAcademyMetal.mat";
    public const string GlassMat = MaterialsFolder + "/ArcAcademyGlass.mat";
    public const string RubberMat = MaterialsFolder + "/ArcAcademyRubber.mat";
    public const string MountainBackdropMat = MaterialsFolder + "/ArcAcademyMountainBackdrop.mat";

    public const string GlossyFloorGraph = ShadersFolder + "/ArcAcademyGlossyFloor.shadergraph";
    public const string MatteWallGraph = ShadersFolder + "/ArcAcademyMatteWall.shadergraph";
    public const string MetalGraph = ShadersFolder + "/ArcAcademyMetal.shadergraph";
    public const string GlassGraph = ShadersFolder + "/ArcAcademyGlass.shadergraph";
    public const string RubberGraph = ShadersFolder + "/ArcAcademyRubber.shadergraph";

    public const string MountainBackdropTexture = TexturesFolder + "/mountain_backdrop.png";
}
#endif
