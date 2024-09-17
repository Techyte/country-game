using System.Collections.Generic;

namespace CountryGame
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class AgreementCreator : MonoBehaviour
    {
        public static AgreementCreator Instance;
        
        private Nation secondaryNation;
        
        [SerializeField] private Toggle nation1Head, nation2Head;
        [SerializeField] private Slider influenceLevelSlider;
        [SerializeField] private TextMeshProUGUI influenceLevelDisplay;
        [SerializeField] private GameObject agreementCreatorScreen;
        [SerializeField] private Image colourButton;
        [SerializeField] private Transform agreementTextParent;
        [SerializeField] private Transform agreementText;
        [SerializeField] private Button preexistingAgreementSendButton;

        [Space] 
        [SerializeField] private Toggle nonAggression;
        [SerializeField] private Toggle militaryAccess;
        [SerializeField] private Toggle autoJoinWar;
        [SerializeField] private Slider influence;
        [SerializeField] private TMP_InputField agreementNameInput;
        [SerializeField] private Image flag1, flag2;
        [SerializeField] private FlexibleColorPicker colourPicker;

        private int preexistingAgreementSelected = -1;

        private void Awake()
        {
            Instance = this;
            agreementCreatorScreen.SetActive(false);
        }

        private void Update()
        {
            if (secondaryNation == null)
            {
                secondaryNation = NationManager.Instance.GetNationByName("France");
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseAgreementScreen();
                ResetUI();
            }
            
            UpdateUI();
        }

        private void ResetUI()
        {
            nonAggression.isOn = false;
            militaryAccess.isOn = false;
            autoJoinWar.isOn = false;
            influence.value = 0;
            nation2Head.isOn = false;
            nation1Head.isOn = true;
            preexistingAgreementSelected = -1;
            colourPicker.gameObject.SetActive(false);
            influenceLevelDisplay.text = $"{PlayerNationManager.Instance.PlayerNation.Name} does not influence {secondaryNation.Name}";
            flag1.sprite = PlayerNationManager.Instance.PlayerNation.flag;
            flag2.sprite = secondaryNation.flag;
        }

        private void UpdateUI()
        {
            string influencedNation =
                nation1Head.isOn ? secondaryNation.Name : PlayerNationManager.Instance.PlayerNation.Name;
            string influencerNation =
                nation1Head.isOn ? PlayerNationManager.Instance.PlayerNation.Name : secondaryNation.Name;
            
            switch (influenceLevelSlider.value)
            {
                case 0:
                    influenceLevelDisplay.text = $"{influencerNation} does not influence {influencedNation}";
                    break;
                case 1:
                    influenceLevelDisplay.text = $"{influencerNation} minimally influences {influencedNation}";
                    break;
                case 2:
                    influenceLevelDisplay.text = $"{influencerNation} influences {influencedNation}";
                    break;
                case 3:
                    influenceLevelDisplay.text = $"{influencerNation} completely influences {influencedNation}";
                    break;
            }

            preexistingAgreementSendButton.interactable = preexistingAgreementSelected != -1;

            colourButton.color = colourPicker.color;
        }
        
        private List<GameObject> currentAgreementDisplays = new List<GameObject>();
        private void SpawnExistingAgreements()
        {
            foreach (var factionDisplay in currentAgreementDisplays)
            {
                Destroy(factionDisplay);
            }
            
            for (int i = 0; i < PlayerNationManager.Instance.PlayerNation.agreements.Count; i++)
            {
                int index = i;
                Agreement agreement = PlayerNationManager.Instance.PlayerNation.agreements[i];
                
                TextMeshProUGUI factionNameText = Instantiate(agreementText, agreementTextParent).GetComponent<TextMeshProUGUI>();
                factionNameText.text = agreement.Name;
                factionNameText.color = agreement.Color;
                    
                factionNameText.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    preexistingAgreementSelected = index;
                });
                    
                currentAgreementDisplays.Add(factionNameText.gameObject);
            }
        }

        public void Head1ValueChanged(bool value)
        {
            nation2Head.isOn = !value;
        }
        
        public void Head2ValueChanged(bool value)
        {
            nation1Head.isOn = !value;
        }

        public void OpenAgreementScreen(Nation nation2)
        {
            secondaryNation = nation2;
            agreementCreatorScreen.SetActive(true);
            ResetUI();
            SpawnExistingAgreements();
        }

        public void CloseAgreementScreen()
        {
            agreementCreatorScreen.SetActive(false);
        }

        public void SendAgreementRequest()
        {
            CloseAgreementScreen();

            Agreement agreement = new Agreement();
            agreement.Name = agreementNameInput.text;
            agreement.influence = (int)influence.value;
            agreement.militaryAccess = militaryAccess.isOn;
            agreement.autoJoinWar = autoJoinWar.isOn;
            agreement.nonAgression = nonAggression.isOn;
            agreement.Color = colourPicker.color;
            
            NationManager.Instance.NewAgreement(agreement);
            
            NationManager.Instance.NationJoinAgreement(PlayerNationManager.Instance.PlayerNation, agreement);
            NationManager.Instance.NationJoinAgreement(secondaryNation, agreement);
        }

        public void SendExistingAgreementRequest()
        {
            CloseAgreementScreen();
            NationManager.Instance.NationJoinAgreement(secondaryNation, PlayerNationManager.Instance.PlayerNation.agreements[preexistingAgreementSelected]);
        }

        public void ToggleColourSelection()
        {
            colourPicker.gameObject.SetActive(!colourPicker.gameObject.activeSelf);
        }
    }

}