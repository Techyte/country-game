using UnityEngine;

public class SpriteScale : MonoBehaviour 
{

    [SerializeField] private SpriteRenderer sr;

    [SerializeField] private float depth;

    void Update()
    {
        // Angle the camera can see above the center.
        float halfFovRadians = Camera.main.fieldOfView * Mathf.Deg2Rad / 2f;

// How high is it from top to bottom of the view frustum,
// in world space units, at our target depth?
        float visibleHeightAtDepth = depth * Mathf.Tan(halfFovRadians) * 2f;

// You could also use Sprite.bounds for this.
        float spriteHeight = sr.sprite.rect.height 
                             / sr.sprite.pixelsPerUnit;

// How many times bigger (or smaller) is the height we want to fill?
        float scaleFactor = visibleHeightAtDepth / spriteHeight;

// Scale to fit, uniformly on all axes.
        Vector3 target = Vector2.ClampMagnitude(Vector3.one * scaleFactor, 2);
        target.z = 1;
        transform.localScale = target;
    }

}