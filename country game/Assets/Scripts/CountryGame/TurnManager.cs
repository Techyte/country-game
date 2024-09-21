using System;
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

        [SerializeField] private int turnCount;
        [SerializeField] private int turnActionPoints;
        [SerializeField] private int maxActionPoints;
        [SerializeField] private TextMeshProUGUI actionPointDisplay;
        [SerializeField] private GameObject confirmPannel;

        public EventHandler<EventArgs> NewTurn;

        private int additionalActionPoints;
        
        public int actionPoints;

        private void Awake()
        {
            turnCount = 1;
            Instance = this;
            confirmPannel.SetActive(false);
        }

        private void Start()
        {
            //ProgressTurn();
            actionPoints = turnActionPoints;
        }

        private void Update()
        {
            actionPointDisplay.text = $"A:{actionPoints}";

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelTurnEnd();
            }
        }

        private void ProgressTurn()
        {
            turnCount++;
            
            if (turnCount%2==0)
            {
                additionalActionPoints++;
            }
            
            NewTurn?.Invoke(this, EventArgs.Empty);
            actionPoints = turnActionPoints+additionalActionPoints;
            actionPoints = Mathf.Clamp(actionPoints, 0, maxActionPoints);
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
            confirmPannel.SetActive(true);
        }

        public void ConfirmTurnEnd()
        {
            confirmPannel.SetActive(false);
            ProgressTurn();
        }

        public void CancelTurnEnd()
        {
            confirmPannel.SetActive(false);
        }
    }
}