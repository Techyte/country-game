using System;
using System.Collections;
using System.Globalization;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class Country : MonoBehaviour
{
    public string countryName;
    [SerializeField] private Nation nation;

    private CountryButton button;

    private void Awake()
    {
        button = GetComponent<CountryButton>();
        if (string.IsNullOrEmpty(countryName))
        {
            countryName = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(gameObject.name.ToLower()).Replace('_', ' ');
        }
    }

    private void Start()
    {
        if (nation != null)
        {
            button.ChangeColor(nation.Color);
        }
    }

    public void ChangeColour(Color color)
    {
        button.ChangeColor(color);
    }

    public Nation GetNation()
    {
        return nation;
    }

    public void ChangeNation(Nation nation)
    {
        ChangeColour(nation.Color);
        this.nation = nation;
    }
}
