using System;
using System.Collections.Generic;
using System.Linq;
using Riptide;
using UnityEngine.UI;

namespace CountryGame
{
    using UnityEngine;

    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance;
        
        public List<War> wars = new List<War>();
        public List<Attack> attacks = new List<Attack>();

        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;
        [SerializeField] private GameObject otherGUIParent;
        [SerializeField] private GameObject invasionScreen;
        [SerializeField] private Transform attackLinesParent;
        [SerializeField] private Material lineMat;
        [SerializeField] private Sprite arrowSprite;

        public bool invading;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            invasionScreen.SetActive(false);
        }

        public void NewWar(War war)
        {
            wars.Add(war);
        }

        public void CompleteAttacks()
        {
            foreach (var attack in attacks)
            {
                attack.calculatedAttack = CalculateAttackAttack(attack);
                attack.calculatedDefense = CalculateAttackDefense(attack);
            }

            foreach (var attack in attacks)
            {
                attack.success = AttackSuccessful(attack);
            }

            List<string> countrysTransfered = new List<string>();
            List<string> countriesThatTook = new List<string>();
            List<string> countriesThatFailedAttack = new List<string>();
            List<string> countriesThatRepelledAttack = new List<string>();

            foreach (var attack in attacks)
            {
                if (attack.someoneElseHadMoreAttack)
                {
                    continue;
                }
                
                if (attack.success && !attack.war.over)
                {
                    countrysTransfered.Add(attack.Target.countryName);
                    countriesThatTook.Add(attack.Source.countryName);
                }
                else
                {
                    countriesThatFailedAttack.Add(attack.Source.countryName);
                    countriesThatRepelledAttack.Add(attack.Target.countryName);
                }
            }

            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.CombatResults);
            message.AddStrings(countrysTransfered.ToArray());
            message.AddStrings(countriesThatTook.ToArray());
            message.AddStrings(countriesThatFailedAttack.ToArray());
            message.AddStrings(countriesThatRepelledAttack.ToArray());
            
            NetworkManager.Instance.Server.SendToAll(message);
        }

        public void HandleCombatResults(List<string> countriesTransfered, List<string> countriesThatTook,
            List<string> countriesThatFailedAttack, List<string> countriesThatRepelledAttack)
        {
            for (int i = 0; i < countriesThatFailedAttack.Count; i++)
            {
                Country country = NationManager.Instance.GetCountryByName(countriesThatFailedAttack[i]);
                foreach (var info in country.troopInfos.Values.ToList())
                {
                    if (info.ControllerNation.IsAtWarWith(NationManager.Instance.GetCountryByName(countriesThatRepelledAttack[i]).GetNation()))
                    {
                        country.MoveTroopsOut(info.ControllerNation, 1);
                    }
                }

                country.Infrastructure--;
            }

            for (int i = 0; i < countriesTransfered.Count; i++)
            {
                Country countryTaken = NationManager.Instance.GetCountryByName(countriesTransfered[i]);
                Nation nationToTakeTerritory = NationManager.Instance.GetCountryByName(countriesThatTook[i]).GetNation();

                countryTaken.Infrastructure -= 1;
                
                countryTaken.ResetTroops();
                if (countryTaken.GetParticipatingTroopsAttacking(nationToTakeTerritory) > 0)
                {
                    NationManager.Instance.GetCountryByName(countriesThatTook[i]).MoveTroopsOut(nationToTakeTerritory, 1);
                }
            }

            List<Country> countries = new List<Country>();
            List<Nation> nations = new List<Nation>();

            for (int i = 0; i < countriesTransfered.Count; i++)
            {
                countries.Add(NationManager.Instance.GetCountryByName(countriesTransfered[i]));
                nations.Add(NationManager.Instance.GetCountryByName(countriesThatTook[i]).GetNation());
            }
            
            NationManager.Instance.MassSwapCountiesNation(countries, nations, false);
            
            foreach (var attack in attacks.ToList())
            {
                Destroy(attack.line.gameObject);
                attacks.Remove(attack);
                
                attack.Source.GetNation().UpdateTroopDisplays();
                attack.Target.GetNation().UpdateTroopDisplays();
            }

            if (NetworkManager.Instance.Host)
            {
                NationManager.Instance.AIWarBehaviour();
            
                AICombatBehaviour();
            }
        }
        
        float infantryAttack = 0.4f;
        float infantryDefense = 1f;
            
        float tanksAttack = 1.1f;
        float tanksDefense = 0.7f;
            
        float marinesAttack = 0.4f;
        float marinesDefense = 0.5f;

        private float CalculateAttackAttack(Attack attack)
        {
            float finalAttack = attack.Source.GetAttack();
            
            foreach (var troopInfo in attack.Source.troopInfos.Values)
            {
                if (!troopInfo.ControllerNation.Wars.Contains(attack.war))
                {
                    continue;
                }
                
                float infantryTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.infantry;
                float tanksTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.tanks;
                float marinesTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.marines;

                finalAttack += infantryAttack * infantryTroops;
                
                finalAttack += tanksAttack * tanksTroops;
                
                finalAttack += marinesAttack * marinesTroops;
            }
            
            float totalDefendingForce = 0;
            float totalAttackingForce = attack.Source.GetParticipatingTroops(attack.war);
            
            foreach (var otherAttack in attacks)
            {
                if (otherAttack.Source == attack.Source)
                {
                    totalDefendingForce += otherAttack.Target.GetParticipatingTroopsAttacking(attack.Source.GetNation());
                }
            }

            if (totalAttackingForce <= 0)
            {
                // defenders will always win
                return 0;
            }
            
            // calculate how much of the defense is being used to counter this attack
            
            float attackScaler = 1f;
            
            if (totalDefendingForce == 0)
            {
                attackScaler = 1;
            }
            else
            {
                attackScaler = attack.Target.GetParticipatingTroops(attack.war) / totalDefendingForce; // amount of attacking force that is being allocated to this attack
            }
            
            
            // calculate how much of the attack is being used to push this attack
            
            return finalAttack * attackScaler;
        }
        private float CalculateAttackDefense(Attack attack)
        {
            float finalDefense = attack.Target.GetDefense();
            
            foreach (var troopInfo in attack.Target.troopInfos.Values)
            {
                if (!troopInfo.ControllerNation.Wars.Contains(attack.war))
                {
                    continue;
                }
                
                float infantryTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.infantry;
                float tanksTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.tanks;
                float marinesTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.marines;

                finalDefense += infantryDefense * infantryTroops;
                
                finalDefense += tanksDefense * tanksTroops;
                
                finalDefense += marinesDefense * marinesTroops;
            }

            float totalAttackingForce = 0;
            float totalDefendingForce = attack.Target.GetParticipatingTroops(attack.war);

            foreach (var otherAttack in attacks)
            {
                if (otherAttack.Target == attack.Target)
                {
                    totalAttackingForce += otherAttack.Source.GetParticipatingTroopsAttacking(attack.Target.GetNation());
                }
            }
            
            if (totalDefendingForce <= 0)
            {
                // attackers will always win
                return 0;
            }
            
            // calculate how much of the defense is being used to counter this attack
            
            float defenseScaler = 1f;
            
            if (totalAttackingForce == 0)
            {
                defenseScaler = 1;
            }
            else
            {
                defenseScaler = attack.Source.GetParticipatingTroops(attack.war) / totalAttackingForce; // amount of attacking force that is being allocated to this attack
            }
            
            // calculate how much of the attack is being used to push this attack
            
            return finalDefense * defenseScaler;
        }

        private bool AttackSuccessful(Attack attack)
        {
            foreach (var otherAttack in attacks)
            {
                if (attack != otherAttack && !otherAttack.someoneElseHadMoreAttack)
                {
                    if (attack.Target == otherAttack.Target)
                    {
                        if (attack.calculatedAttack > otherAttack.calculatedAttack)
                        {
                            otherAttack.someoneElseHadMoreAttack = true;
                        }
                        else
                        {
                            attack.someoneElseHadMoreAttack = true;
                        }
                    }
                }
            }

            return attack.calculatedAttack > attack.calculatedDefense || (attack.calculatedDefense == 0 && attack.Source.GetParticipatingTroopsAttacking(attack.Target.GetNation()) > 0);
        }

        public void AICombatBehaviour()
        {
            foreach (var nation in NationManager.Instance.nations)
            {
                if (!nation.aPlayerNation && nation.Wars.Count > 0)
                {
                    // computer nation at war

                    foreach (var country in nation.Countries)
                    {
                        foreach (var border in country.borders)
                        {
                            bool attacking = Random.Range(0, 3) == 1;
                            if (border.GetNation().IsAtWarWith(nation) && attacking)
                            {
                                // found a border to attack
                                LaunchAttack(border, country, nation);
                            }
                        }
                    }

                    AiTroopMoving(nation);
                }
            }
        }

        private void AiTroopMoving(Nation aiNation)
        {
            if (aiNation.Wars.Count == 0)
            {
                return;
            }
            
            List<TroopInformation> infos = new List<TroopInformation>();
            List<TroopInformation> tempInfos = new List<TroopInformation>();
            int total = 0;
            List<Country> destinationCountries = new List<Country>();

            foreach (var country in aiNation.Countries)
            {
                if (!country.troopInfos.TryGetValue(aiNation, out TroopInformation infoToAdd))
                {
                    infoToAdd = new TroopInformation();
                    infoToAdd.ControllerNation = aiNation;
                    infoToAdd.NumberOfTroops = 0;
                    infoToAdd.Location = country;
                    tempInfos.Add(infoToAdd);
                }
                
                infos.Add(infoToAdd);
                
                foreach (var border in country.borders)
                {
                    if (aiNation.IsAtWarWith(border.GetNation()))
                    {
                        destinationCountries.Add(country);
                    }
                }
                
                total += infoToAdd.NumberOfTroops;
            }

            if (destinationCountries.Count == 0)
            {
                return;
            }

            int avg = total / destinationCountries.Count;

            int dolledOut = 0;

            foreach (var info in infos)
            {
                if (destinationCountries.Contains(info.Location))
                {
                    info.NumberOfTroops = avg;
                    dolledOut += avg;
                }
                else
                {
                    info.Location.MoveTroopsOut(info.ControllerNation, info.NumberOfTroops);
                }
            }

            int index = 0;
            while (dolledOut < total)
            {
                index++;
                index = index % infos.Count;
                infos[index].NumberOfTroops++;
                dolledOut++;
            }
            
            // completed dolling out troops

            if (avg < 4)
            {
                // need more troops, we don't have enough

                Country countryToHire = aiNation.Countries[0];
                NationManager.Instance.HireTroops(countryToHire, aiNation, 4 - avg);
            }

            foreach (var info in tempInfos)
            {
                info.Location.MovedTroopsIn(info.ControllerNation, info.NumberOfTroops);
            }
        }

        public void DeclareWarOn(Nation nationToWarWith)
        {
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.DeclareWar);
            message.AddString(PlayerNationManager.PlayerNation.Name);
            message.AddString(nationToWarWith.Name);

            NetworkManager.Instance.Client.Send(message);
        }

        public void AskToJoinWar(War war, Nation requested, Nation requester)
        {
            Debug.Log($"{requester.Name} asked {requested.Name} to join {war.Name}");
            
            if ((requester.DiplomaticPower < 10 && !requested.AutoJoinWarsWith(requester)) || requested.aPlayerNation)
            {
                return;
            }
            
            Debug.Log("They want to join");
            
            if (requested.Wars.Contains(war))
            {
                return;
            }
            
            Debug.Log("They were not already in the war");

            bool defender = war.Defenders.Contains(requester);
            
            if (defender)
            {
                NationJoinWarDefenders(requested, war);
            }
            else
            {
                NationJoinWarBelligerents(requested, war);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSelected();
            }
        }

        public void UpdateAttackDisplays()
        {
            foreach (var attack in attacks)
            {
                bool canSee = (PlayerNationManager.PlayerNation.Attacking(attack.Target) ||
                               PlayerNationManager.PlayerNation.MilitaryAccessWith(attack.Source.GetNation()) ||
                               PlayerNationManager.PlayerNation == attack.Target.GetNation() ||
                               PlayerNationManager.PlayerNation.MilitaryAccessWith(attack.Target.GetNation())) &&
                              ViewTypeManager.Instance.currentView != ViewType.Diplomacy;
                
                attack.line.enabled = canSee;

                if (canSee)
                {
                    if (attack.Target.GetNation() != PlayerNationManager.PlayerNation && 
                        !PlayerNationManager.PlayerNation.MilitaryAccessWith(attack.Target.GetNation()))
                    {
                        attack.line.startColor = Color.black;
                        attack.line.endColor = Color.black;
                    }
                    else
                    {
                        attack.line.startColor = Color.red;
                        attack.line.endColor = Color.red;
                    }
                }
                else
                {
                    attack.line.startColor = Color.black;
                    attack.line.endColor = Color.black;
                }
            }
        }

        public void DeclaredWarOn(Nation nationThatDeclared, Nation nationToWarWith)
        {
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.DeclareWar);
            message.AddString(nationThatDeclared.Name);
            message.AddString(nationToWarWith.Name);

            NetworkManager.Instance.Client.Send(message);
        }

        public void DeclareWarOn(Nation nationThatDeclared, Nation nationToWarWith)
        {
            War war = new War();
            war.Name = $"{nationThatDeclared.Name} {nationToWarWith.Name} War";
            
            NewWar(war);
            
            war.Belligerents.Add(nationThatDeclared);
            nationThatDeclared.Wars.Add(war);
            
            war.Defenders.Add(nationToWarWith);
            nationToWarWith.Wars.Add(war);
            
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init($"To War!",
                $"Today, {nationThatDeclared.Name} declared war on {nationToWarWith.Name}, this will surely be one to remember",
                () => { CountrySelector.Instance.OpenWarScreen(war); }, 5);
            
            List<Agreement> agreements = nationToWarWith.agreements.ToList();
            
            foreach (var agreement in agreements)
            {
                foreach (var nation in war.Belligerents)
                {
                    if (agreement.Nations.Contains(nation))
                    {
                        NationManager.Instance.NationLeaveAgreement(nation, agreement, false);
                    }
                }
                
                if (agreement.autoJoinWar)
                {
                    foreach (var nation in agreement.Nations)
                    {
                        if (!nation.Wars.Contains(war))
                        {
                            NationJoinWarDefenders(nation, war);
                        }
                    }
                }
            }
            
            agreements = nationThatDeclared.agreements.ToList();
            
            foreach (var agreement in agreements)
            {
                foreach (var nation in war.Defenders)
                {
                    if (agreement.Nations.Contains(nation))
                    {
                        NationManager.Instance.NationLeaveAgreement(nation, agreement, false);
                    }
                }
                
                if (agreement.autoJoinWar)
                {
                    foreach (var nation in agreement.Nations)
                    {
                        if (!nation.Wars.Contains(war))
                        {
                            NationJoinWarBelligerents(nation, war);
                        }
                    }
                }
            }

            foreach (var belligerent in war.Belligerents)
            {
                belligerent.UpdateInfluenceColour();
                belligerent.UpdateTroopDisplays();
            }

            foreach (var defender in war.Defenders)
            {
                defender.UpdateInfluenceColour();
                defender.UpdateTroopDisplays();
            }
            
            if (!nationToWarWith.aPlayerNation && nationToWarWith.Wars.Count > 0)
            {
                // computer nation at war

                bool attacking = Random.Range(0, 2) == 1;

                if (attacking)
                {
                    foreach (var country in nationToWarWith.Countries)
                    {
                        foreach (var border in country.borders)
                        {
                            if (border.GetNation().IsAtWarWith(nationToWarWith))
                            {
                                // found a border to attack
                                    
                                LaunchedAttack(border, country, nationToWarWith);
                            }
                        }
                    }
                }
            }

            nationThatDeclared.DiplomaticPower -= 20;
            
            ViewTypeManager.Instance.UpdateView();
        }

        public void NationJoinWarBelligerents(Nation nationToJoinWar, War warToJoin)
        {
            nationToJoinWar.UpdateTroopDisplays();
            nationToJoinWar.UpdateInfluenceColour();
            
            // already in the war
            if (nationToJoinWar.Wars.Contains(warToJoin))
            {
                return;
            }
            
            nationToJoinWar.JoinWar(warToJoin);
            warToJoin.NationJointedBelligerents(nationToJoinWar);
            
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init($"To War!",
                $"Today, {nationToJoinWar.Name} joined the war on {warToJoin.Defenders[0].Name}, this will surely be one to remember",
                () => { CountrySelector.Instance.OpenWarScreen(warToJoin); }, 5);
            
            List<Agreement> agreements = nationToJoinWar.agreements.ToList();
            
            foreach (var agreement in agreements)
            {
                foreach (var nation in warToJoin.Defenders)
                {
                    if (agreement.Nations.Contains(nation))
                    {
                        NationManager.Instance.NationLeaveAgreement(nation, agreement, false);
                    }
                }
                
                if (agreement.autoJoinWar)
                {
                    foreach (var nation in agreement.Nations)
                    {
                        if (!nation.Wars.Contains(warToJoin))
                        {
                            NationJoinWarBelligerents(nation, warToJoin);
                        }
                    }
                }
            }

            CountrySelector.Instance.DisplayWarMembers(warToJoin);
        }
        
        public void NationJoinWarDefenders(Nation nationToJoinWar, War warToJoin)
        {
            nationToJoinWar.UpdateTroopDisplays();
            nationToJoinWar.UpdateInfluenceColour();
            
            // already in the war
            if (nationToJoinWar.Wars.Contains(warToJoin))
            {
                return;
            }
            
            nationToJoinWar.JoinWar(warToJoin);
            warToJoin.NationJointedDefenders(nationToJoinWar);
            
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init($"To War!",
                $"Today, {nationToJoinWar.Name} joined the war on {warToJoin.Belligerents[0].Name}, this will surely be one to remember",
                () => { CountrySelector.Instance.OpenWarScreen(warToJoin); }, 5);
            
            List<Agreement> agreements = nationToJoinWar.agreements.ToList();
            
            foreach (var agreement in agreements)
            {
                foreach (var nation in warToJoin.Belligerents)
                {
                    if (agreement.Nations.Contains(nation))
                    {
                        NationManager.Instance.NationLeaveAgreement(nation, agreement, false);
                    }
                }
                
                if (agreement.autoJoinWar)
                {
                    foreach (var nation in agreement.Nations)
                    {
                        if (!nation.Wars.Contains(warToJoin))
                        {
                            NationJoinWarDefenders(nation, warToJoin);
                        }
                    }
                }
            }

            CountrySelector.Instance.DisplayWarMembers(warToJoin);
        }

        public void ResetSelected()
        {
            otherGUIParent.SetActive(true);
            invasionScreen.SetActive(false);
            invading = false;
            PlayerNationManager.PlayerNation.UpdateTroopDisplays();
        }

        private Country source;

        public void LaunchAttack()
        {
            invasionScreen.SetActive(true);
            source = TroopMover.Instance.currentCountry;
            TroopMover.Instance.ResetSelected();
            otherGUIParent.SetActive(false);
            invading = true;
            PlayerNationManager.PlayerNation.UpdateTroopDisplays();
            GameCamera.Instance.troopDisplayHover = false;
        }

        public bool AttackAlreadyExists(Country source, Country target)
        {
            foreach (var attack in attacks)
            {
                if (attack.Source == source && attack.Target == target)
                {
                    return true;
                }
            }

            return false;
        }

        public void SelectInvasionTarget(Country target)
        {
            if (source.borders.Contains(target) &&
                (source.HasTroopsOfController(PlayerNationManager.PlayerNation) || source.GetNation().IsAtWarWith(target.GetNation())) && 
                PlayerNationManager.PlayerNation.IsAtWarWith(target.GetNation()) && TurnManager.Instance.CanPerformAction() && 
                !AttackAlreadyExists(source, target) && PlayerNationManager.PlayerNation.CanAfford(TroopMover.Instance.attackCost))
            {
                Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.NewAttack);
                message.AddString(source.countryName);
                message.AddString(target.countryName);
                message.AddString(PlayerNationManager.PlayerNation.Name);
                NetworkManager.Instance.Client.Send(message);
                
                TurnManager.Instance.PerformedAction();
            }
            
            PlayerNationManager.PlayerNation.UpdateTroopDisplays();
        }

        public void WarEnded(War warThatEnded, bool defenderVictory)
        {
            if (defenderVictory)
            {
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"Finality!",
                    $"Today, the {warThatEnded.Name} ended with the Defenders emerging victorious",
                    () => { return; }, 5);
            }
            else
            {
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"Finality!",
                    $"Today, the {warThatEnded.Name} ended with the Belligerents emerging victorious",
                    () => { return; }, 5);
            }

            wars.Remove(warThatEnded);
            
            foreach (var defender in warThatEnded.Defenders)
            {
                defender.Wars.Remove(warThatEnded);
            }
            
            foreach (var belligerent in warThatEnded.Belligerents)
            {
                belligerent.Wars.Remove(warThatEnded);
            }
            
            warThatEnded.Belligerents.Clear();
            warThatEnded.Defenders.Clear();
        }

        public void LaunchAttack(Country target, Country source, Nation instigator)
        {
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.NewAttack);
            message.AddString(source.countryName);
            message.AddString(target.countryName);
            message.AddString(instigator.Name);
            NetworkManager.Instance.Client.Send(message);
        }

        public void LaunchedAttack(Country target, Country source, Nation instigator)
        {
            ResetSelected();

            LineRenderer line = new GameObject("Line renderer").AddComponent<LineRenderer>();
            line.transform.parent = attackLinesParent;

            Vector3 sourcePos = source.center.position;
            Vector3 targetPos = target.center.position;

            line.SetPosition(0, sourcePos);
            line.SetPosition(1, targetPos);
            
            line.sortingOrder = 1;
            line.widthMultiplier = 0.025f;
            line.material = lineMat;
            line.startColor = Color.black;
            line.endColor = Color.black;

            Attack attack = new Attack();
            attack.Source = source;
            attack.Target = target;
            attack.line = line;
            attack.Instigator = instigator;

            foreach (var war in source.GetNation().Wars)
            {
                // defender is launching the attack
                if (war.Defenders.Contains(source.GetNation()) && war.Belligerents.Contains(target.GetNation()))
                {
                    attack.war = war;
                    attack.launchedByDefenders = true;
                }
                // belligerent is launching the attack
                if (war.Belligerents.Contains(source.GetNation()) && war.Defenders.Contains(target.GetNation()))
                {
                    attack.war = war;
                    attack.launchedByDefenders = false;
                }
            }

            foreach (var agreement in source.GetNation().agreements)
            {
                if (agreement.militaryAccess)
                {
                    foreach (var nation in agreement.Nations)
                    {
                        foreach (var war in nation.Wars)
                        {
                            // defender is launching the attack
                            if (war.Defenders.Contains(nation) && war.Belligerents.Contains(target.GetNation()))
                            {
                                attack.war = war;
                                attack.launchedByDefenders = true;
                            }
                            // belligerent is launching the attack
                            if (war.Belligerents.Contains(nation) && war.Defenders.Contains(target.GetNation()))
                            {
                                attack.war = war;
                                attack.launchedByDefenders = false;
                            }
                        }
                    }
                }
            }

            attack.line.enabled = PlayerNationManager.PlayerNation.Wars.Contains(attack.war);
            
            attacks.Add(attack);
            
            target.GetNation().UpdateInfluenceColour();
            target.GetNation().UpdateTroopDisplays();
            
            source.GetNation().UpdateInfluenceColour();
            source.GetNation().UpdateTroopDisplays();
        }
    }
    
    public class War
    {
        public string Name;
        public List<Nation> Defenders = new List<Nation>();
        public List<Nation> Belligerents = new List<Nation>();

        public bool over;

        public void NationJointedDefenders(Nation nationThatJoined)
        {
            Defenders.Add(nationThatJoined);
        }
        
        public void NationJointedBelligerents(Nation nationThatJoined)
        {
            Belligerents.Add(nationThatJoined);
        }

        public void RemoveBelligerent(Nation nationToRemove)
        {
            if (Belligerents.Contains(nationToRemove))
            {
                Belligerents.Remove(nationToRemove);
            }
            CheckIfEnded();
        }

        public void RemoveDefender(Nation nationToRemove)
        {
            if (Defenders.Contains(nationToRemove))
            {
                Defenders.Remove(nationToRemove);
            }
            CheckIfEnded();
        }

        public void CheckIfEnded()
        {
            if (Defenders.Count <= 0)
            {
                foreach (var belligerent in Belligerents)
                {
                    int gain = NationManager.GetDiplomaticPowerGain(belligerent);

                    belligerent.DiplomaticPower += gain;
                }
                CombatManager.Instance.WarEnded(this, false);
                over = true;
            }
            else if (Belligerents.Count <= 0)
            {
                foreach (var defender in Defenders)
                {
                    int gain = NationManager.GetDiplomaticPowerGain(defender);

                    defender.DiplomaticPower += gain;
                }
                CombatManager.Instance.WarEnded(this, true);
                over = true;
            }
        }
    }

    public class Attack
    {
        public Country Source;
        public Country Target;

        public Nation Instigator;

        public War war;
        public bool launchedByDefenders;

        public LineRenderer line;

        public bool success;

        public float calculatedAttack;
        public float calculatedDefense;

        public bool someoneElseHadMoreAttack;
    }
}