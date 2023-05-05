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
    [CustomEditor(typeof(DataBindingForUnity), true)]
    public class DataBindingForUnityEditor : Editor
    {
        private Component TargetComponent
        {
            get { return serializedObject.FindProperty("targetComponent").objectReferenceValue as Component; }
            set { serializedObject.FindProperty("targetComponent").objectReferenceValue = value; }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DataBindingForUnity dataBinding = target as DataBindingForUnity;

            if (dataBinding != null)
            {
                var components = dataBinding.GetComponents(typeof(UnityEngine.Component)).Where(c => c.GetType() != typeof(DataBindingForUnity)).ToArray();
                var typeNames = components.Select(c => c.GetType().FullName).ToList();
                var targetComponentTypeNameProperty = serializedObject.FindProperty("targetComponentType");
                int selectedIndex = UnityEditorHelper.DrawPrefixList("Target Component", TargetComponent == null ? -1 : typeNames.IndexOf(TargetComponent.GetType().FullName), typeNames.ToArray());

                Component targetComponent = null;
                if (selectedIndex >=0 && selectedIndex < components.Length)
                    targetComponent = components[selectedIndex];
                TargetComponent = targetComponent;

                if (targetComponent != null)
                {
                    var bpNames = BindableProperty.GetProperties(targetComponent.GetType()).Select(bp => bp.Name).ToList();
                    var targetPropertyNameProperty = serializedObject.FindProperty("targetPropertyName");
                    if (targetPropertyNameProperty != null)
                        targetPropertyNameProperty.stringValue = UnityEditorHelper.DrawPrefixList("Target Property", targetPropertyNameProperty.stringValue, bpNames.ToArray());
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("enableInverseBinding"));
            }

            //DrawProperty("Path", serializedObject, "path");
            var pathProperty = serializedObject.FindProperty("sourcePropertyPath");
            var sourceTypeProperty = serializedObject.FindProperty("sourceTypeFullName");
            if (pathProperty != null && sourceTypeProperty != null)
            {
                GUILayout.Toggle(true, "<b><size=11>Source Property Path</size></b>", "dragtab", GUILayout.MinWidth(20f));

                var guiSkin_current = typeof(GUISkin).InvokeMember("current", BindingFlags.GetField|BindingFlags.Static|BindingFlags.NonPublic, null, null, null);

                EditorGUILayout.BeginHorizontal("textarea", GUILayout.MinHeight(10f));
                GUILayout.BeginVertical();
                GUILayout.Space(2f);

                string text = $"{pathProperty.stringValue} : ({sourceTypeProperty.stringValue})";
                if (string.IsNullOrEmpty(pathProperty.stringValue) || string.IsNullOrEmpty(sourceTypeProperty.stringValue))
                    text = "";
                GUILayout.TextField(text);
                if (GUILayout.Button("Edit value with UI window"))
                    TypePropertyTreeWindow.ShowWindow(pathProperty.stringValue, sourceTypeProperty.stringValue, OnEditFinished);

                GUILayout.Space(3f);
                GUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3f);
            }

            GUILayout.Toggle(true, "<b><size=11>Converter</size></b>", "dragtab", GUILayout.MinWidth(20f));

            EditorGUILayout.BeginVertical("textarea", GUILayout.MinHeight(10f));
            var converterTypeAssemblyQualifiedName = serializedObject.FindProperty("converterTypeAssemblyQualifiedName").stringValue;
            var btnText = "None";
            Type converterType = null;
            if (!string.IsNullOrEmpty(converterTypeAssemblyQualifiedName))
            {
                converterType = Type.GetType(converterTypeAssemblyQualifiedName, false);
                btnText = converterType == null ? "Missing" : converterTypeAssemblyQualifiedName;
            }
            if (GUILayout.Button(btnText, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), false, OnConverterMenuClicked, null);
                foreach (var type in GetConverterTypes())
                    menu.AddItem(new GUIContent(type.FullName), false, OnConverterMenuClicked, type);
                menu.ShowAsContext();
            }
            if (GUILayout.Button("复制名称到剪切板"))
            {               
                if (converterType != null)
                {
                    TextEditor te = new TextEditor();
                    te.text = converterType.Name;
                    te.SelectAll();
                    te.Copy();
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"), new GUIContent("Priority", "数值越大越优先执行，数值相同的执行顺序不可预知。"));

            serializedObject.ApplyModifiedProperties();
        }

        private IEnumerable<Type> GetConverterTypes()
        {
            yield return typeof(NullToZeroConverter);
            yield return typeof(ResourceNameToSpriteConverter);
            yield return typeof(InverseBoolConverter);
            yield return typeof(EqualNullConverter);
            yield return typeof(NotEqualNullConverter);
            yield return typeof(ColorHexStringToColorInstanceConverter);

            foreach (var assembly in Core.Utility.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass)
                        continue;
                    if (!type.IsPublic)
                        continue;
                    var interfaceTypes = type.FindInterfaces((m, obj) => { return m == typeof(IDataBindingConverter); }, null);
                    if (interfaceTypes == null || interfaceTypes.Length <= 0)
                        continue;
                    try
                    {
                        Activator.CreateInstance(type);
                    }
                    catch
                    {
                        continue;
                    }
                    yield return type;
                }
            }
        }

        private void OnConverterMenuClicked(object t)
        {
            serializedObject.Update();
            serializedObject.FindProperty("converterTypeAssemblyQualifiedName").stringValue = (t == null ? "" : (t as Type).AssemblyQualifiedName);
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEditFinished(string path, Type type)
        {
            serializedObject.Update();
            serializedObject.FindProperty("sourcePropertyPath").stringValue = path;
            serializedObject.FindProperty("sourceTypeFullName").stringValue = type.FullName;
            serializedObject.ApplyModifiedProperties();
        }

        public static SerializedProperty DrawProperty(string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
        {
            SerializedProperty sp = serializedObject.FindProperty(property);
            if (sp != null)
            {
                if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
                else EditorGUILayout.PropertyField(sp, options);
            }
            return sp;
        }
    }

    public class TypePropertyTreeWindow : EditorWindow
    {
        private Type[] types = null;
        private Dictionary<Type, TreeItem[]> dic_type_propertyTrees = new Dictionary<Type, TreeItem[]>();

        public Type SelectedType { get; set; }

        //private string path = "";
        public Action<string, Type> EditFinished = null;

        private string path = null;
        private string typeFullName = null;

        public static void ShowWindow(string path, string typeFullName, Action<string, Type> editFinishedCallback)
        {
            TypePropertyTreeWindow window = EditorWindow.GetWindow<TypePropertyTreeWindow>(true, "Select Soucre Property Path");
            window.EditFinished = editFinishedCallback;
            window.path = path;
            window.typeFullName = typeFullName;
            window.Refresh();
        }

        private static void CreateChildTreeItem(TreeItem parent, string selectedPath)
        {
            if (parent.SelectedPath == selectedPath)
                TreeItem.SelectedItem = parent;

            if (parent.Tag == null)
                return;
            var property = parent.Tag as BindableProperty;
            if (property == null)
                return;

            if (!typeof(INotifyPropertyChanged).IsAssignableFrom(property.PropertyType))
                return;

            List<TreeItem> trees = new List<TreeItem>();
            foreach (var pro in BindableProperty.GetProperties(property.PropertyType))
            {
                TreeItem tree = new TreeItem() { Title = string.Format("{0} : ({1})", pro.Name, GetTypeAlias(pro.PropertyType.FullName)), Tag = pro, Parent = parent };
                //trees.Add(tree);
                CreateChildTreeItem(tree, selectedPath);
            }

            //foreach (var tree in trees.OrderBy(t => t.Title))
            //    tree.Parent = parent;
        }

        private static string GetTypeAlias(string typeName)
        {
            if (typeName == null)
                return "";
            return typeName
                .Replace("System.Boolean", "bool")
                .Replace("System.Byte", "byte")
                .Replace("System.SByte", "sbyte")
                .Replace("System.Char", "char")
                .Replace("System.Decimal", "decimal")
                .Replace("System.Double", "double")
                .Replace("System.Single", "float")
                .Replace("System.Int32", "int")
                .Replace("System.UInt32", "uint")
                .Replace("System.Int64", "long")
                .Replace("System.UInt64", "ulong")
                .Replace("System.Object", "object")
                .Replace("System.Int16", "short")
                .Replace("System.UInt16", "ushort")
                .Replace("System.String", "string");
        }

        private void Refresh()
        {
            DataBinding.ReLoadBindableTypes();

            dic_type_propertyTrees.Clear();
            SelectedType = null;
            TreeItem.SelectedItem = null;
            List<Type> typeList = new List<Type>();

            string selectedTypeFullName = EditorPrefs.GetString("DataBindingForUnityEditor_SelectedType");
            if (!string.IsNullOrEmpty(typeFullName))
                selectedTypeFullName = typeFullName;

            string selectedPath = EditorPrefs.GetString("DataBindingForUnityEditor_SelectedPath");
            if (!string.IsNullOrEmpty(path))
                selectedPath = path;

            foreach (var assembly in Core.Utility.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                        continue;

                    typeList.Add(type);

                    List<TreeItem> trees = new List<TreeItem>();
                    foreach (var property in BindableProperty.GetProperties(type))
                    {
                        TreeItem tree = new TreeItem() { Title = string.Format("{0} : ({1})", property.Name, GetTypeAlias(property.PropertyType.FullName)), Tag = property };
                        trees.Add(tree);
                        CreateChildTreeItem(tree, selectedPath);
                    }
                    dic_type_propertyTrees[type] = trees.OrderBy(t => t.Title).ToArray();

                    if (type.FullName == selectedTypeFullName)
                        SelectedType = type;
                }
            }
            types = typeList.OrderBy(t => t.FullName).ToArray();
        }

        private float halfWidth = 100.0f;
        private Vector2 vecLeft = Vector2.zero;
        private Vector2 vecRight = Vector2.zero;

        void OnGUI()
        {
            EditorGUILayout.HelpBox("Select type in left area and select properties in right area.", MessageType.Info);

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            Rect r = EditorGUILayout.BeginHorizontal("textarea");
            float w = (r.width - 4) / 2;
            if (w > 0)
                halfWidth = w;

            GUILayout.BeginVertical(GUILayout.Width(halfWidth)); // type list, left area
            vecLeft = GUILayout.BeginScrollView(vecLeft);
            foreach (var type in types)
            {
                bool selected = SelectedType == type;
                GUI.backgroundColor = selected ? Color.red : Color.white;
                if (GUILayout.Toggle(selected, type.FullName, "dragtab"))
                    SelectedType = type;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            r = EditorGUILayout.BeginVertical(GUILayout.Width(4), GUILayout.ExpandHeight(true)); // separator
            GUI.color = Color.gray;
            GUI.DrawTexture(r, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
            GUILayout.Label("");
            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(r.width / 2)); // property tree, rigth area
            vecRight = GUILayout.BeginScrollView(vecRight);
            if (SelectedType != null)
            {
                TreeItem[] trees = null;
                if (dic_type_propertyTrees.TryGetValue(SelectedType, out trees))
                {
                    foreach (var tree in trees)
                        tree.Draw();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                Refresh();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK", GUILayout.Width(100)))
            {
                if (EditFinished != null)
                    EditFinished(TreeItem.SelectedItem.SelectedPath, SelectedType);
                Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        void OnDestroy()
        {
            EditorPrefs.SetString("DataBindingForUnityEditor_SelectedType", SelectedType.FullName);
            EditorPrefs.SetString("DataBindingForUnityEditor_SelectedPath", TreeItem.SelectedItem?.SelectedPath);
        }
    }

    internal class TreeItem
    {
        internal static TreeItem SelectedItem { get; set; }

        internal string SelectedPath
        {
            get
            {
                string p = "";
                var current = this;
                while (current != null)
                {
                    var pro = current.Tag as BindableProperty;
                    if (pro != null)
                        p = pro.Name + "." + p;
                    current = current.Parent;
                }
                p = p.TrimEnd('.');
                return p;
            }
        }

        internal string Title { get; set; }

        //public bool IsSelected { get; set; }

        private bool isExpanded = false;
        internal bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                this.isExpanded = value;
                if (value)
                {
                    if (this.parent != null)
                        this.parent.IsExpanded = true;
                }
            }
        }

        internal object Tag { get; set; }

        private List<TreeItem> childItems = new List<TreeItem>();

        private TreeItem parent = null;
        internal TreeItem Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;

                if (parent != null)
                    parent.childItems.Remove(this);
                if (value != null)
                    value.childItems.Add(this);
                parent = value;
            }
        }

        internal TreeItem()
        {
            Title = "";
        }

        internal virtual void Draw()
        {
            GUILayout.BeginHorizontal();
            if (childItems.Count > 0)
                isExpanded = GUILayout.Toggle(isExpanded, isExpanded ? "\u25BC" : "\u25B6", "label", GUILayout.Width(15));
            else
                GUILayout.Space(22);
            bool isSelected = SelectedItem == this;
            GUI.backgroundColor = isSelected ? Color.red : Color.white;
            if (GUILayout.Toggle(isSelected, Title, "dragtab"))
                SelectedItem = this;
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            if (IsExpanded && childItems.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(22);
                GUILayout.BeginVertical();
                foreach (var childItem in childItems)
                    childItem.Draw();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }
    }
}