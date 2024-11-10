using System;
using System.Collections.Generic;

namespace CountryGame
{
    using UnityEngine;

    public class TroopDisplayButton : MonoBehaviour
    {
        private float _currentAlphaMultiplier = 1;
        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }
        
        private void Update()
        {
            if (GameCamera.Instance.IsPointerOverUIObject())
            {
                _currentAlphaMultiplier = 1;
            }
            
            Color targetColor = Color.white * _currentAlphaMultiplier;
            targetColor.a = 1;
            
            sr.color = Color.Lerp(sr.color, targetColor, 0.05f);
        }

        private void OnMouseEnter()
        {
            _currentAlphaMultiplier = 0.65f;
        }

        private void OnMouseOver()
        {
            GameCamera.Instance.troopDisplayHover = true;
        }

        private void OnMouseDown()
        {
            if (GameCamera.Instance.IsPointerOverUIObject())
            {
                return;
            }
            
            _currentAlphaMultiplier = 0.4f;
            
            if (TroopMover.Instance.transferring)
            {
                TroopMover.Instance.SelectedTransferLocation(transform.parent.parent.GetComponent<Country>());
            }
            else
            {
                TroopMover.Instance.Clicked(transform.parent.parent.GetComponent<Country>());
            }
        }

        private void OnMouseUp()
        {
            _currentAlphaMultiplier = 0.65f;
        }

        private void OnMouseExit()
        {
            GameCamera.Instance.troopDisplayHover = false;
            
            _currentAlphaMultiplier = 1f;
        }
    }
}