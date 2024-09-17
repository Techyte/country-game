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

        private bool _countrySelected;
        private bool _agreementScreen;

        private Nation _currentNation;

        private void Awake()
        {
            Instance = this;
            titleCard.position = titleStartPos.position;
            agreementScreen.position = titleStartPos.position;
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
        }

        private List<GameObject> currentAgreementDisplays = new List<GameObject>();
        public void Clicked(Nation nationSelected)
        {
            _currentNation = nationSelected;
            if (nationSelected == PlayerNationManager.Instance.PlayerNation)
            {
                PlayerNationManager.Instance.ClickedPlayerNation();
                return;
            }
            PlayerNationManager.Instance.ResetSelected();
            AgreementCreator.Instance.CloseAgreementScreen();
            
            _agreementScreen = false;
            _countrySelected = true;
            titleCard.position = titleStartPos.position;
            titleText.text = nationSelected.Name;

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
                obj.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>("Flags/" + nation.Name.ToLower().Replace(' ', '_') + "_32");
                
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
        }

        public void OpenAgreementScreen(int factionIndex)
        {
            Agreement agreement = _currentNation.agreements[factionIndex];

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
    }
}