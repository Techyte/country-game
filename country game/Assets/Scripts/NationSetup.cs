using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NationSetup : MonoBehaviour
{
    private Faction un;
    
    private void Start()
    {
        SetupInitialFactions();
        
        List<Country> countries = FindObjectsOfType<Country>().ToList();

        foreach (var country in countries)
        {
            Nation nation = new Nation();
            nation.Color = new Color(Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f, Random.Range(100f, 256f)/256f);
            nation.Name = country.countryName;
            nation.CountryJointed(country);
            
            NationManager.Instance.NewNation(nation);
            
            NationManager.Instance.SwapCountriesNation(country, nation);

            Faction nationFaction = null;

            nationFaction = new Faction();
            nationFaction.color = nation.Color;
            nationFaction.Name = country.countryName;
            nationFaction.privateFaction = true;
            nationFaction.CountryJointed(nation);
            NationManager.Instance.NewFaction(nationFaction);
            
            NationManager.Instance.NationJoinFaction(nation, nationFaction);
            
            foreach (var factionPreset in country.FactionPresets)
            {
                Faction faction = NationManager.Instance.GetFactionByName(factionPreset.FactionName);
                if (factionPreset.FactionLeader)
                {
                    faction.SetFactionLeader(nation);
                }
                NationManager.Instance.NationJoinFaction(nation, faction);
            }

            NationManager.Instance.NationJoinFaction(nation, un);
        }

        foreach (var country in countries)
        {
            if (country.countryName == "Greenland")
            {
                NationManager.Instance.SwapCountriesNation(country, NationManager.Instance.GetNationByName("Denmark"));
            }
        }
    }

    private void SetupInitialFactions()
    {
        un = new Faction();
        un.color = Color.cyan;
        un.Name = "United Nations";
        un.privateFaction = false;
        
        NationManager.Instance.NewFaction(un);
        
        Faction nato = new Faction();
        nato.color = Color.blue;
        nato.Name = "NATO";
        nato.privateFaction = false;
        
        NationManager.Instance.NewFaction(nato);
        
        Faction csto = new Faction();
        csto.color = Color.green;
        csto.Name = "CSTO";
        csto.privateFaction = false;
        
        NationManager.Instance.NewFaction(csto);
        
        Faction aukus = new Faction();
        aukus.color = Color.blue;
        aukus.Name = "AUKUS";
        aukus.privateFaction = false;
        
        NationManager.Instance.NewFaction(aukus);
        
        Faction anzus = new Faction();
        anzus.color = Color.blue;
        anzus.Name = "ANZUS";
        anzus.privateFaction = false;
        
        NationManager.Instance.NewFaction(anzus);
        
        Faction philippinesTreaty = new Faction();
        philippinesTreaty.color = Color.blue;
        philippinesTreaty.Name = "Philippines Treaty";
        philippinesTreaty.privateFaction = false;
        
        NationManager.Instance.NewFaction(philippinesTreaty);
        
        Faction seaTreaty = new Faction();
        seaTreaty.color = Color.blue;
        seaTreaty.Name = "Southeast Asia Treaty";
        seaTreaty.privateFaction = false;
        
        NationManager.Instance.NewFaction(seaTreaty);
        
        Faction japaneseTreaty = new Faction();
        japaneseTreaty.color = Color.black;
        japaneseTreaty.Name = "Japanese Treaty";
        japaneseTreaty.privateFaction = false;
        
        NationManager.Instance.NewFaction(japaneseTreaty);
        
        Faction koreaTreaty = new Faction();
        koreaTreaty.color = Color.black;
        koreaTreaty.Name = "Korea Treaty";
        koreaTreaty.privateFaction = false;
        
        NationManager.Instance.NewFaction(koreaTreaty);
        
        Faction rioTreaty = new Faction();
        rioTreaty.color = Color.green;
        rioTreaty.Name = "Rio Treaty";
        rioTreaty.privateFaction = false;
        
        NationManager.Instance.NewFaction(rioTreaty);
    }
}
