namespace CountryGame.Util
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageTransparencyHandler : MonoBehaviour
    {
        private void Awake()
        {
            foreach (var image in FindObjectsOfType<Image>())
            {
                if (image.color.a > 0 && image.sprite != null)
                {
                    if (image.sprite.texture.isReadable)
                    {
                        image.alphaHitTestMinimumThreshold = 1f;
                    }
                }
            }
        }
    }
}