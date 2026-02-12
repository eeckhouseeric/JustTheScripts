using UnityEngine;
using UnityEngine.UI;

public class SwitchInputLayout : MonoBehaviour
{

    [SerializeField] private Image quitButton;
    [SerializeField] private Image switchTeamButton;
    [SerializeField] private Image readyButton;

    [Header("PlayStation")]
    [SerializeField] private Sprite ps5QuitButton;
    [SerializeField] private Sprite ps5SwitchTeamButton;
    [SerializeField] private Sprite ps5ReadyButton;

    [Header("Xbox Series")]
    [SerializeField] private Sprite xboxQuitButton;
    [SerializeField] private Sprite xboxSwitchTeamButton;
    [SerializeField] private Sprite xboxReadyButton;

    [Header("Nintendo Switch")]
    [SerializeField] private Sprite switchQuitButton;
    [SerializeField] private Sprite switchSwitchTeamButton;
    [SerializeField] private Sprite switchReadyButton;

    [Header("Keyboard and Mouse")]
    [SerializeField] private Sprite kbQuitButton;
    [SerializeField] private Sprite kbSwitchTeamButton;
    [SerializeField] private Sprite kbReadyButton;

    [Header("Steam Dock")]
    [SerializeField] private Sprite steamDockQuitButton;
    [SerializeField] private Sprite steamDockSwitchTeamButton;
    [SerializeField] private Sprite steamDockReadyButton;



    private enum InputLayout
    {
        PlayStation,
        XboxSeries,
        NintendoSwitch,
        KeyboardAndMouse,
        SteamDeck
    }

    private InputLayout InputLayoutUi = InputLayout.KeyboardAndMouse;

    private void ApplyLayout(InputLayout layout) 
    
   { 
        switch (InputLayoutUi)
            {
            case InputLayout.PlayStation:
                quitButton.sprite = ps5QuitButton;
                switchTeamButton.sprite = ps5SwitchTeamButton;
                readyButton.sprite = ps5ReadyButton;
                break;
            case InputLayout.XboxSeries:
                quitButton.sprite = xboxQuitButton;
                switchTeamButton.sprite = xboxSwitchTeamButton;
                readyButton.sprite = xboxReadyButton;
                break;
            case InputLayout.NintendoSwitch:
                quitButton.sprite = switchQuitButton;
                switchTeamButton.sprite = switchSwitchTeamButton;
                readyButton.sprite = switchReadyButton;
                break;
            case InputLayout.KeyboardAndMouse:
                quitButton.sprite = kbQuitButton;
                switchTeamButton.sprite = kbSwitchTeamButton;
                readyButton.sprite = kbReadyButton;
                break;
            case InputLayout.SteamDeck:
                quitButton.sprite = steamDockQuitButton;
                switchTeamButton.sprite = steamDockSwitchTeamButton;
                readyButton.sprite = steamDockReadyButton;
                break;
            default:
                Debug.LogWarning("Unknown input layout. Defaulting to Keyboard and Mouse.");
                quitButton.sprite = kbQuitButton;
                switchTeamButton.sprite = kbSwitchTeamButton;
                readyButton.sprite = kbReadyButton;
                break;
        }



    }

}
