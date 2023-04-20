using Great_Wisdom_Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tyrant.ComponentsForUnity.UnityUIExtention;
using Tyrant.GameCore;
using Tyrant.GameCore.Data;
using Tyrant.GameCore.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickToLoadScene : MonoBehaviour
{
    [SerializeField]
    private SceneSelector scene;
    [SerializeField]
    private LoadSceneMode loadSceneMode;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                if (scene != null)
                    _ = AssetLoader.LoadScene(scene.ScenePath, new LoadSceneParameters(loadSceneMode));
            });
        }
    }
}