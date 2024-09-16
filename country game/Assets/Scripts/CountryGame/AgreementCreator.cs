namespace CountryGame
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections.Generic;
    using System.Linq;

    public class AgreementCreator : MonoBehaviour
    {
        public static AgreementCreator Instance;
        
        private Nation secondaryNation;
        
        [SerializeField] private Toggle nation1Head, nation2Head;
        [SerializeField] private Slider influenceLevelSlider;
        [SerializeField] private TextMeshProUGUI influenceLevelDisplay;
        [SerializeField] private GameObject agreementCreatorScreen;
        [SerializeField] private Image colourButton;

        [Space] 
        [SerializeField] private Toggle nonAggression;
        [SerializeField] private Toggle militaryAccess;
        [SerializeField] private Toggle autoJoinWar;
        [SerializeField] private Slider influence;
        [SerializeField] private TMP_InputField agreementNameInput;
        [SerializeField] private Image flag1, flag2;
        [SerializeField] private FlexibleColorPicker colourPicker;

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

            colourButton.color = colourPicker.color;
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

        public void ToggleColourSelection()
        {
            colourPicker.gameObject.SetActive(!colourPicker.gameObject.activeSelf);
        }
    }

}