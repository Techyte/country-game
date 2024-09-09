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
            _countrySelected = false;
        }
        
        if (_countrySelected)
        {
            titleCard.position = Vector3.Lerp(titleCard.position, titleEndPos.position, titleSpeed);
        }
        else
        {
            titleCard.position = Vector3.Lerp(titleCard.position, titleStartPos.position, titleSpeed);
        }
    }

    public void Clicked(string name)
    {
        name = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(name.ToLower()).Replace('_', ' ');
        
        _currentCountry = name;
        _countrySelected = true;
        titleCard.position = titleStartPos.position;
        titleText.text = name;

        Sprite flag = Resources.Load<Sprite>("Flags/" + name.ToLower().Replace(' ', '_') + "_32");
        flagImage.sprite = flag;
    }
}