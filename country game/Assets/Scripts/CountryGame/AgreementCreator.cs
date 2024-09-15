namespace CountryGame
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class AgreementCreator : MonoBehaviour
    {
        private Nation secondaryNation;
        
        [SerializeField] private Toggle nation1Head, nation2Head;
        [SerializeField] private Slider influenceLevelSlider;
        [SerializeField] private TextMeshProUGUI influenceLevelDisplay;

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

        public void Head1ValueChanged(bool value)
        {
            nation2Head.isOn = !value;
        }

        
        public void Head2ValueChanged(bool value)
        {
            nation1Head.isOn = !value;
        }
    }

}