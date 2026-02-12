using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PersistentSelection : MonoBehaviour
{
    [SerializeField] private GameObject fallbackButton;

    private void Update()
    {
        var current = EventSystem.current.currentSelectedGameObject;

        if (current == null)
        {
            Debug.LogWarning("No UI element selected — restoring fallback.");
            EventSystem.current.SetSelectedGameObject(fallbackButton);
        }
        else
        {
            var button = current.GetComponent<Button>();
            var image = current.GetComponent<Image>();
            Debug.Log($"Selected: {current.name}, Transition: {button?.transition}, Image Color: {image?.color}");
        }
    }
}
