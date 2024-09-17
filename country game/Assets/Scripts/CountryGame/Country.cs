namespace CountryGame
{
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEditor;
    using UnityEngine;

    public class Country : MonoBehaviour
    {
        public string countryName;
        [SerializeField] private Nation nation;

        [HideInInspector] public CountryButton button;

        private void Awake()
        {
            button = GetComponent<CountryButton>();
            if (string.IsNullOrEmpty(countryName))
            {
                countryName = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(gameObject.name.ToLower()).Replace('_', ' ');
            }
        }

        private void Start()
        {
            if (nation != null)
            {
                button.ChangeColor(nation.Color);
            }
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
            button.ChangeColor(nation.Color);
            this.nation = nation;
        }
    }
}