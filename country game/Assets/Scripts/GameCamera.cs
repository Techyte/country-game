using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float scrollSpeed;
    [SerializeField] private float scrollIntensity;
    [SerializeField] private float panSpeed;
    [SerializeField] private float panIntensity;

    [SerializeField] private float minFov, maxFov;

    [SerializeField] private Transform leftViewPoint, rightViewPoint, topViewPoint, bottomViewPoint;
    
    private float targetFov = 0;
    private float currentFov = 0;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        currentFov = cam.fieldOfView;
        targetFov = currentFov;
        maxFov = currentFov;
    }

    private void Update()
    {
        Zoom();
        Pan();
    }

    private void Zoom()
    {
        float scroll = -Input.GetAxisRaw("Mouse ScrollWheel");

        targetFov += scroll * scrollSpeed;

        float oldTarget = targetFov;
        float oldCurrent = currentFov;
        
        targetFov = Mathf.Clamp(targetFov, minFov, maxFov);
        
        currentFov = Mathf.Lerp(currentFov, targetFov, scrollIntensity * Time.deltaTime);

        currentFov = Mathf.Clamp(currentFov, minFov, maxFov);
        
        bool outOfBounds = false;

        if (cam.WorldToScreenPoint(leftViewPoint.position).x >= 0)
        {
            outOfBounds = true;
        }
        if (cam.WorldToScreenPoint(rightViewPoint.position).x <= Screen.width)
        {
            outOfBounds = true;
        }
        if (cam.WorldToScreenPoint(topViewPoint.position).y <= Screen.height)
        {
            outOfBounds = true;
        }
        if (cam.WorldToScreenPoint(bottomViewPoint.position).y >= 0)
        {
            outOfBounds = true;
        }
                
        if (outOfBounds)
        {
            currentFov = oldCurrent;
            targetFov = oldTarget;
        }

        cam.fieldOfView = currentFov;
    }

    private Vector3 prevMousePos = Vector2.zero;
    private void Pan()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cam.nearClipPlane * panSpeed;
            prevMousePos = cam.ScreenToWorldPoint(mousePos);
        }
        
        if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cam.nearClipPlane * panSpeed;
            mousePos = cam.ScreenToWorldPoint(mousePos);
            
            Vector2 distance = mousePos - prevMousePos;
            
            if (distance.magnitude > 0)
            {
                Vector3 oldPos = transform.position;

                transform.position =
                    Vector3.Lerp(transform.position, transform.position - (Vector3)distance, panIntensity * Time.deltaTime);

                bool outOfBounds = false;

                if (cam.WorldToScreenPoint(leftViewPoint.position).x >= 0)
                {
                    outOfBounds = true;
                }
                if (cam.WorldToScreenPoint(rightViewPoint.position).x <= Screen.width)
                {
                    outOfBounds = true;
                }
                if (cam.WorldToScreenPoint(topViewPoint.position).y <= Screen.height)
                {
                    outOfBounds = true;
                }
                if (cam.WorldToScreenPoint(bottomViewPoint.position).y >= 0)
                {
                    outOfBounds = true;
                }
                
                if (outOfBounds)
                {
                    prevMousePos = mousePos;
                    transform.position = oldPos;
                }
            }
        }
    }
}
