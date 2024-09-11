using UnityEngine;

public class BackDropClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        CountrySelector.Instance.ResetSelected();
    }
}
