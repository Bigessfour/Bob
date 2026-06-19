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
            "These buttons do the same work as the top menu: Bob → HDRP → …\n" +
            "Use the Game tab (not Scene view) to judge lighting after fixing.",
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
