using System;

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

        [Space] 
        [SerializeField] private Toggle nonAggression;
        [SerializeField] private Toggle militaryAccess;
        [SerializeField] private Toggle autoJoinWar;
        [SerializeField] private Slider influence;
        [SerializeField] private Image flag1, flag2;

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

            switch (influenceLevelSlider.value)
            {
                case 0:
                    influenceLevelDisplay.text = $"{PlayerNationManager.Instance.PlayerNation.Name} does not influence {secondaryNation.Name}";
                    break;
                case 1:
                    influenceLevelDisplay.text = $"{PlayerNationManager.Instance.PlayerNation.Name} minimally influences {secondaryNation.Name}";
                    break;
                case 2:
                    influenceLevelDisplay.text = $"{PlayerNationManager.Instance.PlayerNation.Name} influences {secondaryNation.Name}";
                    break;
                case 3:
                    influenceLevelDisplay.text = $"{PlayerNationManager.Instance.PlayerNation.Name} completely influences {secondaryNation.Name}";
                    break;
            }
        }

        private void UpdateUI()
        {
            nonAggression.isOn = false;
            militaryAccess.isOn = false;
            autoJoinWar.isOn = false;
            influence.value = 0;
            nation2Head.isOn = false;
            nation1Head.isOn = true;
            influenceLevelDisplay.text = $"{PlayerNationManager.Instance.PlayerNation.Name} does not influence {secondaryNation.Name}";
            flag1.sprite = PlayerNationManager.Instance.PlayerNation.flag;
            flag2.sprite = secondaryNation.flag;
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
            UpdateUI();
            agreementCreatorScreen.SetActive(true);
        }

        public void SendAgreementRequest()
        {
            agreementCreatorScreen.SetActive(false);

            Agreement agreement = new Agreement();
            agreement.Name = "New Agreement";
            agreement.influence = (int)influence.value;
            agreement.militaryAccess = militaryAccess.isOn;
            agreement.autoJoinWar = autoJoinWar.isOn;
            agreement.nonAgression = nonAggression.isOn;
            
            NationManager.Instance.NewAgreement(agreement);
            
            NationManager.Instance.NationJoinAgreement(PlayerNationManager.Instance.PlayerNation, agreement);
            NationManager.Instance.NationJoinAgreement(secondaryNation, agreement);
        }
    }

}