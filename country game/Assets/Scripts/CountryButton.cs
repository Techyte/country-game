using UnityEngine;
using UnityEngine.EventSystems;

public class CountryButton : MonoBehaviour
{
    [SerializeField] private Color baseColour = Color.white;

    private float _currentAlphaMultiplier = 1;

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = baseColour;
    }

    private void Update()
    {
        _renderer.color = Color.Lerp(_renderer.color, baseColour * _currentAlphaMultiplier, 0.01f);
    }

    private void OnMouseEnter()
    {
        _currentAlphaMultiplier = 0.9f;
    }

    private void OnMouseExit()
    {
        _currentAlphaMultiplier = 1f;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
        {
            return;
        }
        
        CountrySelector.Instance.Clicked(gameObject.name);
        
        _currentAlphaMultiplier = 0.5f;
    }

    private void OnMouseUp()
    {
        _currentAlphaMultiplier = 0.9f;
    }
}
