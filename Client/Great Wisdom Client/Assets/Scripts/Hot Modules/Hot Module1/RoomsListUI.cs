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
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;

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
    [SerializeField]
    private Button btn_JoinRoom;

    private ToggleGroup toggleGroup;

    private int currentPage = 0;
    private void Awake()
    {
        toggleGroup = content.GetComponent<ToggleGroup>();
        if (toggleGroup == null)
            toggleGroup = content.gameObject.AddComponent<ToggleGroup>();
        btn_JoinRoom.interactable = toggleGroup.AnyTogglesOn();
        btn_JoinRoom.onClick.AddListener(JoinRoom);

        btn_PrevPage.interactable = false;
        btn_PrevPage.onClick.AddListener(() => GetList(--currentPage));
        btn_NextPage.interactable = false;
        btn_NextPage.onClick.AddListener(() => GetList(++currentPage));

        GetList(currentPage);
    }

    private RoomInfo curSelectedRoomInfo;
    private async void GetList(int pageIndex)
    {
        var config = await Config.Instance;
        var result = await LoadingBox.ShowAsync(config.LoadingBoxPrefab, "正在获取房间列表", new C2S_GetRoomsList { PageIndex = pageIndex }.C2S_CallAsync());
        if (result.Result != RPCError.OK)
            await ModelMessageBox.ShowAsync(config.MessageBoxPrefab, "", $"获取房间列表失败，错误：{result.Result}", MessageBoxButton.OK);
        else
        {
            currentPage = result.CurrentPage;
            btn_PrevPage.interactable = currentPage > 0;
            btn_NextPage.interactable = currentPage < result.TotalPages - 1;
            foreach (Transform child in content.transform)
            {
                if (child != null)
                    Destroy(child.gameObject);
            }
            foreach (var roomInfo in result.RoomInfos)
            {
                var go = Instantiate(roomInfoPrefab);
                var toggle = go.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((isSelected) =>
                {
                    if (isSelected)
                    {
                        btn_JoinRoom.interactable = true;
                        curSelectedRoomInfo = roomInfo;
                    }
                });
                toggle.group = toggleGroup;
                go.transform.SetParent(content, false);
                var text = go.GetComponentInChildren<Text>();
                text.text = $"地图:{roomInfo.MapIndex} {(roomInfo.NeedPassword ? "有" : "无")}密码 人数:{roomInfo.CurrentPlayers} 创建者:{roomInfo.Creater} 描述:{roomInfo.Desc}";
            }
        }
    }

    private async void JoinRoom()
    {
        if (curSelectedRoomInfo == null)
        {
            UnityEngine.Debug.LogError("join room button clicked, but current selected roomInfo is null");
            return;
        }

        var config = await Config.Instance;
        var result = await LoadingBox.ShowAsync(config.LoadingBoxPrefab, "正在加入房间",
            Client.MainClient.EnterMapAsync(0, new EnterRoomParam { RoomInstanceId = curSelectedRoomInfo.InstanceId }));
        if (result.Result != RPCError.OK)
        {
            string msg;
            switch (result.Result)
            {
                case RPCError.AlReadyInMap:
                    msg = "已在该房间内";
                    break;
                default:
                    msg = $"加入房间失败，错误：{result.Result}";
                    break;
            }
            await ModelMessageBox.ShowAsync(config.MessageBoxPrefab, "", msg, MessageBoxButton.OK);
        }
        else
            UnityEngine.Debug.Log("join room succeed;");
    }
}