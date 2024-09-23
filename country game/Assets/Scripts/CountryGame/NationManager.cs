using System;
using System.Linq;
using Unity.VisualScripting;

namespace CountryGame
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NationManager : MonoBehaviour
    {
        public static NationManager Instance;

        public List<Country> counties = new List<Country>();
        public List<Nation> nations = new List<Nation>();
        public List<Agreement> agreements = new List<Agreement>();

        public bool useFactionColour;
        public int beginningTroopCount = 10;
        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;

        private void Awake()
        {
            Instance = this;
            TurnManager.Instance.NewTurn += NewTurn;
        }

        private void NewTurn(object sender, EventArgs e)
        {
            Queue<Agreement> agreementQueue = agreements.ToQueue();
            
            while (agreementQueue.Count > 0)
            {
                Agreement agreement = agreementQueue.Dequeue();
                
                if (agreement.influence == 3)
                {
                    Queue<Nation> nationQueue = agreement.Nations.ToQueue();

                    while (nationQueue.Count > 0)
                    {
                        Nation nation = nationQueue.Dequeue();
                        
                        if (nation != agreement.AgreementLeader)
                        {
                            Queue<Country> countryQueue = nation.Countries.ToQueue();
                            
                            while (countryQueue.Count > 0)
                            {
                                Country country = countryQueue.Dequeue();
                                
                                Notification notification = Instantiate(notificationPrefab, notificationParent);
                                notification.Init($"{nation.Name} joins {agreement.AgreementLeader.Name}!", $"Today, under heavy pressure from {agreement.AgreementLeader.Name}, {nation.Name} gave up independence and joined {agreement.AgreementLeader.Name}! This marks a historic day.", () => {CountrySelector.Instance.Clicked(agreement.AgreementLeader);}, 5);
                                
                                Debug.Log("Country joining head");

                                List<TroopInformation> troopInfos = country.troopInfos.Values.ToList();

                                int originControlledTroops = 0;
                                
                                foreach (var info in troopInfos)
                                {
                                    if (info.ControllerNation == country.GetNation())
                                    {
                                        originControlledTroops = info.NumberOfTroops;
                                    }
                                }

                                int leftOver = Mathf.CeilToInt(originControlledTroops / 2f);
                                if (leftOver < 1)
                                {
                                    leftOver = 1;
                                }

                                country.troopInfos.Remove(country.GetNation());
                                country.MovedTroopsIn(agreement.AgreementLeader, leftOver);
                                
                                SwapCountriesNation(country, agreement.AgreementLeader);
                            }
                        }
                    }
                }
            }
        }

        public void NewCountry(Country countryToAdd)
        {
            counties.Add(countryToAdd);
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

        public Country GetCountryByName(string countryName)
        {
            foreach (var country in counties)
            {
                if (countryName == country.countryName)
                {
                    return country;
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

        private void NationDestroyed(Nation oldNation)
        {
            Queue<Agreement> agreementQueue = agreements.ToQueue();

            while (agreementQueue.Count > 0)
            {
                NationLeaveAgreement(oldNation, agreementQueue.Dequeue());
            }

            List<War> wars = oldNation.Wars.ToList();

            foreach (var war in wars)
            {
                war.RemoveDefender(oldNation);
                war.RemoveBelligerent(oldNation);

                oldNation.Wars.Remove(war);
            }
            
            nations.Remove(oldNation);
        }

        private void AgreementDestroyed(Agreement oldAgreement)
        {
            foreach (var nation in oldAgreement.Nations)
            {
                nation.agreements.Remove(oldAgreement);
            }
            
            agreements.Remove(oldAgreement);
        }

        public void SwapCountriesNation(Country countryToSwap, Nation nationToSwapTo)
        {
            if (countryToSwap.GetNation() != nationToSwapTo)
            {
                Nation oldNation = countryToSwap.GetNation();
                
                // swap all troops in the country to the new one
                foreach (var info in countryToSwap.troopInfos.Values)
                {
                    if (info.ControllerNation == countryToSwap.GetNation())
                    {
                        info.ControllerNation = nationToSwapTo;
                    }
                }
                
                if (oldNation != null)
                {
                    oldNation.CountryLeft(countryToSwap);
                    if (oldNation.CountryCount == 0)
                    {
                        NationDestroyed(oldNation);
                    }
                }

                int influence = nationToSwapTo.HighestInfluence(out Nation highestInfluence);
                
                countryToSwap.button.SetInfluenceColour(highestInfluence.Color, influence/3f);
                countryToSwap.ChangeNation(nationToSwapTo);
                countryToSwap.UpdateTroopDisplay();
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

        public void NationLeaveAgreement(Nation nation, Agreement agreement)
        {
            if (agreement.Nations.Contains(nation))
            {
                agreement.NationLeft(nation);
                nation.agreements.Remove(agreement);

                if (agreement.Nations.Count <= 1)
                {
                    AgreementDestroyed(agreement);
                }

                nation.UpdateTroopDisplays();
            }
        }
    }

    public class Nation
    {
        public string Name;
        public List<Country> Countries = new List<Country>();
        public List<Agreement> agreements = new List<Agreement>();
        public List<War> Wars = new List<War>();
        public int CountryCount => Countries.Count;
        public Color Color;
        public Sprite flag;
        public bool playerNation = false;

        public void CountryJointed(Country countryThatJoined)
        {
            Countries.Add(countryThatJoined);
        }

        public int TotalTroopCount()
        {
            int count = 0;

            foreach (var country in Countries)
            {
                count += country.TotalTroopCount();
            }

            return count;
        }

        public bool MilitaryAccessWith(Nation nationToTest)
        {
            foreach (var agreement in agreements)
            {
                if (agreement.militaryAccess && agreement.Nations.Contains(nationToTest))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAtWarWith(Nation nation)
        {
            foreach (var war in Wars)
            {
                if (war.Belligerents.Contains(this) && war.Defenders.Contains(nation))
                {
                    return true;
                }
                else if (war.Defenders.Contains(this) && war.Belligerents.Contains(nation))
                {
                    return true;
                }
            }

            return false;
        }

        public void CountryLeft(Country countryThatLeft)
        {
            if (Countries.Contains(countryThatLeft))
            {
                Countries.Remove(countryThatLeft);
            }
        }

        public void UpdateTroopDisplays()
        {
            foreach (var country in Countries)
            {
                country.UpdateTroopDisplay();
            }
        }

        public void BecomePlayerNation()
        {
            playerNation = true;

            foreach (var country in Countries)
            {
                country.BecomePlayerNation();
            }
        }

        public void JoinAgreement(Agreement agreementToJoin)
        {
            agreements.Add(agreementToJoin);
            foreach (var country in Countries)
            {
                country.SignedNewAgreement(agreementToJoin);
            }
        }

        public void JoinWar(War war)
        {
            Wars.Add(war);
        }

        public void ChangeInfluence(Nation nation, float influence)
        {
            foreach (var country in Countries)
            {
                country.button.SetInfluenceColour(nation.Color, influence);
            }
        }

        public void ChangeColor(Color color)
        {
            foreach (var country in Countries)
            {
                country.ChangeColour(color);
            }
        }

        public bool Border(Nation testNation)
        {
            foreach (var country in Countries)
            {
                foreach (var borderCountry in country.borders)
                {
                    if (testNation == borderCountry.GetNation())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int HighestInfluence(out Nation nation)
        {
            nation = this;
            int highestInfluence = 0;
            foreach (var agreement in agreements)
            {
                if (agreement.influence > highestInfluence && agreement.AgreementLeader != this)
                {
                    highestInfluence = agreement.influence;
                    nation = agreement.AgreementLeader;
                }
            }

            return highestInfluence;
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

    public static class ListExtension
    {
        public static Queue<T> ToQueue<T>(this List<T> list)
        {
            Queue<T> queue = new Queue<T>();
            
            foreach (var item in list)
            {
                queue.Enqueue(item);
            }

            return queue;
        }
    }
}