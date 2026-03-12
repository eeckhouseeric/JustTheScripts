using UnityEngine;

public class LeadIndicator : MonoBehaviour
{
    private RectTransform rect;
    private Camera cam;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        cam = Camera.main;
    }

    public void SetWorldPosition(Vector3 worldPos)
    {
        if (cam == null) cam = Camera.main;

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Hide if behind camera
        if (screenPos.z < 0)
        {
            rect.gameObject.SetActive(false);
            return;
        }

        rect.gameObject.SetActive(true);
        rect.position = screenPos;
    }
}
