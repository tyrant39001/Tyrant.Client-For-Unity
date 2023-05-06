using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;
using UnityEditor.UI;
using UnityEngine.EventSystems;
using Tyrant.ComponentsForUnity.UnityUIExtention;
using System.Linq;

namespace Tyrant.ComponentsEditorForUnity
{
    public static class UnityEditorHelper
    {
        /// <summary>
        /// 绘制指定的属性
        /// </summary>
        /// <param name="serializedObject">拥有要绘制的属性的对象</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="tooltip">提示</param>
        /// <param name="displayName">显示名称</param>
        /// <param name="options">选项</param>
        /// <returns></returns>
        public static SerializedProperty DrawProperty(SerializedObject serializedObject, string propertyName, string tooltip = null, string displayName = null, params GUILayoutOption[] options)
        {
            SerializedProperty sp = serializedObject.FindProperty(propertyName);
            if (sp != null)
            {
                if (string.IsNullOrEmpty(displayName))
                    displayName = sp.displayName;
                EditorGUILayout.PropertyField(sp, new GUIContent(displayName, tooltip), options);
            }
            return sp;
        }

        /// <summary>
        /// 绘制带前缀的下拉列表
        /// </summary>
        /// <param name="text">前缀</param>
        /// <param name="index">当前选择的索引</param>
        /// <param name="list">列表内容</param>
        /// <param name="options">选项</param>
        /// <returns></returns>
        public static int DrawPrefixList(string text, int index, string[] list, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Popup(text, index, list, "DropDown", options);
        }

        /// <summary>
        /// 绘制带前缀的下拉列表
        /// </summary>
        /// <param name="text">前缀</param>
        /// <param name="value">当前选择的项</param>
        /// <param name="list">列表内容</param>
        /// <param name="options">选项</param>
        /// <returns></returns>
        public static string DrawPrefixList(string text, string value, string[] list, params GUILayoutOption[] options)
        {
            if (list == null)
                return "";
            int index = list.ToList().IndexOf(value);
            index = DrawPrefixList(text, index, list, options);
            if (list.Length <= 0)
                return "";
            if (index < 0 || index >= list.Length)
                return "";
            return list[index];
        }
    }
}