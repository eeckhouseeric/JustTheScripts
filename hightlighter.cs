using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIHighlightDebugger : MonoBehaviour
{
    private void Update()
    {
        var selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            Debug.LogWarning("No UI element is currently selected.");
            return;
        }

        Debug.Log($"Selected GameObject: {selected.name}");

        var button = selected.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"Selected object '{selected.name}' is not a Button.");
            return;
        }

        Debug.Log($"Button Transition: {button.transition}");

        if (button.transition == Selectable.Transition.ColorTint)
        {
            Debug.Log($" Normal Color: {button.colors.normalColor}");
            Debug.Log($"Highlighted Color: {button.colors.highlightedColor}");
            Debug.Log($"Pressed Color: {button.colors.pressedColor}");
            Debug.Log($"Disabled Color: {button.colors.disabledColor}");
        }

        if (button.targetGraphic == null)
        {
            Debug.LogWarning("Button has no Target Graphic assigned.");
        }
        else
        {
            Debug.Log($"Target Graphic: {button.targetGraphic.name} (Type: {button.targetGraphic.GetType().Name})");
        }

        var image = selected.GetComponent<Image>();
        if (image != null)
        {
            Debug.Log($"Image component found. Current color: {image.color}");
        }
        else
        {
            Debug.LogWarning("No Image component found on selected button.");
        }
    }
}