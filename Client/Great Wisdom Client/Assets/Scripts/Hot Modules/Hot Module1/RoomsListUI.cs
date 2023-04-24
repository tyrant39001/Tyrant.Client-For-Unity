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
using UnityEngine.UI;

public class RoomsListUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform content;
    [SerializeField]
    private GameObject roomInfoPrefab;
    [SerializeField]
    private Button btn_PrevPage;
    [SerializeField]
    private Button btn_NextPage;

    private int currentPage = 0;
    private void Awake()
    {
        GetList(currentPage);
    }

    private async void GetList(int pageIndex)
    {
        var config = await Config.Instance;
        var result = await LoadingBox.ShowAsync(config.LoadingBoxPrefab, "正在获取房间列表", new C2S_GetRoomsList { PageIndex = pageIndex }.C2S_CallAsync());
        if (result.Result != RPCError.OK)
            await ModelMessageBox.ShowAsync(config.MessageBoxPrefab, "", $"获取房间列表失败，错误：{result.Result}", MessageBoxButton.OK);
        else
        {
            foreach (var roomInfo in result.RoomInfos)
            {
                var go = Instantiate(roomInfoPrefab);
                go.transform.parent = content;
                var text = go.GetComponentInChildren<Text>();
                text.text = $"地图:{roomInfo.MapIndex} {(roomInfo.NeedPassword ? "有" : "无")}密码 人数:{roomInfo.CurrentPlayers} 创建者:{roomInfo.Creater} {roomInfo.Desc}";
            }
        }
    }
}