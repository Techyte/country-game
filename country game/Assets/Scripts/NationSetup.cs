using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NationSetup : MonoBehaviour
{
    private void Start()
    {
        List<Country> countries = FindObjectsOfType<Country>().ToList();

        foreach (var country in countries)
        {
            Nation nation = new Nation();
            nation.Color = new Color(Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f);
            nation.Name = country.countryName;
            nation.CountryJointed(country);
            
            NationManager.Instance.NewNation(nation);
            
            NationManager.Instance.SwapCountriesNation(country, nation);

            Faction nationFaction = new Faction();
            nationFaction.color = nation.Color;
            nationFaction.Name = country.countryName;
            nationFaction.privateFaction = true;
            nationFaction.CountryJointed(nation);
            NationManager.Instance.NewFaction(nationFaction);
            
            NationManager.Instance.NationJoinFaction(nation, nationFaction);
        }

        foreach (var country in countries)
        {
            if (country.countryName == "Greenland")
            {
                NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Denmark"));
            }
            else if (country.countryName == "Alaska")
            {
                NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United States"));
            }
            else if (country.countryName == "Kaliningrad")
            {
                NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Russia"));
            }
            else if (country.countryName == "French Guiana")
            {
                NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("France"));
            }
            else if (country.countryName == "Northern Ireland")
            {
                NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United Kingdom"));
            }
        }
    }
}
