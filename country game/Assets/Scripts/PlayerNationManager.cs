using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNationManager : MonoBehaviour
{
    public static PlayerNationManager Instance;

    public Nation PlayerNation { get; private set; }
    [SerializeField] private Image flagImage;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        
    }

    public void SetPlayerNation(Nation playerNarion)
    {
        PlayerNation = playerNarion;
    }
}
