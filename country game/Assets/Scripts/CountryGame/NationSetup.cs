using CountryGame.Multiplayer;
using Steamworks;

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
            List<Country> countries = GetComponentsInChildren<Country>().ToList();

            int index = 0;
            foreach (var country in countries)
            {
                //Random.InitState(index + int.Parse(SteamMatchmaking.GetLobbyData(LobbyData.LobbyId, "colorSeed")));
                Random.InitState(index + 3);
                
                NationManager.Instance.NewCountry(country);

                country.infrastructure = 1;

                Country[] parent = country.gameObject.GetComponentsInParent<Country>();

                if (parent.Length > 1)
                {
                    NationManager.Instance.SwapCountriesNation(country, parent[1].GetNation(), true);
                    continue;
                }
                
                Nation nation = new Nation();
                nation.Color = new Color(Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f,
                    Random.Range(100f, 256f)/256f);
                nation.Name = country.nationName;
                nation.flag = Resources.Load<Sprite>("Flags/" + nation.Name.ToLower().Replace(' ', '_') + "_32");
                nation.Money = 50;
                country.MovedTroopsIn(nation, 4);
                
                NationManager.Instance.NewNation(nation);
                
                NationManager.Instance.SwapCountriesNation(country, nation, true);

                index++;
            }

            foreach (var country in countries)
            {
                if (country.countryName == "Greenland")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Denmark"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(NationManager.Instance.GetNationByName("Denmark"), 1);
                }
                else if (country.countryName == "Alaska")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United States"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(NationManager.Instance.GetNationByName("United States"), 1);
                }
                else if (country.countryName == "Kaliningrad")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Russia"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(NationManager.Instance.GetNationByName("Russia"), 1);
                }
                else if (country.countryName == "French Guiana")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("France"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(NationManager.Instance.GetNationByName("France"), 1);
                }
                else if (country.countryName == "Northern Ireland")
                {
                    NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("United Kingdom"), true);
                    country.ResetTroops();
                    country.MovedTroopsIn(NationManager.Instance.GetNationByName("United Kingdom"), 1);
                }
            }

            foreach (var country in countries)
            {
                country.CalculateBorders();
            }

            // if (SteamMatchmaking.GetLobbyOwner(LobbyData.LobbyId) == SteamUser.GetSteamID())
            // {
            //     Debug.Log("finished setup sending connection information");
            //     NetworkManager.Instance.BeginSetup();
            // }

            //Nation playerNation = NationManager.Instance.GetNationByName(SteamMatchmaking.GetLobbyMemberData(LobbyData.LobbyId, SteamUser.GetSteamID(), "nation"));
            // string playerNationName =
            //     SteamMatchmaking.GetLobbyMemberData(LobbyData.LobbyId, SteamUser.GetSteamID(), "nation");
            // Nation playerNation = NationManager.Instance.GetNationByName(playerNationName);
            // PlayerNationManager.Instance.SetPlayerNation(playerNation);
            // PlayerNationManager.Instance.diplomaticPower = startingDiplomaticPower;

            //CombatManager.Instance.DeclareWarOn(NationManager.Instance.GetNationByName("Algeria"), NationManager.Instance.GetNationByName("Niger"));
        }
    }
}