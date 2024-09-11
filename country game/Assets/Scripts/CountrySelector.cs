using System;
using System.Globalization;
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

    private bool _countrySelected;

    private string _currentCountry;

    private void Awake()
    {
        Instance = this;
        titleCard.position = titleStartPos.position;
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
    }

    public void Clicked(Country countrySelected)
    {
        _currentCountry = countrySelected.countryName;
        _countrySelected = true;
        titleCard.position = titleStartPos.position;
        titleText.text = countrySelected.countryName;

        if (!countrySelected.GetNation().faction.privateFaction)
        {
            factionText.text = countrySelected.GetNation().faction.Name;
            factionText.color = countrySelected.GetNation().faction.color;
        }
        else
        {
            factionText.text = "Non-Aligned";
            factionText.color = Color.black;
        }

        Sprite flag = Resources.Load<Sprite>("Flags/" + countrySelected.countryName.ToLower().Replace(' ', '_') + "_32");
        flagImage.sprite = flag;
    }

    public void ResetSelected()
    {
        _currentCountry = "";
        _countrySelected = false;
    }
}