using UnityEngine;
using System.Collections;
using Tyrant.GameCore;
using Tyrant.GameCore.DataBinding;
using System;
using System.Collections.Generic;

namespace Tyrant.ComponentsForUnity.DataBinding
{
    public class DataContextForUnity : MonoBehaviour, IDataContextNode
    {
        public DataContext DataContext { get; private set; }

        /// <summary>
        /// 获取或设置数据上下文值对象。不要在脚本的Awake()方法里调用，改为在Start()方法里调用，<see cref="DataContext.Value"/>属性同样如此。
        /// </summary>
        public object DataContextValue
        {
            get { return DataContext?.Value; }
            set
            {
                if (DataContext != null)
                    DataContext.Value = value;
            }
        }

        public DataContextForUnity()
        {
            DataContext = new DataContext(this);
        }

        IEnumerable<Tyrant.GameCore.DataBinding.DataBinding> IDataContextNode.DataBindings
        {
            get
            {
                foreach (var dataBindingForUnity in GetComponents<DataBindingForUnity>())
                {
                    if (dataBindingForUnity != null)
                        yield return dataBindingForUnity.DataBinding;
                }
            }
        }

        IEnumerable<Tyrant.GameCore.DataBinding.DataBinding> IDataContextNode.AllChildNodesDataBinding
        {
            get
            {
                foreach (var childDataBinding in GetAllChildNodesDataBinding(transform))
                    yield return childDataBinding;
            }
        }

        private static IEnumerable<Tyrant.GameCore.DataBinding.DataBinding> GetAllChildNodesDataBinding(Transform transform)
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<DataContextForUnity>() != null)
                    continue;

                foreach (var childDataBinding in child.GetComponents<DataBindingForUnity>())
                {
                    if (childDataBinding != null)
                        yield return childDataBinding.DataBinding;
                }
                foreach (var dataBinding in GetAllChildNodesDataBinding(child))
                    yield return dataBinding;
            }
        }

        //DataContext IDataContextNode.ParentNodeDataContext
        //{
        //    get
        //    {
        //        Transform tranParent = transform.parent;
        //        while (tranParent != null)
        //        {
        //            var parentDataContext = tranParent.GetComponent<DataContextForUnity>();
        //            if (parentDataContext != null)
        //                return parentDataContext.DataContext;
        //            tranParent = tranParent.parent;
        //        }
        //        return null;
        //    }
        //}
    }

    public static class DataContextForUnityExtention
    {
        /// <summary>
        /// 获取或创建该组件的GameObject之上的DataContextForUnity组件
        /// </summary>
        /// <param name="com">指定的组件</param>
        /// <returns>获取或创建的DataContextForUnity组件</returns>
        public static DataContextForUnity GetOrCreateDataContext(this Component com)
        {
            if (com == null)
                return null;
            return GetOrCreateDataContext(com.gameObject);
        }

        /// <summary>
        /// 获取或创建该GameObject之上的DataContextForUnity组件
        /// </summary>
        /// <param name="go">指定的GameObject</param>
        /// <returns>获取或创建的DataContextForUnity组件</returns>
        public static DataContextForUnity GetOrCreateDataContext(this GameObject go)
        {
            if (go == null)
                return null;
            var dataContext = go.GetComponent<DataContextForUnity>();
            if (dataContext == null)
                dataContext = go.AddComponent<DataContextForUnity>();
            return dataContext;
        }

        /// <summary>
        /// 获取组件的数据绑定的上下文
        /// </summary>
        /// <param name="com">指定的组件</param>
        /// <returns>数据绑定的数据对象</returns>
        public static object GetDataContextValue(this Component com)
        {
            if (com == null)
                return null;
            return GetDataContextValue(com.gameObject);
        }

        /// <summary>
        /// 获取GameObject的数据绑定的上下文
        /// </summary>
        /// <param name="go">指定的GameObject</param>
        /// <returns>数据绑定的数据对象</returns>
        public static object GetDataContextValue(this GameObject go)
        {
            if (go == null)
                return null;
            var dataContext = GetParentNodeDataContext(go.transform);
            return dataContext?.Value;
        }

        private static DataContext GetParentNodeDataContext(Transform transform)
        {
            Transform tranParent = transform;
            while (tranParent != null)
            {
                var parentDataContext = tranParent.GetComponent<DataContextForUnity>();
                if (parentDataContext != null)
                    return parentDataContext.DataContext;
                tranParent = tranParent.parent;
            }
            return null;
        }
    }
}