namespace CountryGame
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class UILimitMovementObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [HideInInspector] public bool mouseOver;

        private void Awake()
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = 0.8f;
        }

        public void OnPointerEnter(PointerEventData e)
        {
            mouseOver = true;
        }

        public void OnPointerExit(PointerEventData e)
        {
            mouseOver = false;
        }
    }
}