using System;
using System.Collections.Generic;

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
        
        public void DeclareWarOn(Nation nationToWarWith)
        {
            War war = new War();
            war.Name = $"{PlayerNationManager.PlayerNation.Name} {nationToWarWith.Name} War";
            
            NewWar(war);
            NationJoinWar(PlayerNationManager.PlayerNation, war);
            NationJoinWar(nationToWarWith, war);
            
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init($"To War!", $"Today, {PlayerNationManager.PlayerNation.Name} declared war on {nationToWarWith.Name}, this will surely be one to remember", () => {CountrySelector.Instance.OpenWarScreen(war);}, 5);
        }

        public void NationJoinWar(Nation nationToJoinWar, War warToJoin)
        {
            if (!nationToJoinWar.Wars.Contains(warToJoin) && !warToJoin.Nations.Contains(nationToJoinWar))
            {
                nationToJoinWar.JoinWar(warToJoin);
                warToJoin.NationJointed(nationToJoinWar);
            }
        }
    }
    
    public class War
    {
        public string Name;
        public List<Nation> Nations = new List<Nation>();

        public void NationJointed(Nation nationThatJoined)
        {
            Nations.Add(nationThatJoined);
        }
    }
}