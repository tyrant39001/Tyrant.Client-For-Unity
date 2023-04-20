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

public class LoginUI : MonoBehaviour
{
    public static string AccountName { get; private set; }

    [SerializeField]
    private InputField if_UserName = null;
    [SerializeField]
    private InputField if_Password = null;
    [SerializeField]
    private Button btn_Login = null;
    [SerializeField, Tooltip("登录成功后跳转到的场景")]
    private SceneSelector SceneJumpTo = null;

    ServerGroupData serverGroupData;
    void Awake()
    {
        if (Client.MainClient == null)
            Client.SetMainClient(new Client());
        Client.MainClient.Disconnected -= Instance_Disconnected;
        Client.MainClient.Disconnected += Instance_Disconnected;

        if_UserName.text = PlayerPrefs.GetString("UserName", "");
        if_Password.text = PlayerPrefs.GetString("Password", "");

        CheckCanLogin();
        if_UserName.onValueChanged.AddListener(CheckCanLogin);
        if_Password.onValueChanged.AddListener(CheckCanLogin);
        btn_Login.onClick.AddListener(Login);
    }


    private async static void Instance_Disconnected(SocketOperation arg1, SocketError arg2, EClientSocketDisconnectType arg3)
    {
        var config = await Config.Instance;
        await ModelMessageBox.ShowAsync(config.MessageBoxPrefab, "", "与服务器断开连接", MessageBoxButton.OK);
        var loginScene = config.LoginScene;
        _ = AssetLoader.LoadScene(loginScene.ScenePath, new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
    }

    private void CheckCanLogin(string text = null)
    {
        btn_Login.interactable = (!string.IsNullOrEmpty(if_UserName.text)) && (!string.IsNullOrEmpty(if_Password.text));
    }

    private async void Login()
    {
        var config = await Config.Instance;

        btn_Login.interactable = false;
        try
        {
            var result = (await LoadingBox.ShowAsync(config.LoadingBoxPrefab, "登录中", LoginTask()));
            if (result != RPCError.OK)
            {
                string msg;
                switch (result)
                {
                    case RPCError.InvalidConnectToken:
                        msg = "无效连接票据";
                        break;
                    case RPCError.Timeout:
                        msg = "请求超时";
                        break;
                    case RPCError.AccountIsOnline:
                        msg = "该账号已经在线";
                        break;
                    case RPCError.AccountNotFound:
                        msg = "账号不存在或密码错误";
                        break;
                    case RPCError.InternalError:
                        msg = "服务器错误";
                        break;
                    case RPCError.NameFormatError:
                        msg = "用户名至少要有4个字符";
                        break;
                    case (RPCError)2:
                        msg = "密码至少要有4个字符";
                        break;
                    case (RPCError)3:
                        msg = "用户名已存在";
                        break;
                    case (RPCError)4:
                        msg = "获取服务器组失败";
                        break;
                    default:
                        msg = result.ToString();
                        break;
                }

                await ModelMessageBox.ShowAsync(config.MessageBoxPrefab, "", msg, MessageBoxButton.OK);
            }
        }
        catch (Exception e)
        {
            var socketException = GetSocketException(e);
            if (socketException != null)
            {
                await ModelMessageBox.ShowAsync(config.MessageBoxPrefab, "", $"无法连接服务器，错误：{socketException.ErrorCode}", MessageBoxButton.OK);
            }
            else
                Tyrant.GameCore.Debug.OutputException(e);
        }
        finally
        {
            if (btn_Login != null)
                btn_Login.interactable = true;
        }
    }

    private async Task<RPCError> LoginTask()
    {
        // 与登录服务器没断开，不需要再走登录逻辑
        if (Client.MainClient.IsLoginConnected && serverGroupData != null)
        {
            return await EnterServerGroup(null);
        }
        else
        {
            var config = await Config.Instance;
            var myLoginAccountParam = new LoginAccountParam { Name = if_UserName.text, Password = if_Password.text };
            UnityEngine.Debug.Log($"Login to {config.ServerHost}:{config.ServerPort}, user:{if_UserName.text}, password:{if_Password.text}");
            var tuple = await Client.MainClient.LoginAccount(config.ServerHost, config.ServerPort, myLoginAccountParam, 10000);
            var result = tuple.Item1;
            UnityEngine.Debug.Log($"Login account result:{result}");
            if (result == RPCError.OK)
            {
                if (tuple.Item2 is ReturnDataAfterLogin returnDataAfterLogin)
                    AccountName = returnDataAfterLogin.Name;
                else
                    UnityEngine.Debug.LogError("login return data is not ReturnDataAfterLogin");

                var serverGroupList = await Client.MainClient.GetServerList();
                if (serverGroupList.Count > 0)
                {
                    serverGroupData = serverGroupList[0];
                    return await EnterServerGroup(myLoginAccountParam);
                }
                else
                    return (RPCError)4;
            }
            else
                return result;
        }
    }

    private async Task<RPCError> EnterServerGroup(LoginAccountParam myLoginAccountParam)
    {
        var result2 = await Client.MainClient.EnterServerGroup(serverGroupData);
        if (result2.Item1 == RPCError.OK)
        {
            if (myLoginAccountParam != null)
            {
                PlayerPrefs.SetString("UserName", myLoginAccountParam.Name);
                PlayerPrefs.SetString("Password", myLoginAccountParam.Password);
            }
            if (SceneJumpTo != null)
                await AssetLoader.LoadScene(SceneJumpTo.ScenePath, new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            return RPCError.OK;
        }
        else
            return result2.Item1;
    }

    private SocketException GetSocketException(Exception e)
    {
        if (e == null)
            return null;
        var result = e as SocketException;
        if (result != null)
            return result;
        return GetSocketException(e.InnerException);
    }
}