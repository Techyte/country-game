using Riptide;

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
        [SerializeField] private Vector3 titleStartPos;
        [SerializeField] private Transform titleEndPos;
        [SerializeField] private Transform agreementMembersParent;
        [SerializeField] private GameObject agreementMemberPrefab;
        [SerializeField] private TextMeshProUGUI agreementName;
        [SerializeField] private Transform agreementScreen;
        [SerializeField] private Transform warScreen;
        [SerializeField] private Transform warDefendersParent;
        [SerializeField] private Transform warBelligerentsParent;
        [SerializeField] private TextMeshProUGUI warName;
        [SerializeField] private Button declareWarButton;
        [SerializeField] private Button signAgreementButton;
        [SerializeField] private GameObject declareWarConfirmationScreen;
        [SerializeField] private GameObject inviteToWarScreen;
        [SerializeField] private Transform warInviteButtonParent;
        [SerializeField] private GameObject warInviteButton;
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
        [SerializeField] private Button joinDefendersButton;
        [SerializeField] private Button joinBelligerentsButton;

        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;

        private bool _countrySelected;
        private bool _agreementScreen;
        private bool _warScreen;

        private Nation _currentNation;

        private void Awake()
        {
            Instance = this;
            titleStartPos = titleCard.position;
            declareWarConfirmationScreen.SetActive(false);
            joinWarDisplay.SetActive(false);
            leaveAgreementConfirmation.SetActive(false);
            inviteToWarScreen.SetActive(false);
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
                titleCard.position = Vector3.Lerp(titleCard.position, titleStartPos, titleSpeed * Time.deltaTime);
            }
            
            if (_agreementScreen)
            {
                agreementScreen.position = Vector3.Lerp(agreementScreen.position, titleEndPos.position, titleSpeed * Time.deltaTime);
            }
            else
            {
                agreementScreen.position = Vector3.Lerp(agreementScreen.position, titleStartPos, titleSpeed * Time.deltaTime);
            }
            
            if (_warScreen)
            {
                warScreen.position = Vector3.Lerp(warScreen.position, titleEndPos.position, titleSpeed * Time.deltaTime);
            }
            else
            {
                warScreen.position = Vector3.Lerp(warScreen.position, titleStartPos, titleSpeed * Time.deltaTime);
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
            titleCard.position = titleCard.position;
            titleText.text = nationSelected.Name;

            declareWarButton.interactable = CanDeclareWar(PlayerNationManager.PlayerNation, nationSelected);
            signAgreementButton.interactable = CanSignAgreement(PlayerNationManager.PlayerNation, nationSelected);

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

            if (nationToDeclare.IsAtWarWith(nationDeclaredOn))
            {
                canDeclareWar = false;
            }

            if (nationToDeclare.NonAgressionWith(nationDeclaredOn))
            {
                canDeclareWar = false;
            }

            return canDeclareWar;
        }

        private bool CanSignAgreement(Nation nationToDeclare, Nation nationDeclaredOn)
        {
            bool canSignAgreement = true;

            if (nationToDeclare.IsAtWarWith(nationDeclaredOn))
            {
                canSignAgreement = false;
            }

            return canSignAgreement;
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
            inviteToWarScreen.SetActive(false);
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

            joinBelligerentsButton.interactable = !PlayerNationManager.PlayerNation.Wars.Contains(currentSelectedWar);
            joinDefendersButton.interactable = !PlayerNationManager.PlayerNation.Wars.Contains(currentSelectedWar);

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
            if (!TurnManager.Instance.CanPerformAction() || PlayerNationManager.PlayerNation.Wars.Contains(currentSelectedWar))
            {
                return;
            }
            
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.JoinedWar);

            for (int i = 0; i < CombatManager.Instance.wars.Count; i++)
            {
                if (currentSelectedWar == CombatManager.Instance.wars[i])
                {
                    message.AddInt(i);
                }
            }

            message.AddString(PlayerNationManager.PlayerNation.Name);
            
            if (joinBelligerents)
            {
                message.AddBool(false);
            }
            else
            {
                message.AddBool(true);
            }

            NetworkManager.Instance.Client.Send(message);
            
            TurnManager.Instance.PerformedAction();
            joinWarDisplay.SetActive(false);
        }

        public void CancelJoinWar()
        {
            joinWarDisplay.SetActive(false);
        }

        public void JoinWar(War warToJoin, Nation nationToJoin, bool defender)
        {
            if (nationToJoin.Wars.Contains(warToJoin))
            {
                return;
            }
            
            if (defender)
            {
                JoinWarDefenders(nationToJoin, warToJoin);
            }
            else
            {
                JoinWarBelligerents(nationToJoin, warToJoin);
            }
        }

        private void JoinWarDefenders(Nation nationToJoin, War warToJoin)
        {
            if (!warToJoin.Defenders.Contains(nationToJoin))
            {
                CombatManager.Instance.NationJoinWarDefenders(nationToJoin, warToJoin);
                
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"To War!",
                    $"Today, {nationToJoin.Name} joined the {warToJoin.Name}, allying themselves with the defenders",
                    () => { Instance.OpenWarScreen(warToJoin); }, 5);
                
                foreach (var defender in warToJoin.Defenders)
                {
                    defender.UpdateInfluenceColour();
                    defender.UpdateTroopDisplays();
                }
            }
        }

        private void JoinWarBelligerents(Nation nationToJoin, War warToJoin)
        {
            if (!warToJoin.Belligerents.Contains(nationToJoin))
            {
                CombatManager.Instance.NationJoinWarBelligerents(nationToJoin, warToJoin);
                
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"To War!",
                    $"Today, {nationToJoin.Name} joined the {warToJoin.Name}, allying themselves with the belligerents",
                    () => { CountrySelector.Instance.OpenWarScreen(warToJoin); }, 5);
                    
                foreach (var belligerent in warToJoin.Belligerents)
                {
                    belligerent.UpdateInfluenceColour();
                    belligerent.UpdateTroopDisplays();
                }
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
            ResetSelected();

            int agreement = 0;
            
            for (int i = 0; i < NationManager.Instance.agreements.Count; i++)
            {
                if (NationManager.Instance.agreements[i] == currentAgreement)
                {
                    agreement = i;
                }
            }
            
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.LeaveAgreement);
            message.AddString(PlayerNationManager.PlayerNation.Name);
            message.AddInt(agreement);

            NetworkManager.Instance.Client.Send(message);
        }

        public void LeaveAgreement(Nation nationToLeave, Agreement agreementToLeave)
        {
            NationManager.Instance.NationLeaveAgreement(nationToLeave, agreementToLeave, false);
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init($"Breaking Ties!",
                $"Today, {nationToLeave.Name} left the {agreementToLeave.Name} agreement, searching to forge its own path!",
                () => { Clicked(nationToLeave); }, 5);
        }

        public void CancelLeaveAgreement()
        {
            leaveAgreementConfirmation.SetActive(false);
        }

        public void BeginCreatingAgreement()
        {
            if (!TurnManager.Instance.CanPerformAction())
            {
                return;
            }
            AgreementCreator.Instance.OpenAgreementScreen(_currentNation);
            ResetSelected();
        }

        public void InviteToWar()
        {
            if (_currentNation.aPlayerNation)
            {
                return;
            }
            inviteToWarScreen.SetActive(true);
            UpdateInviteToWarScreen();
        }

        public void CloseInviteToWar()
        {
            inviteToWarScreen.SetActive(false);
        }

        private int warIndex;

        private void UpdateInviteToWarScreen()
        {
            for (int i = 0; i < PlayerNationManager.PlayerNation.Wars.Count; i++)
            {
                War war = PlayerNationManager.PlayerNation.Wars[i];
                
                GameObject obj = Instantiate(warInviteButton, warInviteButtonParent);
                TextMeshProUGUI[] texts = obj.GetComponentsInChildren<TextMeshProUGUI>();
                Image[] images = obj.GetComponentsInChildren<Image>();
                texts[0].text = war.Name;
                images[1].sprite = war.Belligerents[0].flag;
                images[2].sprite = war.Defenders[0].flag;

                int index = i;
                
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    warIndex = index;
                });
            }
        }

        public void InviteToWarConfirm()
        {
            if (!TurnManager.Instance.CanPerformAction() || _currentNation.aPlayerNation)
            {
                return;
            }
            
            War war = PlayerNationManager.PlayerNation.Wars[warIndex];
            
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.AskToJoinWar);

            for (int i = 0; i < CombatManager.Instance.wars.Count; i++)
            {
                if (war == CombatManager.Instance.wars[i])
                {
                    message.AddInt(i);
                }
            }

            message.AddString(_currentNation.Name);
            message.AddString(PlayerNationManager.PlayerNation.Name);

            NetworkManager.Instance.Client.Send(message);
            
            TurnManager.Instance.PerformedAction();
            
            Debug.Log($"Inviting {_currentNation.Name} to the {war.Name}");
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