namespace CountryGame
{
    using UnityEngine;

    public class BackDropClick : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (!GameCamera.Instance.IsPointerOverUIObject() && !TroopMover.Instance.transferring)
            {
                CountrySelector.Instance.ResetSelected();
                TroopMover.Instance.ResetSelected();
                PlayerNationManager.Instance.ResetSelected();
            }
        }
    }
}