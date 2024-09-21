using System;
using System.Collections.Generic;
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

        private bool open;

        private Country source;
        private Country target;
        
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
            countryTroopInformationDisplay.transform.position = start.position;
        }

        public void ResetSelected()
        {
            open = false;
        }

        private List<GameObject> troopDisplays = new List<GameObject>();
        private void DisplayCountryTroops(Country countryClicked)
        {
            foreach (var display in troopDisplays)
            {
                Destroy(display);
            }

            foreach (var info in countryClicked.troopInfos.Values)
            {
                GameObject troopDisplay = Instantiate(troopDisplayPrefab, troopDisplayParent);

                TextMeshProUGUI nation = troopDisplay.GetComponentsInChildren<TextMeshProUGUI>()[0];
                TextMeshProUGUI count = troopDisplay.GetComponentsInChildren<TextMeshProUGUI>()[1];

                nation.text = info.ControllerNation.Name;
                count.text = $"{info.NumberOfTroops} Troops";
                
                troopDisplays.Add(troopDisplay);
            }

            countryName.text = countryClicked.countryName;
            controllerName.text = countryClicked.GetNation().Name;
            controllerFlag.sprite = countryClicked.GetNation().flag;
        }
    }
}