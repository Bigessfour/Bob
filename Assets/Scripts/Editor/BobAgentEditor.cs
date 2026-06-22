#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BobAgent))]
public class BobAgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "Bob is the ML-Agents player — force tuning and Behavior Parameters live here.\n\n" +
            "HDRP / scene tools are NOT on this object. Either:\n" +
            "• Top menu bar → Bob → HDRP → Fix White Blowout (In-Place)\n" +
            "• Hierarchy → TrainingArena → Inspector → Scene & HDRP Tools",
            MessageType.Info);

        DrawDefaultInspector();
    }
}
#endif
