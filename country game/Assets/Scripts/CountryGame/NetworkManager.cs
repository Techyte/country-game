using System.Collections.Generic;
using System.Linq;
using CountryGame.Multiplayer;
using Riptide;
using Riptide.Utils;
using Steamworks;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CountryGame
{
    using UnityEngine;

    public enum GameMessageId : ushort
    {
        SetupPlayer = 1,
        RequestNewAgreement,
        AgreementSigned,
        AgreementRejected,
        DeclareWar,
        NewAttack,
        EndTurn,
        NewTurn,
        SubsumedNations,
        CombatResults,
        MovedTroops,
        ChangedTroopDistribution,
        JoinedWar,
        AskToJoinWar,
        LeaveAgreement,
        HiredTroops,
        UpgradeInfrastructure,
        ProclaimNewNation
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

        // public Server Server => Multiplayer.NetworkManager.Instance.Server;
        // public Client Client => Multiplayer.NetworkManager.Instance.Client;
        public Server Server;
        public Client Client;
        
        [SerializeField] private GameObject greyedOutObj;
        [SerializeField] private Transform multiplayerScreen;
        [SerializeField] private float titleSpeed = 5.625f;
        [SerializeField] private Vector3 start;
        [SerializeField] private Transform end;
        [SerializeField] private Transform playerParent;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;
        
        [Space] 
        [SerializeField] private GameObject askPlayerAgreementScreen;
        [SerializeField] private Image nonAgression;
        [SerializeField] private Image militaryAccess;
        [SerializeField] private Image autoJoinWars;
        [SerializeField] private TextMeshProUGUI influenceText;
        [SerializeField] private TextMeshProUGUI sourceNationText;
        [SerializeField] private TextMeshProUGUI agreementNameText;
        [SerializeField] private Sprite tick, cross;
        
        public Dictionary<ushort, PlayerInfo> Players = new Dictionary<ushort, PlayerInfo>();
        
        public bool Host = false;
        
        private bool _multiplayerScreen;
        
        private void Awake()
        {
            Instance = this;
            
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

            start = multiplayerScreen.position;
            
            askPlayerAgreementScreen.SetActive(false);

            // if (Server.IsRunning)
            // {
            //     Host = true;
            // }
            //
            // Client.Disconnected += (sender, args) =>
            // {
            //     SceneManager.LoadScene("Multiplayer");
            // };

            Server = new Server();
            Client = new Client();
            
            Client.Connect($"127.0.0.1:7777", messageHandlerGroupId: Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId);
            
            Server.ClientConnected += (sender, args) =>
            {
                if (Server.ClientCount == 1)
                {
                    BeginSetup();
                }
            };
            
            Client.ConnectionFailed += (sender, args) =>
            {
                Server.Start(7777, 4, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId);
                Client.Connect($"127.0.0.1:7777", messageHandlerGroupId: Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId);
                Host = true;
            };
        }

        private void OnDisable()
        {
            Client.Disconnect();
            Server.Stop();
        }

        public void BeginSetup()
        {
            SetupPlayers();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSelected();
            }

            if (_multiplayerScreen)
            {
                multiplayerScreen.position = Vector3.Lerp(multiplayerScreen.position, end.position, titleSpeed * Time.deltaTime);
            }
            else
            {
                multiplayerScreen.position = Vector3.Lerp(multiplayerScreen.position, start, titleSpeed * Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (Server.IsRunning)
            {
                Server.Update();
            }
            
            Client.Update();
        }

        public void ResetSelected()
        {
            _multiplayerScreen = false;
        }

        private void SetupPlayers()
        {
            foreach (var client in Server.Clients)
            {
                Debug.Log("Sending connection information");
                
                Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.SetupPlayer);
                // message.AddULong((ulong)Multiplayer.NetworkManager.Instance.riptideToStemId[client.Id]);
                message.AddUShort(client.Id);
                
                Server.SendToAll(message);
            }
            
            greyedOutObj.SetActive(false);
        }

        private List<GameObject> playerObjects = new List<GameObject>();
        
        private void DisplayPlayers()
        {
            foreach (var obj in playerObjects)
            {
                Destroy(obj);
            }

            foreach (var player in Players.Values)
            {
                GameObject obj = Instantiate(playerPrefab, playerParent);

                obj.GetComponentInChildren<TextMeshProUGUI>().text = SteamFriends.GetFriendPersonaName(player.steamID);
                // Texture2D texture = GetSteamImageAsTexture(SteamFriends.GetLargeFriendAvatar(player.steamID));
                obj.GetComponentsInChildren<Image>()[2].sprite = player.ControlledNation.flag;
                // obj.GetComponentsInChildren<Image>()[1].sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    CountrySelector.Instance.Clicked(player.ControlledNation);
                });
                
                playerObjects.Add(obj);
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

        public void OpenMultiplayerScreen()
        {
            _multiplayerScreen = true;
            DisplayPlayers();
        }

        public void CloseMultiplayerScreen()
        {
            _multiplayerScreen = false;
        }

        [MessageHandler((ushort)GameMessageId.SetupPlayer, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void SetupPlayer(Message message)
        {
            Instance.greyedOutObj.SetActive(false);
            
            // CSteamID id = (CSteamID)message.GetULong();
            CSteamID id = CSteamID.Nil;
            ushort riptideId = message.GetUShort();
            
            // string nation = SteamMatchmaking.GetLobbyMemberData(LobbyData.LobbyId, id, "nation");
            string nation = riptideId == 1 ? "Australia" : "Russia";

            Nation newPlayerNation = NationManager.Instance.GetNationByName(nation);
            newPlayerNation.aPlayerNation = true;
            NationManager.Instance.PlayerNations.Add(newPlayerNation);
            
            newPlayerNation.DiplomaticPower = 30;
            
            // if (id == SteamUser.GetSteamID())
            // {
            //     PlayerNationManager.Instance.MakeThePlayerNation(newPlayerNation);
            // }
            if (riptideId == Instance.Client.Id)
            {
                PlayerNationManager.Instance.MakeThePlayerNation(newPlayerNation);
            }
            
            PlayerInfo info = new PlayerInfo();
            info.ControlledNation = newPlayerNation;
            info.steamID = id;
            info.riptideID = riptideId;
            
            Instance.Players.Add(riptideId, info);
        }

        [MessageHandler((ushort)GameMessageId.DeclareWar, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void DeclareWar(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.DeclareWar, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void DeclareWar(Message message)
        {
            Nation declaring = NationManager.Instance.GetNationByName(message.GetString());
            Nation declaredOn = NationManager.Instance.GetNationByName(message.GetString());

            CombatManager.Instance.DeclareWarOn(declaring, declaredOn);
        }

        [MessageHandler((ushort)GameMessageId.NewAttack, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void LaunchAttack(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.NewAttack, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void LaunchAttack(Message message)
        {
            Country source = NationManager.Instance.GetCountryByName(message.GetString());
            Country target = NationManager.Instance.GetCountryByName(message.GetString());
            Nation instigator = NationManager.Instance.GetNationByName(message.GetString());

            CombatManager.Instance.LaunchedAttack(target, source, instigator);
        }

        [MessageHandler((ushort)GameMessageId.RequestNewAgreement, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void RequestAgreement(ushort fromClientId, Message message)
        {
            Debug.Log("agreement requested");
            bool preexisting = message.GetBool();
            Nation requestingNation = NationManager.Instance.GetNationByName(message.GetString());
            Nation targetNation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement agreementRequested = null;
            if (!preexisting)
            {
                agreementRequested = message.GetAgreement();
            }
            else
            {
                agreementRequested = NationManager.Instance.agreements[message.GetInt()];
            }

            if (targetNation.aPlayerNation)
            {
                foreach (var player in Instance.Players.Values)
                {
                    if (player.ControlledNation == targetNation)
                    {
                        Instance.Server.Send(message, player.riptideID);
                    }
                }
            }
            else
            {
                ComputerAgreementCreator.Instance.PlayerAskedToJoinAgreement(requestingNation, targetNation, agreementRequested, preexisting);
            }
        }

        [MessageHandler((ushort)GameMessageId.RequestNewAgreement, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void RequestAgreement(Message message)
        {
            Debug.Log("got agreement request");
            bool preexisting = message.GetBool();
            Nation requestingNation = NationManager.Instance.GetNationByName(message.GetString());
            Nation targetNation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement agreementRequested = null;
            if (!preexisting)
            {
                agreementRequested = message.GetAgreement();
            }
            else
            {
                agreementRequested = NationManager.Instance.agreements[message.GetInt()];
            }
            
            Instance.AskPlayerForAgreement(agreementRequested, requestingNation, preexisting);
        }
        
        private Agreement currentAgreement;
        private Nation sourceNation;
        private bool prexisting;

        private void AskPlayerForAgreement(Agreement agreement, Nation sourceNation, bool prexisting)
        {
            askPlayerAgreementScreen.SetActive(true);

            currentAgreement = agreement;
            this.sourceNation = sourceNation;
            this.prexisting = prexisting;
            
            nonAgression.sprite = agreement.nonAgression ? tick : cross;
            militaryAccess.sprite = agreement.militaryAccess ? tick : cross;
            autoJoinWars.sprite = agreement.autoJoinWar ? tick : cross;

            sourceNationText.text = sourceNation.Name;
            agreementNameText.text = agreement.Name;

            switch (agreement.influence)
            {
                case 0:
                    influenceText.text = "Influence: None";
                    break;
                case 1:
                    influenceText.text = "Influence: Minimal Influence";
                    break;
                case 2:
                    influenceText.text = "Influence: influenced";
                    break;
                case 3:
                    influenceText.text = "Influence: Completely Influenced";
                    break;
            }
        }

        public void AgreeToAgreement()
        {
            askPlayerAgreementScreen.SetActive(false);
            
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.AgreementSigned);
            message.AddBool(prexisting);
            message.AddString(sourceNation.Name);
            message.AddString(PlayerNationManager.PlayerNation.Name);
            if (!prexisting)
            {
                message.AddAgreement(currentAgreement);
            }
            else
            {
                for (int i = 0; i < NationManager.Instance.agreements.Count; i++)
                {
                    if (NationManager.Instance.agreements[i] == currentAgreement)
                    {
                        message.AddInt(i);
                    }
                }
            }

            Client.Send(message);
        }

        public void RejectAgreement()
        {
            askPlayerAgreementScreen.SetActive(false);

            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.AgreementRejected);
            message.AddBool(prexisting);
            message.AddString(sourceNation.Name);
            message.AddString(PlayerNationManager.PlayerNation.Name);
            if (!prexisting)
            {
                message.AddAgreement(currentAgreement);
            }
            else
            {
                for (int i = 0; i < NationManager.Instance.agreements.Count; i++)
                {
                    if (NationManager.Instance.agreements[i] == currentAgreement)
                    {
                        message.AddInt(i);
                    }
                }
            }

            Client.Send(message);
        }

        [MessageHandler((ushort)GameMessageId.AgreementRejected, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void AgreementRejected(Message message)
        {
            bool preexisting = message.GetBool();
            Nation requestingAgreement = NationManager.Instance.GetNationByName(message.GetString());
            Nation targetNation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement requestedAgreement = null;
            if (!preexisting)
            {
                requestedAgreement = message.GetAgreement();
            }
            else
            {
                requestedAgreement = NationManager.Instance.agreements[message.GetInt()];
            }
            
            // disagree
            if (!targetNation.aPlayerNation)
            {
                float requiredPower = message.GetFloat();
                requestingAgreement.DiplomaticPower -= (int)requiredPower;
            }
                
            Debug.Log($"{targetNation.Name} rejected the {requestedAgreement.Name} agreement");
            
            Notification notification = Instantiate(Instance.notificationPrefab, Instance.notificationParent);
            notification.Init($"{targetNation.Name} Rejects!",
                $"Today, {targetNation.Name} rejected {requestingAgreement.Name} {requestedAgreement.Name} agreement, deciding to forge its own path",
                () => { CountrySelector.Instance.Clicked(targetNation); }, 5);

        }

        [MessageHandler((ushort)GameMessageId.AgreementSigned, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void AgreementAccepted(Message message)
        {
            bool preexisting = message.GetBool();
            Nation requestingAgreement = NationManager.Instance.GetNationByName(message.GetString());
            Nation targetNation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement requestedAgreement = null;
            if (!preexisting)
            {
                requestedAgreement = message.GetAgreement();
            }
            else
            {
                requestedAgreement = NationManager.Instance.agreements[message.GetInt()];
            }
            
            if (!targetNation.aPlayerNation)
            {
                float requiredPower = message.GetFloat();
                requestingAgreement.DiplomaticPower -= (int)requiredPower;
            }

            NationManager.Instance.NewAgreement(requestedAgreement);
            
            NationManager.Instance.NationJoinAgreement(targetNation, requestedAgreement);
            NationManager.Instance.NationJoinAgreement(requestingAgreement, requestedAgreement);

            foreach (var nation in requestedAgreement.Nations)
            {
                foreach (var war in nation.Wars)
                {
                    foreach (var otherNation in requestedAgreement.Nations)
                    {
                        if (!otherNation.Wars.Contains(war))
                        {
                            if (war.Belligerents.Contains(otherNation))
                            {
                                CombatManager.Instance.NationJoinWarBelligerents(otherNation, war);
                            }
                            else
                            {
                                CombatManager.Instance.NationJoinWarDefenders(otherNation, war);
                            }
                        }
                    }
                }
            }
                
            Debug.Log($"{targetNation.Name} accepted the {requestedAgreement.Name} agreement");

            Notification notification = Instantiate(Instance.notificationPrefab, Instance.notificationParent);
            notification.Init($"{targetNation.Name} Agrees!",
                $"Today, {targetNation.Name} agreed to {requestingAgreement.Name} request and signed on to the {requestedAgreement.Name} agreement",
                () => { CountrySelector.Instance.OpenAgreementScreen(requestedAgreement); }, 5);
            
            ViewTypeManager.Instance.UpdateView();
        }

        [MessageHandler((ushort)GameMessageId.AgreementSigned, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void AgreementAccepted(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.MovedTroops, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void TroopsMoved(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.MovedTroops, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void TroopsMoved(Message message)
        {
            Country from = NationManager.Instance.GetCountryByName(message.GetString());
            Country to = NationManager.Instance.GetCountryByName(message.GetString());
            Nation nation = NationManager.Instance.GetNationByName(message.GetString());
            int amount = message.GetInt();
            
            TroopMover.Instance.TransferTroops(from, to, nation, amount);
        }

        [MessageHandler((ushort)GameMessageId.AgreementRejected, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void AgreementRejected(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.EndTurn, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void EndTurn(ushort fromClientId, Message message)
        {
            TurnManager.Instance.SomeoneEndedTheirTurn();
        }

        [MessageHandler((ushort)GameMessageId.NewTurn, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void NewTurn(Message message)
        {
            TurnManager.Instance.ProgressTurnClient();
            NationManager.Instance.HandleFinance();
            NationManager.Instance.HandleHiringTroops();
            NationManager.Instance.HandleInfrastructureUpgrades();
        }

        [MessageHandler((ushort)GameMessageId.ChangedTroopDistribution, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void ChangeTroopDistribution(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.ChangedTroopDistribution, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void ChangeTroopDistribution(Message message)
        {
            Nation nationThatChanged = NationManager.Instance.GetNationByName(message.GetString());
            float infantry = message.GetFloat();
            float tanks = message.GetFloat();
            float marines = message.GetFloat();
            
            PlayerNationManager.Instance.ChangeDistribution(nationThatChanged, infantry, tanks, marines);
        }

        [MessageHandler((ushort)GameMessageId.LeaveAgreement, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void LeaveAgreement(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.LeaveAgreement, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void LeaveAgreement(Message message)
        {
            Nation nation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement agreement = NationManager.Instance.agreements[message.GetInt()];
            
            CountrySelector.Instance.LeaveAgreement(nation, agreement);
        }

        [MessageHandler((ushort)GameMessageId.AskToJoinWar, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void AskToJoinWar(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.AskToJoinWar, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void AskToJoinWar(Message message)
        {
            War war = CombatManager.Instance.wars[message.GetInt()];
            Nation requested = NationManager.Instance.GetNationByName(message.GetString());
            Nation requester = NationManager.Instance.GetNationByName(message.GetString());
            
            CombatManager.Instance.AskToJoinWar(war, requested, requester);
        }

        [MessageHandler((ushort)GameMessageId.JoinedWar, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void JoinWar(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.JoinedWar, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void ChangeTroopDistributionMessage(Message message)
        {
            War war = CombatManager.Instance.wars[message.GetInt()];
            Nation nation = NationManager.Instance.GetNationByName(message.GetString());
            bool defender = message.GetBool();
            
            CountrySelector.Instance.JoinWar(war, nation, defender);
        }

        [MessageHandler((ushort)GameMessageId.HiredTroops, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void HiredTroops(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.HiredTroops, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void HiredTroops(Message message)
        {
            Country country = NationManager.Instance.GetCountryByName(message.GetString());
            Nation nation = NationManager.Instance.GetNationByName(message.GetString());
            int amount = message.GetInt();
            
            NationManager.Instance.HireTroops(country, nation, amount);
        }

        [MessageHandler((ushort)GameMessageId.UpgradeInfrastructure, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void UpgradeInfrastructure(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.UpgradeInfrastructure, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void UpgradeInfrastructure(Message message)
        {
            Country country = NationManager.Instance.GetCountryByName(message.GetString());
            Nation nation = NationManager.Instance.GetNationByName(message.GetString());
            bool upgrading = message.GetBool();
            
            NationManager.Instance.UpgradeInfrastructure(country, nation, upgrading);
        }

        [MessageHandler((ushort)GameMessageId.ProclaimNewNation, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void ProclaimNewNation(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.ProclaimNewNation, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void ProclaimNewNation(Message message)
        {
            Nation nationToChange = NationManager.Instance.GetNationByName(message.GetString());
            string newName = message.GetString();
            string flag = message.GetString();
            Color color = new Color(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
            
            NationManager.Instance.ProclaimNation(nationToChange, newName, flag, color);
        }

        [MessageHandler((ushort)GameMessageId.SubsumedNations, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void SubsumedNations(Message message)
        {
            NationManager.Instance.HandleSubsumedNations(message.GetStrings().ToList(), message.GetStrings().ToList());
        }

        [MessageHandler((ushort)GameMessageId.CombatResults, Multiplayer.NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void CombatResults(Message message)
        {
            CombatManager.Instance.HandleCombatResults(message.GetStrings().ToList(),
                message.GetStrings().ToList(), message.GetStrings().ToList(),
                message.GetStrings().ToList());
        }
    }
    
    public class PlayerInfo
    {
        public Nation ControlledNation;
        public CSteamID steamID;
        public ushort riptideID;
    }

    public static class MessageExtensions
    {
        public static Message AddAgreement(this Message message, Agreement agreement)
        {
            message.Add(agreement.AgreementLeader.Name);
            message.Add(agreement.autoJoinWar);
            message.Add(agreement.nonAgression);
            message.Add(agreement.Name);
            message.Add(agreement.militaryAccess);
            message.Add(agreement.influence);
            message.Add(agreement.turnCreated);
            message.Add(agreement.Color.r);
            message.Add(agreement.Color.g);
            message.Add(agreement.Color.b);
            message.Add(agreement.Color.a);

            return message;
        }
        
        public static Agreement GetAgreement(this Message message)
        {
            Agreement agreement = new Agreement();

            agreement.AgreementLeader = NationManager.Instance.GetNationByName(message.GetString());
            agreement.autoJoinWar = message.GetBool();
            agreement.nonAgression = message.GetBool();
            agreement.Name = message.GetString();
            agreement.militaryAccess = message.GetBool();
            agreement.influence = message.GetInt();
            agreement.turnCreated = message.GetInt();
            float r = message.GetFloat();
            float g = message.GetFloat();
            float b = message.GetFloat();
            float a = message.GetFloat();
            agreement.Color = new Color(r, g, b, a);

            return agreement;
        }
    }
}