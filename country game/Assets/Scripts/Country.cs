using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Country : MonoBehaviour
{
    public string countryName;
    [SerializeField] private Nation nation;

    private CountryButton button;

    public List<PresetFaction> FactionPresets;

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
            if (NationManager.Instance.useFactionColour)
            {
                button.ChangeColor(nation.factions[0].color);
            }
            else
            {
                button.ChangeColor(nation.Color);
            }
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
        if (NationManager.Instance.useFactionColour)
        {
            ChangeColour(nation.factions[0].color);
        }
        else
        {
            ChangeColour(nation.Color);
        }
        this.nation = nation;
    }
}

[Serializable]
public class PresetFaction
{
    public string FactionName;
    public bool FactionLeader;
}