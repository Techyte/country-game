using System;
using System.Collections.Generic;
using System.Linq;
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
            TurnManager.Instance.NewTurn += NewTurn;
        }

        private void Start()
        {
            invasionScreen.SetActive(false);
        }

        public void NewWar(War war)
        {
            wars.Add(war);
        }

        private void NewTurn(object sender, EventArgs e)
        {
            List<Attack> currentAttacks = attacks.ToList();
            
            foreach (var attack in currentAttacks)
            {
                CompleteAttack(attack);
            }
        }

        private void CompleteAttack(Attack attack)
        {
            if (AttackSuccessful(attack))
            {
                Nation nationToTakeTerritory = attack.war.Belligerents[0];

                if (attack.launchedByDefenders)
                {
                    nationToTakeTerritory = attack.war.Defenders[0];
                }
                    
                List<TroopInformation> troopInfos = attack.Target.troopInfos.Values.ToList();

                int originControlledTroops = 0;
                                
                foreach (var info in troopInfos)
                {
                    if (info.ControllerNation == attack.Target.GetNation())
                    {
                        originControlledTroops = info.NumberOfTroops;
                    }
                }

                int leftOver = Mathf.CeilToInt(originControlledTroops / 2f);
                if (leftOver < 1)
                {
                    leftOver = 1;
                }

                attack.Target.troopInfos.Remove(attack.Target.GetNation());
                attack.Target.MovedTroopsIn(nationToTakeTerritory, leftOver);
                    
                NationManager.Instance.SwapCountriesNation(attack.Target, nationToTakeTerritory, false);
                    
                Destroy(attack.line.gameObject);
                attacks.Remove(attack);
            }
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
            
            Debug.Log($"Source attack: {sourceAttack}");
            Debug.Log($"Troop added attack {sourceAttack-originalAttack}");
            Debug.Log($"Target defense: {targetDefense}");
            Debug.Log($"Troop added defense {targetDefense-originalDefense}");
            
            return sourceAttack > targetDefense;
        }

        public void DeclareWarOn(Nation nationToWarWith)
        {
            DeclareWarOn(PlayerNationManager.PlayerNation, nationToWarWith);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSelected();
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
            notification.Init($"To War!", $"Today, {nationThatDeclared.Name} declared war on {nationToWarWith.Name}, this will surely be one to remember", () => {CountrySelector.Instance.OpenWarScreen(war);}, 5);
            
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
        }

        public void NationJoinWarBelligerents(Nation nationToJoinWar, War warToJoin)
        {
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

        public void SelectInvasionTarget(Country target)
        {
            if (source.borders.Contains(target) && PlayerNationManager.PlayerNation.IsAtWarWith(target.GetNation()) && TurnManager.Instance.CanPerformAction())
            {
                LaunchedAttack(target, source);
                TurnManager.Instance.PerformedAction();
            }
        }

        public void WarEnded(War warThatEnded, bool defenderVictory)
        {
            wars.Remove(warThatEnded);

            if (defenderVictory)
            {
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"Finality!", $"Today, the {warThatEnded.Name} ended with the Defenders emerging victorious", () => {CountrySelector.Instance.Clicked(warThatEnded.Defenders[0]);}, 5);
            }
            else
            {
                Notification notification = Instantiate(notificationPrefab, notificationParent);
                notification.Init($"Finality!", $"Today, the {warThatEnded.Name} ended with the Belligerents emerging victorious", () => {CountrySelector.Instance.Clicked(warThatEnded.Belligerents[0]);}, 5);
            }
            
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

        private void LaunchedAttack(Country target, Country source)
        {
            ResetSelected();

            LineRenderer line = new GameObject("Line renderer").AddComponent<LineRenderer>();
            line.transform.parent = attackLinesParent;

            Vector3 sourcePos = source.GetComponent<PolygonCollider2D>().bounds.center;
            Vector3 targetPos = target.GetComponent<PolygonCollider2D>().bounds.center;

            line.SetPosition(0, sourcePos);
            line.SetPosition(1, targetPos);
            
            // SpriteRenderer arrow = new GameObject("Sprite renderer").AddComponent<SpriteRenderer>();
            // arrow.transform.parent = line.transform;
            // arrow.sprite = arrowSprite;
            //
            // arrow.transform.position = targetPos;
            //
            // Vector3 relativePos = targetPos - sourcePos;
            // Quaternion rotation = Quaternion.LookRotation(relativePos);
            // rotation.x = arrow.transform.rotation.x;
            // rotation.y = arrow.transform.rotation.y;
            // if (relativePos.y > 0)
            // {
            //     rotation.z -= 90;
            // }
            // else
            // {
            //     rotation.z += 90;
            // }
            // arrow.transform.rotation = rotation;
            
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
                    attack.war = war;
                    attack.launchedByDefenders = true;
                }
                // belligerent is launching the attack
                if (war.Belligerents.Contains(source.GetNation()) && war.Defenders.Contains(target.GetNation()))
                {
                    Debug.Log($"Found the {war.Name}");
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
                                Debug.Log($"Found the {war.Name}");
                                attack.war = war;
                                attack.launchedByDefenders = false;
                            }
                        }
                    }
                }
            }
            
            attacks.Add(attack);
        }
    }
    
    public class War
    {
        public string Name;
        public List<Nation> Defenders = new List<Nation>();
        public List<Nation> Belligerents = new List<Nation>();

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
                CombatManager.Instance.WarEnded(this, true);
            }
            else if (Belligerents.Count <= 0)
            {
                CombatManager.Instance.WarEnded(this, false);
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
    }
}