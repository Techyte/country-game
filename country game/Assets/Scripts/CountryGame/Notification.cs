using System;
using System.Collections;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CountryGame
{
    using UnityEngine;

    public class Notification : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI titleDisplay;
        [SerializeField] private TextMeshProUGUI contentDisplay;
        [SerializeField] private Button button;

        private float timer;

        private void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            foreach (var image in GetComponentsInChildren<Image>())
            {
                if (image.color.a > 0 && image.sprite != null)
                {
                    if (image.sprite.texture.isReadable)
                    {
                        image.alphaHitTestMinimumThreshold = 1f;
                    }
                }
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Destroy(gameObject);
            }
        }

        public void Init(string title, string content, UnityAction action, float lifetime)
        {
            titleDisplay.text = title;
            contentDisplay.text = content;
            button.onClick.AddListener(action);
            timer = lifetime;
        }
    }
}