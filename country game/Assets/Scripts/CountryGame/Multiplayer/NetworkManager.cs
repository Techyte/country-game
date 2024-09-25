using System;
using Riptide;
using Riptide.Transports.Steam;
using Riptide.Utils;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using SteamClient = Riptide.Transports.Steam.SteamClient;

public enum LobbyMessageId : ushort
{
    BeginGame,
}

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
        public Client Client;

        [SerializeField] private Image profilePicture;
        [SerializeField] private TextMeshProUGUI username;

        [SerializeField] private GameObject mainScreen;
        [SerializeField] private GameObject lobbyScreen;
        [SerializeField] private GameObject gameScreen;

        protected Callback<AvatarImageLoaded_t> AvatarImageLoaded;
        //protected Callback<Lobby> LobbyEntered;
        
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
            
            OpenMainScreen();
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

        private void ClientOnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            //LobbyManager.Instance.DisplayLobbyMembers(LobbyManager.Instance.lobbyId);
        }

        private void ClientOnClientDisconnected(object sender, EventArgs e)
        {
            OpenMainScreen();
        }

        private void ClientOnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            //LobbyManager.Instance.DisplayLobbyMembers(LobbyManager.Instance.lobbyId);
        }

        private void ClientOnClientConnected(object sender, EventArgs e)
        {
            OpenLobbyScreen();
        }

        public void OpenMainScreen()
        {
            mainScreen.SetActive(true);
            lobbyScreen.SetActive(false);
            gameScreen.SetActive(false);
        }

        public void OpenLobbyScreen()
        {
            mainScreen.SetActive(false);
            lobbyScreen.SetActive(true);
            gameScreen.SetActive(false);
        }

        public void OpenGameScreen()
        {
            mainScreen.SetActive(true);
            lobbyScreen.SetActive(false);
            gameScreen.SetActive(true);
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
            Debug.Log("We connected from the lobby");
            if (Server.ClientCount == 1)
            {
                Debug.Log("We are the first in the server");
            }
        }

        private void PlayerDisconnected(object o, ServerDisconnectedEventArgs e)
        {
            Debug.Log("We disconnected from the lobby");
        }

        public void BeginGame()
        {
            OpenGameScreen();
        }

        [MessageHandler((ushort)LobbyMessageId.BeginGame, PlayerHostedDemoMessageHandlerGroupId)]
        private void BeginGameMessageReceived(Message message, ushort fromClientId)
        {
            
        }
    }
}