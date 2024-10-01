using System;
using System.Collections.Generic;
using System.Linq;
using Riptide;
using TMPro;

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
            List<Attack> currentAttacks = attacks.ToList();

            foreach (var attack in currentAttacks)
            {
                attack.success = AttackSuccessful(attack);
            }

            List<string> countrysTransfered = new List<string>();
            List<string> countrysThatTookTerritory = new List<string>();
            List<string> nationsThatTook = new List<string>();
            List<string> countriesThatFailedAttack = new List<string>();
            List<string> countriesThatRepelledAttack = new List<string>();
            List<string> countriesThatHelpedAttack = new List<string>();
            List<string> countriesThatTookAHelpedAttack = new List<string>();

            foreach (var attack in currentAttacks)
            {
                if (attack.disabled)
                {
                    Destroy(attack.line.gameObject);
                    attacks.Remove(attack);
                }
                
                if (attack.success && !attack.war.over)
                {
                    Nation nationToTakeTerritory = attack.war.Belligerents[0];

                    if (attack.launchedByDefenders)
                    {
                        nationToTakeTerritory = attack.war.Defenders[0];
                    }
                    
                    // move the troops to the new territory

                    attack.Target.MovedTroopsIn(nationToTakeTerritory, attack.Target.TroopsOfController(attack.Target.GetNation()));
                    attack.Target.troopInfos.Remove(attack.Target.GetNation());

                    // nullify attacks from the territory we just took
                    foreach (var otherAttack in attacks)
                    {
                        if (otherAttack.Source == attack.Target)
                        {
                            otherAttack.disabled = true;
                        }
                    }
                    
                    countrysTransfered.Add(attack.Target.countryName);
                    nationsThatTook.Add(nationToTakeTerritory.Name);
                    countrysThatTookTerritory.Add(attack.Source.countryName);

                    NationManager.Instance.SwapCountriesNation(attack.Target, nationToTakeTerritory, false);

                    Destroy(attack.line.gameObject);
                    attacks.Remove(attack);
                }
                else if (attack.war.over)
                {
                    // attack was successful it was just got to last
                    
                    Debug.Log("Successful attack but we already knew that");
                    
                    // COPE: by the time we get here the countrys nation has already been changed so we cant know if
                    // the troops were at war with them so for the sake of me not having to fix 
                    // that we will say that they were a distraction thats only purpose was to take troops away from the line while the main attack took place
                    
                    // int troopsToMoveIn = attack.Source.TroopsOfController(attack.Source.GetNation()) / 2;
                    //
                    // foreach (var info in attack.Source.troopInfos.Values)
                    // {
                    //     if (info.ControllerNation.IsAtWarWith(attack.Target.GetNation()))
                    //     {
                    //         attack.Target.MovedTroopsIn(info.ControllerNation, troopsToMoveIn);
                    //         attack.Source.MoveTroopsOut(info.ControllerNation, attack.Source.TroopsOfController(attack.Source.GetNation())-troopsToMoveIn);
                    //     }
                    // }
                    
                    countriesThatHelpedAttack.Add(attack.Source.countryName);
                    countriesThatTookAHelpedAttack.Add(attack.Target.countryName);
                    
                    Destroy(attack.line.gameObject);
                    attacks.Remove(attack);
                }
                else
                {
                    // attack was a failure
                    foreach (var info in attack.Source.troopInfos.Values.ToList())
                    {
                        if (info.ControllerNation.IsAtWarWith(attack.Target.GetNation()))
                        {
                            attack.Source.MoveTroopsOut(info.ControllerNation, 1);
                        }
                    }
                    
                    countriesThatFailedAttack.Add(attack.Source.countryName);
                    countriesThatRepelledAttack.Add(attack.Target.countryName);
                    
                    Destroy(attack.line.gameObject);
                    attacks.Remove(attack);
                }
            }

            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.CombatResults);
            message.AddStrings(countrysTransfered.ToArray());
            message.AddStrings(countrysThatTookTerritory.ToArray());
            message.AddStrings(nationsThatTook.ToArray());
            message.AddStrings(countriesThatFailedAttack.ToArray());
            message.AddStrings(countriesThatRepelledAttack.ToArray());
            message.AddStrings(countriesThatHelpedAttack.ToArray());
            message.AddStrings(countriesThatTookAHelpedAttack.ToArray());
            
            NetworkManager.Instance.Server.SendToAll(message, NetworkManager.Instance.Client.Id);
            
            NationManager.Instance.HandleHiringTroops();
        }

        public void HandleCombatResults(List<string> countriesTransfered, List<string> countriesThatTookTerritory, List<string> nationsThatTook,
            List<string> countriesThatFailedAttack, List<string> countriesThatRepelledAttack, List<string> countriesThatHelpedAttack,
            List<string> countriesThatTookAHelpedAttack)
        {
            for (int i = 0; i < countriesThatFailedAttack.Count; i++)
            {
                Country country = NationManager.Instance.GetCountryByName(countriesThatFailedAttack[i]);
                Country repelled = NationManager.Instance.GetCountryByName(countriesThatRepelledAttack[i]);
                country.MoveTroopsOut(country.GetNation(), 1);
                
                Attack attackRef = null;

                foreach (var attack in attacks)
                {
                    if (attack.Source == country && attack.Target == repelled)
                    {
                        attackRef = attack;
                    }
                }

                Destroy(attackRef.line.gameObject);
                attacks.Remove(attackRef);
            }
            
            for (int i = 0; i < countriesThatHelpedAttack.Count; i++)
            {
                Country source = NationManager.Instance.GetCountryByName(countriesThatHelpedAttack[i]);
                Country target = NationManager.Instance.GetCountryByName(countriesThatTookAHelpedAttack[i]);
                
                Attack attackRef = null;

                foreach (var attack in attacks)
                {
                    if (attack.Source == source && attack.Target == target)
                    {
                        attackRef = attack;
                    }
                }

                Destroy(attackRef.line.gameObject);
                attacks.Remove(attackRef);
            }

            for (int i = 0; i < countriesTransfered.Count; i++)
            {
                Country countryTaken = NationManager.Instance.GetCountryByName(countriesTransfered[i]);
                
                Nation nationToTakeTerritory = NationManager.Instance.GetNationByName(nationsThatTook[i]);
                
                Country countryThatTook = NationManager.Instance.GetCountryByName(countriesThatTookTerritory[i]);

                // move the troops to the new territory

                countryTaken.MovedTroopsIn(nationToTakeTerritory, countryTaken.TroopsOfController(countryTaken.GetNation()));
                countryTaken.troopInfos.Remove(countryTaken.GetNation());

                // nullify attacks from the territory we just took
                foreach (var otherAttack in attacks) 
                {
                    if (otherAttack.Source == countryTaken)
                    {
                        otherAttack.disabled = true;
                    }
                }
                
                nationsThatTook.Add(nationToTakeTerritory.Name);

                NationManager.Instance.SwapCountriesNation(countryTaken, nationToTakeTerritory, false);

                Attack attackRef = null;

                foreach (var attack in attacks)
                {
                    if (attack.Source == countryThatTook && attack.Target == countryTaken)
                    {
                        attackRef = attack;
                    }
                }
                
                Destroy(attackRef.line.gameObject);
                attacks.Remove(attackRef);
            }
            
            NationManager.Instance.HandleHiringTroops();
        }

        private bool AttackSuccessful(Attack attack)
        {
            float infantryAttack = 0.5f;
            float infantryDefense = 0.7f;
            float tanksAttack = 0.2f;
            float tanksDefense = 1.2f;
            float marinesAttack = 0.4f;
            float marinesDefense = 0.5f;

            float originalAttack = attack.Source.attack;
            float originalDefense = attack.Target.defense;

            float sourceAttack = attack.Source.attack;

            float targetDefense = attack.Target.defense;
            
            foreach (var troopInfo in attack.Source.troopInfos.Values)
            {
                if (!troopInfo.ControllerNation.Wars.Contains(attack.war))
                {
                    continue;
                }
                
                float infantryTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.infantry;
                float tanksTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.tanks;
                float marinesTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.marines;

                sourceAttack += infantryAttack * infantryTroops;
                
                sourceAttack += tanksAttack * tanksTroops;
                
                sourceAttack += marinesAttack * marinesTroops;
            }
            
            foreach (var troopInfo in attack.Target.troopInfos.Values)
            {
                if (!troopInfo.ControllerNation.Wars.Contains(attack.war))
                {
                    continue;
                }
                
                float infantryTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.infantry;
                float tanksTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.tanks;
                float marinesTroops = troopInfo.NumberOfTroops * troopInfo.ControllerNation.marines;

                targetDefense += infantryDefense * infantryTroops;
                
                targetDefense += tanksDefense * tanksTroops;
                
                targetDefense += marinesDefense * marinesTroops;
            }

            List<Attack> attacksOnTarget = new List<Attack>();
            List<Attack> attacksFromSource = new List<Attack>();

            float totalAttackingForce = 0;
            float totalDefendingForce = 0;

            foreach (var otherAttack in attacks)
            {
                if (otherAttack.Target == attack.Target)
                {
                    attacksOnTarget.Add(otherAttack);
                    totalAttackingForce += otherAttack.Source.GetParticipatingTroops(attack.war);
                }

                if (otherAttack.Source == attack.Source)
                {
                    attacksFromSource.Add(attack);
                    totalDefendingForce += otherAttack.Target.GetParticipatingTroops(attack.war);
                }
            }
            
            if (totalDefendingForce <= 0)
            {
                // attackers will always win
                return true;
            }

            if (totalAttackingForce <= 0)
            {
                // defenders will always win
                return false;
            }
            
            // calculate how much of the defense is being used to counter this attack

            float attackScaler = attack.Target.GetParticipatingTroops(attack.war) / totalDefendingForce; // amount of attacking force that is being allocated to this attack
            float defenseScaler = attack.Source.GetParticipatingTroops(attack.war) / totalAttackingForce; // amount of defending force that is being allocated to this attack
            
            // calculate how much of the attack is being used to push this attack
            
            return sourceAttack * attackScaler > targetDefense * defenseScaler;
        }

        public void DeclareWarOn(Nation nationToWarWith)
        {
            Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.DeclareWar);
            message.AddString(PlayerNationManager.PlayerNation.Name);
            message.AddString(nationToWarWith.Name);

            NetworkManager.Instance.Client.Send(message);
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
                attack.line.enabled = PlayerNationManager.PlayerNation.Attacking(attack.Target);
            }
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

            nationThatDeclared.DiplomaticPower -= 30;
            nationToWarWith.DiplomaticPower += 30;
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
        }

        private Country source;

        public void LaunchAttack()
        {
            invasionScreen.SetActive(true);
            source = TroopMover.Instance.currentCountry;
            TroopMover.Instance.ResetSelected();
            otherGUIParent.SetActive(false);
            invading = true;
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
                !AttackAlreadyExists(source, target))
            {
                Message message = Message.Create(MessageSendMode.Reliable, GameMessageId.NewAttack);
                message.AddString(source.countryName);
                message.AddString(target.countryName);
                NetworkManager.Instance.Client.Send(message);
                
                TurnManager.Instance.PerformedAction();
            }
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

        public void LaunchedAttack(Country target, Country source)
        {
            ResetSelected();

            LineRenderer line = new GameObject("Line renderer").AddComponent<LineRenderer>();
            line.transform.parent = attackLinesParent;

            Vector3 sourcePos = source.GetComponent<PolygonCollider2D>().bounds.center;
            Vector3 targetPos = target.GetComponent<PolygonCollider2D>().bounds.center;

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

            foreach (var war in source.GetNation().Wars)
            {
                // defender is launching the attack
                if (war.Defenders.Contains(source.GetNation()) && war.Belligerents.Contains(target.GetNation()))
                {
                    Debug.Log($"found the {war.Name}");
                    attack.war = war;
                    attack.launchedByDefenders = true;
                }
                // belligerent is launching the attack
                if (war.Belligerents.Contains(source.GetNation()) && war.Defenders.Contains(target.GetNation()))
                {
                    Debug.Log($"found the {war.Name}");
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
                                Debug.Log($"found the {war.Name}");
                                attack.war = war;
                                attack.launchedByDefenders = true;
                            }
                            // belligerent is launching the attack
                            if (war.Belligerents.Contains(nation) && war.Defenders.Contains(target.GetNation()))
                            {
                                Debug.Log($"found the {war.Name}");
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
                CombatManager.Instance.WarEnded(this, false);
                over = true;
            }
            else if (Belligerents.Count <= 0)
            {
                CombatManager.Instance.WarEnded(this, true);
                over = true;
            }
        }
    }

    public class Attack
    {
        public Country Source;
        public Country Target;

        public War war;
        public bool launchedByDefenders;

        public LineRenderer line;

        public bool success;

        public bool disabled = false;
    }
}