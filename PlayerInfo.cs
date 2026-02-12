using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInfo
{
    public Fusion.PlayerRef playerRef { get; set; }

    public string PlayFabID { get; set; }// unique backend ID
    public string SessionTicket { get; set; } // auth token for multiplayer
    public bool HasAuthority { get; set; }
    private PlayerInfo playerInfo;

    // Reactive name property
    private string _playerName;
    public string PlayerName
    
    {
        get => _playerName;
        set
        {
            var newName = value ?? "";


            if (_playerName != value)
            {
                _playerName = value;
               Debug.Log($"[PlayerInfo] Name changed to: {_playerName}");
                OnNameChanged?.Invoke(_playerName);
            }
        
            
        }
    }
    private bool _isReady;
    public bool IsReady
    {
        get => _isReady;
        set
        {
            if (_isReady != value)
            {
                _isReady = value;
                OnReadyChanged?.Invoke(_isReady);
            }
        }
    }

    public event Action<bool> OnReadyChanged;
    // Event for UI binding
    public event Action<string> OnNameChanged;

    //  Core player data
    public int playerId;
    public bool isBot;
    public int teamID;
    public Team team;
    public Color teamColor;
    public InputDeviceType inputDeviceType;




    // Optional: Constructor for quick setup
    public PlayerInfo(string name, int id, int teamID, bool isBot = false, bool isReady = false,
                      Fusion.PlayerRef playerRef = default, string playFabId = null, string sessionTicket = null)
    {
        this.PlayerName = name ?? "";
        {
            this.PlayerName = name ?? "";
            this.playerId = id;
            this.teamID = teamID;
            this.team = teamID == 0 ? Team.Red : Team.Blue;
            this.isBot = isBot;
            this.IsReady = isReady;
            this.playerRef = playerRef;
            PlayFabID = PlayFabID;
            SessionTicket = SessionTicket;
            inputDeviceType = isBot ? InputDeviceType.AI : InputDeviceType.None;

            Debug.Log($"[PlayerInfo] Created: Name={PlayerName}, ID={playerId}, Team={teamID}, IsBot={isBot}, isReady{isReady},InputType={inputDeviceType}");
        }   
    }

    //  Input types
    public enum InputDeviceType
    {
        Keyboard,
        Gamepad,
        Touch,
        AI,
        None
    }

    // Team types
    public enum Team
    {
        Red,
        Blue
    }

    public override string ToString()
    {
        return $"PlayerInfo(Name={PlayerName}, PlayFabID ={PlayFabID} ,ID={playerId}, Team={teamID}, IsBot={isBot}, IsReady={IsReady})";
    }

}
