namespace CountryGame
{
    using UnityEngine;

    public class TroopDisplayButton : MonoBehaviour
    {
        private float _currentAlphaMultiplier = 1;
        private SpriteRenderer renderer;

        private void Awake()
        {
            renderer = GetComponent<SpriteRenderer>();
        }
        
        private void Update()
        {
            if (GameCamera.Instance.IsPointerOverUIObject())
            {
                _currentAlphaMultiplier = 1;
            }
            
            Color targetColor = Color.white * _currentAlphaMultiplier;
            targetColor.a = 1;
            
            renderer.color = Color.Lerp(renderer.color, targetColor, 0.05f);
        }

        private void OnMouseEnter()
        {
            if (GameCamera.Instance.IsPointerOverUIObject())
            {
                return;
            }

            _currentAlphaMultiplier = 0.65f;
        }

        private void OnMouseOver()
        {
            GameCamera.Instance.troopDisplayHover = true;
        }

        private void OnMouseDown()
        {
            _currentAlphaMultiplier = 0.4f;
            TroopMover.Instance.Clicked(transform.parent.parent.GetComponent<Country>());
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