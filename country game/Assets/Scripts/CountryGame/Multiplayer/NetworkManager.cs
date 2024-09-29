using System;
using System.Collections.Generic;
using Riptide;
using Riptide.Transports.Steam;
using Riptide.Utils;
using Steamworks;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SteamClient = Riptide.Transports.Steam.SteamClient;

namespace CountryGame.Multiplayer
{
    using UnityEngine;

    public enum LobbyMessageID : ushort
    {
        BeginGame = 100,
        ResetSelectedNation,
        IdUpdate,
    }

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
        
        public const byte PlayerHostedDemoMessageHandlerGroupId = 8;
        
        public Server Server;
        public Client Client;

        [SerializeField] private Image profilePicture;
        [SerializeField] private TextMeshProUGUI username;

        [SerializeField] private GameObject mainScreen;
        [SerializeField] private GameObject lobbyScreen;

        protected Callback<AvatarImageLoaded_t> AvatarImageLoaded;
        protected Callback<LobbyDataUpdate_t> LobbyDataUpdated;

        public Dictionary<ushort, CSteamID> riptideToStemId = new Dictionary<ushort, CSteamID>();

        private bool inGame;
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }
            
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        }

        private void Start()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }
            
            SteamServer steamServer = new SteamServer();
            Server = new Server(steamServer);
            
            Server.ClientConnected += PlayerConnected;
            Server.ClientDisconnected += PlayerDisconnected;

            Client = new Client(new SteamClient(steamServer));
            
            Client.ClientConnected += ClientOnClientConnected;
            Client.Connected += ClientOnClientConnected;
            Client.ClientDisconnected += ClientOnClientDisconnected;
            Client.Disconnected += ClientOnClientDisconnected;

            AvatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
            LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            
            Debug.Log($"Logged in as {SteamFriends.GetPersonaName()}");
            username.text = SteamFriends.GetPersonaName();
            int imageId = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());

            if (imageId == -1)
            {
                return;
            }

            Texture2D texture = GetSteamImageAsTexture(imageId);

            profilePicture.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
        {
            int total = 0;

            CSteamID lobbyId = (CSteamID)callback.m_ulSteamIDLobby;

            List<CSteamID> alreadySentTo = new List<CSteamID>();
            
            for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobbyId); i++)
            {
                if (SteamMatchmaking.GetLobbyMemberData(LobbyData.LobbyId, SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i), "ready") == "1")
                {
                    total++;
                }

                string iNation = SteamMatchmaking.GetLobbyMemberData(lobbyId, SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i),
                    "nation");

                if (iNation == "" || iNation == "none")
                {
                    continue;
                }
                for (int j = 0; j < SteamMatchmaking.GetNumLobbyMembers(lobbyId); j++)
                {
                    iNation = SteamMatchmaking.GetLobbyMemberData(lobbyId, SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i), "nation");
                    string jNation = SteamMatchmaking.GetLobbyMemberData(lobbyId, SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, j), "nation");

                    if (jNation == "" || jNation == "none")
                    {
                        continue;
                    }

                    if (i == j)
                    {
                        continue;
                    }
                    
                    if (iNation == jNation && !alreadySentTo.Contains(SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i)) && !alreadySentTo.Contains(SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, j)))
                    {
                        Debug.Log("Collision Detected");
                        foreach (var client in Server.Clients)
                        {
                            if (((SteamConnection)client).SteamId == SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i))
                            {
                                alreadySentTo.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i));

                                Message message = Message.Create(MessageSendMode.Reliable,
                                    LobbyMessageID.ResetSelectedNation);
                                Server.Send(message, client.Id);
                            }
                            else if (((SteamConnection)client).SteamId == SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, j))
                            {
                                alreadySentTo.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, j));

                                Message message = Message.Create(MessageSendMode.Reliable,
                                    LobbyMessageID.ResetSelectedNation);
                                Server.Send(message, client.Id);
                            }
                        }
                    }
                }
            }

            if (total == SteamMatchmaking.GetNumLobbyMembers(lobbyId) && !inGame)
            {
                Debug.Log(inGame);
                inGame = true;
                EveryoneReady();
            }
        }

        private void ClientOnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            
        }

        private void ClientOnClientDisconnected(object sender, EventArgs e)
        {
            
        }

        private void ClientOnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            
        }

        private void ClientOnClientConnected(object sender, EventArgs e)
        {
            Message message = Message.Create(MessageSendMode.Reliable, LobbyMessageID.IdUpdate);
            message.AddULong((ulong)SteamUser.GetSteamID());

            Client.Send(message);
        }

        public void OpenGame()
        {
            Debug.Log("loading game");
            inGame = true;
            SceneManager.LoadScene("Game");
        }

        [MessageHandler((ushort)LobbyMessageID.IdUpdate, PlayerHostedDemoMessageHandlerGroupId)]
        private static void IdUpdate(ushort fromClientId, Message message)
        {
            CSteamID id = (CSteamID)message.GetULong();
            
            Instance.riptideToStemId.Add(fromClientId, id);
        }

        protected void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
        {
            if (callback.m_steamID == SteamUser.GetSteamID())
            {
                Texture2D texture = GetSteamImageAsTexture(callback.m_iImage);

                profilePicture.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
        }

        private Texture2D GetSteamImageAsTexture(int iImage)
        {
            Texture2D texture = null;

            bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

            if (isValid)
            {
                byte[] image = new byte[width * height * 4];

                isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

                if (isValid)
                {
                    texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                }
;            }

            return texture;
        }

        private void FixedUpdate()
        {
            if (Server.IsRunning)
            {
                Server.Update();
            }
            
            Client.Update();
        }

        private void OnApplicationQuit()
        {
            StopServer();
            DisconnectClient();
            Server.ClientConnected -= PlayerConnected;
            Server.ClientDisconnected -= PlayerDisconnected;
            Client.ClientConnected -= ClientOnClientConnected;
            Client.Connected -= ClientOnClientConnected;
            Client.ClientDisconnected -= ClientOnClientDisconnected;
            Client.Disconnected -= ClientOnClientDisconnected;
            SteamMatchmaking.LeaveLobby(SteamUser.GetSteamID());
        }

        internal void StopServer()
        {
            Server.Stop();
        }
        
        internal void DisconnectClient()
        {
            Client.Disconnect();
        }

        private void PlayerConnected(object o, ServerConnectedEventArgs e)
        {
            
        }

        private void PlayerDisconnected(object o, ServerDisconnectedEventArgs e)
        {
            
        }

        private void EveryoneReady()
        {
            Debug.Log("sending ready message");
            Message message = Message.Create(MessageSendMode.Reliable, LobbyMessageID.BeginGame);
            
            Server.SendToAll(message);
        }

        [MessageHandler((ushort)LobbyMessageID.BeginGame, PlayerHostedDemoMessageHandlerGroupId)]
        private static void BeginGame(Message message)
        {
            Instance.OpenGame();
        }

        [MessageHandler((ushort)LobbyMessageID.ResetSelectedNation, PlayerHostedDemoMessageHandlerGroupId)]
        private static void ResetSelectedNation(Message message)
        {
            SteamMatchmaking.SetLobbyMemberData(LobbyData.LobbyId, "nation", "none");
            CountrySelector.Instance.Clicked(CountrySelector.Instance.currentNation);
        }
    }
}