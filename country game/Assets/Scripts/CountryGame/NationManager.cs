namespace CountryGame
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NationManager : MonoBehaviour
    {
        public static NationManager Instance;
        
        public List<Nation> nations = new List<Nation>();
        public List<Agreement> agreements = new List<Agreement>();

        public bool useFactionColour;

        private void Awake()
        {
            Instance = this;
        }

        public void NewNation(Nation nationToAdd)
        {
            nations.Add(nationToAdd);
        }

        public void NewAgreement(Agreement agreementToAdd)
        {
            agreements.Add(agreementToAdd);
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

        public Agreement GetAgreementByName(string agreementName)
        {
            foreach (var agreement in agreements)
            {
                if (agreementName == agreement.Name)
                {
                    return agreement;
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
                    if (oldNation.CountryCount == 0)
                    {
                        nations.Remove(oldNation);
                    }
                }
                
                countryToSwap.ChangeNation(nationToSwapTo);
                nationToSwapTo.CountryJointed(countryToSwap);
            }
        }

        public void NationJoinAgreement(Nation nationToSwap, Agreement agreementToJoin)
        {
            if (!nationToSwap.agreements.Contains(agreementToJoin) && !agreementToJoin.Nations.Contains(nationToSwap))
            {
                nationToSwap.JoinAgreement(agreementToJoin);
                agreementToJoin.CountryJointed(nationToSwap); 
            }
        }
    }

    public class Nation
    {
        public string Name;
        public List<Country> Countries = new List<Country>();
        public List<Agreement> agreements = new List<Agreement>();
        public int CountryCount => Countries.Count;
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

        public void JoinAgreement(Agreement agreementToJoin)
        {
            agreements.Add(agreementToJoin);
        }
    }
}