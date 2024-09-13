using UnityEngine;
using UnityEngine.EventSystems;

public class BackDropClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (!GameCamera.Instance.IsPointerOverUIObject())
        {
            CountrySelector.Instance.ResetSelected();
        }
    }
}
