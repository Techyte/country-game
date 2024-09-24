using System;
using Riptide;
using Riptide.Transports.Steam;
using Riptide.Utils;
using Steamworks;

namespace CountryGame.Multiplayer
{
    using UnityEngine;

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                if (_instance == null)
                {
                    _instance = value;
                }
                else
                {
                    Debug.LogWarning("Multiple Instances of NetworkManager, destroying new one");
                    Destroy(value.gameObject);
                }
            }
        }

        private static NetworkManager _instance;
        
        public const byte PlayerHostedDemoMessageHandlerGroupId = 255;
        
        public Server Server;
        
        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }
            
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

            SteamServer steamServer = new SteamServer();
            Server = new Server(steamServer);
            Server.ClientConnected += PlayerConnected;
            Server.ClientDisconnected += PlayerDisconnected;
        }

        private void Start()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }
        }

        private void Update()
        {
            if (Server.IsRunning)
            {
                Server.Update();
            }
        }

        private void OnApplicationQuit()
        {
            StopServer();
            Server.ClientConnected -= PlayerConnected;
            Server.ClientDisconnected -= PlayerDisconnected;
        }

        internal void StopServer()
        {
            Server.Stop();
        }

        private void PlayerConnected(object o, ServerConnectedEventArgs e)
        {
            
        }

        private void PlayerDisconnected(object o, ServerDisconnectedEventArgs e)
        {
            
        }
    }
}