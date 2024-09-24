using System;
using Steamworks;

namespace CountryGame.Multiplayer
{
    using UnityEngine;

    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance
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
                    Debug.LogWarning("Multiple Instances of LobbyManager, destroying new one");
                    Destroy(value.gameObject);
                }
            }
        }

        private static LobbyManager _instance;
        
        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> LobbyEnter;
        
        private const string HostAddressKey = "HostAddress";
        private CSteamID lobbyId;

        private void Awake()
        {
            Instance = this;
        }
        //
        // private void Start()
        // {
        //     if (!SteamManager.Initialized)
        //     {
        //         Debug.LogError("Steam is not initialized!");
        //         return;
        //     }
        //     
        //     LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated); 
        //     GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        //     LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        // }
        //
        // internal void CreateLobby()
        // {
        //     SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 5);
        // }
        //
        // private void OnLobbyCreated(LobbyCreated_t callback)
        // {
        //     if (callback.m_eResult != EResult.k_EResultOK)
        //     {
        //         UIManager.Singleton.LobbyCreationFailed();
        //         return;
        //     }
        //
        //     lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        //     UIManager.Singleton.LobbyCreationSucceeded(callback.m_ulSteamIDLobby);
        //
        //     NetworkManager.Instance.Server.Start(0, 5, NetworkManager.PlayerHostedDemoMessageHandlerGroupId);
        //     NetworkManager.Instance.Client.Connect("127.0.0.1", messageHandlerGroupId: NetworkManager.PlayerHostedDemoMessageHandlerGroupId);
        // }
    }
}