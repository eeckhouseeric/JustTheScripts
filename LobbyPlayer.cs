using Fusion;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [Networked] public bool IsReady { get; set; }
    [Networked] public bool IsBot {  get; set; }  
    [Networked] public string PlayerName { get; set; }
    [Networked] public bool IsInitialized { get; set; }


    private bool _lastIsReady;
    private string _lastPlayerName;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        { 
           
            IsReady = false;
            IsBot = false;
            PlayerName = PlayerPrefs.GetString("Playername", "Player");

          IsInitialized = true;// Mark as initialized
          // Register this player object so GetPlayerObject works
        }
        // Initialize cache to avoid false positives on first frame
        _lastIsReady = IsReady;
        _lastPlayerName = PlayerName;
        UpdateLobbyCard(); // Initial sync
    }

    public override void FixedUpdateNetwork()
    {
        if (_lastIsReady != IsReady || _lastPlayerName != PlayerName)
        {
            UpdateLobbyCard();
            _lastIsReady = IsReady;
            _lastPlayerName = PlayerName;
        }
    }

    private void UpdateLobbyCard()// Initial sync 
    {
        if (LobbyManager.instance == null)
        {
            Debug.LogWarning("[LobbyPlayer] LobbyManager.instance is null.");
        }

        LobbyManager.instance.UpdateCardForPlayer(Object.InputAuthority,PlayerName,IsReady);
    }


}
