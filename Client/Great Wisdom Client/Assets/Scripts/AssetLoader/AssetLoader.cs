using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement;
using UnityEngine.SceneManagement;

public static class AssetLoader
{
    //public static async Task LoadContentCatalog(string key)
    //{
    //    await Addressables.InitializeAsync().Task;
    //    Addressables.ClearResourceLocators();
    //    await Addressables.LoadContentCatalogAsync(key, false, "").Task;
    //}

    public static Task<TObject> LoadAsset<TObject>(string key) where TObject : Object
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE_IN_EDITOR
        return Task.FromResult(AssetDatabase.LoadAssetAtPath<TObject>(key));
#else
        return Addressables.LoadAssetAsync<TObject>(key).Task;
#endif
    }

    public static Task<TObject[]> LoadAssets<TObject>(string key) where TObject : Object
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE_IN_EDITOR
        return Task.FromResult(AssetDatabase.LoadAllAssetsAtPath(key).Where(o => o is TObject).Cast<TObject>().ToArray());
#else
        return Addressables.LoadAssetAsync<TObject[]>(key).Task;
#endif
    }

    public static async Task LoadScene(string key, LoadSceneParameters loadSceneParameters)
    {
        var asyncOperation = await LoadSceneAsyncOperation(key, loadSceneParameters);
        await AsyncOperationFinishTask(asyncOperation);
    }

    public static Task AsyncOperationFinishTask(AsyncOperation asyncOperation)
    {
        if (asyncOperation.isDone)
            return Task.CompletedTask;
        var taskCompletionSource = new TaskCompletionSource<object>();
        asyncOperation.completed += (obj) => taskCompletionSource.SetResult(null);
        return taskCompletionSource.Task;
    }

    public static
#if UNITY_EDITOR && USE_ASSETBUNDLE_IN_EDITOR
        async
#elif !UNITY_EDITOR
        async
#endif
        Task<AsyncOperation> LoadSceneAsyncOperation(string key, LoadSceneParameters loadSceneParameters)
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE_IN_EDITOR
        return Task.FromResult(UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(key, loadSceneParameters));
#else
        var sceneInstance = await Addressables.LoadSceneAsync(key, loadSceneParameters.loadSceneMode).Task;
        return sceneInstance.ActivateAsync();
#endif
    }

    public static void Release(Object obj)
    {
#if UNITY_EDITOR && !USE_ASSETBUNDLE_IN_EDITOR

#else
        Addressables.Release(obj);
#endif
    }
}