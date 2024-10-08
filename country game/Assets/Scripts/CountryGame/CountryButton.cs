namespace CountryGame
{
    using UnityEngine;

    public class CountryButton : MonoBehaviour
    {
        public Color baseColour = Color.white;

        private float _currentAlphaMultiplier = 1;

        private SpriteRenderer _renderer;

        private Color targetColour;
        private float targetColourInfluence = 0;
        
        public bool doOverrideColour;
        public Color overrideColour;

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

            Color targetColor = baseColour;

            if (!doOverrideColour)
            {
                targetColor = Color.Lerp(baseColour, targetColour, targetColourInfluence) * _currentAlphaMultiplier;
            }
            else
            {
                targetColor = overrideColour * _currentAlphaMultiplier;
            }
            
            targetColor.a = 1;
            
            _renderer.color = Color.Lerp(_renderer.color, targetColor, 0.05f);
        }

        private void OnMouseEnter()
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

        public void ChangeColor(Color color)
        {
            baseColour = color;
            _renderer.color = color;
        }

        private void OnMouseDown()
        {
            if (ViewTypeManager.Instance.currentView == ViewType.Infrastructure)
            {
                PlayerNationManager.Instance.UpgradeInfrastructure(GetComponent<Country>());
                return;
            }
            
            if (Input.GetMouseButton(2) || Input.GetMouseButton(1) || GameCamera.Instance.IsPointerOverUIObject() || 
                GameCamera.Instance.troopDisplayHover || TroopMover.Instance.transferring || TurnManager.Instance.endedTurn)
            {
                return;
            }

            if (CombatManager.Instance.invading)
            {
                CombatManager.Instance.SelectInvasionTarget(GetComponent<Country>());
            }
            else
            {
                CountrySelector.Instance.Clicked(GetComponent<Country>().GetNation());
            }
            
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

        public void SetInfluenceColour(Color color, float influence)
        {
            targetColour = color;
            targetColourInfluence = influence;
        }
    }
}