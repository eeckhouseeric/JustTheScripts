using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public Transform Plane;
    public float distance = 100f;


    private RectTransform crossHairUI;
    private Camera cam;

    void Awake()
    {
        crossHairUI = GetComponent<RectTransform>();
        cam = Camera.main;

    }

    void LateUpdate()
    {
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
