using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    /// <summary>
    /// 模式消息框
    /// </summary>
    public class ModelMessageBox : MonoBehaviour
    {
        //[SerializeField, Tooltip("所有其他控件的父级，其所在的GameObject初始设置为非激活状态")]
        //private Image backImg = null;
        [SerializeField]
        private Text titleText = null;
        [SerializeField]
        private Text contentText = null;
        [SerializeField]
        private Button leftButton = null;
        [SerializeField]
        private Button middleButton = null;
        [SerializeField]
        private Button rightButton = null;
        [SerializeField]
        private Text leftButtonText = null;
        [SerializeField]
        private Text middleButtonText = null;
        [SerializeField]
        private Text rightButtonText = null;
        [SerializeField]
        public string yesString = "Yes";
        [SerializeField]
        public string noString = "No";
        [SerializeField]
        public string okString = "OK";
        [SerializeField]
        public string cancelString = "Cancel";

        //private static ModelMessageBox instance = null;
        //private static ModelMessageBox Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //            instance = FindObjectOfType<ModelMessageBox>();
        //        return instance;
        //    }
        //}

        /// <summary>
        /// 即将显示时的事件，第一个参数为标题，第二个参数为内容，第三个参数为按钮组合枚举
        /// </summary>
        public static event Action<string, string, MessageBoxButton> Showing;

        private void Awake()
        {
            leftButton?.onClick.AddListener(OnLeftButtonClick);
            middleButton?.onClick.AddListener(OnMiddleButtonClick);
            rightButton?.onClick.AddListener(OnRightButtonClick);
        }

        private Buttons buttonClicked = Buttons.None;
        private void OnLeftButtonClick()
        {
            //Hide();
            buttonClicked = Buttons.Left;
            clickCallBack?.Invoke();
        }

        private void OnMiddleButtonClick()
        {
            //Hide();
            buttonClicked = Buttons.Middle;
            clickCallBack?.Invoke();
        }

        private void OnRightButtonClick()
        {
            //Hide();
            buttonClicked = Buttons.Right;
            clickCallBack?.Invoke();
        }

        private Action clickCallBack = null;
        private GameObject inputBlock = null;
        private void ShowInternal(string title, string content, MessageBoxButton button, Action callBack = null)
        {
            inputBlock = CreateInputBlock(transform);

            transform.SetAsLastSibling();
            if (titleText != null)
                titleText.text = title;
            if (contentText != null)
                contentText.text = content;
            switch (button)
            {
                case MessageBoxButton.OK:
                    if (leftButton != null)
                        leftButton.gameObject.SetActive(true);
                    if (leftButtonText != null)
                        leftButtonText.text = okString;
                    if (middleButton != null)
                        middleButton.gameObject.SetActive(false);
                    if (rightButton != null)
                        rightButton.gameObject.SetActive(false);

                    LayoutBtn(leftButton, 0.25f, 0.75f);
                    break;
                case MessageBoxButton.OKCancel:
                    if (leftButton != null)
                        leftButton.gameObject.SetActive(true);
                    if (leftButtonText != null)
                        leftButtonText.text = okString;
                    if (middleButton != null)
                        middleButton.gameObject.SetActive(false);
                    if (rightButton != null)
                        rightButton.gameObject.SetActive(true);
                    if (rightButtonText != null)
                        rightButtonText.text = cancelString;

                    TowBtnLayout();
                    break;
                case MessageBoxButton.YesNo:
                    leftButton?.gameObject.SetActive(true);
                    if (leftButtonText != null)
                        leftButtonText.text = yesString;
                    middleButton?.gameObject.SetActive(false);
                    rightButton?.gameObject.SetActive(true);
                    if (rightButtonText != null)
                        rightButtonText.text = noString;

                    TowBtnLayout();
                    break;
                case MessageBoxButton.YesNoCancel:
                    leftButton?.gameObject.SetActive(true);
                    if (leftButtonText != null)
                        leftButtonText.text = yesString;
                    middleButton?.gameObject.SetActive(true);
                    if (middleButtonText != null)
                        middleButtonText.text = noString;
                    rightButton?.gameObject.SetActive(true);
                    if (rightButtonText != null)
                        rightButtonText.text = cancelString;

                    LayoutBtn(leftButton, 0.08f, 0.32f);
                    LayoutBtn(middleButton, 0.38f, 0.62f);
                    LayoutBtn(rightButton, 0.68f, 0.92f);
                    break;
            }

            clickCallBack = callBack;
            try
            {
                Showing?.Invoke(title, content, button);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            void TowBtnLayout()
            {
                LayoutBtn(leftButton, 0.143f, 0.429f);
                LayoutBtn(rightButton, 0.572f, 0.858f);
            }

            void LayoutBtn(Button btn, float anchorMinX, float anchorMaxX)
            {
                var btnTranform = btn.GetComponent<RectTransform>();
                if (btnTranform != null)
                {
                    btnTranform.pivot = new Vector2(0.5f, 0.5f);
                    btnTranform.anchorMin = new Vector2(anchorMinX, btnTranform.anchorMin.y);
                    btnTranform.anchorMax = new Vector2(anchorMaxX, btnTranform.anchorMax.y);
                    btnTranform.offsetMin = new Vector2(0f, btnTranform.offsetMin.y);
                    btnTranform.offsetMax = new Vector2(0f, btnTranform.offsetMax.y);
                }
            }
        }

        /// <summary>
        /// 从预制体创建，异步显示模式对话框
        /// </summary>
        /// <param name="messageBoxPrefab">模式对话框预制体</param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="button">按钮组合</param>
        /// <returns></returns>
        public static Task<MessageBoxResult> ShowAsync(ModelMessageBox messageBoxPrefab, string title, string content, MessageBoxButton button, Transform parent = null)
        {
            if (messageBoxPrefab == null)
                throw new ArgumentNullException(nameof(messageBoxPrefab));

            if (!Application.isPlaying)
                return Task.FromResult(MessageBoxResult.None);

            var messageBoxObj = Instantiate(messageBoxPrefab.gameObject, parent ?? (FindObjectOfType<Canvas>()?.transform));
            messageBoxObj.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            var messageBox = messageBoxObj.GetComponent<ModelMessageBox>();
            return ShowAsyncInternal(messageBox, title, content, button, true);
        }

        ///// <summary>
        ///// 直接使用场景中的对象，异步显示模式对话框
        ///// </summary>
        ///// <param name="messageBox">模式对话框组件</param>
        ///// <param name="title">标题</param>
        ///// <param name="content">内容</param>
        ///// <param name="button">按钮组合</param>
        ///// <returns></returns>
        //public static Task<MessageBoxResult> ShowAsync(ModelMessageBox messageBox, string title, string content, MessageBoxButton button)
        //{
        //    messageBox.gameObject.SetActive(true);
        //    return ShowAsyncInternal(messageBox, title, content, button, false);
        //}

        private static Task<MessageBoxResult> ShowAsyncInternal(ModelMessageBox messageBox, string title, string content, MessageBoxButton button, bool createFromPrefab)
        {
            var taskCompletionSource = new TaskCompletionSource<MessageBoxResult>();
            messageBox.ShowInternal(title, content, button, new Action(() =>
            {
                taskCompletionSource.SetResult(messageBox.GetResult(button));
                if (createFromPrefab)
                    Destroy(messageBox.gameObject);
                else
                    messageBox.gameObject.SetActive(false);
                Destroy(messageBox.inputBlock);
            }));
            return taskCompletionSource.Task;
        }

        private MessageBoxResult GetResult(MessageBoxButton button)
        {
            switch (buttonClicked)
            {
                case Buttons.Left:
                    switch (button)
                    {
                        case MessageBoxButton.OK:
                        case MessageBoxButton.OKCancel:
                            return MessageBoxResult.OK;
                        case MessageBoxButton.YesNo:
                        case MessageBoxButton.YesNoCancel:
                            return MessageBoxResult.OK;
                    }
                    break;
                case Buttons.Middle:
                    switch (button)
                    {
                        case MessageBoxButton.YesNoCancel:
                            return MessageBoxResult.No;
                    }
                    break;
                case Buttons.Right:
                    switch (button)
                    {
                        case MessageBoxButton.OKCancel:
                            return MessageBoxResult.Cancel;
                        case MessageBoxButton.YesNo:
                            return MessageBoxResult.No;
                        case MessageBoxButton.YesNoCancel:
                            return MessageBoxResult.Cancel;
                    }                  
                    break;
            }
            return MessageBoxResult.None;
        }

        private enum Buttons
        {
            None,
            Left,
            Middle,
            Right,
        }

        internal static GameObject CreateInputBlock(Transform currentTransform)
        {
            GameObject result = null;
            // 获取Canvas
            Canvas canvas = null;
            currentTransform = currentTransform.parent;
            while (currentTransform != null)
            {
                canvas = currentTransform.GetComponent<Canvas>();
                if (canvas != null)
                    break;
                currentTransform = currentTransform.parent;
            }
            if (canvas != null) // 添加Image组件阻挡其他输入
            {
                result = new GameObject("Message Box Input Block");
                result.transform.SetParent(canvas.transform, false);
                result.transform.SetAsLastSibling();
                var image = result.AddComponent<Image>();
                image.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                var rectTransform = result.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            return result;
        }
    }

    /// <summary>
    /// 消息框返回结果
    /// </summary>
    public enum MessageBoxResult
    {
        None,
        Yes,
        No,
        OK,
        Cancel,
    }

    /// <summary>
    /// 显示在消息框上的按钮组合
    /// </summary>
    public enum MessageBoxButton
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel,
    }
}