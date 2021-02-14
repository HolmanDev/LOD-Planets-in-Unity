using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Presets))]
public class PresetsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Presets presets = (Presets)target;

        if (GUILayout.Button("Generate Quad Template"))
        {
            presets.GenerateQuadTemplate(Presets.quadRes);
        }

        if (GUILayout.Button("Clear Quad Template"))
        {
            presets.ClearQuadTemplate();
        }
    }
}
