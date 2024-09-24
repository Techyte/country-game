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
        [SerializeField] private float displaySpeed;
        [SerializeField] private int diplomaticPowerGain = 20;
        [SerializeField] private Button warButtonPrefab;
        [SerializeField] private Transform warButtonParent;
        [SerializeField] private Transform agreementTextParent;
        [SerializeField] private GameObject agreementText;
        [SerializeField] private GameObject manageTroopsScreen;
        [SerializeField] private RectTransform troopTypeBreakdownObj;
        [SerializeField] private RectTransform infantryBreakdownImage;
        [SerializeField] private RectTransform tanksBreakdownImage;
        [SerializeField] private RectTransform marinesBreakdownImage;
        [SerializeField] private Slider infantrySlider;
        [SerializeField] private Slider tanksSlider;
        [SerializeField] private Slider marinesSlider;
        [SerializeField] private TMP_InputField infantryInputField;
        [SerializeField] private TMP_InputField tanksInputField;
        [SerializeField] private TMP_InputField marinesInputField;
        [SerializeField] private TextMeshProUGUI totalTroopPercentage;
        [SerializeField] private Button changeTroopDistributionButton;
        
        [Space]
        [SerializeField] private Image influencedFlag;
        [SerializeField] private TextMeshProUGUI influencedToolTip;

        public int diplomaticPower;

        private bool playerNationSelected;

        private void Awake()
        {
            Instance = this;
            playerNationDisplay.position = displayStart.position;
            manageTroopsScreen.SetActive(false);
            TurnManager.Instance.NewTurn += NewTurn;
            startingBreakdownWidth = troopTypeBreakdownObj.rect.width;

            infantrySlider.onValueChanged.AddListener(arg0 =>
            {
                InfantrySliderMoved(arg0);
            });
                
            infantryInputField.onValueChanged.AddListener(arg0 =>
            {
                InfantryTextChanged(arg0);
            });

            tanksSlider.onValueChanged.AddListener(arg0 =>
            {
                TanksSliderMoved(arg0);
            });
                
            tanksInputField.onValueChanged.AddListener(arg0 =>
            {
                TanksTextChanged(arg0);
            });

            marinesSlider.onValueChanged.AddListener(arg0 =>
            {
                MarinesSliderMoved(arg0);
            });
                
            marinesInputField.onValueChanged.AddListener(arg0 =>
            {
                MarinesTextChanged(arg0);
            });
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
            manageTroopsScreen.SetActive(false);
            manageTroopsScreen.SetActive(false);
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
        }

        private void SetupTroopUI()
        {
            infantrySlider.value = PlayerNation.infantry;
            infantryInputField.text = (PlayerNation.infantry*100).ToString();
            
            tanksSlider.value = PlayerNation.tanks;
            tanksInputField.text = (PlayerNation.tanks*100).ToString();
            
            marinesSlider.value = PlayerNation.marines;
            marinesInputField.text = (PlayerNation.marines*100).ToString();
        }

        private float startingBreakdownWidth;

        private void UpdateTroopUI()
        {
            float currentInfantry = infantrySlider.value;
            float currentTanks = tanksSlider.value;
            float currentMarines = marinesSlider.value;

            infantryBreakdownImage.sizeDelta = new Vector2(startingBreakdownWidth * currentInfantry, infantryBreakdownImage.rect.height);
            tanksBreakdownImage.sizeDelta = new Vector2(startingBreakdownWidth * currentTanks, tanksBreakdownImage.rect.height);
            marinesBreakdownImage.sizeDelta = new Vector2(startingBreakdownWidth * currentMarines, marinesBreakdownImage.rect.height);

            float total = ((currentInfantry + currentTanks + currentMarines) * 100);

            totalTroopPercentage.text = total.ToString();

            bool availableToChange = false;
            if (!Mathf.Approximately(currentInfantry, PlayerNation.infantry))
            {
                availableToChange = true;
            }
            if (!Mathf.Approximately(currentTanks, PlayerNation.tanks))
            {
                availableToChange = true;
            }
            if (!Mathf.Approximately(currentMarines, PlayerNation.marines))
            {
                availableToChange = true;
            }

            changeTroopDistributionButton.interactable = availableToChange && Mathf.Approximately(total, 100);

            totalTroopPercentage.color = Mathf.Approximately(total, 100) ? availableToChange ? Color.blue : Color.black : Color.red;
        }

        public void ChangeTroopDistribution()
        {
            if (!TurnManager.Instance.CanPerformAction())
            {
                return;
            }
            
            TurnManager.Instance.PerformedAction();
            
            float currentInfantry = infantrySlider.value;
            float currentTanks = tanksSlider.value;
            float currentMarines = marinesSlider.value;

            PlayerNation.infantry = currentInfantry;
            PlayerNation.tanks = currentTanks;
            PlayerNation.marines = currentMarines;
            
            UpdateTroopUI();
        }

        public void InfantrySliderMoved(float value)
        {
            infantryInputField.text = (value * 100).ToString();
            UpdateTroopUI();
        }

        public void InfantryTextChanged(string value)
        {
            if (float.TryParse(value, out float result))
            {
                infantrySlider.value = result/100;
            }
            UpdateTroopUI();
        }

        public void TanksSliderMoved(float value)
        {
            tanksInputField.text = (value * 100).ToString();
            UpdateTroopUI();
        }

        public void TanksTextChanged(string value)
        {
            if (float.TryParse(value, out float result))
            {
                tanksSlider.value = result/100;
            }
            UpdateTroopUI();
        }

        public void MarinesSliderMoved(float value)
        {
            marinesInputField.text = (value * 100).ToString();
            UpdateTroopUI();
        }

        public void MarinesTextChanged(string value)
        {
            if (float.TryParse(value, out float result))
            {
                marinesSlider.value = result/100;
            }
            UpdateTroopUI();
        }

        public void SetPlayerNation(Nation playerNation)
        {
            PlayerNation = playerNation;
            playerNation.BecomePlayerNation();
            SetupUI();
        }

        public void ClickedPlayerNation()
        {
            playerNationSelected = true;
            CountrySelector.Instance.ResetSelected();
            TroopMover.Instance.ResetSelected();
            AgreementCreator.Instance.CloseAgreementScreen();
            SetupUI();
        }

        public void ClickedManageTroops()
        {
            manageTroopsScreen.SetActive(true);
            SetupTroopUI();
            UpdateTroopUI();
        }

        public void CloseManageTroops()
        {
            manageTroopsScreen.SetActive(false);
        }
    }
}