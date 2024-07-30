using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetProjectiles
{

    public class MouseRotation : MonoBehaviour
    {
        public Camera cam;
        public float maxLenght;

        private Quaternion currentRotation = Quaternion.identity;

        void Update()
        {
            if (cam != null)
            {
                RaycastHit hit;
                Ray rayMouse = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(rayMouse.origin, rayMouse.direction, out hit, maxLenght))
                {
                    RotateToMouseDirection(hit.point);
                }
                else
                {
                    Vector3 pos = rayMouse.GetPoint(maxLenght);
                    RotateToMouseDirection(pos);
                }
            }
            else
            {
                Debug.Log("No camera");
            }
        }

        void RotateToMouseDirection(Vector3 destination)
        {
            Vector3 direction = destination - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 1);
            currentRotation = rotation;
        }

        public Quaternion GetRotation()
        {
            return currentRotation;
        }
    }
}












