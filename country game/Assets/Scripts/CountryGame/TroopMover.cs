using System;
using System.Collections.Generic;
using Riptide;
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
        [SerializeField] private Slider amountSlider;

        private bool open;
        public Country currentCountry;

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

        private void Start()
        {
            moveTroopDisplay.SetActive(false);
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
            NetworkManager.Instance.ResetSelected();
        }

        public void ResetSelected()
        {
            if (transferring)
            {
                countryTroopInformationDisplay.transform.position = start.position;
            }
            
            open = false;
            transferring = false;
            currentCountry = null;

            source = null;
            target = null;

            nationIndex = 0;
            amount = 1;
            
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

                Button troopDisplayButton = troopDisplay.GetComponent<Button>();
                
                troopDisplayButton.onClick.AddListener(() =>
                {
                    StartTransferringTroops(i);
                });

                bool controllable = true;

                if (info.ControllerNation != PlayerNationManager.PlayerNation)
                {
                    controllable = false;
                    
                    foreach (var agreement in info.ControllerNation.agreements)
                    {
                        if (agreement.influence > 1 && agreement.AgreementLeader == PlayerNationManager.PlayerNation)
                        {
                            // interactable
                            controllable = true;
                        }
                    }
                }
                else
                {
                    if (info.NumberOfTroops == 1)
                    {
                        controllable = false;
                    }
                }
                
                troopDisplayButton.interactable = controllable;

                nation.text = info.ControllerNation.Name;
                count.text = $"{info.NumberOfTroops} Troops";
                amountSlider.value = amount;
                
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
            
            switch (source.DistanceTo(target))
            {
                case < 2f:
                    // close
                    actionPointCost = 1;
                    break;
                case >= 3.3f:
                    actionPointCost = 2;
                    break;
            }
            
            UpdateTroopTransferScreen();
        }

        private int actionPointCost = 1;
        private void UpdateTroopTransferScreen()
        {
            amount = 1;
            sourceNationFlag.sprite = controllers[nationIndex].flag;
            sourceNationName.text = controllers[nationIndex].Name;
            sourceCountryName.text = source.countryName;
            targetCountryName.text = target.countryName;
            costText.text = $"-{actionPointCost} Action Point";
        }

        public void TransferTroops()
        {
            if (!source.CanMoveNumTroopsOut(controllers[nationIndex], amount) || !TurnManager.Instance.CanPerformAction())
            {
                return;
            }

            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.MovedTroops);
            message.AddString(source.countryName);
            message.AddString(target.countryName);
            message.AddString(PlayerNationManager.PlayerNation.Name);
            message.AddInt(amount);

            NetworkManager.Instance.Client.Send(message);
            
            ResetSelected();
            
            TurnManager.Instance.PerformedAction();
        }

        public void TransferTroops(Country source, Country target, Nation controller, int amount)
        {
            if (!source.CanMoveNumTroopsOut(controller, amount))
            {
                return;
            }

            Debug.Log($"Transferring {amount} troops controlled by {controller.Name} from {source.name} to {target.name}");
            
            source.MoveTroopsOut(controller, amount);
            target.MovedTroopsIn(controller, amount);
        }

        public void SwapSourceTarget()
        {
            (source, target) = (target, source);
            UpdateTroopTransferScreen();
        }

        public void UpdateAmountDisplay(float value)
        {
            amount = (int)value;
            amountDisplayText.text = ((int)value).ToString();
        }
    }
}