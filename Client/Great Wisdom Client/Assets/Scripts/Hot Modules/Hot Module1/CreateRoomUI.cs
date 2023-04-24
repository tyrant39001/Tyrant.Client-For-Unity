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

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField]
    private Dropdown dp_Map;
    [SerializeField]
    private InputField if_Password;
    [SerializeField]
    private InputField if_desc;
    [SerializeField]
    private Toggle tg_Public;
    [SerializeField]
    private Toggle tg_Private;
    [SerializeField]
    private Button btn_CreateRoom;

    private void Awake()
    {
        btn_CreateRoom.onClick.AddListener(CreateRoom);
    }

    private async void CreateRoom()
    {
        btn_CreateRoom.interactable = false;
        var config = await Config.Instance;
        var result = await LoadingBox.ShowAsync(config.LoadingBoxPrefab, "正在创建房间",
            Client.MainClient.EnterMapAsync(0, new CreateRoomParam
            {
                MapIndex = int.Parse(dp_Map.options[dp_Map.value].text),
                Password = if_Password.text,
                Desc = if_desc.text,
                IsPublic = tg_Public.isOn,
                Creater = LoginUI.AccountName,
            }));
        if (result.Result != RPCError.OK)
            await ModelMessageBox.ShowAsync(config.MessageBoxPrefab, "", $"创建房间失败，错误：{result.Result}", MessageBoxButton.OK);
        else
            UnityEngine.Debug.Log("create room succeed;");
        btn_CreateRoom.interactable = true;
    }
}