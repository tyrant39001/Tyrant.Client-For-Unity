using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;
using UnityEditor.UI;
using UnityEngine.EventSystems;
using Tyrant.ComponentsForUnity.UnityUIExtention;
using System.IO;
using System.Linq;
using Tyrant.ComponentsForUnity;
using System;
using Tyrant.GameCore.Net;

namespace Tyrant.ComponentsEditorForUnity
{
    public static class UIHelper
    {
        private static MethodInfo CreateUIElementRootMethod = null;
        private static MethodInfo CreateUIObjectMethod = null;
        //private static MethodInfo SetParentAndAlignMethod = null;
        private static MethodInfo PlaceUIElementRootMethod = null;

        static UIHelper()
        {
            CreateUIElementRootMethod = typeof(DefaultControls).GetMethod("CreateUIElementRoot", BindingFlags.NonPublic | BindingFlags.Static);
            CreateUIObjectMethod = typeof(DefaultControls).GetMethod("CreateUIObject", BindingFlags.NonPublic | BindingFlags.Static);
            //SetParentAndAlignMethod = typeof(DefaultControls).GetMethod("SetParentAndAlign", BindingFlags.NonPublic | BindingFlags.Static);
            var menuOptionsType = typeof(ButtonEditor).Assembly.GetType("UnityEditor.UI.MenuOptions");
            PlaceUIElementRootMethod = menuOptionsType.GetMethod("PlaceUIElementRoot", BindingFlags.NonPublic | BindingFlags.Static);
        }

        /// <summary>
        /// 创建一个带有RectTransform组件并指定大小的GameObject
        /// </summary>
        /// <param name="name">GameObject的名称</param>
        /// <param name="size">赋值给RectTransform组件的sizeDelta属性</param>
        /// <returns>一个GameObject，其name属性的值为参数name指定的值，其带有RectTransform组件，RectTransform组件的sizeDelta属性的值为参数size指定的值</returns>
        public static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            return CreateUIElementRootMethod.Invoke(null, new object[] { name, size }) as GameObject;
        }

        /// <summary>
        /// 创建一个带有RectTransform组件并指定父节点的GameObject
        /// </summary>
        /// <param name="name">GameObject的名称</param>
        /// <param name="parent">GameObject的父节点</param>
        /// <returns>一个GameObject，其name属性的值为参数name指定的值，其带有RectTransform组件，其父节点为参数parent指定的值</returns>
        public static GameObject CreateUIObject(string name, GameObject parent)
        {
            return CreateUIObjectMethod.Invoke(null, new object[] { name, parent }) as GameObject;
        }

        /// <summary>
        /// 为参数element设置一个唯一名称，并将参数menuCommand的属性context设置为element的父节点，最后将element设置为当前选中的对象
        /// </summary>
        /// <param name="element"></param>
        /// <param name="menuCommand"></param>
        public static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            PlaceUIElementRootMethod.Invoke(null, new object[] { element, menuCommand });
        }
    }

    internal static class Menu
    {
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            UnityEditorProgressBarEvents.Displayed -= UnityEditorProgressBarEvents_Displayed;
            UnityEditorProgressBarEvents.Displayed += UnityEditorProgressBarEvents_Displayed;
            UnityEditorProgressBarEvents.Cleared -= UnityEditorProgressBarEvents_Cleared;
            UnityEditorProgressBarEvents.Cleared += UnityEditorProgressBarEvents_Cleared;
            UnityExtention.EnableDebug = true;
        }

        private static void UnityEditorProgressBarEvents_Cleared()
        {
            EditorUtility.ClearProgressBar();
        }

        private static void UnityEditorProgressBarEvents_Displayed(string arg1, string arg2, float arg3)
        {
            EditorUtility.DisplayProgressBar(arg1, arg2, arg3);
        }

        [MenuItem("GameObject/Tyrant.ComponentsForUnity/UI/Dialog Panel", false, 0)]
        static void AddDialogPanel(MenuCommand menuCommand)
        {
            var panel = UIHelper.CreateUIElementRoot("Dialog Panel", Vector2.zero);       
            panel.AddComponent<ClickToDeactiveSelf>();
            var image = panel.GetComponent<Image>();
            image.color = new Color(1, 1, 1, 0);
            var mainContent = UIHelper.CreateUIObject("Main Content", panel);
            mainContent.AddComponent<ClickDoNothing>();
            var closeButton = UIHelper.CreateUIObject("Close Button", mainContent);
            closeButton.AddComponent<Image>();
            closeButton.AddComponent<Button>();
            var close = closeButton.AddComponent<ClickToDeactiveObj>();
            close.ObjToClose = panel;
            var rectTransform = close.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.one;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = new Vector2(-20, -20);
            rectTransform.sizeDelta = new Vector2(30, 30);
            UIHelper.PlaceUIElementRoot(panel, menuCommand); 
            rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
        }

        //[MenuItem("GameObject/Tyrant.ComponentsForUnity/UI/Progress Bar", false, 0)]
        //static void AddProgressBar(MenuCommand menuCommand)
        //{
        //    var backGround = UIHelper.CreateUIElementRoot("Progress Bar", new Vector2(100.0f, 100.0f));
        //    backGround.AddComponent(typeof(Image));
        //    var progressBar = backGround.AddComponent<ProgressBar>();
        //    var smoothMask = backGround.AddComponent<SmoothMask>();
        //    var progress = UIHelper.CreateUIObject("Image", backGround);
        //    var progressImage = progress.AddComponent<Image>();
        //    var progressImageRectTransform = progressImage.GetComponent<RectTransform>();
        //    progressImageRectTransform.anchorMin = Vector2.zero;
        //    progressImageRectTransform.anchorMax = Vector2.one;
        //    progressImageRectTransform.sizeDelta = Vector2.zero;
        //    progressBar.ProgressImage = progressImage;
        //    smoothMask.AddContent(progressImage);
        //    UIHelper.PlaceUIElementRoot(backGround, menuCommand);
        //}

        //[MenuItem("Tyrant/Reload Bindable Types", false, 1)]
        //public static void ReloadBindableProperties()
        //{
        //    Tyrant.GameCore.DataBinding.DataBinding.ReLoadBindableTypes();
        //}

        //[MenuItem("Tyrant/Build AssetBundles")]
        //static public void BuildAssetBundles()
        //{
        //    AssetBundleWindow.ShowWindow();
        //}

        [MenuItem("Tyrant/Excel表格工具")]
        public static void ShowDesignerDataBuilderAndLoaderTool()
        {
            DesignerDataToolEditorWindow.ShowWindow();
        }

        //[MenuItem("Tyrant/Build ILCode For IOS And Build IOS")]
        //public static void BuildILCode()
        //{
        //    Debug.Log("Begin Build ILCode");
        //    var rpcExecType = typeof(RPCExecManager);
        //    var rpcExec = Activator.CreateInstance(rpcExecType);
        //    var p = rpcExecType.GetProperty("IsForceBuildCode", BindingFlags.Instance | BindingFlags.NonPublic);
        //    if (p != null)
        //        p.SetMethod?.Invoke(rpcExec, new object[] { true });
        //    var method = rpcExecType.GetMethod("BuildAssemblyRPC", BindingFlags.Instance | BindingFlags.NonPublic);
        //    if (method != null)
        //        method.Invoke(rpcExec, new object[] { });

        //    var inputFile = $"{AppDomain.CurrentDomain.BaseDirectory}\\RPCExecAssembly.dll";
        //    var outputFile = $"{Application.dataPath}\\RPCExecAssembly.dll";
        //    //Debug.Log(file);
        //    var type = typeof(Core.SignFileDll);
        //    type.InvokeMember("SignDll", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic, null, null, new object[] { inputFile, outputFile });
        //    AssetDatabase.Refresh();
        //    BuildOptions buildOptions = BuildOptions.None;
        //    if (EditorUserBuildSettings.allowDebugging)
        //        buildOptions |= BuildOptions.AllowDebugging;
        //    if (EditorUserBuildSettings.development)
        //        buildOptions |= BuildOptions.Development;
        //    if (EditorUserBuildSettings.connectProfiler)
        //        buildOptions |= BuildOptions.ConnectWithProfiler;
        //    if (EditorUserBuildSettings.buildScriptsOnly)
        //        buildOptions |= BuildOptions.BuildScriptsOnly;
        //    //if ()
        //    BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Where(i => i.enabled == true).ToArray(), "IOS", BuildTarget.iOS, buildOptions);
        //    //EditorBuildSettings.scenes
        //}
    }
}