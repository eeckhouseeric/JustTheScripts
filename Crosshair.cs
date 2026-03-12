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
       screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
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
        Vector2 offset = currentPos - screenCenter; offset = Vector2.ClampMagnitude(offset, clampRadius); 
        currentPos = screenCenter + offset; rect.position = currentPos;
        Debug.Log($"[Crosshair] Update Position={rect.position}, Input=({inputX},{inputY})");
    }
}


