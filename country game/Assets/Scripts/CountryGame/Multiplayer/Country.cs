namespace CountryGame.Multiplayer
{
    using System.Globalization;
    using UnityEngine;

    public class Country : MonoBehaviour
    {
        public string countryName;
        private Nation nation;

        [HideInInspector] public CountryButton button;

        public int defense;
        public int attack;

        private void OnValidate()
        {
            if (defense == 0)
            {
                defense = 7;
            }

            if (attack == 0)
            {
                attack = 7;
            }

            if (string.IsNullOrEmpty(countryName))
            {
                countryName = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(gameObject.name.ToLower())
                    .Replace('_', ' ');
            }
        }

        private void Awake()
        {
            button = GetComponent<CountryButton>();
            if (string.IsNullOrEmpty(countryName))
            {
                countryName = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(gameObject.name.ToLower())
                    .Replace('_', ' ');
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

        public void ChangeNation(Nation nation)
        {
            ChangeColour(nation.Color);
            this.nation = nation;
        }
    }
}