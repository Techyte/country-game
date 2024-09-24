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
                country.MovedTroopsIn(nation, 4);
                
                NationManager.Instance.NewNation(nation);
                
                NationManager.Instance.SwapCountriesNation(country, nation, true);
            }

            foreach (var country in countries)
            {
                if (country.countryName == "Greenland")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Denmark"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                }
                else if (country.countryName == "Alaska")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United States"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                    country.borders.Add(NationManager.Instance.GetCountryByName("Russia"));
                }
                else if (country.countryName == "Kaliningrad")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Russia"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                }
                else if (country.countryName == "French Guiana")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("France"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                }
                else if (country.countryName == "Northern Ireland")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United Kingdom"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(country.GetNation(), 3);
                }
            }

            Nation playerNation = NationManager.Instance.GetNationByName("Australia");
            PlayerNationManager.Instance.SetPlayerNation(playerNation);
            PlayerNationManager.Instance.diplomaticPower = startingDiplomaticPower;

            Agreement agreement = new Agreement();
            agreement.Name = "test";
            agreement.Color = Color.black;
            agreement.AgreementLeader = NationManager.Instance.GetNationByName("Indonesia");
            agreement.autoJoinWar = true;
            agreement.influence = 2;
            
            NationManager.Instance.NewAgreement(agreement);
            NationManager.Instance.NationJoinAgreement(NationManager.Instance.GetNationByName("Indonesia"), agreement);
            NationManager.Instance.NationJoinAgreement(NationManager.Instance.GetNationByName("Papua New Guinea"), agreement);
            NationManager.Instance.NationJoinAgreement(NationManager.Instance.GetNationByName("Australia"), agreement);

            //CombatManager.Instance.DeclareWarOn(NationManager.Instance.GetNationByName("Algeria"), NationManager.Instance.GetNationByName("Niger"));
        }
    }
}