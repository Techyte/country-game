using System;
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
        
        Color targetColor = baseColour * _currentAlphaMultiplier;
        targetColor.a = 1;
        
        _renderer.color = Color.Lerp(_renderer.color, targetColor, 0.05f);
    }

    private void OnMouseEnter()
    {
        if (GameCamera.Instance.IsPointerOverUIObject())
        {
            return;
        }
        
        _currentAlphaMultiplier = 0.65f;
    }

    private void OnMouseExit()
    {
        _currentAlphaMultiplier = 1f;
    }

    public void ChangeColor(Color color)
    {
        baseColour = color;
        _renderer.color = color;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1) || GameCamera.Instance.IsPointerOverUIObject())
        {
            return;
        }
        
        CountrySelector.Instance.Clicked(GetComponent<Country>().GetNation());
        
        _currentAlphaMultiplier = 0.4f;
    }

    private void OnMouseUp()
    {
        _currentAlphaMultiplier = 0.9f;
    }
}
