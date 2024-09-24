using System;
using System.Collections;
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
        [SerializeField] private int maxActionPoints;
        [SerializeField] private TextMeshProUGUI actionPointDisplay;
        [SerializeField] private GameObject confirmPannel;

        public EventHandler<EventArgs> NewTurn;
        
        public int actionPoints;

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

        private void ProgressTurn()
        {
            currentTurn++;
            
            NewTurn?.Invoke(this, EventArgs.Empty);
            actionPoints = Mathf.Clamp(turnActionPoints, 0, maxActionPoints);
        }

        public IEnumerator TurnProgressDelay()
        {
            yield return new WaitForSeconds(1);
            ProgressTurn();
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