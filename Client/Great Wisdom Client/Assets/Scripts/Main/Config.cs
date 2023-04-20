using System.Threading.Tasks;
using Tyrant.ComponentsForUnity.UnityUIExtention;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class Config : ScriptableObject
{
    [Tooltip("服务器的Ip地址或域名")]
    public string ServerHost = "";
    [Tooltip("服务器的端口号")]
    public ushort ServerPort;
    public ModelMessageBox MessageBoxPrefab;
    public LoadingBox LoadingBoxPrefab;
    public SceneSelector LoginScene;

    private static Task<Config> instance = null;
    public static Task<Config> Instance
    {
        get
        {
            if (instance != null)
                return instance;
            instance = AssetLoader.LoadAsset<Config>("Assets/Config/Config.asset");
            return instance;
        }
    }
}