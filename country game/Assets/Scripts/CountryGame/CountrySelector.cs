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
        [SerializeField] private Transform factionTextParent;
        [SerializeField] private GameObject factionText;
        [SerializeField] private Image flagImage;
        [SerializeField] private Transform titleStartPos, titleEndPos;
        [SerializeField] private Transform factionMembersParent;
        [SerializeField] private GameObject factionMemberPrefab;
        [SerializeField] private TextMeshProUGUI factionName;
        [SerializeField] private Transform factionScreen;

        private bool _countrySelected;
        private bool _factionScreen;

        private Nation _currentNation;

        private void Awake()
        {
            Instance = this;
            titleCard.position = titleStartPos.position;
            factionScreen.position = titleStartPos.position;
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
            
            if (_factionScreen)
            {
                factionScreen.position = Vector3.Lerp(factionScreen.position, titleEndPos.position, titleSpeed * Time.deltaTime);
            }
            else
            {
                factionScreen.position = Vector3.Lerp(factionScreen.position, titleStartPos.position, titleSpeed * Time.deltaTime);
            }
        }

        private List<GameObject> currentFactionDisplays = new List<GameObject>();
        public void Clicked(Nation nationSelected)
        {
            _currentNation = nationSelected;
            if (nationSelected == PlayerNationManager.Instance.PlayerNation)
            {
                PlayerNationManager.Instance.ClickedPlayerNation();
                return;
            }
            PlayerNationManager.Instance.ResetSelected();
            
            _factionScreen = false;
            _countrySelected = true;
            titleCard.position = titleStartPos.position;
            titleText.text = nationSelected.Name;

            foreach (var factionDisplay in currentFactionDisplays)
            {
                Destroy(factionDisplay);
            }

            for (int i = 0; i < nationSelected.factions.Count; i++)
            {
                int index = i;
                Faction faction = nationSelected.factions[i];
                
                if (!faction.privateFaction)
                {
                    TextMeshProUGUI factionNameText = Instantiate(factionText, factionTextParent).GetComponent<TextMeshProUGUI>();
                    factionNameText.text = faction.Name;
                    factionNameText.color = faction.color;
                    
                    factionNameText.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        OpenFactionScreen(index);
                    });
                    
                    currentFactionDisplays.Add(factionNameText.gameObject);
                }
            }
            
            if (nationSelected.factions.Count == 0)
            {
                TextMeshProUGUI nonAlignedText = Instantiate(factionText, factionTextParent).GetComponent<TextMeshProUGUI>();
                nonAlignedText.text = "Non-Aligned";
                nonAlignedText.color = Color.black;
                
                currentFactionDisplays.Add(nonAlignedText.gameObject);
            }

            flagImage.sprite = nationSelected.flag;
        }

        private List<GameObject> currentFactionMembers = new List<GameObject>();

        private void DisplayFactionMembers(Faction faction)
        {
            foreach (var oldMember in currentFactionMembers)
            {
                Destroy(oldMember);
            }
            
            foreach (var nation in faction.Nations)
            {
                GameObject obj = Instantiate(factionMemberPrefab, factionMembersParent);

                obj.GetComponentInChildren<TextMeshProUGUI>().text = nation.Name;
                obj.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>("Flags/" + nation.Name.ToLower().Replace(' ', '_') + "_32");
                
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Clicked(nation);
                });
                
                currentFactionMembers.Add(obj);
            }
        }

        public void ResetSelected()
        {
            _currentNation = null;
            _countrySelected = false;
            _factionScreen = false;
        }

        public void OpenFactionScreen(int factionIndex)
        {
            Faction faction = _currentNation.factions[factionIndex];

            Debug.Log(faction.privateFaction);
            if (!faction.privateFaction)
            {
                DisplayFactionMembers(faction);
                _factionScreen = true;
                _countrySelected = false;
                factionName.text = faction.Name;
            }
        }
    }
}