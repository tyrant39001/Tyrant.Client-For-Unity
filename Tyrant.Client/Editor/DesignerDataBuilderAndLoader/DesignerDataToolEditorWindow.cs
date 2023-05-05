using UnityEngine;
using UnityEditor;
using System.Collections;
using Tyrant.Core;
using System;
using System.Reflection;
using Tyrant.ComponentsForUnity;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tyrant.GameCore;

namespace Tyrant.ComponentsEditorForUnity
{
    /// <summary>
    /// 策划数据编辑器工具
    /// </summary>
    public class DesignerDataToolEditorWindow : EditorWindow
    {
        internal static GUIContent AssemblyContent = new GUIContent("Assembly", "drag a dll file which contains designer data types from Project Window to here");

        //[Tooltip("输出目录，第一个目录为客户端加载目录")]
        //protected PathItem _OutputDirectory = new PathItem();

        public static void ShowWindow()
        {
            GetWindow<DesignerDataToolEditorWindow>(true, "Excel表格工具", true);
        }

        //public static void DrawClickButton()
        //{
        //    if (GUILayout.Button("Edit value with UI window"))
        //        ShowWindow();
        //}

        //private IEnumerator buildEnumerator = null;
        //private bool isExpanded = true;
        void OnGUI()
        {
            //开始检查是否有修改
            EditorGUI.BeginChangeCheck();

            //源gzg文件所在目录
            DesignerDataToolData.Instance.ExcelFilesRootPath = DrawDirectorySelector(DesignerDataToolData.Instance.ExcelFilesRootPath, "Root Directory", "Excel gzg文件所在跟目录", true); 

            //输出目录
            DesignerDataToolData.Instance.OutputDirectory = DrawDirectorySelector(DesignerDataToolData.Instance.OutputDirectory, "Output Directory", "输出目录", true);

            ////程序集目录：即Tyrant策划文件 所在目录
            //DrawDragAssembly(DesignerDataToolData.Instance.AssemblyData);

            //绘制Build按钮 执行生成数据
            DrawBuild();

            if (EditorGUI.EndChangeCheck())
                DesignerDataToolData.Instance.Save();
        }

        #region 绘制


        /// <summary>
        /// 绘制执行生成数据
        /// </summary>
        private void DrawBuild()
        {
            GUILayout.BeginHorizontal();
            //Build按钮
            var guiEnabledCache = GUI.enabled;
            GUI.enabled = !DesignerDataBuilder.IsExcuting && (!string.IsNullOrEmpty(DesignerDataToolData.Instance.ExcelFilesRootPath)) && (!string.IsNullOrEmpty(DesignerDataToolData.Instance.OutputDirectory));
            if (GUILayout.Button(new GUIContent("Build", "点击生成数据"), GUILayout.ExpandWidth(true)))
                DesignerDataBuilder.BuildInternal();
            GUI.enabled = guiEnabledCache;
            //是否运行游戏时
            DesignerDataToolData.Instance.BuildOnRuntimeInitialize = GUILayout.Toggle(DesignerDataToolData.Instance.BuildOnRuntimeInitialize, new GUIContent("运行时生成", "勾选此项Unity运行时生成文件"), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制文件夹选择器
        /// </summary>
        /// <param name="propertyValue">属性名称</param>
        /// <param name="titile">标题</param>
        /// <param name="toolTip">提示</param>
        /// <param name="isRelative">是绝对路径</param>
        /// <returns></returns>
        private static string DrawDirectorySelector(string propertyValue, string titile, string toolTip, bool isRelative)
        {
            GUILayout.BeginHorizontal();

            GUIContent inputFolderContent = new GUIContent(titile, toolTip);
            EditorGUIUtility.labelWidth = 120.0f;
            propertyValue = EditorGUILayout.TextField(inputFolderContent, propertyValue, GUILayout.MinWidth(500));

            if (GUILayout.Button(new GUIContent("...", "click to select directory"), GUILayout.MaxWidth(30)))
            {
                string path = "";
                try
                {
                    path = isRelative ? Utility.GetAbsolutePath(Environment.CurrentDirectory.AppendDirectorySeparatorIfNotEndWith(), propertyValue.AppendDirectorySeparatorIfNotEndWith()) : propertyValue;
                }
                catch { }
                string selectedPath = EditorUtility.OpenFolderPanel("", path, "").Replace("/", "\\");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (isRelative)
                    {
                        try
                        {
                            propertyValue = Utility.GetRelativePath(Environment.CurrentDirectory.AppendDirectorySeparatorIfNotEndWith(), selectedPath.AppendDirectorySeparatorIfNotEndWith());
                        }
                        catch
                        {
                            propertyValue = selectedPath;
                        }
                    }
                    else
                        propertyValue = selectedPath;
                }
            }
            //if (GUILayout.Button(new GUIContent("Delete All Files/Directorys", "删除此目录下所有文件和文件夹"), GUILayout.MaxWidth(200)))
            //{
            //    string path = "";
            //    try
            //    {
            //        path = isRelative ? Utility.GetAbsolutePath(Environment.CurrentDirectory.AppendDirectorySeparatorIfNotEndWith(), propertyValue.AppendDirectorySeparatorIfNotEndWith()) : propertyValue;
            //    }
            //    catch { }
            //    Directory.Delete(path, true);
            //}
            GUILayout.EndHorizontal();
            return propertyValue;
        }

        #endregion
    }

    [InitializeOnLoad]
    public class DesignerDataBuilder
    {
        static DesignerDataBuilder()
        {
            EditorApplication.update += EditorApplicationUpdate;
        }

        private static volatile ProgressTask ProgressTask;
        private static volatile bool gettingProgressTask;
        public static bool IsExcuting => gettingProgressTask || (ProgressTask != null && !ProgressTask.GetAwaiter().IsCompleted);
        private static bool excutingLastUpdate;

        public static Action BuildFinised;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Build()
        {
            if (!DesignerDataToolData.Instance.BuildOnRuntimeInitialize)
                return;
            BuildInternal();
        }

        internal static async void BuildInternal()
        {
            gettingProgressTask = true;        
            ProgressTask = await GameCore.DesignerDataTranslator.Excute(DesignerDataToolData.Instance.ExcelFilesRootPath, "bytes", DesignerDataToolData.Instance.OutputDirectory);
            gettingProgressTask = false;
        }

        static void EditorApplicationUpdate()
        {
            try
            {
                if (IsExcuting)
                {
                    var progressValue = 0.0f;
                    var progressInfo = "";
                    if ((ProgressTask != null && !ProgressTask.GetAwaiter().IsCompleted))
                    {
                        progressValue = (float)ProgressTask.CurrentProgress / ProgressTask.TotalProgress;
                        progressInfo = $"进度:{ProgressTask.CurrentProgress}/{ProgressTask.TotalProgress}";
                    }
                    UnityEditorProgressBarEvents.Display("正在生成Excel数据", progressInfo, progressValue);
                }
                else
                {
                    UnityEditorProgressBarEvents.Clear();
                    if (excutingLastUpdate)
                    {
                        AssetDatabase.Refresh();
                        BuildFinised?.Invoke();
                    }
                }
            }
            finally
            {
                excutingLastUpdate = IsExcuting;
            }
        }
    }
}