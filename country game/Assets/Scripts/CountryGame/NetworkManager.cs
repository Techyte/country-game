using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Riptide;
using Riptide.Utils;
using TMPro;
using UnityEngine.UI;

namespace CountryGame
{
    using UnityEngine;

    public enum GameMessageId : ushort
    {
        SetupPlayer,
        NewPlayerSetup,
        ExistingPlayerInfo,
        RequestNewAgreement,
        AgreementSigned,
        AgreementRejected,
        DeclareWar,
        NewAttack,
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

        public Server Server;
        public Client Client;

        public int port = 7777;
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
            Server = new Server();
            
            Client = new Client();
            Client.Connect($"127.0.0.1:{port}");
            
            Client.Connected += ClientOnClientConnected;
            Client.ConnectionFailed += (sender, args) =>
            {
                Server.Start((ushort)port, 4);
                Client.Connect($"127.0.0.1:{port}");
                Host = true;
            };
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

        private void OnDisable()
        {
            Client.Connected -= ClientOnClientConnected;
            Client.Disconnect();
            Server.Stop();
        }

        private void ClientOnClientConnected(object sender, EventArgs e)
        {
            string nationName = Client.Id == 1 ? "Australia" : "India";
            string username = Client.Id == 1 ? "bozo 1" : "idiot 2";
            
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.SetupPlayer);
            message.AddString(nationName);
            message.AddString(username);

            Client.Send(message);
            
            Debug.Log("Sending connection information");
            
            Nation newPlayerNation = NationManager.Instance.GetNationByName(nationName);
            PlayerNationManager.Instance.MakeThePlayerNation(newPlayerNation);
            newPlayerNation.Countries[0].MovedTroopsIn(newPlayerNation, 6);
            newPlayerNation.DiplomaticPower = 30;
            
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

                obj.GetComponentInChildren<TextMeshProUGUI>().text = player.Username;
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
        private static void SetupPlayer(ushort fromClientId, Message message)
        {
            string nation = message.GetString();
            string username = message.GetString();

            Nation newPlayerNation = NationManager.Instance.GetNationByName(nation);
            newPlayerNation.aPlayerNation = true;
            if (fromClientId != Instance.Client.Id)
            {
                newPlayerNation.Countries[0].MovedTroopsIn(newPlayerNation, 6);
                newPlayerNation.DiplomaticPower = 30;
            }
            
            PlayerInfo info = new PlayerInfo();
            info.Id = fromClientId;
            info.ControlledNation = newPlayerNation;
            info.Username = username;
            
            Instance.Players.Add(fromClientId, info);

            // send the new player info to the other clients
            Message updateMessage = Message.Create(MessageSendMode.Reliable, GameMessageId.NewPlayerSetup);
            updateMessage.AddPlayerInfo(info);
            
            Instance.Server.SendToAll(updateMessage, fromClientId);
            
            // send all the existing info to the new player

            updateMessage = Message.Create(MessageSendMode.Reliable, GameMessageId.ExistingPlayerInfo);
            updateMessage.AddPlayerInfos(Instance.Players.Values.ToArray());

            Instance.Server.Send(updateMessage, fromClientId);
            
            Debug.Log("sending existing player stuff");
        }

        [MessageHandler((ushort)GameMessageId.NewPlayerSetup)]
        private static void NewPlayerSetup(Message message)
        {
            Debug.Log("received new player setup");
            PlayerInfo info = message.GetPlayerInfo();

            if (!Instance.Players.ContainsKey((ushort)info.Id))
            {
                Instance.Players.Add((ushort)info.Id, info);
            }
        }

        [MessageHandler((ushort)GameMessageId.ExistingPlayerInfo)]
        private static void ReceivedExistingInfos(Message message)
        {
            Debug.Log("receiving existing player stuff");
            PlayerInfo[] infos = message.GetPlayerInfos();

            foreach (var info in infos)
            {
                if (!Instance.Players.ContainsKey((ushort)info.Id))
                {
                    Instance.Players.Add((ushort)info.Id, info);
                }
            }
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
                        Instance.Server.Send(message, (ushort)player.Id);
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

        private bool IsPortAvailable(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port==port)
                {
                    return false;
                }
            }

            return true;
        }
    }
    
    public class PlayerInfo
    {
        public int Id;
        public Nation ControlledNation;
        public string Username;
    }

    public static class MessageExtensions
    {
        public static Message AddPlayerInfo(this Message message, PlayerInfo info)
        {
            message.Add(info.Id);
            message.Add(info.Username);
            message.Add(info.ControlledNation.Name);

            return message;
        }
        
        public static PlayerInfo GetPlayerInfo(this Message message)
        {
            PlayerInfo info = new PlayerInfo();
            info.Id = message.GetInt();
            info.Username = message.GetString();
            info.ControlledNation = NationManager.Instance.GetNationByName(message.GetString());

            return info;
        }
        
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
        
        public static Message AddPlayerInfos(this Message message, PlayerInfo[] infos)
        {
            message.AddInt(infos.Length);
            foreach (var info in infos)
            {
                message.AddPlayerInfo(info);
            }

            return message;
        }
        
        public static PlayerInfo[] GetPlayerInfos(this Message message)
        {
            PlayerInfo[] infos = new PlayerInfo[message.GetInt()];

            for (int i = 0; i < infos.Length; i++)
            {
                infos[i] = GetPlayerInfo(message);
            }

            return infos;
        }
    }
}