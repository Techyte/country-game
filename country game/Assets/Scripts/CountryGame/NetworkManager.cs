using System;
using System.Collections.Generic;
using System.Linq;
using CountryGame.Multiplayer;
using Riptide;
using Riptide.Transports.Steam;
using Riptide.Utils;
using Steamworks;
using TMPro;
using UnityEngine.UI;
using SteamClient = Steamworks.SteamClient;

namespace CountryGame
{
    using UnityEngine;

    public enum GameMessageId : ushort
    {
        SetupPlayer,
        RequestNewAgreement,
        AgreementSigned,
        AgreementRejected,
        DeclareWar,
        NewAttack,
        EndTurn,
        NewTurn,
        SubsumedNations,
        CombatResults,
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

        public Server Server => Multiplayer.NetworkManager.Instance.Server;
        public Client Client => Multiplayer.NetworkManager.Instance.Client;
        
        [SerializeField] private GameObject greyedOutObj;
        [SerializeField] private Transform multiplayerScreen;
        [SerializeField] private float titleSpeed = 5.625f;
        [SerializeField] private Transform start, end;
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
            multiplayerScreen.position = start.position;
            
            askPlayerAgreementScreen.SetActive(false);
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
                multiplayerScreen.position = Vector3.Lerp(multiplayerScreen.position, start.position, titleSpeed * Time.deltaTime);
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
            
                Nation newPlayerNation = NationManager.Instance.GetNationByName(SteamMatchmaking.GetLobbyMemberData(LobbyData.LobbyId, SteamUser.GetSteamID(), "nation"));
                PlayerNationManager.Instance.MakeThePlayerNation(newPlayerNation);
                newPlayerNation.Countries[0].MovedTroopsIn(newPlayerNation, 6);
                newPlayerNation.DiplomaticPower = 30;
                
                Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.SetupPlayer);
                message.AddULong((ulong)Multiplayer.NetworkManager.Instance.riptideToStemId[client.Id]);
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
                obj.GetComponentsInChildren<Image>()[2].sprite = player.ControlledNation.flag;
                
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    CountrySelector.Instance.Clicked(player.ControlledNation);
                });
                
                playerObjects.Add(obj);
            }
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

        [MessageHandler((ushort)GameMessageId.SetupPlayer)]
        private void SetupPlayer(Message message)
        {
            Debug.Log("Setting up player");
            CSteamID id = (CSteamID)message.GetULong();
            ushort riptideId = message.GetUShort();
            
            string nation = SteamMatchmaking.GetLobbyMemberData(LobbyData.LobbyId, id, "nation");

            Nation newPlayerNation = NationManager.Instance.GetNationByName(nation);
            newPlayerNation.aPlayerNation = true;
            if (id != SteamUser.GetSteamID())
            {
                newPlayerNation.Countries[0].MovedTroopsIn(newPlayerNation, 6);
                newPlayerNation.DiplomaticPower = 30;
            }
            
            PlayerInfo info = new PlayerInfo();
            info.ControlledNation = newPlayerNation;
            info.steamID = id;
            info.riptideID = riptideId;
            
            Players.Add(riptideId, info);
        }

        [MessageHandler((ushort)GameMessageId.DeclareWar)]
        private static void DeclareWar(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.DeclareWar)]
        private static void DeclareWar(Message message)
        {
            Nation declaring = NationManager.Instance.GetNationByName(message.GetString());
            Nation declaredOn = NationManager.Instance.GetNationByName(message.GetString());

            CombatManager.Instance.DeclareWarOn(declaring, declaredOn);
        }

        [MessageHandler((ushort)GameMessageId.NewAttack)]
        private static void LaunchAttack(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.NewAttack)]
        private static void LaunchAttack(Message message)
        {
            Country source = NationManager.Instance.GetCountryByName(message.GetString());
            Country target = NationManager.Instance.GetCountryByName(message.GetString());

            CombatManager.Instance.LaunchedAttack(target, source);
        }

        [MessageHandler((ushort)GameMessageId.RequestNewAgreement)]
        private static void RequestAgreement(ushort fromClientId, Message message)
        {
            Nation requestingNation = NationManager.Instance.GetNationByName(message.GetString());
            Nation targetNation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement agreementRequested = message.GetAgreement();
            bool preexisting = message.GetBool();

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

        [MessageHandler((ushort)GameMessageId.RequestNewAgreement)]
        private static void RequestAgreement(Message message)
        {
            Nation requestingNation = NationManager.Instance.GetNationByName(message.GetString());
            Nation targetNation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement agreementRequested = message.GetAgreement();
            bool preexisting = message.GetBool();

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
            message.AddString(sourceNation.Name);
            message.AddString(PlayerNationManager.PlayerNation.Name);
            message.AddAgreement(currentAgreement);
            message.AddBool(prexisting);

            Client.Send(message);
        }

        public void RejectAgreement()
        {
            askPlayerAgreementScreen.SetActive(false);

            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.AgreementRejected);
            message.AddString(sourceNation.Name);
            message.AddString(PlayerNationManager.PlayerNation.Name);
            message.AddAgreement(currentAgreement);
            message.AddBool(prexisting);

            Client.Send(message);
        }

        [MessageHandler((ushort)GameMessageId.AgreementRejected)]
        private static void AgreementRejected(Message message)
        {
            Nation requestingAgreement = NationManager.Instance.GetNationByName(message.GetString());
            Nation targetNation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement requestedAgreement = message.GetAgreement();
            float requiredPower = message.GetFloat();
            
            // disagree
            requestingAgreement.DiplomaticPower -= (int)requiredPower;
                
            Debug.Log($"{targetNation.Name} rejected the {requestedAgreement.Name} agreement");
            
            Notification notification = Instantiate(Instance.notificationPrefab, Instance.notificationParent);
            notification.Init($"{targetNation.Name} Rejects!",
                $"Today, {targetNation.Name} rejected {requestingAgreement.Name} {requestedAgreement.Name} agreement, deciding to forge its own path",
                () => { CountrySelector.Instance.Clicked(targetNation); }, 5);

        }

        [MessageHandler((ushort)GameMessageId.AgreementSigned)]
        private static void AgreementAccepted(Message message)
        {
            Nation requestingAgreement = NationManager.Instance.GetNationByName(message.GetString());
            Nation targetNation = NationManager.Instance.GetNationByName(message.GetString());
            Agreement requestedAgreement = message.GetAgreement();
            bool preexisting = message.GetBool();
            
            if (!targetNation.aPlayerNation)
            {
                float requiredPower = message.GetFloat();
                requestingAgreement.DiplomaticPower -= (int)(requiredPower / 2);
            }

            NationManager.Instance.NewAgreement(requestedAgreement);
                
            // agree
            if (!preexisting)
            {
                NationManager.Instance.NationJoinAgreement(requestingAgreement, requestedAgreement);
            }
            NationManager.Instance.NationJoinAgreement(targetNation, requestedAgreement);
                
            Debug.Log($"{targetNation.Name} accepted the {requestedAgreement.Name} agreement");

            Notification notification = Instantiate(Instance.notificationPrefab, Instance.notificationParent);
            notification.Init($"{targetNation.Name} Agrees!",
                $"Today, {targetNation.Name} agreed to {requestingAgreement.Name} request and signed on to the {requestedAgreement.Name} agreement",
                () => { CountrySelector.Instance.OpenAgreementScreen(requestedAgreement); }, 5);
        }

        [MessageHandler((ushort)GameMessageId.AgreementSigned)]
        private static void AgreementAccepted(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.AgreementRejected)]
        private static void AgreementRejected(ushort fromClientId, Message message)
        {
            Instance.Server.SendToAll(message);
        }

        [MessageHandler((ushort)GameMessageId.EndTurn)]
        private static void EndTurn(ushort fromClientId, Message message)
        {
            TurnManager.Instance.SomeoneEndedTheirTurn();
        }

        [MessageHandler((ushort)GameMessageId.NewTurn)]
        private static void NewTurn(Message message)
        {
            TurnManager.Instance.ProgressTurnClient();
        }

        [MessageHandler((ushort)GameMessageId.SubsumedNations)]
        private static void SubsumedNations(Message message)
        {
            NationManager.Instance.HandleSubsumedNations(message.GetStrings().ToList(), message.GetStrings().ToList());
        }

        [MessageHandler((ushort)GameMessageId.CombatResults)]
        private static void CombatResults(Message message)
        {
            CombatManager.Instance.HandleCombatResults(message.GetStrings().ToList(), message.GetStrings().ToList(),
                message.GetStrings().ToList(), message.GetStrings().ToList());
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