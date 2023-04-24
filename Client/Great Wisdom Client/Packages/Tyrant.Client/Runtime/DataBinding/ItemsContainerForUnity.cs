using UnityEngine;
using System.Collections;
using Tyrant.GameCore.DataBinding;
using System;
using System.Collections.Generic;

namespace Tyrant.ComponentsForUnity.DataBinding
{
    public class ItemsContainerForUnity : MonoBehaviour, IItemsContainerNode
    {
        [SerializeField]
        private UnityEngine.Object itemTemplate;
        //[SerializeField]
        //private UnityEngine.Object[] additionalItemTemplates;

        /// <summary>
        /// 获取或设置项模版，必须为可实例化为GameObject类型的对象
        /// </summary>
        /// <exception cref="ArgumentException">值不能被实例化为GameObject类型的对象</exception>
        public UnityEngine.Object ItemTemplate
        {
            get { return itemTemplate; }
            set
            {
                if (itemTemplate == value)
                    return;

                //if (value != null)
                //{
                //    var obj = Instantiate(value); // 这里实例化的时候回进行一次数据绑定，降低了运行效率
                //    try
                //    {
                //        if (!(obj is GameObject))
                        //{

                        //}
                //    }
                //    finally
                //    {
                //        Destroy(obj);
                //    }
                //}

                itemTemplate = value;
                ItemsContainer.ItemTemplate = value;
            }
        }

        /// <summary>
        /// 获取或设置数据源集合
        /// </summary>
        public IList SourceCollection
        {
            get { return ItemsContainer.SourceCollection; }
            set { ItemsContainer.SourceCollection = value; }
        }

        public ItemsContainer ItemsContainer { get; private set; }

        object IItemsContainerNode.InstantiateItemTemplate(int index)
        {
            if (ItemTemplate == null)
                return null;
            var instantiateObj = Instantiate(ItemTemplate) as GameObject;
            if (instantiateObj == null)
                throw new InvalidOperationException("");
            instantiateObj.transform.SetParent(transform, false);
            if (index >= 0)
                instantiateObj.transform.SetSiblingIndex(index);
            instantiateObj.transform.localScale = Vector3.one;
            return instantiateObj;
        }

        void IItemsContainerNode.SetChild(object item, int index)
        {
            if (item == null)
                return;
            var go = item as GameObject;
            if (go == null)
                return;
            go.transform.SetParent(transform, false);
            if (index >= 0)
                go.transform.SetSiblingIndex(index);
            go.transform.localScale = Vector3.one;
        }

        IEnumerable<object> IItemsContainerNode.DetachAllChildren()
        {
            foreach (Transform child in transform)
                yield return child.gameObject;
            transform.DetachChildren();
        }

        void IItemsContainerNode.DestoryAllChildren()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            transform.DetachChildren();
        }

        IEnumerable<DataContext> IItemsContainerNode.GetDataContext(object item)
        {
            var go = item as GameObject;
            if (go == null)
                yield return null;

            var dataContextCompenentArray = go.GetComponents<DataContextForUnity>();
            if (dataContextCompenentArray == null || dataContextCompenentArray.Length <= 0)
            {
                yield return go.AddComponent<DataContextForUnity>().DataContext;
            }
            else
            {
                foreach (var dataContextCompenent in dataContextCompenentArray)
                    yield return dataContextCompenent.DataContext;
            }
        }

        void IItemsContainerNode.DestroyItems(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                var obj = item as UnityEngine.Object;
                if (obj != null)
                    Destroy(obj);
                var go = obj as GameObject;
                if (go != null)
                    go.transform.SetParent(null, false);
            }
        }

        public ItemsContainerForUnity()
        {
            ItemsContainer = new ItemsContainer(this);
            ItemsContainer.UpdateItemsFinished += ItemsContainer_UpdateItemsFinished;
            ItemsContainer.ItemCreated += ItemsContainer_ItemCreated;
        }

        //[SerializeField]
        //private string itemTemplateSelectorTypeAssemblyQualifiedName = "";

        void Awake()
        {
            //ItemsContainer.ItemTemplateSelectorType = Type.GetType(itemTemplateSelectorTypeAssemblyQualifiedName, false);
            //ItemsContainer.AdditionalItemTemplates = additionalItemTemplates;
            var temp = itemTemplate;
            itemTemplate = null;  // 置空是为了下一句属性赋值不会在属性值相等的判断上返回
            ItemTemplate = temp;
        }

        void OnDestroy()
        {
            ItemsContainer.Dispose();
            ItemsContainer = null;
        }

        /// <summary>
        /// 每当项被创建时触发此事件
        /// </summary>
        public event Action<GameObject> ItemCreated;

        /// <summary>
        /// 当所有项更新完毕时触发此事件
        /// </summary>
        public event Action UpdateItemsFinished;

        private void ItemsContainer_ItemCreated(object obj)
        {
            ItemCreated?.Invoke(obj as GameObject);
        }

        private void ItemsContainer_UpdateItemsFinished()
        {
            UpdateItemsFinished?.Invoke();
        }
    }
}