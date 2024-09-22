using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

namespace CountryGame
{
    using UnityEngine;

    public class TroopMover : MonoBehaviour
    {
        public static TroopMover Instance;

        [SerializeField] private GameObject countryTroopInformationDisplay;
        [SerializeField] private Transform start, end;
        [SerializeField] private float speed;
        [SerializeField] private GameObject troopDisplayPrefab;
        [SerializeField] private Transform troopDisplayParent;
        [SerializeField] private TextMeshProUGUI countryName;
        [SerializeField] private TextMeshProUGUI controllerName;
        [SerializeField] private Image controllerFlag;
        [SerializeField] private GameObject moveTroopDisplay;
        [SerializeField] private GameObject otherGUIParent;
        [SerializeField] private GameObject amountDisplay;
        [SerializeField] private TextMeshProUGUI amountDisplayText;

        [SerializeField] private Image sourceNationFlag;
        [SerializeField] private TextMeshProUGUI sourceNationName;
        [SerializeField] private TextMeshProUGUI sourceCountryName;
        [SerializeField] private TextMeshProUGUI targetCountryName;
        [SerializeField] private TextMeshProUGUI costText;

        private bool open;
        private Country currentCountry;

        public bool transferring;

        private Country source;
        private Country target;

        private int nationIndex;
        private int amount;
        
        private void Awake()
        {
            Instance = this;
            countryTroopInformationDisplay.transform.position = start.position;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSelected();
            }

            if (open)
            {
                countryTroopInformationDisplay.transform.position =
                    Vector3.Lerp(countryTroopInformationDisplay.transform.position, end.position, speed * Time.deltaTime);
            }
            else
            {
                countryTroopInformationDisplay.transform.position =
                    Vector3.Lerp(countryTroopInformationDisplay.transform.position, start.position, speed * Time.deltaTime);
            }
        }

        public void Clicked(Country countryClicked)
        {
            DisplayCountryTroops(countryClicked);
            open = true;
            currentCountry = countryClicked;
            countryTroopInformationDisplay.transform.position = start.position;
            CountrySelector.Instance.ResetSelected();
        }

        public void ResetSelected()
        {
            open = false;
            transferring = false;
            currentCountry = null;

            source = null;
            target = null;

            nationIndex = 0;
            amount = 0;
            
            otherGUIParent.SetActive(true);
            amountDisplay.SetActive(false);
            moveTroopDisplay.SetActive(false);
        }

        private List<GameObject> troopDisplays = new List<GameObject>();

        private List<Nation> controllers = new List<Nation>();
        
        private void DisplayCountryTroops(Country countryClicked)
        {
            foreach (var display in troopDisplays)
            {
                Destroy(display);
            }
            
            controllers.Clear();

            int index = 0;
            foreach (var info in countryClicked.troopInfos.Values)
            {
                int i = index;
                GameObject troopDisplay = Instantiate(troopDisplayPrefab, troopDisplayParent);

                TextMeshProUGUI nation = troopDisplay.GetComponentsInChildren<TextMeshProUGUI>()[0];
                TextMeshProUGUI count = troopDisplay.GetComponentsInChildren<TextMeshProUGUI>()[1];
                
                troopDisplay.GetComponent<Button>().onClick.AddListener(() =>
                {
                    StartTransferringTroops(i);
                });

                nation.text = info.ControllerNation.Name;
                count.text = $"{info.NumberOfTroops} Troops";
                
                troopDisplays.Add(troopDisplay);
                controllers.Add(info.ControllerNation);
                index++;
            }

            countryName.text = countryClicked.countryName;
            controllerName.text = countryClicked.GetNation().Name;
            controllerFlag.sprite = countryClicked.GetNation().flag;
        }

        public void StartTransferringTroops(int index)
        {
            source = currentCountry;
            nationIndex = index;
            moveTroopDisplay.SetActive(true);
            otherGUIParent.SetActive(false);
            transferring = true;
        }

        public void SelectedTransferLocation(Country destination)
        {
            target = destination;
            amountDisplay.SetActive(true);
            UpdateTroopTransferScreen();
        }

        private void UpdateTroopTransferScreen()
        {
            amount = 1;
            sourceNationFlag.sprite = controllers[nationIndex].flag;
            sourceNationName.text = controllers[nationIndex].Name;
            sourceCountryName.text = source.countryName;
            targetCountryName.text = target.countryName;
        }

        public void TransferTroops()
        {
            source.MoveTroopsOut(controllers[nationIndex], amount);
            target.MovedTroopsIn(controllers[nationIndex], amount);
            
            ResetSelected();
        }

        public void UpdateAmountDisplay(float value)
        {
            amount = (int)value;
            amountDisplayText.text = ((int)value).ToString();
        }
    }
}