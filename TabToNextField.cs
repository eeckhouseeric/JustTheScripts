using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TabToNextField : MonoBehaviour
{
    [SerializeField] private TMP_InputField nextfield;

    private TMP_InputField current;

    private void Awake()
    {
        current = GetComponent<TMP_InputField>();
    }

    private void Update()
    {
        if( current.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
          
            EventSystem.current.SetSelectedGameObject(nextfield.gameObject);
            nextfield.ActivateInputField();

        }
    }




}
