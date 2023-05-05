using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tyrant.ComponentsForUnity.DataBinding;
using Tyrant.GameCore.DataBinding;

namespace Tyrant.ComponentsEditorForUnity
{
    //[CustomEditor(typeof(ItemsContainerForUnity), true)]
    //public class ItemsContainerForUnityEditor : Editor
    //{
    //    public override void OnInspectorGUI()
    //    {
    //        base.OnInspectorGUI();

    //        serializedObject.Update();
    //        GUILayout.Toggle(true, "<b><size=11>Converter</size></b>", "dragtab", GUILayout.MinWidth(20f));
    //        EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
    //        var converterTypeAssemblyQualifiedName = serializedObject.FindProperty("itemTemplateSelectorTypeAssemblyQualifiedName").stringValue;
    //        var btnText = "None";
    //        if (!string.IsNullOrEmpty(converterTypeAssemblyQualifiedName))
    //        {
    //            var converterType = Type.GetType(converterTypeAssemblyQualifiedName, false);
    //            btnText = converterType == null ? "Missing" : converterTypeAssemblyQualifiedName;
    //        }
    //        if (GUILayout.Button(btnText, EditorStyles.popup))
    //        {
    //            GenericMenu menu = new GenericMenu();
    //            menu.AddItem(new GUIContent("None"), false, OnConverterMenuClicked, null);
    //            foreach (var assembly in DataBinding.GetAssemblies())
    //            {
    //                try
    //                {
    //                    foreach (var type in assembly.GetTypes())
    //                    {
    //                        if (!type.IsClass)
    //                            continue;
    //                        if (!type.IsPublic)
    //                            continue;
    //                        var interfaceTypes = type.FindInterfaces((m, obj) => { return m == typeof(IItemTemplateSelector); }, null);
    //                        if (interfaceTypes == null || interfaceTypes.Length <= 0)
    //                            continue;
    //                        try
    //                        {
    //                            Activator.CreateInstance(type);
    //                        }
    //                        catch
    //                        {
    //                            continue;
    //                        }
    //                        menu.AddItem(new GUIContent(type.FullName), false, OnConverterMenuClicked, type);
    //                    }
    //                }
    //                catch (Exception e)
    //                {
    //                    Debug.LogException(e);
    //                }
    //            }
    //            menu.ShowAsContext();
    //        }
    //        EditorGUILayout.EndHorizontal();
    //        serializedObject.ApplyModifiedProperties();
    //    }

    //    private void OnConverterMenuClicked(object t)
    //    {
    //        serializedObject.Update();
    //        serializedObject.FindProperty("itemTemplateSelectorTypeAssemblyQualifiedName").stringValue = (t == null ? "" : (t as Type).AssemblyQualifiedName);
    //        serializedObject.ApplyModifiedProperties();
    //    }
    //}
}
