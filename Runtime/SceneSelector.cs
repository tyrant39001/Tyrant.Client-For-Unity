using System;
using UnityEngine;

[System.Serializable]
public class SceneSelector
{
    [SerializeField]
    private string scenePath = null;

    public string ScenePath => scenePath;
    public string SceneName => System.IO.Path.GetFileNameWithoutExtension(scenePath);

    public void LoadScene()
    {
        if (scenePath != null)
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
    }

    public AsyncOperation LoadSceneAsync()
    {
        if (scenePath != null)
            return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName);
        return null;
    }

    public static bool operator ==(SceneSelector a, SceneSelector b)
    {
        var aIsNull = ReferenceEquals(a, null);
        var bIsNull = ReferenceEquals(b, null);
        if (aIsNull && bIsNull)
            return true;
        else if (!aIsNull && !bIsNull)
            return a.scenePath == b.scenePath;
        else
        {
            if (aIsNull)
                return string.IsNullOrWhiteSpace(b.scenePath);
            else if (bIsNull)
                return string.IsNullOrWhiteSpace(a.scenePath);
            else
                return false;
        }
    }

    public static bool operator !=(SceneSelector a, SceneSelector b) => !(a == b);
    public override bool Equals(object obj) => (obj is SceneSelector sceneSelector) && this == sceneSelector;
    public override int GetHashCode() => string.IsNullOrWhiteSpace(scenePath) ? "".GetHashCode() : scenePath.GetHashCode();
}