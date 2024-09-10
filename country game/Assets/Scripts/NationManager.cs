using System;
using System.Collections.Generic;
using UnityEngine;

public class NationManager : MonoBehaviour
{
    public static NationManager Instance;
    
    public List<Nation> nations;

    private void Awake()
    {
        Instance = this;
    }

    public void NewNation(Nation nationToAdd)
    {
        nations.Add(nationToAdd);
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
}

[Serializable]
public class Nation
{
    public string Name;
    public List<Country> Countries = new List<Country>();
    public int CountryCount => Countries.Count;
    public Color Color;
}
