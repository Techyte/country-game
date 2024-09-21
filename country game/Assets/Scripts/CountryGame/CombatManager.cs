namespace CountryGame
{
    using UnityEngine;

    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        [SerializeField] private Notification notificationPrefab;
        [SerializeField] private Transform notificationParent;
        
        public void DeclareWarOn(Nation nationToWarWith)
        {
            War war = new War();
            war.Name = $"{PlayerNationManager.PlayerNation.Name} {nationToWarWith.Name} War";
            
            NationManager.Instance.NewWar(war);
            NationManager.Instance.NationJoinWar(PlayerNationManager.PlayerNation, war);
            NationManager.Instance.NationJoinWar(nationToWarWith, war);
            
            Notification notification = Instantiate(notificationPrefab, notificationParent);
            notification.Init($"To War!", $"Today, {PlayerNationManager.PlayerNation.Name} declared war on {nationToWarWith.Name}, this will surely be one to remember", () => {CountrySelector.Instance.OpenWarScreen(war);}, 5);
        }
    }
}