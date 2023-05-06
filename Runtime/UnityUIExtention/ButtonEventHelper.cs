using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    /// <summary>
    /// UnityEngine.UI.Button响应点击事件的简单方法。用法：“button.GetClickedEventHelper().Clicked += ”，然后利用vs或monodevelop等IDE自动生成代码。其中button为UnityEngine.UI.Button的实例，需要引用命名空间Tyrant.ComponentsForUnity。事件ClickedForCoroutine用法相同，只不过此事件在协程中执行。
    /// </summary>
    public class ButtonEventHelper : MonoBehaviour
    {
        public Button Button { get; private set; }
        public event Action<Button> Clicked;
        public event Func<Button, IEnumerator> ClickedForCoroutine;

        void Awake()
        {
            Button = transform.GetComponent<Button>();
            if (Button != null)
                Button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (Clicked != null)
            {
                try
                {
                    foreach (Action<Button> d in Clicked.GetInvocationList())
                        d(Button);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            if (ClickedForCoroutine != null)
            {
                foreach (Func<Button, IEnumerator> d in ClickedForCoroutine.GetInvocationList())
                {
                    try
                    {
                        StartCoroutine(d(Button));
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }

    public static class ButtonExtention
    {
        public static ButtonEventHelper GetClickedEventHelper(this Button button)
        {
            if (button == null)
                return null;
            return GetClickedEventHelper(button.gameObject);
        }

        public static ButtonEventHelper GetClickedEventHelper(this GameObject obj)
        {
            if (obj == null)
                return null;
            var buttonEventHelper = obj.GetComponent<ButtonEventHelper>();
            if (buttonEventHelper == null)
                buttonEventHelper = obj.AddComponent<ButtonEventHelper>();
            return buttonEventHelper;
        }
    }
}