using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Movement Settings")]
    public float sensitivity = 300f;
    public float clampRadius = 150f;


    private RectTransform rect;
    private Vector2 screenCenter;
    private Vector2 currentPos;

    private float inputX;
    private float inputY;
    
    void Awake()
    {
       rect = GetComponent<RectTransform>();
       screenCenter =Vector2.zero;
       currentPos = screenCenter;
       rect.position = currentPos;

    }
    public void BindInputSource(PlaneInputFeeder feeder) 
    { 
        if (feeder == null) 
        { 
            Debug.LogError("[Crosshair] BindInputSource FAILED feeder is NULL");
            return; 
        } 
        feeder.OnInputChanged += UpdateInput; 
        Debug.Log("[Crosshair] BindInputSource SUCCESS Subscribed to PlaneInputFeeder");
    }
   
    private void UpdateInput(float x, float y) 
    { 
        inputX = x;
        inputY = y;
        Debug.Log($"[Crosshair] Input updated  X={x}, Y={y}");
    }




    void Update()
    {
        if (rect == null) 
        { Debug.LogError("[Crosshair] ERROR RectTransform is NULL");
            return; 
        }
        Vector2 delta = new Vector2(inputX, inputY) * sensitivity * Time.deltaTime; currentPos += delta;
        currentPos += delta;
        
        currentPos = Vector2.ClampMagnitude(currentPos,clampRadius);
        
        Debug.Log($"[Crosshair] Update Position={rect.position}, Input=({inputX},{inputY})");
    }
}


