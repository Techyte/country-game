using UnityEngine.EventSystems;

namespace CountryGame
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class GameCamera : MonoBehaviour
    {
        public static GameCamera Instance;
        
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
            Instance = this;
            
            cam = GetComponent<Camera>();
            currentFov = cam.fieldOfView;
            targetFov = currentFov;
        }

        private bool hoveringThisFrame = false;

        public bool troopDisplayHover;

        private void Update()
        {
            hoveringThisFrame = PointerOverUIObject();
            
            Vector3 oldPos = transform.position;

            float oldTarget = targetFov;
            float oldFOV = currentFov;
            
            Zoom();

            bool outOfBounds = false;
            
            if (cam.WorldToScreenPoint(leftViewPoint.position).x >= 0)
            {
                outOfBounds = true;
            }
            else if (cam.WorldToScreenPoint(rightViewPoint.position).x <= Screen.width)
            {
                outOfBounds = true;
            }
            else if (cam.WorldToScreenPoint(topViewPoint.position).y <= Screen.height)
            {
                outOfBounds = true;
            }
            else if (cam.WorldToScreenPoint(bottomViewPoint.position).y >= 0)
            {
                outOfBounds = true;
            }

            if (outOfBounds)
            {
                targetFov = oldTarget;
                currentFov = oldFOV;
            }
            
            Pan();
            
            if (cam.WorldToScreenPoint(leftViewPoint.position).x >= 0)
            {
                transform.position = new Vector3(oldPos.x, transform.position.y, transform.position.z);
            }
            if (cam.WorldToScreenPoint(rightViewPoint.position).x <= Screen.width)
            {
                transform.position = new Vector3(oldPos.x, transform.position.y, transform.position.z);
            }
            if (cam.WorldToScreenPoint(topViewPoint.position).y <= Screen.height)
            {
                transform.position = new Vector3(transform.position.x, oldPos.y, transform.position.z);
            }
            if (cam.WorldToScreenPoint(bottomViewPoint.position).y >= 0)
            {
                transform.position = new Vector3(transform.position.x, oldPos.y, transform.position.z);
            }
        }

        private void Zoom()
        {
            if (!hoveringThisFrame)
            {
                float scroll = -Input.GetAxisRaw("Mouse ScrollWheel");

                targetFov += scroll * scrollSpeed;
            
                targetFov = Mathf.Clamp(targetFov, minFov, maxFov);
            }
            currentFov = Mathf.Lerp(currentFov, targetFov, scrollIntensity * Time.deltaTime);

            currentFov = Mathf.Clamp(currentFov, minFov, maxFov);

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
                Vector2 distance = Vector2.zero;
                if (!hoveringThisFrame)
                {
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = cam.nearClipPlane * panSpeed;
                    mousePos = cam.ScreenToWorldPoint(mousePos);
                
                    distance = mousePos - prevMousePos;
                }
                
                if (distance.magnitude > 0)
                {
                    transform.position =
                        Vector3.Lerp(transform.position, transform.position - (Vector3)distance, panIntensity * Time.deltaTime);
                }
            }
        }
        
        private bool PointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        public bool IsPointerOverUIObject()
        {
            return hoveringThisFrame;
        }
    }
}