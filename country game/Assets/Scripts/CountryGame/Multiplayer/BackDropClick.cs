namespace CountryGame.Multiplayer
{
    using UnityEngine;

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
}