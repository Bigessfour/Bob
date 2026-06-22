#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ArcAcademyManager))]
public class ArcAcademyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Scene & HDRP Tools", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "For minimal training court: Bob → Setup → Simple Free Throw Trainer.\n" +
            "Fix White Blowout is for the full warehouse scene only.",
            MessageType.Info);

        if (GUILayout.Button("Fix White Blowout (In-Place)", GUILayout.Height(28f)))
        {
            ArcAcademyLabSceneFix.FixWhiteBlowoutInPlace();
        }

        if (GUILayout.Button("Apply Lab Lighting", GUILayout.Height(24f)))
        {
            ArcAcademyHdrpMaterialMaintenance.ApplyLabLightingMenu();
        }

        if (GUILayout.Button("Rebuild Arc Academy (HDRP)", GUILayout.Height(24f)))
        {
            if (EditorUtility.DisplayDialog(
                    "Rebuild scene?",
                    "Rebuilds BobTraining with lab lighting and materials. Continue?",
                    "Rebuild",
                    "Cancel"))
            {
                BobTrainingSceneBuilder.CreateTrainingSceneMenu();
            }
        }
    }
}
#endif
