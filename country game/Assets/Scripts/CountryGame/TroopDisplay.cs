namespace CountryGame
{
    using UnityEngine;
    using TMPro;

    public class TroopDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshPro numberDisplay;
        [SerializeField] private GameObject display;

        private Country country;

        public void UpdateDisplay(Country country, bool visible)
        {
            this.country = country;
            numberDisplay.text = this.country.troopCount.ToString();
            display.SetActive(visible);

            Vector2 pos = country.GetComponent<PolygonCollider2D>().bounds.center;

            transform.position = pos;
        }
    }
}