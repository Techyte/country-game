using System;
using System.Linq;
using Riptide;

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
        public List<TroopHiringInfo> hiringInfos = new List<TroopHiringInfo>();
        public Dictionary<Country, InfrastructureUpgradeInfo> UpgradeInfos = new Dictionary<Country, InfrastructureUpgradeInfo>();

        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;

        public List<Nation> PlayerNations = new List<Nation>();

        private void Awake()
        {
            Instance = this;
            TurnManager.Instance.NewTurn += NewTurn;
        }

        private void NewTurn(object sender, EventArgs e)
        {
            if (!NetworkManager.Instance.Host)
            {
                return;
            }

            List<string> nationsSubsumed = new List<string>();
            List<string> nationsThatSubsumed = new List<string>();
            
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
                                notification.Init($"{nation.Name} joins {agreement.AgreementLeader.Name}!",
                                    $"Today, under heavy pressure from {agreement.AgreementLeader.Name}, " +
                                    $"{nation.Name} gave up independence and joined {agreement.AgreementLeader.Name}! This marks a historic day.",
                                    () => { CountrySelector.Instance.Clicked(agreement.AgreementLeader); }, 5);
                                
                                Debug.Log("Country joining head");

                                country.MovedTroopsIn(agreement.AgreementLeader, country.TroopsOfController(country.GetNation()));
                                country.troopInfos.Remove(country.GetNation());
                                
                                SwapCountriesNation(country, agreement.AgreementLeader, true);
                            }
                            
                            nationsSubsumed.Add(nation.Name);
                            nationsThatSubsumed.Add(agreement.AgreementLeader.Name);
                        }
                    }
                }
            }

            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.SubsumedNations);
            message.AddStrings(nationsSubsumed.ToArray());
            message.AddStrings(nationsThatSubsumed.ToArray());
            
            NetworkManager.Instance.Server.SendToAll(message, NetworkManager.Instance.Client.Id);

            CombatManager.Instance.CompleteAttacks();
        }

        public void HandleSubsumedNations(List<string> nationsSubsumed, List<string> nationsThatSubsumed)
        {
            for (int i = 0; i < nationsSubsumed.Count; i++)
            {
                List<Country> countries = GetNationByName(nationsSubsumed[i]).Countries.ToList();
                Nation nationThatSubsumed = GetNationByName(nationsThatSubsumed[i]);

                foreach (var country in countries)
                {
                    Notification notification = Instantiate(notificationPrefab, notificationParent);
                    notification.Init($"{country.GetNation().Name} joins {nationThatSubsumed.Name}!",
                        $"Today, under heavy pressure from {nationThatSubsumed.Name}, " +
                        $"{country.GetNation().Name} gave up independence and joined {nationThatSubsumed.Name}! This marks a historic day.",
                        () => { CountrySelector.Instance.Clicked(nationThatSubsumed); }, 5);
                    
                    Debug.Log("Country joining head");

                    country.MovedTroopsIn(nationThatSubsumed, country.TroopsOfController(country.GetNation()));
                    country.troopInfos.Remove(country.GetNation());
                    
                    SwapCountriesNation(country, nationThatSubsumed, true);
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

        public void AIWarBehaviour()
        {
            foreach (var nation in nations)
            {
                if (nation.aPlayerNation)
                {
                    if (nation.DiplomaticPower <= 0)
                    {
                        foreach (var country in nation.Countries)
                        {
                            foreach (var border in country.borders)
                            {
                                if (border.GetNation() == nation || border.GetNation().IsAtWarWith(nation))
                                {
                                    continue;
                                }
                                
                                bool goingToDeclareWar = Random.Range(0, 4) == 1;

                                if (goingToDeclareWar)
                                {
                                    CombatManager.Instance.DeclaredWarOn(border.GetNation(), nation);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public void HireTroops(Country country, Nation nation, int amount)
        {
            TroopHiringInfo info = new TroopHiringInfo();
            info.country = country;
            info.OriginalNation = nation;
            info.Amount = amount;
            info.turnCreated = TurnManager.Instance.currentTurn;
            
            hiringInfos.Add(info);

            nation.Money -= amount * TroopMover.Instance.flatHireCostPerTroop;
        }

        public void HandleHiringTroops()
        {
            List<TroopHiringInfo> infos = hiringInfos.ToList();
            
            foreach (var info in infos)
            {
                if (TurnManager.Instance.currentTurn - info.turnCreated >= 1)
                {
                    if (info.country.GetNation() == info.OriginalNation && info.country.CanMoveNumTroopsIn(info.OriginalNation, info.Amount))
                    {
                        info.country.MovedTroopsIn(info.OriginalNation, info.Amount);
                    }

                    hiringInfos.Remove(info);
                }
            }
        }
        
        public void UpgradeInfrastructure(Country country, Nation nation, bool upgrading)
        {
            if (upgrading)
            {
                InfrastructureUpgradeInfo info = new InfrastructureUpgradeInfo();
                info.country = country;
                info.OriginalNation = nation;
            
                UpgradeInfos.Add(country, info);
                country.ChangeUpgradingStatus(true);

                if (nation == PlayerNationManager.PlayerNation)
                {
                    TurnManager.Instance.PerformedAction();
                }
            }
            else
            {
                country.ChangeUpgradingStatus(false);
                UpgradeInfos.Remove(country);
                
                if (nation == PlayerNationManager.PlayerNation)
                {
                    TurnManager.Instance.actionPoints += 1;
                }
            }
            
            ViewTypeManager.Instance.UpdateView();
        }

        public void HandleInfrastructureUpgrades()
        {
            List<InfrastructureUpgradeInfo> infos = UpgradeInfos.Values.ToList();
            
            foreach (var info in infos)
            {
                if (!info.country.GetNation().MilitaryAccessWith(info.OriginalNation))
                {
                    continue;
                }

                float cost = GetUpgradeExpense(info);
                
                info.OriginalNation.Money -= Mathf.CeilToInt(cost);
                
                info.country.infrastructure++;

                UpgradeInfos.Remove(info.country);

                info.country.ChangeUpgradingStatus(false);
                
                if (info.OriginalNation.Money < 0)
                {
                    info.OriginalNation.Money = 0;
                }
            }
        }

        public void HandleFinance()
        {
            HandleProfits();
            HandleExpenses();
        }

        public void ProclaimNation(Nation nation, string newName, string flag, Color color)
        {
            string oldName = nation.Name;

            nation.Name = newName;
            nation.flag = Resources.Load<Sprite>("Flags/"+flag);
            nation.Color = color;
            
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init(newName+"!",
                $"Today, the nation previously known as {oldName} proclaimed {newName}, a successor state destined to carry on its legacy",
                () => CountrySelector.Instance.Clicked(nation), 5);
            
            nation.UpdateInfluenceColour();
            nation.UpdateTroopDisplays();
        }

        public static int GetDiplomaticPowerGain(Nation nation)
        {
            if (nation.DiplomaticPower <= 10)
            {
                return 7;
            }
            int gain = Math.Clamp(250 / (nation.DiplomaticPower - 10) - 3, 1, 7);
            return gain;
        }

        public int GetLandCost(Nation nation)
        {
            int cost = 0;
            
            foreach (var country in nation.Countries)
            {
                cost += 10;
            }

            return cost;
        }

        public int GetTroopCost(Nation nation)
        {
            List<Country> countriesToSearch = new List<Country>();
            countriesToSearch.AddRange(nation.Countries);
                
            foreach (var agreement in nation.agreements)
            {
                if (agreement.militaryAccess)
                {
                    foreach (var agreementNation in agreement.Nations)
                    {
                        countriesToSearch.AddRange(agreementNation.Countries);
                    }
                }
            }

            float cost = 0;

            foreach (var country in countriesToSearch)
            {
                foreach (var value in country.troopInfos.Values)
                {
                    if (value.ControllerNation == nation)
                    {
                        cost += GetCostOfTroops(value.NumberOfTroops);
                    }
                }
            }

            return Mathf.RoundToInt(cost);
        }

        public int GetCostOfTroops(int num)
        {
            return num * 4;
        }

        public float GetNationWarCosts(Nation nation)
        {
            int cost = 0;
            
            foreach (var attack in CombatManager.Instance.attacks)
            {
                if (attack.Instigator == nation)
                {
                    cost += TroopMover.Instance.attackCost;
                }
            }

            return cost;
        }

        public int GetNationProfits(Nation nation)
        {
            int profits = 0;
            
            foreach (var country in nation.Countries)
            {
                for (int i = 0; i < country.infrastructure; i++)
                {
                    profits += 25 / (i + 2);
                }
            }

            return profits;
        }

        public float GetUpgradeExpense(InfrastructureUpgradeInfo info)
        {
            return Mathf.Pow(13f / 10f, info.country.infrastructure) * 3f;
        }

        private void HandleProfits()
        {
            foreach (var nation in nations)
            {
                nation.Money += GetNationProfits(nation);
            }
        }

        private void HandleExpenses()
        {
            foreach (var nation in nations)
            {
                int landCost = GetLandCost(nation);
                nation.Money -= landCost;
                
                int cost = GetTroopCost(nation);
                nation.Money -= cost;

                float warCost = GetNationWarCosts(nation);
                nation.Money -= Mathf.RoundToInt(warCost);
                
                if (nation.Money < 0)
                {
                    nation.Money = 0;
                }
            }
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

        public Nation GetNationByFlag(Sprite flag)
        {
            foreach (var nation in nations)
            {
                if (flag == nation.flag)
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

        private void NationDestroyed(Nation oldNation, bool willing)
        {
            Queue<Agreement> agreementQueue = agreements.ToQueue();

            while (agreementQueue.Count > 0)
            {
                NationLeaveAgreement(oldNation, agreementQueue.Dequeue(), willing);
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

        private void AgreementDestroyed(Agreement oldAgreement, bool willing)
        {
            if (!willing)
            {
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"Dissolution!",
                    $"Today, The {oldAgreement.Name} was dissolved, every nation having decided to turn away to seek their own destiny",
                    null, 5);
            }
            
            foreach (var nation in oldAgreement.Nations)
            {
                nation.agreements.Remove(oldAgreement);
            }
            
            agreements.Remove(oldAgreement);
        }

        public void MassSwapCountiesNation(List<Country> countries, List<Nation> nations, bool willing)
        {
            List<Nation> oldNations = new List<Nation>();
            
            for (int i = 0; i < countries.Count; i++)
            {
                Country countryToSwap = countries[i];
                Nation nationToSwapTo = nations[i];
                
                if (countryToSwap.GetNation() != nationToSwapTo)
                {
                    Nation oldNation = countryToSwap.GetNation();
                    
                    oldNations.Add(oldNation);
                
                    oldNation.CountryLeft(countryToSwap);

                    int influence = nationToSwapTo.HighestInfluence(out Nation highestInfluence);
                
                    countryToSwap.button.SetInfluenceColour(highestInfluence.Color, influence/3f);
                    countryToSwap.ChangeNation(nationToSwapTo);
                    countryToSwap.UpdateTroopDisplay();
                    nationToSwapTo.CountryJointed(countryToSwap);
                }
            }

            for (int i = 0; i < oldNations.Count; i++)
            {
                if (oldNations[i].CountryCount == 0)
                {
                    NationDestroyed(oldNations[i], willing);
                }
            }
        }

        public void SwapCountriesNation(Country countryToSwap, Nation nationToSwapTo, bool willing)
        {
            if (countryToSwap.GetNation() != nationToSwapTo)
            {
                Nation oldNation = countryToSwap.GetNation();
                
                if (oldNation != null)
                {
                    oldNation.CountryLeft(countryToSwap);
                    if (oldNation.CountryCount == 0)
                    {
                        NationDestroyed(oldNation, willing);
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

                nationToSwap.UpdateInfluenceColour();
                nationToSwap.UpdateTroopDisplays();
            }
        }

        public void NationLeaveAgreement(Nation nation, Agreement agreement, bool willing)
        {
            if (agreement.Nations.Contains(nation))
            {
                agreement.NationLeft(nation);
                nation.agreements.Remove(agreement);

                foreach (var country in nation.Countries)
                {
                    foreach (var troopInfo in country.troopInfos.Values)
                    {
                        if (troopInfo.ControllerNation != country.GetNation())
                        {
                            Debug.Log("transferring troops because a nation left an agreement");
                            TroopMover.Instance.TransferTroops(country, nation.Countries[0], troopInfo.ControllerNation, troopInfo.NumberOfTroops);
                        }
                    }
                }

                if (agreement.Nations.Count <= 1)
                {
                    AgreementDestroyed(agreement, willing);
                }
                else if (agreement.AgreementLeader == nation)
                {
                    agreement.AgreementLeader = agreement.Nations[0];
                }

                foreach (var updateNation in agreement.Nations)
                {
                    updateNation.UpdateTroopDisplays();
                    updateNation.UpdateInfluenceColour();
                }

                nation.UpdateTroopDisplays();
                nation.UpdateInfluenceColour();
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
        public bool aPlayerNation = false;
        public int Money = 0;

        public float infantry = 0.4f;
        public float tanks = 0.3f;
        public float marines = 0.3f;

        public int DiplomaticPower
        {
            get
            {
                return _diplomaticPower;
            }
            set
            {
                _diplomaticPower = Math.Clamp(value, -100, 100);
            }
        }

        private int _diplomaticPower;

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

        public bool CanAfford(int amount)
        {
            return Money - amount >= 0;
        }

        public bool MilitaryAccessWith(Nation nationToTest)
        {
            if (nationToTest == this)
            {
                return true;
            }
            
            foreach (var agreement in agreements)
            {
                if (agreement.militaryAccess && agreement.Nations.Contains(nationToTest))
                {
                    return true;
                }
            }

            if (InvolvedInWarWith(nationToTest))
            {
                return true;
            }

            return false;
        }

        public bool AutoJoinWarsWith(Nation nationToTest)
        {
            if (nationToTest == this)
            {
                return true;
            }
            
            foreach (var agreement in agreements)
            {
                if (agreement.autoJoinWar && agreement.Nations.Contains(nationToTest))
                {
                    return true;
                }
            }

            if (InvolvedInWarWith(nationToTest))
            {
                return true;
            }

            return false;
        }

        public bool NonAgressionWith(Nation nationToTest)
        {
            foreach (var agreement in agreements)
            {
                if (agreement.nonAgression && agreement.Nations.Contains(nationToTest))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Attacking(Country country)
        {
            foreach (var ourCountry in Countries)
            {
                foreach (var attack in CombatManager.Instance.attacks)
                {
                    if (attack.Source == ourCountry && attack.Target == country)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Defending(Nation nation)
        {
            foreach (var ourCountry in Countries)
            {
                foreach (var attack in CombatManager.Instance.attacks)
                {
                    if (attack.Source.GetNation() == nation && attack.Target.GetNation() == this)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool InvolvedInWarWith(Nation nationToTest)
        {
            if (nationToTest == this)
            {
                return true;
            }
            
            foreach (var war in Wars)
            {
                if (war.Defenders.Contains(nationToTest) && war.Defenders.Contains(this))
                {
                    return true;
                }
                if (war.Belligerents.Contains(nationToTest) && war.Belligerents.Contains(this))
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

        public void UpdateTroopDisplaysNonRecursivly()
        {
            foreach (var country in Countries)
            {
                country.UpdateTroopDisplay();
            }
        }

        public void UpdateTroopDisplays()
        {
            foreach (var country in Countries)
            {
                country.UpdateTroopDisplay();
            }

            foreach (var agreement in agreements)
            {
                if (agreement.militaryAccess)
                {
                    foreach (var nation in agreement.Nations)
                    {
                        nation.UpdateTroopDisplaysNonRecursivly();
                    }
                }
            }

            foreach (var war in Wars)
            {
                if (war.Belligerents.Contains(this))
                {
                    foreach (var bel in war.Belligerents)
                    {
                        bel.UpdateTroopDisplaysNonRecursivly();
                    }
                }
                else
                {
                    foreach (var def in war.Defenders)
                    {
                        def.UpdateTroopDisplaysNonRecursivly();
                    }
                }
            }

            CombatManager.Instance.UpdateAttackDisplays();
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

        public void UpdateInfluenceColour()
        {
            Nation highestInfluenceNation;
            int highestInfluence = HighestInfluence(out highestInfluenceNation);
            
            ChangeInfluence(highestInfluenceNation, highestInfluence/3f);
            CombatManager.Instance.UpdateAttackDisplays();
            
            foreach (var country in Countries)
            {
                country.ChangeColour(Color);
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
            float smallestDistance = 100000;

            foreach (var country1 in Countries)
            {
                foreach (var country2 in nation.Countries)
                {
                    float distance = country1.GetComponent<PolygonCollider2D>().Distance(country2.collider).distance;
                    
                    if (smallestDistance > distance)
                    {
                        smallestDistance = distance;
                    }
                }
            }

            return smallestDistance;
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

    public class TroopHiringInfo
    {
        public Country country;
        public Nation OriginalNation;
        public int Amount;
        public int turnCreated;
    }

    public class InfrastructureUpgradeInfo
    {
        public Country country;
        public Nation OriginalNation;
    }
}