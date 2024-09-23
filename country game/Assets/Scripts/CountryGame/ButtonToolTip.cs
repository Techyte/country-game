using UnityEngine.UI;

namespace CountryGame
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ToolTipButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Transform toolTip;
        [SerializeField] private Button button;

        private void Start()
        {
            toolTip.gameObject.SetActive(false);
        }

        private void Update()
        {
            toolTip.position = Input.mousePosition;
        }

        private void OnDisable()
        {
            toolTip.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable)
            {
                toolTip.gameObject.SetActive(true);
            }
        }
    
        public void OnPointerExit(PointerEventData eventData)
        {
            toolTip.gameObject.SetActive(false);
        }
    }
}