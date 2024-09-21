namespace CountryGame
{
    using UnityEngine;

    public class BackDropClick : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (!GameCamera.Instance.IsPointerOverUIObject())
            {
                CountrySelector.Instance.ResetSelected();
                TroopMover.Instance.ResetSelected();
            }
        }
    }
}