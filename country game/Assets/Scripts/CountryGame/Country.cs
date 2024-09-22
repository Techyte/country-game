using System;

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

        public List<Country> borders = new List<Country>();

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
        }

        private void Start()
        {
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
                if (info.NumberOfTroops <= 0)
                {
                    troopInfos.Remove(controller);
                }
            }
            troopDisplay.UpdateDisplay(this);
        }

        public bool CanMoveNumTroopsOut(Nation controller, int amount)
        {
            if (troopInfos.TryGetValue(controller, out TroopInformation info))
            {
                int minimum = 0;

                if (controller == nation)
                {
                    minimum = 1;
                }
                
                if (info.NumberOfTroops - amount >= minimum)
                {
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }
        
        public void ResetTroops()
        {
            troopInfos.Clear();
        }

        public Nation GetNation()
        {
            return nation;
        }

        public float DistanceTo(Country country)
        {
            return (GetComponent<PolygonCollider2D>().bounds.center - country.gameObject.GetComponent<PolygonCollider2D>().bounds.center).magnitude;
        }

        public List<Country> GetBorders()
        {
            PolygonCollider2D thisCollider = GetComponent<PolygonCollider2D>();
            
            List<Collider2D> overlappingColliders = new List<Collider2D>();

            ContactFilter2D filter = new ContactFilter2D();
            
            Physics2D.OverlapCollider(thisCollider, filter, overlappingColliders);

            List<Country> borderNations = new List<Country>();

            foreach (var colider in overlappingColliders)
            {
                if (colider.Distance(thisCollider).distance < -0.02f)
                {
                    Country country = colider.GetComponent<Country>();
                
                    if (country != null)
                    {
                        if (!borderNations.Contains(country))
                        {
                            borderNations.Add(country);
                        }
                    }
                }
            }
            if (countryName == "France")
            { 
                borders.Add(NationManager.Instance.GetCountryByName("United Kingdom"));
            }
            else if (countryName == "United Kingdom") 
            { 
                borders.Add(NationManager.Instance.GetCountryByName("France"));
            }
            else if (countryName == "Russia")
            { 
                borders.Add(NationManager.Instance.GetCountryByName("Alaska")); 
                borders.Add(NationManager.Instance.GetCountryByName("Japan"));
            }
            else if (countryName == "Alaska")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Russia"));
            }
            else if (countryName == "United States")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Cuba"));
            }
            else if (countryName == "Mexico")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Cuba"));
            }
            else if (countryName == "Cuba")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Mexico"));
                borders.Add(NationManager.Instance.GetCountryByName("United States"));
            }
            else if (countryName == "Australia")
            {
                borders.Add(NationManager.Instance.GetCountryByName("New Zealand"));
                borders.Add(NationManager.Instance.GetCountryByName("Indonesia"));
            }
            else if (countryName == "New Zealand")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Australia"));
            }
            else if (countryName == "China")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Taiwan"));
            }
            else if (countryName == "Taiwan")
            {
                borders.Add(NationManager.Instance.GetCountryByName("China"));
            }
            else if (countryName == "Indonesia")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Australia"));
            }
            else if (countryName == "Madagascar")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Mozambique"));
            }
            else if (countryName == "Mozambique")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Madagascar"));
            }
            else if (countryName == "Spain")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Morocco"));
            }
            else if (countryName == "Morocco")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Spain"));
            }
            else if (countryName == "Japan")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Russia"));
                borders.Add(NationManager.Instance.GetCountryByName("South Korea"));
            }
            else if (countryName == "Japan")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Russia"));
                borders.Add(NationManager.Instance.GetCountryByName("South Korea"));
            }
            else if (countryName == "South Korea")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Japan"));
            }
            else if (countryName == "Iceland")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Greenland"));
            }
            else if (countryName == "Greenland")
            {
                borders.Add(NationManager.Instance.GetCountryByName("Iceland"));
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