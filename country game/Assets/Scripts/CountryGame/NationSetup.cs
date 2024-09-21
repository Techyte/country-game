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
                Nation nation = new Nation();
                nation.Color = new Color(Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f);
                nation.Name = country.countryName;
                nation.flag = Resources.Load<Sprite>("Flags/" + nation.Name.ToLower().Replace(' ', '_') + "_32");
                country.troopCount = NationManager.Instance.beginningTroopCount;
                
                NationManager.Instance.NewNation(nation);
                
                NationManager.Instance.SwapCountriesNation(country, nation);
            }

            foreach (var country in countries)
            {
                if (country.countryName == "Greenland")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Denmark"));
                    country.troopCount = 3;
                }
                else if (country.countryName == "Alaska")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United States"));
                    country.troopCount = 3;
                }
                else if (country.countryName == "Kaliningrad")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Russia"));
                    country.troopCount = 3;
                }
                else if (country.countryName == "French Guiana")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("France"));
                    country.troopCount = 3;
                }
                else if (country.countryName == "Northern Ireland")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United Kingdom"));
                    country.troopCount = 3;
                }
            }

            Nation playerNation = NationManager.Instance.GetNationByName("Switzerland");
            PlayerNationManager.Instance.SetPlayerNation(playerNation);
            PlayerNationManager.Instance.diplomaticPower = startingDiplomaticPower;
        }
    }
}