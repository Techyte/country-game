using System;
using System.Collections.Generic;
using TMPro;

namespace CountryGame
{
    using UnityEngine;

    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance;
        
        public List<War> wars = new List<War>();

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

            foreach (var agreement in nationThatDeclared.agreements)
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
            }
            
            foreach (var agreement in nationToWarWith.agreements)
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
        }

        public void LaunchAttack()
        {
            otherGUIParent.SetActive(false);
            invasionScreen.SetActive(true);
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
}