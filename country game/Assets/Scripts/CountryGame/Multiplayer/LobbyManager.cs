using System.Collections.Generic;
using Riptide;
using Steamworks;
using TMPro;
using UnityEngine.UI;

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
        protected Callback<AvatarImageLoaded_t> AvatarImageLoaded;
        protected Callback<LobbyChatUpdate_t> LobbyChatUpdate;
        protected Callback<LobbyDataUpdate_t> LobbyDataUpdate;
        
        private const string HostAddressKey = "HostAddress";
        public CSteamID lobbyId;

        [SerializeField] private TextMeshProUGUI idText;
        [SerializeField] private TMP_InputField idInput;
        [SerializeField] private TextMeshProUGUI lobbyName;
        [SerializeField] private Transform lobbyClientParent;
        [SerializeField] private GameObject lobbyClientPrefab;
        [SerializeField] private Button beginButton;

        public CSteamID lastConnectedUserId;

        private bool ready;

        public bool inLobby;

        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam is not initialized!");
                return;
            }
            
            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
            LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            AvatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
            LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        }
        
        public void CreateLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 5);
            Debug.Log("Requested Lobby creation");
        }
        
        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                return;
            }
            
            Debug.Log("Lobby created success");
        
            lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        
            NetworkManager.Instance.Server.Start(0, 5, NetworkManager.PlayerHostedDemoMessageHandlerGroupId);
            NetworkManager.Instance.Client.Connect("127.0.0.1", messageHandlerGroupId: NetworkManager.PlayerHostedDemoMessageHandlerGroupId);
            
            idText.text = $"Lobby ID: {callback.m_ulSteamIDLobby.ToString()}";
            SteamMatchmaking.SetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "name",
                SteamFriends.GetPersonaName() + "s Lobby");
            SteamMatchmaking.SetLobbyData((CSteamID)callback.m_ulSteamIDLobby, "nation",
                "none");
            
            lobbyName.text = SteamFriends.GetPersonaName() + "s Lobby";
            
            DisplayLobbyMembers(lobbyId);

            lastConnectedUserId = SteamUser.GetSteamID();
            inLobby = true;
        }

        private void OnLobbyEnter(LobbyEnter_t callback)
        {
            if (NetworkManager.Instance.Server.IsRunning)
                return;
            
            Debug.Log("Entered Lobby");

            lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            CSteamID hostId = SteamMatchmaking.GetLobbyOwner(lobbyId);
            
            NetworkManager.Instance.Client.Connect(hostId.ToString(), messageHandlerGroupId: NetworkManager.PlayerHostedDemoMessageHandlerGroupId);
            idText.text = $"Lobby ID: {callback.m_ulSteamIDLobby}";
            lobbyName.text = SteamMatchmaking.GetLobbyData(lobbyId, "name");
            
            DisplayLobbyMembers(lobbyId);
            inLobby = true;
        }

        private Dictionary<CSteamID, GameObject> currentLobbyClients = new Dictionary<CSteamID, GameObject>();
        public void DisplayLobbyMembers(CSteamID lobbyId)
        {
            foreach (var obj in currentLobbyClients.Values)
            {
                Destroy(obj);
            }
            
            currentLobbyClients.Clear();
            
            int numOfMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);

            for (int i = 0; i < numOfMembers; i++)
            {
                GameObject obj = Instantiate(lobbyClientPrefab, lobbyClientParent);
                
                CSteamID userId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
                string username = SteamFriends.GetFriendPersonaName(userId);

                obj.GetComponentsInChildren<TextMeshProUGUI>()[0].text = username;

                bool userReady = SteamMatchmaking.GetLobbyMemberData(lobbyId, userId, "ready") == "1";
                
                obj.GetComponentsInChildren<TextMeshProUGUI>()[1].text = userReady ? "Ready" : "Unready";
                obj.GetComponentsInChildren<TextMeshProUGUI>()[1].color = userReady ? Color.green : Color.red;
                
                currentLobbyClients.Add(userId, obj);
                
                int imageId = SteamFriends.GetLargeFriendAvatar(userId);

                if (imageId == -1)
                {
                    return;
                }

                Texture2D texture = GetSteamImageAsTexture(imageId);

                obj.GetComponentInChildren<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
        }
        
        protected void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
        {
            if (callback.m_steamID == SteamUser.GetSteamID())
            {
                Texture2D texture = GetSteamImageAsTexture(SteamFriends.GetLargeFriendAvatar(callback.m_steamID));

                if (currentLobbyClients.TryGetValue(callback.m_steamID, out GameObject obj))
                {
                    obj.GetComponentInChildren<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }
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
            }

            return texture;
        }

        private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log($"We are asking to join {callback.m_steamIDLobby}s lobby");
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        public void JoinLobby()
        {
            SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(idInput.text)));
        }

        // called by each user
        public void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            Debug.Log("on lobby chat update");
            switch (callback.m_rgfChatMemberStateChange)
            {
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                    DisplayLobbyMembers(lobbyId);
                    break;
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    lastConnectedUserId = (CSteamID)callback.m_ulSteamIDUserChanged;
                    break;
            }
            
            DisplayLobbyMembers(lobbyId);
        }

        private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
        {
            DisplayLobbyMembers(lobbyId);
            Debug.Log($"On data updated by {callback.m_ulSteamIDMember}");

            if (CountrySelector.Instance.currentNation == null)
            {
                return;
            }
            
            for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobbyId); i++)
            {
                CSteamID clientId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);

                string nation = SteamMatchmaking.GetLobbyMemberData(lobbyId, clientId, "nation");

                if (nation == CountrySelector.Instance.currentNation.Name)
                {
                    CountrySelector.Instance.SomeoneTookCurrentNation();
                }
            }
        }

        public void LeaveLobby()
        {
            NetworkManager.Instance.Client.Disconnect();
            SteamMatchmaking.LeaveLobby(lobbyId);
            lobbyId = CSteamID.Nil;
            idText.text = "Lobby ID: None";
            inLobby = false;
            Camera.main.transform.position = new Vector3(0, 0, -10);
            FindObjectOfType<GameCamera>().targetFov = 40;
            FindObjectOfType<GameCamera>().currentFov = 40;
            CountrySelector.Instance.ResetSelected();
        }
        
        public void ToggleReady()
        {
            ready = !ready;
            SteamMatchmaking.SetLobbyMemberData(lobbyId, "ready", ready ? "1" : "0");
            DisplayLobbyMembers(lobbyId);
        }
    }
}