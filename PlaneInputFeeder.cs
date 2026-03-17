using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneInputFeeder : MonoBehaviour, INetworkRunnerCallbacks
{
    private PlaneControls controls;

    // crossHair subscribes to this event to know when to show/hide based on input activity
    public event Action<float, float> OnInputChanged;

    private void Update()
    {
        // Disable input feeder until gameplay scene (index >= 3)
        if (FusionCallbackHandler.lastSceneIndex >= 3)
        {
            // Gameplay scene  ensure feeder is ON
            if (!enabled)
                enabled = true;
        }
        else
        {
            // Menu or Lobby  ensure feeder is OFF
            if (enabled)
                enabled = false;
        }
    }


    // Setup Input System
    private void Awake()
    {
       // Debug.Log(" PlaneInputFeeder Awake");
        controls = new PlaneControls();
    }

    private void OnEnable()
    {
        //Debug.Log(" PlaneInputFeeder OnEnable");
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
       // Debug.Log(" PlaneInputFeeder OnDisable");
    }

    // Delayed registration to ensure NetworkRunner is initialized
    private void Start()
    {
        StartCoroutine(RegisterWithRunnerWhenReady());
    }

    private IEnumerator RegisterWithRunnerWhenReady()
    {
        NetworkRunner runner = null;
        while (runner == null)
        {
            runner = UnityEngine.Object.FindFirstObjectByType<NetworkRunner>();
            yield return null;
        }

        runner.AddCallbacks(this);
        //Debug.Log("PlaneInputFeeder successfully registered with NetworkRunner");
    }


    // this is re-registers the feeder after gameplay scene loads
    public void OnSceneLoadDone(NetworkRunner runner)
    {

        if (FusionCallbackHandler.lastSceneIndex >= 3)
        { 
        
            controls.Enable();
            enabled = true;

            runner.AddCallbacks(this);
                Debug.Log("[Input Feeder] re-registered after gameplay scene load.");
        }
    
    }


    //  Feed input to Fusion
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (FusionCallbackHandler.lastSceneIndex < 3)
            // Preven Fusion from porcessing input outside gameplay scene
            return;

        Debug.Log($"[Input Feeder] OnInput Called - sceneIndex= {FusionCallbackHandler.lastSceneIndex}");

        var flight = controls.Flight;

        var data = new PlaneInputData
        {
            Thrust                      = flight.Throttle.ReadValue<float>(),
            Pitch                       = flight.Pitch.ReadValue<float>(),
            Yaw                         = flight.Yaw.ReadValue<float>(),
            Roll                        = flight.Roll.ReadValue<float>(),
            Fire                        = flight.Fire1.IsPressed(),
            SecondaryFire               = flight.Fire2.IsPressed()
        };
            OnInputChanged?.Invoke(data.Pitch, data.Yaw);

        input.Set(data);
    }


    //  Required callbacks (currently unused)
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnObjectSpawned(NetworkRunner runner, NetworkObject obj) { }
    public void OnObjectDespawned(NetworkRunner runner, NetworkObject obj) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
