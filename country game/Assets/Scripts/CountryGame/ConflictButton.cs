using System.Collections.Generic;
using UnityEngine;

namespace CountryGame
{
    public class ConflictButton : MonoBehaviour
    {
        public Color baseColour = Color.white;

        private float _currentAlphaMultiplier = 1;

        private SpriteRenderer _renderer;

        private Color targetColour;
        private float targetColourInfluence = 0;
        
        public List<Attack> Attacks = new List<Attack>();
        public Country countryA;
        public Country countryB;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            targetColour = baseColour;
        }

        private void Start()
        {
            _renderer.color = baseColour;
        }

        private void Update()
        {
            if (GameCamera.Instance.IsPointerOverUIObject())
            {
                _currentAlphaMultiplier = 1;
            }

            targetColour = baseColour * _currentAlphaMultiplier;
            
            targetColour.a = 1;
            
            _renderer.color = Color.Lerp(_renderer.color, targetColour, 0.05f);
        }

        private void OnMouseEnter()
        {
            if (GameCamera.Instance.IsPointerOverUIObject() || GameCamera.Instance.troopDisplayHover || TroopMover.Instance.transferring)
            {
                return;
            }
            
            _currentAlphaMultiplier = 0.65f;
        }

        private void OnMouseOver()
        {
            if (GameCamera.Instance.IsPointerOverUIObject() || GameCamera.Instance.troopDisplayHover || TroopMover.Instance.transferring)
            {
                return;
            }
            
            _currentAlphaMultiplier = 0.65f;
        }

        private void OnMouseExit()
        {
            if (GameCamera.Instance.IsPointerOverUIObject() || GameCamera.Instance.troopDisplayHover || TroopMover.Instance.transferring)
            {
                return;
            }
            
            _currentAlphaMultiplier = 1f;
        }

        private void OnMouseDown()
        {
            if (Input.GetMouseButton(2) || Input.GetMouseButton(1) || GameCamera.Instance.IsPointerOverUIObject() || 
                GameCamera.Instance.troopDisplayHover)
            {
                return;
            }
            
            if (TroopMover.Instance.transferring)
            {
                return;
            }
            
            Debug.Log("open conflict pannel");
            
            _currentAlphaMultiplier = 0.4f;
        }

        private void OnMouseUp()
        {
            if (GameCamera.Instance.IsPointerOverUIObject() || GameCamera.Instance.troopDisplayHover || TroopMover.Instance.transferring)
            {
                return;
            }
            
            _currentAlphaMultiplier = 0.9f;
        }
    }
}