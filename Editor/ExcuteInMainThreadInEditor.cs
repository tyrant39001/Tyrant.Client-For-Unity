using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;
using UnityEditor.UI;
using UnityEngine.EventSystems;
using Tyrant.ComponentsForUnity.UnityUIExtention;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine.SceneManagement;
using Tyrant.ComponentsForUnity;

namespace Tyrant.ComponentsEditorForUnity
{
    public class ExcuteInMainThreadInEditor
    {
        public static Thread MainThread { get; private set; }
        private static ConcurrentQueue<IExcutionItem> excutionQuene = new ConcurrentQueue<IExcutionItem>();
        private static ConcurrentDictionary<Scene, TaskCompletionSource<object>> sceneSet = new ConcurrentDictionary<Scene, TaskCompletionSource<object>>();
        private static ConcurrentDictionary<int, ConcurrentQueue<TaskCompletionSource<object>>> nextFrameSet = new ConcurrentDictionary<int, ConcurrentQueue<TaskCompletionSource<object>>>();

        [InitializeOnLoadMethod]
        private static void Init()
        {
            MainThread = Thread.CurrentThread;
            //NetworkClient.HandleAllMessagesPerFrame = true;
            //NetworkClient.DebugModeInEditor = true;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        public static Task Invoke(Action action)
        {
            if (MainThread == Thread.CurrentThread)
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                return Task.CompletedTask;
            }

            var item = new ExcutionItem(action);
            excutionQuene.Enqueue(item);
            return item.TaskCompletionSource.Task;
        }

        public static Task<T> Invoke<T>(Func<T> func)
        {
            if (MainThread == Thread.CurrentThread)
            {
                try
                {
                    return Task.FromResult(func());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var item = new ExcutionItem<T>(func);
            excutionQuene.Enqueue(item);
            return item.TaskCompletionSource.Task;
        }

        public static Task AwaitSceneLoaded(Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));
            var taskCompletionSource = new TaskCompletionSource<object>();
            if (!sceneSet.TryAdd(scene, taskCompletionSource))
                throw new InvalidOperationException($"{nameof(ExcuteInMainThreadInEditor)}.{nameof(AwaitSceneLoaded)}:the scene '{scene.name}' already waiting");
            return taskCompletionSource.Task;
        }

        public static Task WaitUntilNextFrame()
        {
            var quene = nextFrameSet.GetOrAdd(Time.frameCount + 1, (key) => new ConcurrentQueue<TaskCompletionSource<object>>());
            var taskCompletionSource = new TaskCompletionSource<object>();
            quene.Enqueue(taskCompletionSource);
            return taskCompletionSource.Task;
        }

        private static void Update()
        {
            var count = excutionQuene.Count;
            for (int i = 0; i < count; ++i)
            {
                if (excutionQuene.TryDequeue(out var item))
                    item.Invoke();
            }

            if (nextFrameSet.TryGetValue(Time.frameCount, out var quene))
            {
                while (quene.TryDequeue(out var taskCompletionSource))
                {
                    try
                    {
                        taskCompletionSource.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            foreach (var scene in sceneSet)
            {
                if (scene.Key.isLoaded)
                {
                    try
                    {
                        scene.Value.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                sceneSet.TryRemove(scene.Key, out _);
            }

            //NetworkClient.Update();
        }
    }

    internal interface IExcutionItem
    {
        void Invoke();
    }

    internal struct ExcutionItem<T> : IExcutionItem
    {
        internal Func<T> Func;
        internal ExcutionItem(Func<T> func)
        {
            Func = func;
            TaskCompletionSource = new TaskCompletionSource<T>();
        }
        internal TaskCompletionSource<T> TaskCompletionSource { get; private set; }
        void IExcutionItem.Invoke()
        {
            T result;
            try
            {
                result = Func.Invoke();
                TaskCompletionSource?.SetResult(result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                TaskCompletionSource?.SetException(e);
            }
        }
    }

    internal struct ExcutionItem : IExcutionItem
    {
        private Action Action;
        internal ExcutionItem(Action action)
        {
            Action = action;
            TaskCompletionSource = new TaskCompletionSource<object>();
        }
        internal TaskCompletionSource<object> TaskCompletionSource { get; private set; }
        void IExcutionItem.Invoke()
        {
            try
            {
                Action.Invoke();
                TaskCompletionSource?.SetResult(null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                TaskCompletionSource?.SetException(e);
            }
        }
    }
}