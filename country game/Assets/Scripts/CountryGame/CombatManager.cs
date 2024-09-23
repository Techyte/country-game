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

        private void Awake()
        {
            Instance = this;
            TurnManager.Instance.NewTurn += NewTurn;
        }

        public void NewWar(War war)
        {
            wars.Add(war);
        }

        private void NewTurn(object sender, EventArgs e)
        {
            
        }

        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;
        [SerializeField] private GameObject otherGUIParent;
        [SerializeField] private GameObject invasionScreen;
        [SerializeField] private Transform attackLinesParent;
        [SerializeField] private Material lineMat;
        [SerializeField] private Sprite arrowSprite;

        public bool invading;

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
            NationJoinWarBelligerents(nationThatDeclared, war);
            NationJoinWarDefenders(nationToWarWith, war);
            
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init($"To War!", $"Today, {nationThatDeclared.Name} declared war on {nationToWarWith.Name}, this will surely be one to remember", () => {CountrySelector.Instance.OpenWarScreen(war);}, 5);

            List<Agreement> agreements = nationThatDeclared.agreements.ToList();
            
            foreach (var agreement in agreements)
            {
                if (agreement.autoJoinWar && agreement.AgreementLeader == nationThatDeclared)
                {
                    foreach (var nation in agreement.Nations)
                    {
                        if (!nation.Wars.Contains(war))
                        {
                            NationJoinWarBelligerents(nation, war);
                        }
                    }
                }

                if (agreement.Nations.Contains(nationToWarWith))
                {
                    NationManager.Instance.NationLeaveAgreement(nationToWarWith, agreement);
                }
            }
            
            agreements = nationToWarWith.agreements.ToList();
            
            foreach (var agreement in agreements)
            {
                if (agreement.autoJoinWar && agreement.AgreementLeader == nationThatDeclared)
                {
                    foreach (var nation in agreement.Nations)
                    {
                        if (!nation.Wars.Contains(war))
                        {
                            NationJoinWarDefenders(nation, war);
                        }
                    }
                }

                if (agreement.Nations.Contains(nationThatDeclared))
                {
                    NationManager.Instance.NationLeaveAgreement(nationThatDeclared, agreement);
                }
            }
        }

        public void NationJoinWarBelligerents(Nation nationToJoinWar, War warToJoin)
        {
            if (!nationToJoinWar.Wars.Contains(warToJoin) && !warToJoin.Belligerents.Contains(nationToJoinWar))
            {
                nationToJoinWar.JoinWar(warToJoin);
                warToJoin.NationJointedBelligerents(nationToJoinWar);
            }

            CountrySelector.Instance.DisplayWarMembers(warToJoin);
        }
        
        public void NationJoinWarDefenders(Nation nationToJoinWar, War warToJoin)
        {
            if (!nationToJoinWar.Wars.Contains(warToJoin) && !warToJoin.Defenders.Contains(nationToJoinWar))
            {
                nationToJoinWar.JoinWar(warToJoin);
                warToJoin.NationJointedDefenders(nationToJoinWar);
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
            otherGUIParent.SetActive(false);
            invasionScreen.SetActive(true);
            source = TroopMover.Instance.currentCountry;
            TroopMover.Instance.ResetSelected();
            invading = true;
        }

        public void SelectInvasionTarget(Country target)
        {
            bool canInvade = false;

            foreach (var country in PlayerNationManager.PlayerNation.Countries)
            {
                if (country.borders.Contains(target) && PlayerNationManager.PlayerNation.IsAtWarWith(target.GetNation()))
                {
                    canInvade = true;
                }
            }

            if (canInvade)
            {
                LaunchedAttack(target, source);
            }
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
    }

    public class Attack
    {
        public Country Source;
        public Country Target;

        public LineRenderer line;
    }
}