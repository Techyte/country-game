using System.Linq;

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
        [SerializeField] private Transform warMembersParent;
        [SerializeField] private TextMeshProUGUI warName;
        [SerializeField] private Button declareWarButton;
        [SerializeField] private GameObject declareWarConfirmationScreen;

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
        public void Clicked(Nation nationSelected)
        {
            _currentNation = nationSelected;
            if (nationSelected == PlayerNationManager.PlayerNation)
            {
                PlayerNationManager.Instance.ClickedPlayerNation();
                return;
            }
            PlayerNationManager.Instance.ResetSelected();
            AgreementCreator.Instance.CloseAgreementScreen();
            
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

            flagImage.sprite = nationSelected.flag;
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
                if (war.Nations.Contains(nationDeclaredOn))
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
        }

        public void OpenAgreementScreen(int factionIndex)
        {
            Agreement agreement = _currentNation.agreements[factionIndex];

            DisplayAgreementMembers(agreement);
            _agreementScreen = true;
            _countrySelected = false;
            _warScreen = false;
            AgreementCreator.Instance.CloseAgreementScreen();
            agreementName.text = agreement.Name;
        }
        
        private List<GameObject> currentWarMembers = new List<GameObject>();

        private void DisplayWarMembers(War war)
        {
            foreach (var oldMember in currentWarMembers)
            {
                Destroy(oldMember);
            }
            
            foreach (var nation in war.Nations)
            {
                GameObject obj = Instantiate(agreementMemberPrefab, warMembersParent);

                obj.GetComponentInChildren<TextMeshProUGUI>().text = nation.Name;
                obj.GetComponentsInChildren<Image>()[1].sprite = nation.flag;
                
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Clicked(nation);
                });
                
                currentWarMembers.Add(obj);
            }
        }

        public void OpenWarScreen(War war)
        {
            _agreementScreen = false;
            _countrySelected = false;
            _warScreen = true;

            DisplayWarMembers(war);
            warName.text = war.Name;
        }
        
        public void OpenAgreementScreen(Agreement agreement)
        {
            DisplayAgreementMembers(agreement);
            _agreementScreen = true;
            _countrySelected = false;
            AgreementCreator.Instance.CloseAgreementScreen();
            agreementName.text = agreement.Name;
        }

        public void BeginCreatingAgreement()
        {
            AgreementCreator.Instance.OpenAgreementScreen(_currentNation);
            ResetSelected();
        }

        public void DeclareWar()
        {
            Debug.Log("declare war");
            declareWarConfirmationScreen.SetActive(true);
        }

        public void ConfirmedDeclareWar()
        {
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