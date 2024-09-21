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

        public void UpdateDisplay(Country newCountry, bool visible)
        {
            country = newCountry;
            numberDisplay.text = country.troopCount.ToString();
            display.SetActive(visible);

            Vector3 pos = country.GetComponent<PolygonCollider2D>().bounds.center;
            pos.z = -1;

            transform.position = pos;
        }
    }
}