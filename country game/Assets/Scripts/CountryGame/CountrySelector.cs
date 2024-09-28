namespace CountryGame
{
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class CountrySelector : MonoBehaviour
    {
        public static CountrySelector Instance;
        
        [SerializeField] private Transform titleCard;
        [SerializeField] private float titleSpeed;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform agreementTextParent;
        [SerializeField] private GameObject agreementText;
        [SerializeField] private Image flagImage;
        [SerializeField] private Transform titleStartPos, titleEndPos;
        [SerializeField] private Transform agreementMembersParent;
        [SerializeField] private GameObject agreementMemberPrefab;
        [SerializeField] private TextMeshProUGUI agreementName;
        [SerializeField] private Transform agreementScreen;
        [SerializeField] private Transform warScreen;
        [SerializeField] private Transform warDefendersParent;
        [SerializeField] private Transform warBelligerentsParent;
        [SerializeField] private TextMeshProUGUI warName;
        [SerializeField] private Button declareWarButton;
        [SerializeField] private GameObject declareWarConfirmationScreen;
        [SerializeField] private Image influencedFlag;
        [SerializeField] private TextMeshProUGUI influencedToolTip;
        [SerializeField] private Image nonAgressionImage;
        [SerializeField] private Image militaryAccessImage;
        [SerializeField] private Image autoJoinWarsImage;
        [SerializeField] private Sprite cross;
        [SerializeField] private Sprite tick;
        [SerializeField] private Button leaveButton;
        [SerializeField] private GameObject leaveAgreementConfirmation;
        [SerializeField] private Button warButtonPrefab;
        [SerializeField] private Transform warButtonParent;
        [SerializeField] private GameObject joinWarDisplay;
        [SerializeField] private TextMeshProUGUI sideToJoinText;

        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;

        private bool _countrySelected;
        private bool _agreementScreen;
        private bool _warScreen;

        private Nation _currentNation;

        private void Awake()
        {
            Instance = this;
            titleCard.position = titleStartPos.position;
            agreementScreen.position = titleStartPos.position;
            declareWarConfirmationScreen.SetActive(false);
            joinWarDisplay.SetActive(false);
            leaveAgreementConfirmation.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSelected();
            }
            
            if (_countrySelected)
            {
                titleCard.position = Vector3.Lerp(titleCard.position, titleEndPos.position, titleSpeed * Time.deltaTime);
            }
            else
            {
                titleCard.position = Vector3.Lerp(titleCard.position, titleStartPos.position, titleSpeed * Time.deltaTime);
            }
            
            if (_agreementScreen)
            {
                agreementScreen.position = Vector3.Lerp(agreementScreen.position, titleEndPos.position, titleSpeed * Time.deltaTime);
            }
            else
            {
                agreementScreen.position = Vector3.Lerp(agreementScreen.position, titleStartPos.position, titleSpeed * Time.deltaTime);
            }
            
            if (_warScreen)
            {
                warScreen.position = Vector3.Lerp(warScreen.position, titleEndPos.position, titleSpeed * Time.deltaTime);
            }
            else
            {
                warScreen.position = Vector3.Lerp(warScreen.position, titleStartPos.position, titleSpeed * Time.deltaTime);
            }
        }

        private List<GameObject> currentAgreementDisplays = new List<GameObject>();
        private List<GameObject> currentWarButtons = new List<GameObject>();
        public void Clicked(Nation nationSelected)
        {
            _currentNation = nationSelected;
            if (nationSelected == PlayerNationManager.PlayerNation)
            {
                PlayerNationManager.Instance.ClickedPlayerNation();
                return;
            }
            PlayerNationManager.Instance.ResetSelected();
            TroopMover.Instance.ResetSelected();
            AgreementCreator.Instance.CloseAgreementScreen();
            NetworkManager.Instance.ResetSelected();
            
            _agreementScreen = false;
            _countrySelected = true;
            _warScreen = false;
            titleCard.position = titleStartPos.position;
            titleText.text = nationSelected.Name;

            declareWarButton.interactable = CanDeclareWar(PlayerNationManager.PlayerNation, nationSelected);

            foreach (var factionDisplay in currentAgreementDisplays)
            {
                Destroy(factionDisplay);
            }
            
            foreach (var warButton in currentWarButtons)
            {
                Destroy(warButton);
            }

            for (int i = 0; i < nationSelected.agreements.Count; i++)
            {
                int index = i;
                Agreement agreement = nationSelected.agreements[i];
                
                TextMeshProUGUI factionNameText = Instantiate(agreementText, agreementTextParent).GetComponent<TextMeshProUGUI>();
                factionNameText.text = agreement.Name;
                factionNameText.color = agreement.Color;
                    
                factionNameText.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OpenAgreementScreen(index);
                });
                    
                currentAgreementDisplays.Add(factionNameText.gameObject);
            }
            
            if (nationSelected.agreements.Count == 0)
            {
                TextMeshProUGUI nonAlignedText = Instantiate(agreementText, agreementTextParent).GetComponent<TextMeshProUGUI>();
                nonAlignedText.text = "None";
                nonAlignedText.color = Color.black;
                
                currentAgreementDisplays.Add(nonAlignedText.gameObject);
            }

            foreach (var war in nationSelected.Wars)
            {
                Button warButton = Instantiate(warButtonPrefab, warButtonParent);
                warButton.GetComponentInChildren<TextMeshProUGUI>().text = war.Name;
                
                warButton.onClick.AddListener(() =>
                {
                    OpenWarScreen(war);
                });
                
                currentWarButtons.Add(warButton.gameObject);
            }

            flagImage.sprite = nationSelected.flag;
            
            int influence = nationSelected.HighestInfluence(out Nation influencer);

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
                influencedFlag.sprite = nationSelected.flag;
            }
        }

        private bool CanDeclareWar(Nation nationToDeclare, Nation nationDeclaredOn)
        {
            bool canDeclareWar = true;

            foreach (var agreement in nationToDeclare.agreements)
            {
                if (agreement.nonAgression && agreement.Nations.Contains(nationDeclaredOn))
                {
                    canDeclareWar = false;
                }
            }

            foreach (var war in nationToDeclare.Wars)
            {
                if (war.Belligerents.Contains(nationDeclaredOn) || war.Defenders.Contains(nationDeclaredOn))
                {
                    canDeclareWar = false;
                }
            }
            
            foreach (var war in nationDeclaredOn.Wars)
            {
                if (war.Belligerents.Contains(nationToDeclare) || war.Defenders.Contains(nationToDeclare))
                {
                    canDeclareWar = false;
                }
            }

            return canDeclareWar;
        }

        private List<GameObject> currentAgreementMembers = new List<GameObject>();

        private void DisplayAgreementMembers(Agreement agreement)
        {
            foreach (var oldMember in currentAgreementMembers)
            {
                Destroy(oldMember);
            }
            
            foreach (var nation in agreement.Nations)
            {
                GameObject obj = Instantiate(agreementMemberPrefab, agreementMembersParent);

                obj.GetComponentInChildren<TextMeshProUGUI>().text = nation.Name;
                obj.GetComponentsInChildren<Image>()[1].sprite = nation.flag;
                
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Clicked(nation);
                });
                
                currentAgreementMembers.Add(obj);
            }
        }

        public void ResetSelected()
        {
            _currentNation = null;
            _countrySelected = false;
            _agreementScreen = false;
            _warScreen = false;
            declareWarConfirmationScreen.SetActive(false);
            joinWarDisplay.SetActive(false);
            leaveAgreementConfirmation.SetActive(false);
        }

        public void OpenAgreementScreen(int factionIndex)
        {
            OpenAgreementScreen(_currentNation.agreements[factionIndex]);
        }
        
        private List<GameObject> currentWarMembers = new List<GameObject>();

        public void DisplayWarMembers(War war)
        {
            foreach (var oldMember in currentWarMembers)
            {
                Destroy(oldMember);
            }
            
            foreach (var nation in war.Defenders)
            {
                GameObject obj = Instantiate(agreementMemberPrefab, warDefendersParent);

                obj.GetComponentInChildren<TextMeshProUGUI>().text = nation.Name;
                obj.GetComponentsInChildren<Image>()[1].sprite = nation.flag;
                
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Clicked(nation);
                });
                
                currentWarMembers.Add(obj);
            }
            
            foreach (var nation in war.Belligerents)
            {
                GameObject obj = Instantiate(agreementMemberPrefab, warBelligerentsParent);

                obj.GetComponentInChildren<TextMeshProUGUI>().text = nation.Name;
                obj.GetComponentsInChildren<Image>()[1].sprite = nation.flag;
                
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Clicked(nation);
                });
                
                currentWarMembers.Add(obj);
            }
        }

        private War currentSelectedWar;

        public void OpenWarScreen(War war)
        {
            _agreementScreen = false;
            _countrySelected = false;
            _warScreen = true;

            currentSelectedWar = war;

            DisplayWarMembers(war);
            warName.text = war.Name;
            PlayerNationManager.Instance.ResetSelected();
        }

        private bool joinBelligerents;

        public void ClickedJoinDefenders()
        {
            joinBelligerents = false;
            joinWarDisplay.SetActive(true);
            sideToJoinText.text = "Side: Defenders";
        }
        
        public void ClickedJoinBelligerents()
        {
            joinBelligerents = true;
            joinWarDisplay.SetActive(true);
            sideToJoinText.text = "Side: Belligerents";
        }

        public void ConfirmedJoinWar()
        {
            if (!TurnManager.Instance.CanPerformAction())
            {
                return;
            }
            
            if (joinBelligerents)
            {
                JoinWarBelligerents();
            }
            else
            {
                JoinWarDefenders();
            }
            
            TurnManager.Instance.PerformedAction();
            joinWarDisplay.SetActive(false);
        }

        public void CancelJoinWar()
        {
            joinWarDisplay.SetActive(false);
        }

        private void JoinWarDefenders()
        {
            if (!currentSelectedWar.Defenders.Contains(PlayerNationManager.PlayerNation))
            {
                CombatManager.Instance.NationJoinWarDefenders(PlayerNationManager.PlayerNation, currentSelectedWar);
                
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"To War!", $"Today, {PlayerNationManager.PlayerNation.Name} joined the {currentSelectedWar.Name}, allying themselves with the defenders", () => {CountrySelector.Instance.OpenWarScreen(currentSelectedWar);}, 5);
            }
        }

        private void JoinWarBelligerents()
        {
            if (!currentSelectedWar.Belligerents.Contains(PlayerNationManager.PlayerNation))
            {
                CombatManager.Instance.NationJoinWarBelligerents(PlayerNationManager.PlayerNation, currentSelectedWar);
                
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"To War!", $"Today, {PlayerNationManager.PlayerNation.Name} joined the {currentSelectedWar.Name}, allying themselves with the belligerents", () => {CountrySelector.Instance.OpenWarScreen(currentSelectedWar);}, 5);
            }
        }

        private Agreement currentAgreement = null;
        
        public void OpenAgreementScreen(Agreement agreement)
        {
            currentAgreement = agreement;
            PlayerNationManager.Instance.ResetSelected();
            DisplayAgreementMembers(agreement);
            _agreementScreen = true;
            _countrySelected = false;
            AgreementCreator.Instance.CloseAgreementScreen();
            agreementName.text = agreement.Name;
            nonAgressionImage.sprite = agreement.nonAgression ? tick : cross;
            militaryAccessImage.sprite = agreement.militaryAccess ? tick : cross;
            autoJoinWarsImage.sprite = agreement.autoJoinWar ? tick : cross;
            leaveButton.gameObject.SetActive(true);
            leaveButton.interactable = agreement.Age() >= 3;
            leaveButton.gameObject.SetActive(agreement.Nations.Contains(PlayerNationManager.PlayerNation));
        }

        public void ClickedLeaveAgreement()
        {
            leaveAgreementConfirmation.SetActive(true);
        }

        public void ConfirmedLeaveAgreement()
        {
            leaveAgreementConfirmation.SetActive(false);
            
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init($"Breaking Ties!", $"Today, {PlayerNationManager.PlayerNation.Name} left the {currentAgreement.Name} agreement, searching to forge its own path!", () => {Clicked(PlayerNationManager.PlayerNation);}, 5);
            
            NationManager.Instance.NationLeaveAgreement(PlayerNationManager.PlayerNation, currentAgreement, false);
            ResetSelected();
        }

        public void CancelLeaveAgreement()
        {
            leaveAgreementConfirmation.SetActive(false);
        }

        public void BeginCreatingAgreement()
        {
            AgreementCreator.Instance.OpenAgreementScreen(_currentNation);
            ResetSelected();
        }

        public void DeclareWar()
        {
            declareWarConfirmationScreen.SetActive(true);
        }

        public void ConfirmedDeclareWar()
        {
            if (!TurnManager.Instance.CanPerformAction())
            {
                return;
            }
            
            TurnManager.Instance.PerformedAction();
            CombatManager.Instance.DeclareWarOn(_currentNation);
            ResetSelected();
            declareWarConfirmationScreen.SetActive(false);
        }

        public void CancelDeclareWar()
        {
            declareWarConfirmationScreen.SetActive(false);
        }
    }
}