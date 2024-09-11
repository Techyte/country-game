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
    [SerializeField] private TextMeshProUGUI factionText;
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

    public void Clicked(Nation nationSelected)
    {
        _factionScreen = false;
        _currentNation = nationSelected;
        _countrySelected = true;
        titleCard.position = titleStartPos.position;
        titleText.text = nationSelected.Name;

        if (!nationSelected.faction.privateFaction)
        {
            factionText.text = nationSelected.faction.Name;
            factionText.color = nationSelected.faction.color;
        }
        else
        {
            factionText.text = "Non-Aligned";
            factionText.color = Color.black;
        }

        Sprite flag = Resources.Load<Sprite>("Flags/" + nationSelected.Name.ToLower().Replace(' ', '_') + "_32");
        flagImage.sprite = flag;
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

    public void OpenFactionScreen()
    {
        Faction faction = _currentNation.faction;

        if (!faction.privateFaction)
        {
            DisplayFactionMembers(faction);
            _factionScreen = true;
            _countrySelected = false;
            factionName.text = faction.Name;
        }
    }
}