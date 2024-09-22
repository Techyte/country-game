using System;
using System.Collections.Generic;

namespace CountryGame
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlayerNationManager : MonoBehaviour
    {
        public static PlayerNationManager Instance;

        public static Nation PlayerNation { get; private set; }
        [SerializeField] private Image flagImage;
        [SerializeField] private TextMeshProUGUI countryName;
        [SerializeField] private Transform playerNationDisplay;
        [SerializeField] private Transform displayStart, displayEnd;
        [SerializeField] private TextMeshProUGUI diplomaticPowerDisplay;
        [SerializeField] private TextMeshProUGUI troopNumDisplay;
        [SerializeField] private float displaySpeed;
        [SerializeField] private int diplomaticPowerGain = 20;
        [SerializeField] private Button warButtonPrefab;
        [SerializeField] private Transform warButtonParent;
        [SerializeField] private Transform agreementTextParent;
        [SerializeField] private GameObject agreementText;
        
        [Space]
        [SerializeField] private Image influencedFlag;
        [SerializeField] private TextMeshProUGUI influencedToolTip;

        public int diplomaticPower;

        private bool playerNationSelected;

        private void Awake()
        {
            Instance = this;
            playerNationDisplay.position = displayStart.position;
            TurnManager.Instance.NewTurn += NewTurn;
        }

        private void NewTurn(object sender, EventArgs e)
        {
            diplomaticPower += diplomaticPowerGain;
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
            
            UpdateUI();
        }

        public void ResetSelected()
        {
            playerNationSelected = false;
        }
        
        private List<GameObject> currentAgreementDisplays = new List<GameObject>();
        private List<GameObject> currentWarButtons = new List<GameObject>();
        private void SetupUI()
        {
            flagImage.sprite = PlayerNation.flag;
            countryName.text = PlayerNation.Name;
            
            
            foreach (var factionDisplay in currentAgreementDisplays)
            {
                Destroy(factionDisplay);
            }
            
            foreach (var warButton in currentWarButtons)
            {
                Destroy(warButton);
            }

            for (int i = 0; i < PlayerNation.agreements.Count; i++)
            {
                int index = i;
                Agreement agreement = PlayerNation.agreements[i];
                
                TextMeshProUGUI factionNameText = Instantiate(agreementText, agreementTextParent).GetComponent<TextMeshProUGUI>();
                factionNameText.text = agreement.Name;
                factionNameText.color = agreement.Color;
                    
                factionNameText.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    CountrySelector.Instance.OpenAgreementScreen(agreement);
                });
                    
                currentAgreementDisplays.Add(factionNameText.gameObject);
            }
            
            if (PlayerNation.agreements.Count == 0)
            {
                TextMeshProUGUI nonAlignedText = Instantiate(agreementText, agreementTextParent).GetComponent<TextMeshProUGUI>();
                nonAlignedText.text = "None";
                nonAlignedText.color = Color.black;
                
                currentAgreementDisplays.Add(nonAlignedText.gameObject);
            }
            
            foreach (var war in PlayerNation.Wars)
            {
                Button warButton = Instantiate(warButtonPrefab, warButtonParent);
                warButton.GetComponentInChildren<TextMeshProUGUI>().text = war.Name;
                
                warButton.onClick.AddListener(() =>
                {
                    CountrySelector.Instance.OpenWarScreen(war);
                });
                
                currentWarButtons.Add(warButton.gameObject);
            }
            
            int influence = PlayerNation.HighestInfluence(out Nation influencer);

            if (influence > 0)
            {
                influencedFlag.sprite = influencer.flag;
                influencedFlag.color = Color.white;
                switch (influence)
                {
                    case 1:
                        influencedToolTip.text = $"Minimally Influenced by {influencer.Name}";
                        break;
                    case 2:
                        influencedToolTip.text = $"Influenced by {influencer.Name}";
                        break;
                    case 3:
                        influencedToolTip.text = $"Completely Influenced by {influencer.Name}";
                        break;
                }
            }
            else
            {
                influencedToolTip.text = "Non influenced";
                influencedFlag.sprite = PlayerNation.flag;
            }
        }

        private void UpdateUI()
        {
            diplomaticPowerDisplay.text = $"DPP: {diplomaticPower}";
            troopNumDisplay.text = $"TRPS: {PlayerNation.TotalTroopCount()}";
        }

        public void SetPlayerNation(Nation playerNation)
        {
            PlayerNation = playerNation;
            playerNation.BecomePlayerNation();
        }

        public void ClickedPlayerNation()
        {
            playerNationSelected = true;
            CountrySelector.Instance.ResetSelected();
            TroopMover.Instance.ResetSelected();
            AgreementCreator.Instance.CloseAgreementScreen();
            SetupUI();
        }
    }
}