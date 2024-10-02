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
        [SerializeField] private Vector3 start;
        [SerializeField] private Transform end;
        [SerializeField] private float speed;
        [SerializeField] private GameObject troopDisplayPrefab;
        [SerializeField] private Transform troopDisplayParent;
        [SerializeField] private TextMeshProUGUI countryName;
        [SerializeField] private TextMeshProUGUI controllerName;
        [SerializeField] private Image controllerFlag;
        [SerializeField] private GameObject moveTroopDisplay;
        [SerializeField] private GameObject hireTroopScreen;
        [SerializeField] private TextMeshProUGUI hireTroopActionPointText;
        [SerializeField] private TextMeshProUGUI hireAmountText;
        [SerializeField] private GameObject otherGUIParent;
        [SerializeField] private GameObject amountDisplay;
        [SerializeField] private TextMeshProUGUI amountDisplayText;
        [SerializeField] private Button launchAttackButton;
        [SerializeField] private Button hireTroopsButton;
        [SerializeField] private Slider hireTroopSlider;
        [SerializeField] private TextMeshProUGUI totalText;

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
            start = countryTroopInformationDisplay.transform.position;
        }

        private void Start()
        {
            moveTroopDisplay.SetActive(false);
            hireTroopScreen.SetActive(false);
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
                    Vector3.Lerp(countryTroopInformationDisplay.transform.position, start, speed * Time.deltaTime);
            }

            hireTroopActionPointText.text = $"-{hireTroopSlider.value} Action Points";
            hireAmountText.text = hireTroopSlider.value.ToString();
        }

        public void Clicked(Country countryClicked)
        {
            open = true;
            currentCountry = countryClicked;
            DisplayCountryTroops(countryClicked);
            countryTroopInformationDisplay.transform.position = start;
            CountrySelector.Instance.ResetSelected();
            NetworkManager.Instance.ResetSelected();
        }

        public void ResetSelected()
        {
            if (transferring)
            {
                countryTroopInformationDisplay.transform.position = start;
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
            hireTroopScreen.SetActive(false);
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
            
            launchAttackButton.interactable =
                PlayerNationManager.PlayerNation.MilitaryAccessWith(currentCountry.GetNation());
            hireTroopsButton.interactable =
                PlayerNationManager.PlayerNation.MilitaryAccessWith(currentCountry.GetNation());

            totalText.text = $"Total: {currentCountry.TotalTroopCount().ToString()}/{currentCountry.troopCapacity}";
        }

        public void StartTransferringTroops(int index)
        {
            source = currentCountry;
            nationIndex = index;
            moveTroopDisplay.SetActive(true);
            otherGUIParent.SetActive(false);
            transferring = true;
        }

        public void StartHiringTroops()
        {
            hireTroopScreen.SetActive(true);
        }

        public void StopHiringTroops()
        {
            hireTroopScreen.SetActive(false);
        }
        
        public void ConfirmHiringTroops()
        {
            if (!currentCountry.CanMoveNumTroopsIn(currentCountry.GetNation(), (int)hireTroopSlider.value))
            {
                return;
            }

            if (TurnManager.Instance.actionPoints - (int)hireTroopSlider.value < 0)
            {
                return;
            }
            
            for (int i = 0; i < hireTroopSlider.value; i++)
            {
                TurnManager.Instance.PerformedAction();
            }
            
            hireTroopScreen.SetActive(false);

            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.HiredTroops);
            message.AddString(currentCountry.countryName);
            message.AddString(currentCountry.GetNation().Name);
            message.AddInt((int)hireTroopSlider.value);

            NetworkManager.Instance.Client.Send(message);
        }

        public void SelectedTransferLocation(Country destination)
        {
            if (destination.GetNation().IsAtWarWith(PlayerNationManager.PlayerNation))
            {
                return;
            }
            
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
            if (!source.CanMoveNumTroopsOut(controllers[nationIndex], amount) || !target.CanMoveNumTroopsIn(controllers[nationIndex], amount) || !TurnManager.Instance.CanPerformAction())
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