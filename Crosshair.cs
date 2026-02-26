using Fusion;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public float distance = 100f;

    public Transform Plane;

    private RectTransform crossHairUI;
    private Camera cam;

    void Awake()
    {
        crossHairUI = GetComponent<RectTransform>();
        cam = Camera.main;

    }

    void LateUpdate()
    {

        // If we don't have the plane yet, try to find it
        if (Plane == null) 
        { foreach (var no in FindObjectsOfType<NetworkObject>()) 
            { if (no.HasInputAuthority) 
               
                {
                    Plane = no.transform;
                    break;

                }
            } 
        }

        if (Plane == null || cam == null)
            return;

        // Point in front of the plane
        Vector3 forwardPosition = Plane.position + Plane.forward * distance;

        //Convert to screen space
        Vector3 screenPos = cam.WorldToScreenPoint(forwardPosition);

        // if behind the camera, hide the crosshair
        if (screenPos.z < 0f)
        {
            return;
        }

        //Move UI
        crossHairUI.position = screenPos;
    }

}
