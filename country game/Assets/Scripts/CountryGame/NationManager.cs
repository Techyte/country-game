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
                agreementToJoin.NationJointed(nationToSwap);

                if (agreementToJoin.influence > 0 && agreementToJoin.AgreementLeader != nationToSwap)
                {
                    nationToSwap.ChangeInfluence(agreementToJoin.AgreementLeader, agreementToJoin.influence/3f);
                }
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

        public int TotalTroopCount;

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

        public void ChangeInfluence(Nation nation, float influence)
        {
            foreach (var country in Countries)
            {
                country.button.SetInfluenceColour(nation.Color, influence);
            }
        }

        public bool Border(Nation testNation)
        {
            foreach (var country in Countries)
            {
                foreach (var borderNation in country.GetBorders())
                {
                    if (testNation == borderNation)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Vector2 AvgPos()
        {
            float x = 0;
            float y = 0;

            foreach (var country in Countries)
            {
                x += country.GetComponent<PolygonCollider2D>().bounds.center.x;
                y += country.GetComponent<PolygonCollider2D>().bounds.center.y;
            }

            x /= Countries.Count;
            y /= Countries.Count;

            return new Vector2(x, y);
        }

        public float DistanceTo(Nation nation)
        {
            return (nation.AvgPos() - AvgPos()).magnitude;
        }
    }
}