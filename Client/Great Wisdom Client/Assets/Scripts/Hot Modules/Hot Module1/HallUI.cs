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

public class HallUI : MonoBehaviour
{
    [SerializeField]
    private Text nickNameText;

    private void Awake()
    {
        nickNameText.text = LoginUI.AccountName;
    }

    public void CreateRoom()
    {

    }

    public void JoinRoom()
    {

    }
}