namespace CountryGame
{
    using UnityEngine;

    public class ViewTypeManager : MonoBehaviour
    {
        public static ViewTypeManager Instance;
        
        public ViewType currentView = ViewType.Main;

        [SerializeField] private GameObject upgradingOverlay;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                MainView();
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                WarView();
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                DiplomacyView();
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                InfrastructureView();
            }
        }

        public void UpdateView()
        {
            upgradingOverlay.SetActive(false);
            foreach (var country in NationManager.Instance.counties)
            {
                switch (currentView)
                {
                    case ViewType.Main:
                        MainViewColor(country);
                        break;
                    case ViewType.War:
                        WarViewColor(country);
                        break;
                    case ViewType.Diplomacy:
                        DiplomacyViewColor(country);
                        break;
                    case ViewType.Infrastructure:
                        InfrastructureViewColor(country);
                        break;
                }
            }

            if (currentView == ViewType.Infrastructure)
            {
                upgradingOverlay.SetActive(true);
            }
            
            CombatManager.Instance.UpdateAttackDisplays();
            PlayerNationManager.PlayerNation.UpdateTroopDisplays();
        }

        private void MainViewColor(Country country)
        {
            country.button.overrideColour = Color.white;
            country.button.doOverrideColour = false;
        }

        private void WarViewColor(Country country)
        {
            country.button.overrideColour = Color.grey;
            
            if (country.GetNation().IsAtWarWith(PlayerNationManager.PlayerNation))
            {
                country.button.overrideColour = Color.red;
            }
            
            if (country.GetNation().MilitaryAccessWith(PlayerNationManager.PlayerNation))
            {
                country.button.overrideColour = Color.cyan;
            }

            if (country.GetNation() == PlayerNationManager.PlayerNation)
            {
                country.button.overrideColour = Color.blue;
            }
            
            country.button.doOverrideColour = true;
        }

        private void DiplomacyViewColor(Country country)
        {
            country.button.overrideColour = Color.grey;

            int influence = country.GetNation().HighestInfluence(out Nation nation);

            if (influence > 0 || country.GetNation().aPlayerNation)
            {
                country.button.overrideColour = Color.Lerp(country.button.baseColour, nation.Color, influence / 3f);
            }

            foreach (var agreement in country.GetNation().agreements)
            {
                if (agreement.militaryAccess)
                {
                    country.button.overrideColour = Color.Lerp(country.button.baseColour, agreement.AgreementLeader.Color, 0.75f);
                }

                if (agreement.autoJoinWar)
                {
                    country.button.overrideColour = Color.Lerp(country.button.baseColour, agreement.AgreementLeader.Color, 1);
                }
                
                if (agreement.AgreementLeader == country.GetNation() && agreement.influence > 0)
                {
                    country.button.overrideColour = country.GetNation().Color;
                }
            }
            
            country.button.doOverrideColour = true;
        }

        private void InfrastructureViewColor(Country country)
        {
            country.button.overrideColour = Color.grey;

            bool milAccess = PlayerNationManager.PlayerNation.MilitaryAccessWith(country.GetNation());

            if (milAccess)
            {
                country.button.overrideColour = Color.Lerp(Color.red, Color.green, country.Infrastructure / 10f);
            }

            if (country.upgradingThisTurn && milAccess)
            {
                country.button.overrideColour = Color.Lerp(country.button.overrideColour, Color.blue, 1/3f);
            }
            
            country.button.doOverrideColour = true;
        }
        
        public void MainView()
        {
            currentView = ViewType.Main;
            UpdateView();
        }
        
        public void WarView()
        {
            currentView = ViewType.War;
            UpdateView();
        }
        
        public void InfrastructureView()
        {
            currentView = ViewType.Infrastructure;
            UpdateView();
        }
        
        public void DiplomacyView()
        {
            currentView = ViewType.Diplomacy;
            UpdateView();
        }
    }

    public enum ViewType
    {
        Main,
        War,
        Infrastructure,
        Diplomacy
    }
}