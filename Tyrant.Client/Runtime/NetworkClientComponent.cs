using UnityEngine;
using System.Collections;
using System;
using Tyrant.GameCore;
using System.Text;
using Tyrant.Core;
using Client = Tyrant.GameCore.Net.Client;
using Debug = Tyrant.GameCore.Debug;
using System.Reflection;
using Tyrant.GameCore.Net;
using System.Collections.Generic;
using UnityEngine.LowLevel;
using System.Collections.Concurrent;
using System.Threading;

namespace Tyrant.ComponentsForUnity
{
    //public class NetworkClientComponent : MonoBehaviour
    //{
    //    public bool HandleAllMessagesPerFrame = true;
    //    public bool DebugModeInEditor = true;

    //    private static NetworkClientComponent instance = null;

    //    void Awake()
    //    {
    //        if (instance == null)
    //        {
    //            DontDestroyOnLoad(gameObject);
    //            instance = this;
    //            NetworkClient.DebugModeInEditor = DebugModeInEditor;
    //            NetworkClient.HandleAllMessagesPerFrame = HandleAllMessagesPerFrame;
    //        }
    //        else
    //        {
    //            Debug.OutputWarning("there is already a ClientComponent, destory this one");
    //            Destroy(this);
    //        }
    //    }

    //    void OnDestroy()
    //    {
    //        if (instance == this)
    //            instance = null;
    //    }

    //    void Update()
    //    {
    //        NetworkClient.Update();
    //    }
    //}

    public class ExcuteInFixedUpdateAttribute : Attribute { }

    public static class NetworkClient
    {
        private static ConcurrentQueue<Action> networkMessageByUpdateAction = new ConcurrentQueue<Action>();
        public static int UpdateMessageQueueCount => networkMessageByUpdateAction.Count;

        private static ConcurrentQueue<Action> networkMessageByFixedUpdateAction = new ConcurrentQueue<Action>();
        public static int FixedMessageQueueCount => networkMessageByFixedUpdateAction.Count;

        public static bool HandleAllMessagesPerFrame { get; set; } = true;
        private static bool debugModeInEditor = true;
        public static bool DebugModeInEditor
        {
            get => debugModeInEditor;
            set
            {
                debugModeInEditor = value;
                if (value && Application.isEditor)
                    RPCExecManager.IsDebug = true;
            }
        }

        [RuntimeInitializeOnLoadMethod()]
        static void IntializeNetworkClient()
        {
            Debug.Output("NetworkClient.IntializeNetworkClient");
            DebugModeInEditor = true;

            if (Client.MainClient == null)
                Client.SetMainClient(new Client());
            Client.ReceiveNetworkMessageHandler += Instance_ReceiveNetworkMessageHandler;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            SetCustomPlayerLoop(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        static void SetCustomPlayerLoop(ref PlayerLoopSystem playerLoop)
        {
            PlayerLoopSystem beforeUpdate = new PlayerLoopSystem()
            {
                type = typeof(NetworkClient),
                updateDelegate = () =>
                {
                    if (Client.MainClient != null)
                        Update();
                }
            };

            PlayerLoopSystem beforeFixedUpdate = new PlayerLoopSystem()
            {
                type = typeof(NetworkClient),
                updateDelegate = () =>
                {
                    if (Client.MainClient != null)
                        FixedUpdate();
                }
            };

            // before update
            int sysIndex = Array.FindIndex(playerLoop.subSystemList, (s) => s.type.Name == "Update");
            PlayerLoopSystem updateSystem = playerLoop.subSystemList[sysIndex];
            var updateSubsystemList = new List<PlayerLoopSystem>(updateSystem.subSystemList);
            updateSubsystemList.Insert(0, beforeUpdate); // Update() before
            updateSystem.subSystemList = updateSubsystemList.ToArray();
            playerLoop.subSystemList[sysIndex] = updateSystem;

            // before fixed udpate
            sysIndex = Array.FindIndex(playerLoop.subSystemList, (s) => s.type.Name == "FixedUpdate");
            PlayerLoopSystem fixedUpdateSystem = playerLoop.subSystemList[sysIndex];
            var fixedUpdateSubsystemList = new List<PlayerLoopSystem>(fixedUpdateSystem.subSystemList);
            fixedUpdateSubsystemList.Insert(0, beforeFixedUpdate);
            fixedUpdateSystem.subSystemList = fixedUpdateSubsystemList.ToArray();
            playerLoop.subSystemList[sysIndex] = fixedUpdateSystem;
        }

        private static void Instance_ReceiveNetworkMessageHandler(Action obj, RPCSerialize serialize)
        {
            if (serialize != null && serialize.GetType().GetCustomAttribute<ExcuteInFixedUpdateAttribute>() != null)
                networkMessageByFixedUpdateAction.Enqueue(obj);
            else
                networkMessageByUpdateAction.Enqueue(obj);
        }

        static void Update()
        {
            var count = networkMessageByUpdateAction.Count;
            while (count > 0)
            {
                try
                {
                    if (networkMessageByUpdateAction.TryDequeue(out var action))
                        action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.OutputException(e);
                }

                count--;
                if (!HandleAllMessagesPerFrame)
                    return;
            }
        }

        static void FixedUpdate()
        {
            var count = networkMessageByFixedUpdateAction.Count;
            while (count > 0)
            {
                try
                {
                    if (networkMessageByFixedUpdateAction.TryDequeue(out var action))
                        action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.OutputException(e);
                }

                count--;
                if (!HandleAllMessagesPerFrame)
                    return;
            }
        }
    }
}