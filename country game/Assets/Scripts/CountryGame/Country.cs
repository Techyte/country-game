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

        private CountryButton button;

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

        public void GetBorders()
        {
            PolygonCollider2D thisCollider = GetComponent<PolygonCollider2D>();
            
            List<Collider2D> overlappingColliders = new List<Collider2D>();

            ContactFilter2D filter = new ContactFilter2D();
            
            Physics2D.OverlapCollider(thisCollider, filter, overlappingColliders);

            foreach (var colider in overlappingColliders)
            {
                if (colider.Distance(thisCollider).distance < -0.02f)
                {
                    Country country = colider.GetComponent<Country>();
                
                    if (country != null)
                    {
                        Debug.Log(country.countryName);
                    }
                }
            }
        }

        public void ChangeNation(Nation nation)
        {
            button.ChangeColor(nation.Color);
            this.nation = nation;
        }
    }

    [CustomEditor(typeof(Country))]
    public class countryEditor : Editor
    {
        // Rendering code for the PixelCollider2D custom inspector
        public override void OnInspectorGUI()
        {
            Country country = (Country)target;
            if (GUILayout.Button("Check Borders"))
            {
                country.GetBorders();
            }
        }
    }
}