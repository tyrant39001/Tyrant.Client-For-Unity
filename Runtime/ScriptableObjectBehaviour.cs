using UnityEngine;
using System;
using System.Collections.Generic;
using Tyrant.Core;

namespace Tyrant.ComponentsForUnity
{
    /// <summary>
    /// 承载一个ScriptableObject，并可调用静态方法获取其实例，每一个类型的ScriptableObject只能存在一个
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ScriptableObjectBehaviour : MonoBehaviour
    {
        private static Dictionary<Type, ScriptableObject> dic_type_scriptableObject = new Dictionary<Type, ScriptableObject>();

        [SerializeField]
        private ScriptableObject scriptableObject = null;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (scriptableObject != null)
                dic_type_scriptableObject[scriptableObject.GetType()] = scriptableObject;
        }

        /// <summary>
        /// 获取在编辑器中附加到scriptableObject的实例
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <returns>T类型的实例</returns>
        /// <exception cref="System.InvalidCastException">编辑器中附加的类型不能转换为T</exception>
        public static T GetScriptableObjectInstance<T>() where T : ScriptableObject
        {
            return (T)dic_type_scriptableObject.TryGetValue(typeof(T));
        }
    }
}