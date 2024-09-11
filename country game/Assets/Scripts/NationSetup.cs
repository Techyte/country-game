using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NationSetup : MonoBehaviour
{
    private void Start()
    {
        SetupInitialFactions();
        
        List<Country> countries = FindObjectsOfType<Country>().ToList();

        foreach (var country in countries)
        {
            Nation nation = new Nation();
            nation.Color = new Color(Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f);
            nation.Name = country.countryName;
            nation.CountryJointed(country);

            Faction faction = null;

            if (string.IsNullOrEmpty(country.presetFactionName))
            {
                faction = new Faction();
                faction.color = nation.Color;
                faction.Name = country.countryName;
                faction.privateFaction = true;
                faction.CountryJointed(nation);
                NationManager.Instance.NewFaction(faction);
            }
            else
            {
                faction = NationManager.Instance.GetFactionByName(country.presetFactionName);
                if (country.presetFactionLeader)
                {
                    faction.SetFactionLeader(nation);
                }
            }

            NationManager.Instance.NewNation(nation);
            NationManager.Instance.SwapNationsFaction(nation, faction);
            NationManager.Instance.SwapCountriesNation(country, nation);
        }

        foreach (var country in countries)
        {
            if (country.countryName == "Greenland")
            {
                NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Denmark"));
            }
        }
    }

    private void SetupInitialFactions()
    {
        Faction nato = new Faction();
        nato.color = Color.blue;
        nato.Name = "NATO";
        nato.privateFaction = false;
        
        NationManager.Instance.NewFaction(nato);
        
        Faction csto = new Faction();
        csto.color = Color.green;
        csto.Name = "CSTO";
        csto.privateFaction = false;
        
        NationManager.Instance.NewFaction(csto);
    }
}
