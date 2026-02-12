using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerInfo;

public class LobbyPlayerCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public TextMeshProUGUI playerNameIdText;
    [SerializeField] public TextMeshProUGUI isReadyText;
    [SerializeField] private Image controllerIcon;
    [SerializeField] private TextMeshProUGUI botTag;

    [Header("Controller Sprites")]
    [SerializeField] private Sprite controllerSprite;
    [SerializeField] private Sprite aiControllerSprite;
    [SerializeField] private Sprite waitingForPlayerSprite;
    [SerializeField] private int teamID;
    public int TeamID => teamID;

    public int PlayerID { get; private set; }
    public bool IsFilled { get; private set; } = false;
    private PlayerInfo playerInfo;
    public Fusion.PlayerRef PlayerRef => playerInfo?.playerRef ?? default;
    private bool lastReadyState = false;
    private bool lastIsBot = false;


    private void Awake()
    {
        Validate();
    }



    public void Setup(PlayerInfo info)
    {
        Debug.Log($"[LobbyPlayerCardUI] Setup called for card {name} with PlayerInfo: {info?.PlayerName}, ID={info?.playerId}, IsBot={info?.isBot}, Ready={info?.IsReady}");

        this.playerInfo = info;
        IsFilled = (!string.IsNullOrEmpty(info.PlayerName) || info.isBot);
        PlayerID = info.playerId;

        if (playerNameIdText == null)
        {
            Debug.LogError($"playerNameIdText is not assigned on {name}", this);
            return;
        }


        if (info == null)
        {
            Debug.Log("[LobbyPlayerCardUI] Setup called with null PlayerInfo.");
            return;
        }
      
       
        //playerNameIdText.text = "";

        if (!string.IsNullOrEmpty(info.PlayerName))
        {
            playerNameIdText.text = info.PlayerName;
        }
        else if (info.isBot)
        {

            playerNameIdText.text = $"Bot_{info.playerId}";
            
        }
        else 
        {
            playerNameIdText.text = "Waiting for player";
        }


        //---Sync ready/bot state from PlayerInfo---
        lastReadyState = playerInfo.IsReady;
        lastIsBot = playerInfo.isBot;   


        Debug.Log($"[LobbyPlayerCardUI] Setup called on {name} with PlayerInfo: {info.PlayerName}");
        // Assign name if missing
        if (string.IsNullOrWhiteSpace(playerInfo.PlayerName) && playerInfo.isBot)
        {

            playerInfo.PlayerName = BotGeneratorName.generateName();

            Debug.Log($"[LobbyPlayerCardUI] Assigned name: {playerInfo.PlayerName}");
            Refresh();
        }


        if (!playerInfo.isBot && string.IsNullOrWhiteSpace(playerInfo.PlayerName))
        {
            Debug.Log($"[LobbyPlayerCardUI] Placeholder setup for slot {PlayerID} — awaiting player.");
        }

        // Update UI and bind event
        Refresh();

        // Avoid duplicate event binding
        playerInfo.OnNameChanged -= UpdateName;
        playerInfo.OnNameChanged += UpdateName;

        Debug.Log($"[LobbyPlayerCardUI] Setup complete for Player {PlayerID} ({playerInfo.PlayerName}) on {name}");
    }
    public void Refresh()
    {
        if (playerInfo == null)
        {
            playerNameIdText.text = "Waiting for player";
            Debug.LogWarning("[LobbyPlayerCardUI] Refresh called without valid PlayerInfo.");
            return;
        }

        Debug.Log($"[LobbyPlayerCardUI] Refreshing card {name} for {playerInfo.PlayerName}");

        UpdateName(playerInfo.PlayerName);
        UpdateBotTag();
        UpdateControllerIcon();
        RefreshReadyState();
    }


  
    private void UpdateName(string newName)
    {
        Debug.Log($"[LobbyCard] UpdateName triggered | Name: {newName} | isBot: {playerInfo?.isBot}");

        if (playerNameIdText == null)
        {
            Debug.LogWarning("[LobbyPlayerCardUI] playerNameIdText is not assigned.");
            return;
        }

        if (playerInfo == null|| string.IsNullOrWhiteSpace(newName))
        {
            playerNameIdText.text = "Waiting for player";
            return;
        }

        string displayName = playerInfo != null && playerInfo.isBot
            ? $"Bot_{newName}"
            : newName;

        playerNameIdText.text = FormatName(displayName);

        Debug.Log($"[LobbyPlayerCardUI] Name updated to: {playerNameIdText.text} on {gameObject.name}");
    }


    private string FormatName(string name)
    {
        return string.IsNullOrWhiteSpace(name) ? "[Unnamed]" : name.Trim();
    }


    private void UpdateBotTag()
    {
        if (botTag == null || playerInfo == null)return;

        if (playerInfo.isBot && LobbyManager.instance != null && LobbyManager.instance.IsBotInjectionPhase)
        {
            botTag.gameObject.SetActive(true);
            botTag.text = "Bot";

        }

        else
        {
            botTag.gameObject.SetActive(false);
            
        }

           // bool shouldShowBotTag = playerInfo.isBot && LobbyManager.instance != null && LobbyManager.instance.IsBotInjectionPhase;
       // botTag.SetActive(shouldShowBotTag);
    }

    private void UpdateControllerIcon()
    {
        if (controllerIcon == null || playerInfo == null) 
            return;

        Sprite icon = playerInfo.isBot
            ? aiControllerSprite
            : GetControllerSprite(playerInfo.inputDeviceType);

        controllerIcon.sprite = icon;
        controllerIcon.gameObject.SetActive(icon != null);
    }

    private Sprite GetControllerSprite(InputDeviceType inputType)
    {
        return inputType switch
        {
            InputDeviceType.Gamepad => controllerSprite,
            InputDeviceType.Keyboard => aiControllerSprite,
            _ => waitingForPlayerSprite
        };
    }

    private void OnDestroy()
    {
        if (playerInfo != null)
            playerInfo.OnNameChanged -= UpdateName;
    }
    public void Validate()
    {
        if (playerNameIdText == null)
            Debug.LogError($"Missing playerNameIdText on {name}", this);

        if (string.IsNullOrEmpty(playerNameIdText.text))
            Debug.LogWarning($"playerNameIdText is empty on {name}", this);
    }

    public void setReadyState(bool isReady, bool isBot = false)
    {
        Debug.Log($"[LobbyPlayerCardUI] SetReadyState called isReady: {isReady}, isBot: {isBot}");

        if (isReadyText != null)
        {
            isReadyText.text = isReady ? "Ready" : "Not Ready";
            isReadyText.color = isReady ? Color.green : Color.grey;

        }
        else
        {
            Debug.LogWarning($"[LobbyPlayerCardUI] isReadyText is null on {gameObject.name}.");
        }
    }
    private void RefreshReadyState()
    {
        if (playerInfo != null)
        {
            setReadyState(playerInfo.IsReady, playerInfo.isBot);
        }
     else
        {
            setReadyState(false, false);
        }
    }

}


