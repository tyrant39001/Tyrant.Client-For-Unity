using UnityEngine;
using System.Collections;
using Tyrant.GameCore;
using Tyrant.GameCore.DataBinding;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Tyrant.Core;
using UnityEngine.UI;

namespace Tyrant.ComponentsForUnity.DataBinding
{
    public class DataBindingForUnity : MonoBehaviour, IDataBindingNode
    {
        static DataBindingForUnity()
        {
            try  // 防止代码被剥离
            {
                (new ResourceNameToSpriteConverter() as IDataBindingConverter).ConvertTo(null, null, null);
                (new ColorHexStringToColorInstanceConverter() as IDataBindingConverter).ConvertTo(null, null, null);
            }
            catch { }

            BindableProperty.CreateTargetLogicalProperty("Active", typeof(bool), typeof(Component), (target, value) =>
            {
                var component = target as Component;
                if (value is bool && component != null && component.gameObject != null)
                    component.gameObject.SetActive((bool)value);
            });

            BindableProperty.CreateTargetLogicalProperty("Enable", typeof(bool), typeof(Behaviour), (target, value) =>
            {
                var behaviour = target as Behaviour;
                if (value is bool && behaviour != null)
                    behaviour.enabled = (bool)value;
            });

            BindableProperty.CreateTargetLogicalProperty("Interactable", typeof(bool), typeof(Selectable), (target, value) =>
            {
                var selectable = target as Selectable;
                if (value is bool && selectable != null)
                    selectable.interactable = (bool)value;
            });

            BindableProperty.CreateTargetLogicalProperty("Text", typeof(string), typeof(Text), (target, value) =>
            {
                var text = target as Text;
                if (text != null)
                    text.text = value == null ? "" : value.ToString();
            });

            BindableProperty.CreateTargetLogicalProperty("Text", typeof(string), typeof(InputField), (target, value) =>
            {
                var text = target as InputField;
                if (text != null)
                    text.text = value == null ? "" : value.ToString();
            });

            BindableProperty.CreateTargetLogicalProperty("Color", typeof(Color), typeof(Graphic), (target, value) =>
            {
                var graphic = target as Graphic;
                if (value is Color && graphic != null)
                    graphic.color = (Color)value;
            });

            BindableProperty.CreateTargetLogicalProperty("Color", typeof(Color), typeof(Outline), (target, value) =>
            {
                var outline = target as Outline;
                if (value is Color && outline != null)
                    outline.effectColor = (Color)value;
            });

            BindableProperty.CreateTargetLogicalProperty("Alignment", typeof(TextAnchor), typeof(Text), (target, value) =>
            {
                var text = target as Text;
                if (value is TextAnchor && text != null)
                    text.alignment = (TextAnchor)value;
            });

            BindableProperty.CreateTargetLogicalProperty("ProgressValue", typeof(float), typeof(Slider), (target, value) =>
            {
                var slider = target as Slider;
                if (value is int && slider != null)
                    slider.value = Convert.ToSingle(value);
            });

            BindableProperty.CreateTargetLogicalProperty("MinValue", typeof(float), typeof(Slider), (target, value) =>
            {
                var slider = target as Slider;
                if (value is int && slider != null)
                    slider.minValue = Convert.ToSingle(value);
            });

            BindableProperty.CreateTargetLogicalProperty("MaxValue", typeof(float), typeof(Slider), (target, value) =>
            {
                var slider = target as Slider;
                if (value is int && slider != null)
                    slider.maxValue = Convert.ToSingle(value);
            });

            BindableProperty.CreateTargetLogicalProperty("Sprite", typeof(Sprite), typeof(Image), (target, value) =>
            {
                var image = target as Image;
                if (value is Sprite && image != null)
                    image.sprite = value as Sprite;
            });

            BindableProperty.CreateTargetLogicalProperty("SpriteNameInResources", typeof(string), typeof(Image), (target, value) =>
            {
                var image = target as Image;
                if (value != null && image != null)
                {
                    try
                    {
                        image.sprite = Resources.Load<Sprite>(value.ToString());
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            });

            BindableProperty.CreateTargetLogicalProperty("IsOn", typeof(bool), typeof(Toggle), (target, value) =>
            {
                var toggle = target as Toggle;
                if (value is bool && toggle != null)
                    toggle.isOn = (bool)value;
            });

            BindableProperty.CreateTargetLogicalProperty("SourceCollection", typeof(IList), typeof(ItemsContainerForUnity), (target, value) =>
            {
                var itemsContainerForUnity = target as ItemsContainerForUnity;
                if (value is IList && itemsContainerForUnity != null)
                    itemsContainerForUnity.SourceCollection = (IList)value;
            });

            BindableProperty.CreateTargetLogicalProperty("Alpha", typeof(float), typeof(CanvasGroup), (target, value) =>
            {
                var canvasGroup = target as CanvasGroup;
                float a = 1.0f;
                try
                {
                    a = Convert.ToSingle(value);
                }
                catch { }
                if (canvasGroup != null)
                    canvasGroup.alpha = a;
            });

            BindableProperty.CreateTargetLogicalProperty("ChildResourceName", typeof(string), typeof(Component), (target, value) =>
            {
                var component = target as Component;
                if (component != null && value != null)
                {
                    var res = Resources.Load(value.ToString());
                    if (res != null)
                    {
                        var obj = Instantiate(res);
                        if (obj is GameObject)
                            (obj as GameObject).transform.SetParent(component.transform, false);
                    }
                }
            });

            BindableProperty.CreateTargetLogicalProperty("ProgressValue", typeof(float), typeof(ProgressBar), (target, value) =>
            {
                var progressBar = target as ProgressBar;
                if (value is int && progressBar != null)
                    progressBar.Value = Convert.ToSingle(value);
            });

            BindableProperty.CreateTargetLogicalProperty("MinValue", typeof(float), typeof(ProgressBar), (target, value) =>
            {
                var progressBar = target as ProgressBar;
                if (value is int && progressBar != null)
                    progressBar.MinValue = Convert.ToSingle(value);
            });

            BindableProperty.CreateTargetLogicalProperty("MaxValue", typeof(float), typeof(ProgressBar), (target, value) =>
            {
                var progressBar = target as ProgressBar;
                if (value is int && progressBar != null)
                    progressBar.MaxValue = Convert.ToSingle(value);
            });


            //BindableProperty.CreateTargetLogicalProperty("TargetGraphic", typeof(Graphic), typeof(Selectable), (target, value) =>
            //{
            //    var selectable = target as Selectable;
            //    if (value is Graphic && selectable != null)
            //        selectable.targetGraphic = (Graphic)value;
            //});
        }

        [ContextMenu("Reload Bindable Properties")]
        public void ReloadBindableProperties()
        {
            Tyrant.GameCore.DataBinding.DataBinding.ReLoadBindableTypes();
        }

        public Tyrant.GameCore.DataBinding.DataBinding DataBinding { get; private set; }

        DataContext IDataBindingNode.DataContextSelfOrParent
        {
            get
            {
                try
                {
                    var current = transform;
                    while (current != null)
                    {
                        var dataContext = current.GetComponent<DataContextForUnity>()?.DataContext;
                        if (dataContext != null)
                            return dataContext;
                        current = current.parent;
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Tyrant.GameCore.Debug.OutputException(e);
                    return null;
                }
            }
        }
        DataContext IDataBindingNode.DataContext
        {
            get { return GetComponent<DataContextForUnity>()?.DataContext; }
        }

        public DataBindingForUnity()
        {
            DataBinding = new Tyrant.GameCore.DataBinding.DataBinding(this);
        }

        private Action<object> targetValueChanged;
        event Action<object> IDataBindingNode.TargetValueChanged
        {
            add
            {
                targetValueChanged += value;
            }

            remove
            {
                targetValueChanged -= value;
            }
        }

#pragma warning disable 0169
        [SerializeField]
        private string sourceTypeFullName;
#pragma warning restore 0169
        [SerializeField]
        private string sourcePropertyPath = "";
        [SerializeField]
        private Component targetComponent = null;
        [SerializeField]
        private string targetPropertyName = "";
        [SerializeField]
        private string converterTypeAssemblyQualifiedName = "";
        [SerializeField]
        private bool enableInverseBinding = true;

        public bool EnableInverseBinding
        {
            get { return enableInverseBinding; }
            set
            {
                enableInverseBinding = value;
                DataBinding.EnableInverseBinding = value;
            }
        }

        [SerializeField]
        private int priority;

        /// <summary>
        /// 获取或设置在同一<see cref="DataContextForUnity"/>之下的优先级。设置此属性并不会更新绑定。
        /// </summary>
        public int Priority
        {
            get { return DataBinding.Priority; }
            set { priority = value; DataBinding.Priority = value; }
        }

        void Awake()
        {
            EnableInverseBinding = enableInverseBinding;
            Priority = priority;
            if (targetComponent != null && !string.IsNullOrEmpty(targetPropertyName))
            {
                var inputField = targetComponent as InputField;
                if (inputField != null)
                    inputField.onEndEdit.AddListener((text) => { targetValueChanged?.Invoke(text); });
                else
                {
                    var toggle = targetComponent as Toggle;
                    if (toggle != null)
                        toggle.onValueChanged.AddListener((isOn) => { targetValueChanged?.Invoke(isOn); });
                }
                GameObjectDataBindingInitializer.Regist(gameObject);
            }
        }

        internal void Init()
        {
            DataBinding.Init(sourcePropertyPath, targetComponent, targetPropertyName, Type.GetType(converterTypeAssemblyQualifiedName, false), true);
        }

        //void OnEnable()
        //{
        //    DataBinding.UpdateBinding();
        //}

        void OnDestroy()
        {
            DataBinding.Dispose();
            DataBinding = null;
        }
    }

    public class ResourceNameToSpriteConverter : IDataBindingConverter
    {
        object IDataBindingConverter.ConvertTo(object sourceValue, object sourceObject, Tyrant.GameCore.DataBinding.DataBinding dataBinding)
        {
            if (sourceValue == null)
                return null;
            var resourceName = sourceValue?.ToString();
            if (string.IsNullOrEmpty(resourceName))
                return null;
            return Resources.Load(resourceName, typeof(Sprite)) as Sprite;
        }
    }

    public class ColorHexStringToColorInstanceConverter : IDataBindingConverter
    {
        object IDataBindingConverter.ConvertTo(object sourceValue, object sourceObject, GameCore.DataBinding.DataBinding dataBinding)
        {
            if (sourceValue == null)
                return Color.black;
            var colorHexString = sourceValue.ToString().TrimStart('#');
            float r = 0.0f, g = 0.0f, b = 0.0f, a = 1.0f;
            var byteArray = colorHexString.ConvertHexStringToByteArray();
            if (byteArray.Length > 0)
                r = byteArray[0] / 255.0f;
            if (byteArray.Length > 1)
                g = byteArray[1] / 255.0f;
            if (byteArray.Length > 2)
                b = byteArray[2] / 255.0f;
            if (byteArray.Length > 3)
                a = byteArray[3] / 255.0f;
            return new Color(r, g, b, a);
        }
    }

    internal class GameObjectDataBindingInitializer
    {
        private static Dictionary<GameObject, int> dic_go_count = new Dictionary<GameObject, int>(); // GameObject和其DataBindingForUnity组件的数量

        internal static void Regist(GameObject gameObject)
        {
            int count;
            dic_go_count.TryGetValue(gameObject, out count);
            dic_go_count[gameObject] = ++count;
            var dataBindings = gameObject.GetComponents<DataBindingForUnity>();
            if (count >= dataBindings.Length)
            {
                dic_go_count.Remove(gameObject);
                foreach (var dataBinding in dataBindings.OrderByDescending(i => i.Priority))
                    dataBinding.Init();
            }
        }
    }

    //public abstract class TextColorConverter : IDataBindingConverter
    //{
    //    object IDataBindingConverter.ConvertTo(object sourceValue, object sourceObject, DataBinding dataBinding)
    //    {
    //        Text text = dataBinding.TargetObject as Text;
    //        int count = 0;
    //        if (sourceValue is int)
    //            count = (int)sourceValue;
    //        if (text != null)
    //        {
    //            var compareValue = CompareValue(sourceValue, sourceObject, dataBinding);
    //            bool b = false;
    //            if (EqualCompare)
    //                b = count <= compareValue;
    //            else
    //                b = count < compareValue;
    //            text.color = b ? TestTrueColor : TestFalseColor;
    //        }
    //        return ReturnValue(sourceValue, sourceObject, dataBinding, count);
    //    }

    //    protected virtual bool EqualCompare { get { return false; } }
    //    protected abstract int CompareValue(object sourceValue, object sourceObject, DataBinding dataBinding);
    //    protected virtual Color TestTrueColor { get { return Color.red; } }
    //    protected virtual Color TestFalseColor { get { return Color.white; } }
    //    protected virtual object ReturnValue(object sourceValue, object sourceObject, DataBinding dataBinding, int count) { return count; }
    //}
}