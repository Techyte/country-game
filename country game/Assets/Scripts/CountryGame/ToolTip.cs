using System;
using UnityEngine.UI;

namespace CountryGame
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Transform toolTip;

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
            toolTip.gameObject.SetActive(true);
        }
    
        public void OnPointerExit(PointerEventData eventData)
        {
            toolTip.gameObject.SetActive(false);
        }
    }
}