using UnityEngine;
using UnityEngine.EventSystems;

public class BackDropClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            CountrySelector.Instance.ResetSelected();
        }
    }
}
