using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

[CustomPropertyDrawer(typeof(SceneSelector), true)]
public class SceneSelectorPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var scenePathProperty = property.FindPropertyRelative("scenePath");
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProperty.stringValue);
        var newSceneAsset = EditorGUI.ObjectField(position, new GUIContent(label.text, string.IsNullOrWhiteSpace(label.tooltip) ? "drag a scene asset from project browser to here" : label.tooltip), sceneAsset, typeof(SceneAsset), false);
        if (newSceneAsset != sceneAsset)
            scenePathProperty.stringValue = AssetDatabase.GetAssetPath(newSceneAsset);
    }
}