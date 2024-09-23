namespace CountryGame
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class NationSetup : MonoBehaviour
    {
        [SerializeField] private int startingDiplomaticPower = 25;
        private void Start()
        {
            List<Country> countries = FindObjectsOfType<Country>().ToList();

            foreach (var country in countries)
            {
                NationManager.Instance.NewCountry(country);
                
                Nation nation = new Nation();
                nation.Color = new Color(Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f);
                nation.Name = country.countryName;
                nation.flag = Resources.Load<Sprite>("Flags/" + nation.Name.ToLower().Replace(' ', '_') + "_32");
                country.MovedTroopsIn(nation, 9);
                
                NationManager.Instance.NewNation(nation);
                
                NationManager.Instance.SwapCountriesNation(country, nation);
            }

            foreach (var country in countries)
            {
                if (country.countryName == "Greenland")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Denmark"));
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                }
                else if (country.countryName == "Alaska")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United States"));
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                    country.borders.Add(NationManager.Instance.GetCountryByName("Russia"));
                }
                else if (country.countryName == "Kaliningrad")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Russia"));
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                }
                else if (country.countryName == "French Guiana")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("France"));
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                }
                else if (country.countryName == "Northern Ireland")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United Kingdom"));
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                }
            }

            Nation playerNation = NationManager.Instance.GetNationByName("Australia");
            PlayerNationManager.Instance.SetPlayerNation(playerNation);
            PlayerNationManager.Instance.diplomaticPower = startingDiplomaticPower;
            
            //CombatManager.Instance.DeclareWarOn(NationManager.Instance.GetNationByName("Algeria"), NationManager.Instance.GetNationByName("Niger"));
        }
    }
}