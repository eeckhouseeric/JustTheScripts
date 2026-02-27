using Fusion;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("How far ahead of the plane to aim")]
    public float distance = 100f;

    private Transform plane;
    private RectTransform crossHairUI;
    private Camera cam;

    void Awake()
    {
        crossHairUI = GetComponent<RectTransform>();
        cam = Camera.main;

        Debug.Log($"[Crosshair] Awake: UI = {crossHairUI}, Camera= {cam?.name}");
    }

    public void SetPlane(Transform T)
    { 
        plane = T;

        Debug.Log($"[Crosshair] setPlane() called. Bound to: {plane?.name}");
    }

    private void TryBindCamera()
    {
        if (cam != null)
            return;
     
        cam = Camera.main;
        if (cam != null)
        {
            Debug.Log($"[Crosshair] Camera found and bound: {cam.name}");
        }
 
    }

    void LateUpdate()
    {

        // If we don't have the plane yet, try to find it
        if (plane == null) 
        {
            Debug.Log($"[Crosshair] plane = null. Searching for NetworkObject with InputAuthority");

            foreach (var no in FindObjectsOfType<NetworkObject>())
            {
                Debug.Log($"[Crosshair] Checking object: {no.name}, Authority= {no.HasInputAuthority}");

                if (no.HasInputAuthority)
                {

                    var controls = no.GetComponent<PlaneControls>();
                    Debug.Log($"[Crosshair] plane = null. Searching for NetworkObject with InputAuthority");

                    if (controls == null)
                    { 
                        plane = no.transform;
                        Debug.Log($"[Crosshair] Found plane : {plane.name}");

                        break;
                    }
                }
            
            
            }
        }

        if (plane == null)
        {
            Debug.Log($"[Crosshair] No plane found with InputAuthority + PlaneControls");
            return; 
        }
            

        if (cam == null)
        {
            Debug.Log($"[Crosshair] Camera.main is null. Crosshair cannot project");
            return; 
        }




        // Point in front of the plane
        Vector3 forwardPosition = plane.position + plane.forward * distance;
        Debug.Log($"[Crosshair] Forward point: {forwardPosition}");

        //Convert to screen space
        Vector3 screenPos = cam.WorldToScreenPoint(forwardPosition);
        Debug.Log($"[Crosshair] ScreenPos: {screenPos}");

        // if behind the camera, hide the crosshair
        if (screenPos.z < 0f)
        {
            Debug.Log($"[Crosshair] Target point is behind the camera. Hiding crosshair");
            return;
        }

        //Move UI
        crossHairUI.position = screenPos;
        Debug.Log($"[Crosshair] UI moved to: {screenPos}");

    }

}
