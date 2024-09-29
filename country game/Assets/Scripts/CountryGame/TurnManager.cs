using System;
using Riptide;
using TMPro;

namespace CountryGame
{
    using UnityEngine;

    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    return FindObjectOfType<TurnManager>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        private static TurnManager _instance;

        public int currentTurn;
        [SerializeField] private int turnActionPoints;
        [SerializeField] private TextMeshProUGUI actionPointDisplay;
        [SerializeField] private GameObject confirmPannel;
        [SerializeField] private GameObject mainUI;

        public EventHandler<EventArgs> NewTurn;
        
        public int actionPoints;

        public bool endedTurn;

        private void Awake()
        {
            currentTurn = 0;
            Instance = this;
            confirmPannel.SetActive(false);
        }

        private void Start()
        {
            actionPoints = turnActionPoints;
        }

        private void Update()
        {
            actionPointDisplay.text = $"AP:{actionPoints}";

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelTurnEnd();
            }
        }

        private int totalEnded = 0;

        public void SomeoneEndedTheirTurn()
        {
            totalEnded++;

            if (totalEnded >= NetworkManager.Instance.Players.Values.Count)
            {
                ProgressTurn();
                totalEnded = 0;
            }
        }

        public void ProgressTurn()
        {
            ProgressTurnClient();
            
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.NewTurn);
            NetworkManager.Instance.Server.SendToAll(message, NetworkManager.Instance.Client.Id);
        }

        public void ProgressTurnClient()
        {
            currentTurn++;
            
            NewTurn?.Invoke(this, EventArgs.Empty);
            actionPoints = turnActionPoints;
            
            endedTurn = false;
            mainUI.SetActive(true);
        }

        public void PerformedAction()
        {
            actionPoints--;
        }

        public bool CanPerformAction()
        {
            return actionPoints > 0;
        }

        public void PressedTurnEnd()
        {
            if (endedTurn)
            {
                return;
            }
            confirmPannel.SetActive(true);
        }

        public void ConfirmTurnEnd()
        {
            confirmPannel.SetActive(false);

            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.EndTurn);

            NetworkManager.Instance.Client.Send(message);

            actionPoints = 0;
            
            endedTurn = true;
            mainUI.SetActive(false);
        }

        public void CancelTurnEnd()
        {
            confirmPannel.SetActive(false);
        }
    }
}