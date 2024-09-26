namespace CountryGame.Multiplayer
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NationManager : MonoBehaviour
    {
        public static NationManager Instance;

        public List<Country> counties = new List<Country>();
        public List<Nation> nations = new List<Nation>();

        private void Awake()
        {
            Instance = this;
        }

        public void NewCountry(Country countryToAdd)
        {
            counties.Add(countryToAdd);
        }

        public void NewNation(Nation nationToAdd)
        {
            nations.Add(nationToAdd);
        }

        public Nation GetNationByName(string nationName)
        {
            foreach (var nation in nations)
            {
                if (nationName == nation.Name)
                {
                    return nation;
                }
            }

            return null;
        }

        public void SwapCountriesNation(Country countryToSwap, Nation nationToSwapTo)
        {
            if (countryToSwap.GetNation() != nationToSwapTo)
            {
                Nation oldNation = countryToSwap.GetNation();
                
                if (oldNation != null)
                {
                    oldNation.CountryLeft(countryToSwap);
                }
                
                countryToSwap.ChangeNation(nationToSwapTo);
                nationToSwapTo.CountryJointed(countryToSwap);
            }
        }
    }

    public class Nation
    {
        public string Name;
        public List<Country> Countries = new List<Country>();
        public Color Color;
        public Sprite flag;

        public void CountryJointed(Country countryThatJoined)
        {
            Countries.Add(countryThatJoined);
        }

        public void CountryLeft(Country countryThatLeft)
        {
            if (Countries.Contains(countryThatLeft))
            {
                Countries.Remove(countryThatLeft);
            }
        }
    }
}