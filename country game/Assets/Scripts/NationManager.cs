using System.Collections.Generic;
using UnityEngine;

public class NationManager : MonoBehaviour
{
    public static NationManager Instance;

    public Nation PlayerNation;
    
    public List<Nation> nations = new List<Nation>();
    public List<Faction> factions = new List<Faction>();

    public bool useFactionColour;

    private void Awake()
    {
        Instance = this;
    }

    public void NewNation(Nation nationToAdd)
    {
        nations.Add(nationToAdd);
    }

    public void NewFaction(Faction factionToAdd)
    {
        factions.Add(factionToAdd);
    }

    public Nation GetNationByName(string nationName)
    {
        foreach (var nation in nations)
        {
            if (nationName == nation.Name)
            {
                return nation;
            }
        }

        return null;
    }

    public Faction GetFactionByName(string factionName)
    {
        foreach (var faction in factions)
        {
            if (factionName == faction.Name)
            {
                return faction;
            }
        }

        return null;
    }

    public void SwapCountriesNation(Country countryToSwap, Nation nationToSwapTo)
    {
        if (countryToSwap.GetNation() != nationToSwapTo)
        {
            Nation oldNation = countryToSwap.GetNation();
            if (oldNation != null)
            {
                oldNation.CountryLeft(countryToSwap);
                if (oldNation.CountryCount == 0)
                {
                    nations.Remove(oldNation);
                }
            }
            
            countryToSwap.ChangeNation(nationToSwapTo);
            nationToSwapTo.CountryJointed(countryToSwap);
        }
    }

    public void NationJoinFaction(Nation nationToSwap, Faction factionToSwap)
    {
        if (!nationToSwap.factions.Contains(factionToSwap) && !factionToSwap.Nations.Contains(nationToSwap))
        {
            nationToSwap.JoinFaction(factionToSwap);
            factionToSwap.CountryJointed(nationToSwap); 
        }
    }
}

public class Nation
{
    public string Name;
    public List<Country> Countries = new List<Country>();
    public List<Faction> factions = new List<Faction>();
    public int CountryCount => Countries.Count;
    public Color Color;

    public void CountryJointed(Country countryThatJoined)
    {
        Countries.Add(countryThatJoined);
    }

    public void CountryLeft(Country countryThatLeft)
    {
        if (Countries.Contains(countryThatLeft))
        {
            Countries.Remove(countryThatLeft);
        }
    }

    public void JoinFaction(Faction factionToJoin)
    {
        factions.Add(factionToJoin);
    }
}

public class Faction
{
    public string Name;
    public Nation FactionLeader;
    public List<Nation> Nations = new List<Nation>();
    public int NationCount => Nations.Count;
    public Color color;
    public bool privateFaction;

    public void CountryJointed(Nation nationThatJoined)
    {
        Nations.Add(nationThatJoined);
    }

    public void NationLeft(Nation nationThatLeft)
    {
        if (Nations.Contains(nationThatLeft))
        {
            Nations.Remove(nationThatLeft);
        }
    }

    public void SetFactionLeader(Nation newFactionLeader)
    {
        FactionLeader = newFactionLeader;

        if (Nations.Contains(newFactionLeader))
        {
            Nations.Remove(newFactionLeader);
            Nations.Insert(0, newFactionLeader);
        }
        else
        {
            Nations.Insert(0, newFactionLeader);
        }
    }
}
