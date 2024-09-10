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
            nation.Countries.Add(country);
            
            country.ChangeNation(nation);
            NationManager.Instance.NewNation(nation);
        }

        foreach (var country in countries)
        {
            if (country.countryName == "Greenland")
            {
                country.ChangeNation(NationManager.Instance.GetNationByName("Denmark"));
            }
        }
    }
}
