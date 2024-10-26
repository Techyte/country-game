using System.Linq;
using UnityEditor;

namespace CountryGame
{
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEngine;

    public class Country : MonoBehaviour
    {
        public string nationName; 
        public string countryName; 
        private Nation nation;
        private TroopDisplay troopDisplay;

        [HideInInspector] public PolygonCollider2D collider;
        public Transform center;

        [HideInInspector] public CountryButton button;

        [HideInInspector] public List<Country> borders = new List<Country>();

        public Dictionary<Nation, TroopInformation> troopInfos = new Dictionary<Nation, TroopInformation>();

        [SerializeField] private int troopCapacity;

        public int Infrastructure
        {
            get
            {
                return infrastructure;
            }
            set
            {
                infrastructure = Mathf.Max(0, value);
            }
        }

        private int infrastructure = 0;

        [SerializeField] private int defense;
        [SerializeField] private int attack;
        
        public bool upgradingThisTurn;

        public SpriteMask mask;

        private void OnValidate()
        {
            if (defense == 9)
            {
                defense = 7;
            }

            if (attack == 0)
            {
                attack = 7;
            }

            if (troopCapacity == 15)
            {
                troopCapacity = 4;
            }
        }

        private void Awake()
        {
            mask = GetComponent<SpriteMask>();
            
            mask.enabled = false;
            
            button = GetComponent<CountryButton>();
            if (string.IsNullOrEmpty(countryName))
            {
                countryName = CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(gameObject.name.ToLower()).Replace('_', ' ');
            }

            if (troopDisplay == null)
            {
                troopDisplay = Instantiate(Resources.Load<TroopDisplay>("TroopDisplay"), transform);
            }

            collider = GetComponent<PolygonCollider2D>();
        }

        public int GetAttack()
        {
            return attack + infrastructure;
        }

        public int GetDefense()
        {
            return defense + infrastructure;
        }

        public int GetTroopCapacity()
        {
            return troopCapacity + infrastructure;
        }

        public void ChangeUpgradingStatus(bool newUpgrading)
        {
            upgradingThisTurn = newUpgrading;
            mask.enabled = newUpgrading && PlayerNationManager.PlayerNation.MilitaryAccessWith(nation);
        }

        public void CalculateBorders()
        {
            borders = GetBorders();
        }

        public void SignedNewAgreement(Agreement agreement)
        {
            UpdateTroopDisplay();
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
                if (TotalTroopCount() + numberOfTroops > GetTroopCapacity())
                {
                    info.NumberOfTroops += GetTroopCapacity() - TotalTroopCount();
                }
                else
                {
                    info.NumberOfTroops += numberOfTroops;
                }
            }
            else
            {
                TroopInformation newInfo = new TroopInformation();
                newInfo.ControllerNation = source;
                newInfo.Location = this;
                if (TotalTroopCount() + numberOfTroops > GetTroopCapacity())
                {
                    newInfo.NumberOfTroops = GetTroopCapacity() - TotalTroopCount();
                }
                else
                {
                    newInfo.NumberOfTroops = numberOfTroops;
                }
                
                troopInfos.Add(source, newInfo);
            }
            
            UpdateTroopDisplay();
        }

        public bool HasTroopsParticipatingInWar(War war)
        {
            foreach (var defender in war.Defenders)
            {
                if (troopInfos.Keys.Contains(defender))
                {
                    return true;
                }
            }
            
            foreach (var belligerent in war.Belligerents)
            {
                if (troopInfos.Keys.Contains(belligerent))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasTroopsOfController(Nation nation)
        {
            return troopInfos.Keys.Contains(nation);
        }

        public void UpdateTroopDisplay()
        {
            if (troopDisplay != null && PlayerNationManager.PlayerNation != null && nation != null)
            {
                troopDisplay.UpdateDisplay(this,
                    (nation.MilitaryAccessWith(PlayerNationManager.PlayerNation) ||
                     nation == PlayerNationManager.PlayerNation ||
                     PlayerNationManager.PlayerNation.Attacking(this) ||
                     PlayerNationManager.PlayerNation.Defending(this)) && 
                    !CombatManager.Instance.invading && ViewTypeManager.Instance.currentView != ViewType.Diplomacy && 
                    !CombatManager.Instance.invading && ViewTypeManager.Instance.currentView != ViewType.Infrastructure);
            }
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
            UpdateTroopDisplay();
        }

        public bool CanMoveNumTroopsOut(Nation controller, int amount)
        {
            Debug.Log(countryName);
            if (troopInfos.TryGetValue(controller, out TroopInformation info))
            {
                Debug.Log("found that controller");
                if (info.NumberOfTroops - amount >= 0)
                {
                    Debug.Log("moving that amount of troops would result in negative troops, cancelling");
                    return true;
                }

                Debug.Log("moving that amount of troops would NOT result in negative troops, cancelling");
                return false;
            }
            else
            {
                return false;
            }
        }

        public bool CanMoveNumTroopsIn(Nation controller, int amount)
        {
            if (TotalTroopCount() + amount > GetTroopCapacity())
            {
                return false;
            }

            return true;
        }

        public int TroopsOfController(Nation controller)
        {
            int total = 0;

            foreach (var info in troopInfos.Values)
            {
                if (info.ControllerNation == controller)
                {
                    total += info.NumberOfTroops;
                }
            }
            
            return total;
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

        public int GetParticipatingTroops(War war)
        {
            int total = 0;
            
            foreach (var info in troopInfos.Values)
            {
                if (info.ControllerNation.Wars.Contains(war))
                {
                    total += info.NumberOfTroops;
                }
            }

            return total;
        }

        public int GetParticipatingTroopsAttacking(Nation target)
        {
            int total = 0;
            
            foreach (var info in troopInfos.Values)
            {
                if (info.ControllerNation.IsAtWarWith(target))
                {
                    total += info.NumberOfTroops;
                }
            }

            return total;
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
                borderNations.Add(NationManager.Instance.GetCountryByName("United Kingdom"));
                borderNations.Add(NationManager.Instance.GetCountryByName("French Guiana"));
            }
            else if (countryName == "French Guiana") 
            { 
                borderNations.Add(NationManager.Instance.GetCountryByName("France"));
            }
            else if (countryName == "United Kingdom") 
            { 
                borderNations.Add(NationManager.Instance.GetCountryByName("France"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Iceland"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Belgium"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Northern Ireland"));
            }
            else if (countryName == "Northern Ireland") 
            { 
                borderNations.Add(NationManager.Instance.GetCountryByName("United Kingdom"));
            }
            else if (countryName == "Philippines") 
            { 
                borderNations.Add(NationManager.Instance.GetCountryByName("Malaysia"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Indonesia"));
            }
            else if (countryName == "Kamchatka")
            { 
                borderNations.Add(NationManager.Instance.GetCountryByName("Alaska")); 
                borderNations.Add(NationManager.Instance.GetCountryByName("Japan"));
            }
            else if (countryName == "Alaska")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Kamchatka"));
            }
            else if (countryName == "Southern United States")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Cuba"));
            }
            else if (countryName == "Southern Mexico")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Cuba"));
            }
            else if (countryName == "Cuba")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Southern Mexico"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Southern United States"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Haiti"));
            }
            else if (countryName == "Victoria")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("New Zealand"));
            }
            else if (countryName == "New Zealand")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Victoria"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Eastern Australia"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Papua New Guinea"));
            }
            else if (countryName == "Papua New Guinea")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Northern Australia"));
                borderNations.Add(NationManager.Instance.GetCountryByName("New Zealand"));
            }
            else if (countryName == "Belgium")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("United Kingdom"));
            }
            else if (countryName == "Norway")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Denmark"));
            }
            else if (countryName == "Sweden")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Denmark"));
            }
            else if (countryName == "Denmark")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Sweden"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Norway"));
            }
            else if (countryName == "Southern China")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Taiwan"));
            }
            else if (countryName == "Taiwan")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Southern China"));
            }
            else if (countryName == "Eastern Indonesia")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Northern Australia"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Philippines"));
            }
            else if (countryName == "Western Indonesia")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Western Australia"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Philippines"));
            }
            else if (countryName == "Western Australia")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Western Indonesia"));
            }
            else if (countryName == "Northern Australia")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Eastern indonesia"));
            }
            else if (countryName == "Eastern Australia")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("New Zealand"));
            }
            else if (countryName == "Madagascar")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Mozambique"));
            }
            else if (countryName == "Mozambique")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Madagascar"));
            }
            else if (countryName == "Spain")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Morocco"));
            }
            else if (countryName == "Morocco")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Spain"));
            }
            else if (countryName == "Japan")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Kamchatka"));
                borderNations.Add(NationManager.Instance.GetCountryByName("South Korea"));
                borderNations.Add(NationManager.Instance.GetCountryByName("Khabarovsk"));
            }
            else if (countryName == "Khabarovsk")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Japan"));
            }
            else if (countryName == "Kamchatka")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Japan"));
            }
            else if (countryName == "South Korea")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Japan"));
            }
            else if (countryName == "Iceland")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Greenland"));
                borderNations.Add(NationManager.Instance.GetCountryByName("United Kingdom"));
            }
            else if (countryName == "Greenland")
            {
                borderNations.Add(NationManager.Instance.GetCountryByName("Iceland"));
            }

            return borderNations;
        }

        public void ChangeNation(Nation nation)
        {
            ChangeColour(nation.Color);
            this.nation = nation;
        }
    }

    public class TroopInformation
    {
        public Nation ControllerNation;
        public int NumberOfTroops;
        public Country Location;
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(Country))]
    [CanEditMultipleObjects]
    public class CountryEditor : Editor
    {
        // Rendering code for the PixelCollider2D custom inspector
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Country country = (Country)target;
            if (GUILayout.Button("Reset Troops"))
            {
                country.ResetTroops();
            }
        }
    }
    #endif
}