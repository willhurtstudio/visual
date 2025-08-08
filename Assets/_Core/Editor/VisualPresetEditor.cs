#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VisualPresetSO))]
public class VisualPresetEditor : Editor {
    public override void OnInspectorGUI() {
        var p = (VisualPresetSO)target;
        EditorGUILayout.LabelField("Visual Preset", EditorStyles.boldLabel);

        p.affectsBackground = EditorGUILayout.Toggle("Affects Background", p.affectsBackground);
        if (p.affectsBackground)
            p.backgroundColor = EditorGUILayout.ColorField("Background Color", p.backgroundColor);

        EditorGUILayout.Space();
        p.affectsTransform = EditorGUILayout.Toggle("Affects Transform", p.affectsTransform);
        if (p.affectsTransform) {
            p.targetScale = EditorGUILayout.Vector3Field("Target Scale", p.targetScale);
            p.spinSpeed = EditorGUILayout.FloatField("Spin Speed", p.spinSpeed);
        }

        if (GUI.changed) EditorUtility.SetDirty(p);
    }
}
#endif
