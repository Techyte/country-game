namespace CountryGame
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlayerNationManager : MonoBehaviour
    {
        public static PlayerNationManager Instance;

        public Nation PlayerNation { get; private set; }
        [SerializeField] private Image flagImage;
        [SerializeField] private TextMeshProUGUI countryName;
        [SerializeField] private Transform playerNationDisplay;
        [SerializeField] private Transform displayStart, displayEnd;
        [SerializeField] private float displaySpeed;

        private bool playerNationSelected;

        private void Awake()
        {
            Instance = this;
            playerNationDisplay.position = displayStart.position;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSelected();
            }
            
            if (playerNationSelected)
            {
                playerNationDisplay.position = Vector3.Lerp(playerNationDisplay.position, displayEnd.position, displaySpeed * Time.deltaTime);
            }
            else
            {
                playerNationDisplay.position = Vector3.Lerp(playerNationDisplay.position, displayStart.position, displaySpeed * Time.deltaTime);
            }
        }

        public void ResetSelected()
        {
            playerNationSelected = false;
        }

        private void SetupUI()
        {
            flagImage.sprite = PlayerNation.flag;
            countryName.text = PlayerNation.Name;
        }

        public void SetPlayerNation(Nation playerNarion)
        {
            PlayerNation = playerNarion;
            SetupUI();
        }

        public void ClickedPlayerNation()
        {
            playerNationSelected = true;
            CountrySelector.Instance.ResetSelected();
        }
    }
}