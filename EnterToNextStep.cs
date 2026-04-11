using UnityEngine;
using TMPro;

public class EnterToNextStep : MonoBehaviour
{

    [SerializeField] private MonoBehaviour stepScript;
    [SerializeField] private string methodName = "OnNextButtonPressed";

    private TMP_InputField field;


    void Awake()
    {
        field =  GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if (field.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            stepScript.Invoke(methodName, 0f);
            
        }
    }
}
