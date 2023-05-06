using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Tyrant.ComponentsForUnity.UnityUIExtention
{
    /// <summary>
    /// 加载框
    /// </summary>
    public class LoadingBox : MonoBehaviour
    {
        [SerializeField]
        private Text content = null;

        private GameObject inputBlock = null;
        /// <summary>
        /// 异步显示加载框
        /// </summary>
        /// <param name="loadingBoxPrefab">加载框预制体</param>
        /// <param name="content">内容</param>
        /// <param name="bindingTask">一个异步任务，当此任务完成时，销毁加载框</param>
        /// <returns></returns>
        public static Task ShowAsync(LoadingBox loadingBoxPrefab, string content, Task bindingTask, Transform parent = null)
        {
            ShowInternal(loadingBoxPrefab, content, bindingTask, parent);
            return bindingTask;
        }

        public static Task<T> ShowAsync<T>(LoadingBox loadingBoxPrefab, string content, Task<T> bindingTask, Transform parent = null)
        {
            ShowInternal(loadingBoxPrefab, content, bindingTask, parent);
            return bindingTask;
        }

        private static void ShowInternal(LoadingBox loadingBoxPrefab, string content, Task bindingTask, Transform parent)
        {
            if (bindingTask == null)
                throw new ArgumentNullException(nameof(bindingTask));

            if (!Application.isPlaying)
                return;

            var loadingBoxObj = Instantiate(loadingBoxPrefab, parent ?? (FindObjectOfType<Canvas>()?.transform));
            var loadingBox = loadingBoxObj.GetComponent<LoadingBox>();
            if (loadingBox.content != null)
                loadingBox.content.text = content;
            loadingBox.inputBlock = ModelMessageBox.CreateInputBlock(loadingBoxObj.transform);
            loadingBoxObj.transform.SetAsLastSibling();
            if (bindingTask.IsCompleted)
            {
                DestoryGameObjects();
            }
            else
            {
                bindingTask.GetAwaiter().OnCompleted(DestoryGameObjects);
            }

            void DestoryGameObjects()
            {
                if (loadingBox != null)
                {
                    if (loadingBox.gameObject != null)
                        Destroy(loadingBox.gameObject);
                    if (loadingBox.inputBlock != null)
                        Destroy(loadingBox.inputBlock);
                }
            }
        }
    }
}