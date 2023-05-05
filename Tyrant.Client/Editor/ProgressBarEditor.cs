using UnityEngine;
using UnityEditor;
using Tyrant.ComponentsForUnity.UnityUIExtention;

[CustomEditor(typeof(ProgressBar))]
public class ProgressBarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        var progressBar = target as ProgressBar;
        EditorGUILayout.Slider(serializedObject.FindProperty("value"), progressBar.MinValue, progressBar.MaxValue);
        serializedObject.ApplyModifiedProperties();
    }
}