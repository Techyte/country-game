namespace CountryGame.Util
{
    using TMPro;
    using UnityEngine;

    public class TextAutoScalerTool : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private float intialSize;
    
        private void Awake()
        {
            intialSize = text.rectTransform.rect.height;
        }

        private void Update()
        {
            if (text.enableAutoSizing)
            {
                int linesAdded = Mathf.FloorToInt(text.rectTransform.rect.height / intialSize) - 1;
            
                if (text.fontSize < text.fontSizeMax)
                {
                    float change = text.fontSize / text.fontSizeMax;

                    float linesToAdd = 1 / change;

                    if (linesToAdd > 1)
                    {
                        text.rectTransform.sizeDelta = new Vector2(text.rectTransform.rect.width,
                            intialSize + intialSize * (Mathf.FloorToInt(linesToAdd) + linesAdded));
                    }
                }
            }
        }
    }
}
