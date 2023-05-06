using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Tyrant.ComponentsForUnity.UnityUIExtention;
using Tyrant.GameCore.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Tyrant.ComponentsForUnity
{
    internal class AOTBinding
    {
        [UnityEngine.Scripting.Preserve]
        internal void Bind()
        {
            #region ArrayFieldDesc
            new ArrayFieldDesc<Vector2>();
            new ArrayFieldDesc<Vector2?>();
            new ArrayFieldDesc<Vector2Int>();
            new ArrayFieldDesc<Vector2Int?>();
            new ArrayFieldDesc<Vector3>();
            new ArrayFieldDesc<Vector3?>();
            new ArrayFieldDesc<Vector3Int>();
            new ArrayFieldDesc<Vector3Int?>();
            new ArrayFieldDesc<Vector4>();
            new ArrayFieldDesc<Vector4?>();
            new ArrayFieldDesc<Rect>();
            new ArrayFieldDesc<Rect?>();
            new ArrayFieldDesc<RectInt>();
            new ArrayFieldDesc<RectInt?>();
            new ArrayFieldDesc<Quaternion>();
            new ArrayFieldDesc<Quaternion?>();
            new ArrayFieldDesc<Matrix4x4>();
            new ArrayFieldDesc<Matrix4x4?>();
            new ArrayFieldDesc<Color>();
            new ArrayFieldDesc<Color?>();
            new ArrayFieldDesc<Color32>();
            new ArrayFieldDesc<Color32?>();
            new ArrayFieldDesc<LayerMask>();
            new ArrayFieldDesc<LayerMask?>();
            #endregion

            #region UnmanagedTypeSerializer
            new UnmanagedTypeSerializer<Vector2>();
            new UnmanagedTypeSerializer<Vector2Int>();
            new UnmanagedTypeSerializer<Vector3>();
            new UnmanagedTypeSerializer<Vector3Int>();
            new UnmanagedTypeSerializer<Vector4>();
            new UnmanagedTypeSerializer<Rect>();
            new UnmanagedTypeSerializer<RectInt>();
            new UnmanagedTypeSerializer<Quaternion>();
            new UnmanagedTypeSerializer<Matrix4x4>();
            new UnmanagedTypeSerializer<Color>();
            new UnmanagedTypeSerializer<Color32>();
            new UnmanagedTypeSerializer<LayerMask>();
            #endregion

            #region NullableSerializer
            new NullableSerializer<Vector2>();
            new NullableSerializer<Vector2Int>();
            new NullableSerializer<Vector3>();
            new NullableSerializer<Vector3Int>();
            new NullableSerializer<Vector4>();
            new NullableSerializer<Rect>();
            new NullableSerializer<RectInt>();
            new NullableSerializer<Quaternion>();
            new NullableSerializer<Matrix4x4>();
            new NullableSerializer<Color>();
            new NullableSerializer<Color32>();
            new NullableSerializer<LayerMask>();
            #endregion NullableSerializer

            #region LoadingBox.ShowAsync
            LoadingBox.ShowAsync<bool>(null, null, null);
            LoadingBox.ShowAsync<char>(null, null, null);
            LoadingBox.ShowAsync<byte>(null, null, null);
            LoadingBox.ShowAsync<sbyte>(null, null, null);
            LoadingBox.ShowAsync<ushort>(null, null, null);
            LoadingBox.ShowAsync<short>(null, null, null);
            LoadingBox.ShowAsync<uint>(null, null, null);
            LoadingBox.ShowAsync<int>(null, null, null);
            LoadingBox.ShowAsync<ulong>(null, null, null);
            LoadingBox.ShowAsync<long>(null, null, null);
            LoadingBox.ShowAsync<float>(null, null, null);
            LoadingBox.ShowAsync<double>(null, null, null);
            LoadingBox.ShowAsync<decimal>(null, null, null);
            LoadingBox.ShowAsync<TimeSpan>(null, null, null);
            LoadingBox.ShowAsync<DateTime>(null, null, null);
            LoadingBox.ShowAsync<Guid>(null, null, null);
            LoadingBox.ShowAsync<Vector2>(null, null, null);
            LoadingBox.ShowAsync<Vector2Int>(null, null, null);
            LoadingBox.ShowAsync<Vector3>(null, null, null);
            LoadingBox.ShowAsync<Vector3Int>(null, null, null);
            LoadingBox.ShowAsync<Vector4>(null, null, null);
            LoadingBox.ShowAsync<Rect>(null, null, null);
            LoadingBox.ShowAsync<RectInt>(null, null, null);
            LoadingBox.ShowAsync<Quaternion>(null, null, null);
            LoadingBox.ShowAsync<Matrix4x4>(null, null, null);
            LoadingBox.ShowAsync<Color>(null, null, null);
            LoadingBox.ShowAsync<Color32>(null, null, null);
            LoadingBox.ShowAsync<LayerMask>(null, null, null);
            LoadingBox.ShowAsync<RPCError>(null, null, null);
            #endregion
        }
    }

    public static class UnityEditorProgressBarEvents
    {
        public static event Action<string, string, float> Displayed;
        public static event Action Cleared;

        public static void Display(string title, string info, float progress) { Displayed?.Invoke(title, info, progress); }
        public static void Clear() { Cleared?.Invoke(); }
    }

    public static class UnityExtention
    {
        static UnityExtention()
        {
            Tyrant.GameCore.Debug.DebugHandler -= Debug_DebugHandler;
            Tyrant.GameCore.Debug.DebugHandler += Debug_DebugHandler;
            Tyrant.GameCore.Debug.ExceptionHandler -= Debug_ExceptionHandler;
            Tyrant.GameCore.Debug.ExceptionHandler += Debug_ExceptionHandler;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() { }

        /// <summary>
        /// 是否将调试信息输出到Unity编辑器的Console窗口
        /// </summary>
        public static bool EnableDebug { get; set; } = true;

        private static void Debug_DebugHandler(string arg1, GameCore.DebugLevel arg2, DateTime dateTime, GameCore.DebugDetailLevel detailLevel, GameCore.DebugModule module)
        {
            if (!EnableDebug)
                return;

            switch (arg2)
            {
                case GameCore.DebugLevel.Info:
                    Debug.Log(arg1);
                    break;
                case GameCore.DebugLevel.Warning:
                    Debug.LogWarning(arg1);
                    break;
                case GameCore.DebugLevel.Error:
                    Debug.LogError(arg1);
                    break;
            }
        }

        private static void Debug_ExceptionHandler(Exception obj, DateTime dateTime, GameCore.DebugDetailLevel detailLevel, GameCore.DebugModule module)
        {
            if (EnableDebug)
                Debug.LogException(obj);
        }

        /// <summary>
        /// 获取一个GameObject的子节点。
        /// </summary>
        /// <param name="component">从其所在的GameObject获取子节点</param>
        /// <param name="name">子节点的名称，包含路径</param>
        /// <returns>具有指定名称的子节点，找不到则返回空引用</returns>
        public static GameObject GetGameObjectInChild(this Component component, string name)
        {
            if (component == null)
                return null;
            return GetGameObjectInChild(component.gameObject, name);
        }

        /// <summary>
        /// 获取一个GameObject的子节点。
        /// </summary>
        /// <param name="gameObj">从其获取子节点</param>
        /// <param name="name">子节点的名称，包含路径</param>
        /// <returns>具有指定名称的子节点，找不到则返回空引用</returns>
        public static GameObject GetGameObjectInChild(this GameObject gameObj, string name)
        {
            var transform = gameObj.transform.Find(name);
            if (transform == null)
                return null;
            return transform.gameObject;
        }

        /// <summary>
        /// 在一个GameObject的子节点上获取指定类型的组件
        /// </summary>
        /// <typeparam name="T">指定要获取的组件的类型</typeparam>
        /// <param name="component">从其所在的GameObject的子节点上获取组件</param>
        /// <param name="name">子节点的名称，包含路径</param>
        /// <returns>若找到指定名称和类型的组件，则返回组件实例，否则返回空引用</returns>
        public static T GetComponentInChild<T>(this Component component, string name) where T : Component
        {
            if (component == null)
                return default(T);
            return GetComponentInChild<T>(component.gameObject, name);
        }

        /// <summary>
        /// 在一个GameObject的子节点上获取指定类型的组件
        /// </summary>
        /// <typeparam name="T">指定要获取的组件的类型</typeparam>
        /// <param name="gameObj">从其子节点上获取组件</param>
        /// <param name="name">子节点的名称，包含路径</param>
        /// <returns>若找到指定名称和类型的组件，则返回组件实例，否则返回空引用</returns>
        public static T GetComponentInChild<T>(this GameObject gameObj, string name) where T : Component
        {
            if (gameObj == null)
                return default(T);
            return (T)GetComponentInChild(gameObj, name, typeof(T));
        }

        /// <summary>
        /// 在一个GameObject的子节点上获取指定类型的组件
        /// </summary>
        /// <param name="component">从其所在的GameObject的子节点上获取组件</param>
        /// <param name="name">子节点的名称，包含路径</param>
        /// <param name="componentType">指定要获取的组件的类型</param>
        /// <returns>若找到指定名称和类型的组件，则返回组件实例，否则返回空引用</returns>
        public static Component GetComponentInChild(this Component component, string name, Type componentType)
        {
            if (component == null)
                return null;
            return GetComponentInChild(component.gameObject, name, componentType);
        }

        /// <summary>
        /// 在一个GameObject的子节点上获取指定类型的组件
        /// </summary>
        /// <param name="gameObj">从其子节点上获取组件</param>
        /// <param name="name">子节点的名称，包含路径</param>
        /// <param name="componentType">指定要获取的组件的类型</param>
        /// <returns>若找到指定名称和类型的组件，则返回组件实例，否则返回空引用</returns>
        public static Component GetComponentInChild(this GameObject gameObj, string name, Type componentType)
        {
            var transform = gameObj.transform.Find(name);
            if (transform == null)
                return null;
            return transform.GetComponent(componentType);
        }
    }
}
