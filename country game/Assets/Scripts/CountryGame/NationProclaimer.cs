using System;

namespace CountryGame
{
    using UnityEngine;

    public class NationProclaimer : MonoBehaviour
    {
        public static NationProclaimer Instance;
        
        [SerializeField] private GameObject proclaimScreen;

        private void Awake()
        {
            Instance = this;
        }

        public void OpenProclaimScreen()
        {
            
        }

        public void CloseProclaimScreen()
        {
            proclaimScreen.SetActive(false);
        }
    }
}