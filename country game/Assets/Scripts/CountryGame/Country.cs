namespace CountryGame
{
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEngine;

    public class Country : MonoBehaviour
    {
        public string countryName; 
        private Nation nation;
        [SerializeField] private TroopDisplay troopDisplay;

        [HideInInspector] public CountryButton button;

        public List<Nation> borders = new List<Nation>();

        public int troopCount;

        private void Awake()
        {
            button = GetComponent<CountryButton>();
            if (string.IsNullOrEmpty(countryName))
            {
                countryName = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(gameObject.name.ToLower()).Replace('_', ' ');
            }

            if (troopDisplay == null)
            {
                troopDisplay = Instantiate(Resources.Load<TroopDisplay>("TroopDisplay"), transform);
            }

            borders = GetBorders();
        }

        public void SignedNewAgreement(Agreement agreement)
        {
            if (agreement.militaryAccess && agreement.Nations.Contains(PlayerNationManager.PlayerNation) || nation.playerNation)
            {
                troopDisplay.UpdateDisplay(this, true);
            }
        }

        public void BecomePlayerNation()
        {
            troopDisplay.UpdateDisplay(this, true);
        }

        public void ChangeColour(Color color)
        {
            button.ChangeColor(color);
        }

        public Nation GetNation()
        {
            return nation;
        }

        public List<Nation> GetBorders()
        {
            PolygonCollider2D thisCollider = GetComponent<PolygonCollider2D>();
            
            List<Collider2D> overlappingColliders = new List<Collider2D>();

            ContactFilter2D filter = new ContactFilter2D();
            
            Physics2D.OverlapCollider(thisCollider, filter, overlappingColliders);

            List<Nation> borderNations = new List<Nation>();

            foreach (var colider in overlappingColliders)
            {
                if (colider.Distance(thisCollider).distance < -0.02f)
                {
                    Country country = colider.GetComponent<Country>();
                
                    if (country != null)
                    {
                        if (!borderNations.Contains(country.nation))
                        {
                            borderNations.Add(country.nation);
                        }
                    }
                }
            }

            return borderNations;
        }

        public void ChangeNation(Nation nation)
        {
            ChangeColour(nation.Color);
            this.nation = nation;
            troopDisplay.UpdateDisplay(this, nation.playerNation);
        }
    }
}