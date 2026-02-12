using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpawnOnPlayerJoin : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefab and Managers")]
    public GameObject planePrefab;
    public GameObject lobbyPlayerPref;
    public SpawnManager spawnManager; // Assign via Inspector

    void Start()
    {
        var runner = FindAnyObjectByType<NetworkRunner>();
        if (runner != null)
        {
            runner.AddCallbacks(this);
            Debug.Log("SpawnOnPlayerJoin registered with runner.");
        }
        else
        {
            Debug.LogWarning("NetworkRunner not found — spawn logic not active.");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.LocalPlayer == player)
        {
            int teamID = GameRunner.Instance.GetTeamID(player);
            int playerIndex = GameRunner.Instance.GetPlayerIndex(player);

            TeamSpawnPoint.Team teamEnum = (TeamSpawnPoint.Team)teamID;
            Transform spawnPoint = spawnManager.GetSpawnPoint(teamEnum, playerIndex);
            if (spawnPoint != null)
            {
                runner.Spawn(planePrefab, spawnPoint.position, spawnPoint.rotation, player);
                Debug.Log($"Spawned plane for Player {player} at Team {teamID}, Index {playerIndex}.");
            }
            else
            {
                Debug.LogError($"Spawn point not found for Team {teamID}, Index {playerIndex}.");
            }
        }
    }

    public void OnPlayerJoin(NetworkRunner runner, PlayerRef player)
    {
        if (runner.LocalPlayer == player)    
        {
            var lobby = runner.Spawn(lobbyPlayerPref, Vector3.zero, Quaternion.identity, player);
            var name = BotGeneratorName.generateName(); // Or pull the from UI
            var lobbyPlayer = GetComponent<LobbyPlayer>();
            lobbyPlayer.PlayerName = name;
        }
    }



    // Required empty callbacks (Fusion lifecycle hooks)
    public void OnInput(NetworkRunner r, NetworkInput i) { }
    public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput i) { }
    public void OnPlayerLeft(NetworkRunner r, PlayerRef p) { }
    public void OnShutdown(NetworkRunner r, ShutdownReason rsn) { }
    public void OnConnectedToServer(NetworkRunner r) { }
    public void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner r, NetAddress addr, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) { }
    public void OnSceneLoadStart(NetworkRunner r) { }
    public void OnSceneLoadDone(NetworkRunner r) { }
    public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> list) { }
    public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner r, HostMigrationToken token) { }
    public void OnReliableDataReceived(NetworkRunner r, PlayerRef p, ReliableKey k, ArraySegment<byte> d) { }
    public void OnReliableDataProgress(NetworkRunner r, PlayerRef p, ReliableKey k, float progress) { }
    public void OnObjectEnterAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
    public void OnObjectExitAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
    public void OnObjectSpawned(NetworkRunner r, NetworkObject obj) { }
    public void OnObjectDespawned(NetworkRunner r, NetworkObject obj) { }
}
