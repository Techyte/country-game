namespace CountryGame.Multiplayer
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class NationSetup : MonoBehaviour
    {
        private void Start()
        {
            List<Country> countries = GetComponentsInChildren<Country>().ToList();

            //LobbyData.ColorSeed = Random.Range(0, 10000);
            LobbyData.ColorSeed = 42;
            
            int index = 0;
            foreach (var country in countries)
            {
                if (index == 0)
                {
                    Debug.Log(country.countryName);
                }
                Random.InitState(index + LobbyData.ColorSeed);
                
                NationManager.Instance.NewCountry(country);
                
                Nation nation = new Nation();
                nation.Color = new Color(Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f);
                nation.Name = country.countryName;
                nation.flag = Resources.Load<Sprite>("Flags/" + nation.Name.ToLower().Replace(' ', '_') + "_32");
                
                NationManager.Instance.NewNation(nation);
                
                NationManager.Instance.SwapCountriesNation(country, nation);

                index++;
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
}