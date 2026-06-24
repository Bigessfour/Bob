using UnityEngine;

/// <summary>
/// Shared local positions and scales for Bob's sphere eyes and smile on the -Z face (toward hoop).
/// </summary>
public static class BobFaceLayout
{
    public const string LeftEyeName = "LeftEye";
    public const string RightEyeName = "RightEye";
    public const string MouthName = "HappyMouth";
    public const string ScleraName = "Sclera";
    public const string PupilName = "Pupil";

    public static readonly Vector3 LeftEyeLocalPosition = new(-0.18f, 0.1f, -0.505f);
    public static readonly Vector3 RightEyeLocalPosition = new(0.18f, 0.1f, -0.505f);
    public static readonly Vector3 MouthLocalPosition = new(0f, -0.1f, -0.505f);

    /// <summary>~26% of face width — readable from sideline camera (AI Warehouse scale).</summary>
    public static readonly Vector3 ScleraLocalScale = new(0.26f, 0.26f, 0.08f);
    public static readonly Vector3 PupilLocalScale = new(0.1f, 0.1f, 0.05f);
    public static readonly Vector3 PupilLocalOffset = new(0f, 0f, -0.04f);

    public const float DefaultExpressionScale = 1f;
    public const float FocusExpressionScale = 0.65f;
    public const float HappyExpressionScale = 1.1f;
    public const float SurprisedExpressionScale = 1.35f;

    public static readonly Color ScleraColor = Color.white;
    public static readonly Color PupilColor = new(0.08f, 0.08f, 0.1f);
    public static readonly Color MouthColor = new(0.35f, 0.16f, 0.05f);
    public const float MouthLineWidth = 0.028f;

    public static readonly Vector3[] MouthSmileLocalPoints =
    {
        new(-0.14f, 0.02f, 0f),
        new(-0.08f, -0.04f, 0f),
        new(0f, -0.06f, 0f),
        new(0.08f, -0.04f, 0f),
        new(0.14f, 0.02f, 0f),
    };
}
