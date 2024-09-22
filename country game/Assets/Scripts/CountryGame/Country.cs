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

        public Dictionary<Nation, TroopInformation> troopInfos = new Dictionary<Nation, TroopInformation>();

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

        public int TotalTroopCount()
        {
            int total = 0;
            
            foreach (var info in troopInfos.Values)
            {
                total += info.NumberOfTroops;
            }

            return total;
        }

        public void MovedTroopsIn(Nation source, int numberOfTroops)
        {
            if (troopInfos.TryGetValue(source, out TroopInformation info))
            {
                info.NumberOfTroops += numberOfTroops;
            }
            else
            {
                TroopInformation newInfo = new TroopInformation();
                newInfo.ControllerNation = source;
                newInfo.NumberOfTroops = numberOfTroops;
                
                troopInfos.Add(source, newInfo);
            }
            
            troopDisplay.UpdateDisplay(this);
        }

        public void MoveTroopsOut(Nation controller, int numberOfTroops)
        {
            if (troopInfos.TryGetValue(controller, out TroopInformation info))
            {
                info.NumberOfTroops -= numberOfTroops;
            }
            troopDisplay.UpdateDisplay(this);
        }
        
        public void ResetTroops()
        {
            troopInfos.Clear();
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
            if (countryName == "France")
            { 
                borders.Add(NationManager.Instance.GetNationByName("United Kingdom"));
            }
            else if (countryName == "United Kingdom") 
            { 
                borders.Add(NationManager.Instance.GetNationByName("France"));
            }
            else if (countryName == "Russia")
            { 
                borders.Add(NationManager.Instance.GetNationByName("Alaska")); 
                borders.Add(NationManager.Instance.GetNationByName("Japan"));
            }
            else if (countryName == "Alaska")
            {
                borders.Add(NationManager.Instance.GetNationByName("Russia"));
            }
            else if (countryName == "United States")
            {
                borders.Add(NationManager.Instance.GetNationByName("Cuba"));
            }
            else if (countryName == "Mexico")
            {
                borders.Add(NationManager.Instance.GetNationByName("Cuba"));
            }
            else if (countryName == "Cuba")
            {
                borders.Add(NationManager.Instance.GetNationByName("Mexico"));
                borders.Add(NationManager.Instance.GetNationByName("United States"));
            }
            else if (countryName == "Australia")
            {
                borders.Add(NationManager.Instance.GetNationByName("New Zealand"));
                borders.Add(NationManager.Instance.GetNationByName("Indonesia"));
            }
            else if (countryName == "New Zealand")
            {
                borders.Add(NationManager.Instance.GetNationByName("Australia"));
            }
            else if (countryName == "China")
            {
                borders.Add(NationManager.Instance.GetNationByName("Taiwan"));
            }
            else if (countryName == "Taiwan")
            {
                borders.Add(NationManager.Instance.GetNationByName("China"));
            }
            else if (countryName == "Indonesia")
            {
                borders.Add(NationManager.Instance.GetNationByName("Australia"));
            }
            else if (countryName == "Madagascar")
            {
                borders.Add(NationManager.Instance.GetNationByName("Mozambique"));
            }
            else if (countryName == "Mozambique")
            {
                borders.Add(NationManager.Instance.GetNationByName("Madagascar"));
            }
            else if (countryName == "Spain")
            {
                borders.Add(NationManager.Instance.GetNationByName("Morocco"));
            }
            else if (countryName == "Morocco")
            {
                borders.Add(NationManager.Instance.GetNationByName("Spain"));
            }
            else if (countryName == "Japan")
            {
                borders.Add(NationManager.Instance.GetNationByName("Russia"));
                borders.Add(NationManager.Instance.GetNationByName("South Korea"));
            }
            else if (countryName == "Japan")
            {
                borders.Add(NationManager.Instance.GetNationByName("Russia"));
                borders.Add(NationManager.Instance.GetNationByName("South Korea"));
            }
            else if (countryName == "South Korea")
            {
                borders.Add(NationManager.Instance.GetNationByName("Japan"));
            }
            else if (countryName == "Iceland")
            {
                borders.Add(NationManager.Instance.GetNationByName("Greenland"));
            }
            else if (countryName == "Greenland")
            {
                borders.Add(NationManager.Instance.GetNationByName("Iceland"));
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

    public class TroopInformation
    {
        public Nation ControllerNation;
        public int NumberOfTroops;
    }
}