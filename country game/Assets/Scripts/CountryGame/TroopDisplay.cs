using System;

namespace CountryGame
{
    using UnityEngine;
    using TMPro;

    public class TroopDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshPro numberDisplay;
        [SerializeField] private GameObject display;

        private Country country;

        private void Awake()
        {
            display.SetActive(false);
        }

        public void UpdateDisplay(Country newCountry, bool visible)
        {
            country = newCountry;
            numberDisplay.text = country.TotalTroopCount().ToString();
            display.SetActive(visible);

            Vector3 pos = country.GetComponent<PolygonCollider2D>().bounds.center;
            pos.z = -0.001f;

            transform.position = pos;
        }

        public void UpdateDisplay(Country newCountry)
        {
            country = newCountry;
            numberDisplay.text = country.TotalTroopCount().ToString();

            Vector3 pos = country.GetComponent<PolygonCollider2D>().bounds.center;
            pos.z = -0.001f;

            transform.position = pos;
        }
    }
}