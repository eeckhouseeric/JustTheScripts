using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionCallbackHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private bool spawnBotsInGame = false;

    private readonly List<PlayerRef> pendingPlayers = new List<PlayerRef>();
    private CinemachineCamera cineCam;
    private Crosshair crosshair;


    public static FusionCallbackHandler instance;
    public static int lastSceneIndex = -1;

    private void Awake()
    {
        instance = this;
    }

    public static void SetSceneIndex(int index)
    {
        Debug.Log($"[FusionCallbackHandler] Scene index set to {index}");
        lastSceneIndex = index;
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log($"[FusionCallbackHandler] Scene load started. Index={lastSceneIndex}");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        int loadedIndex = SceneManager.GetActiveScene().buildIndex;

        Debug.Log($"[FusionCallbackHandler] Scene load done. Index={loadedIndex}");
       
        if (loadedIndex >= 3)
        {
            //Find gameplay camera and crossshair 
            cineCam = FindObjectOfType<CinemachineCamera>(true);
            crosshair = FindObjectOfType<Crosshair>(true);
            Debug.Log($"[FusionCallbackHandler] Found CineCam= {cineCam?.name}, crosshair={crosshair?.name}");


            Debug.Log("GreyBox loaded, spawning players…");
            foreach (var player in pendingPlayers)
            {
                SpawnPlayerForScene(runner, player);
            }
            pendingPlayers.Clear();
        }
        else
        {
            Debug.Log($"Unknown scene index: {lastSceneIndex}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[FusionCallbackHandler] OnPlayerJoined fired for {player}");
        pendingPlayers.Add(player);
        StartCoroutine(WaitForLobbyThenInject(player));
        Debug.Log("[FusionCallbackHandler] Player joined – waiting for scene load callback to decide spawn.");
    }

    private IEnumerator WaitForLobbyThenInject(PlayerRef player)
    {
        while (LobbyManager.instance == null)
            yield return null;

        // Wait for PlayFab username to be ready
        while (string.IsNullOrEmpty(PlayerSession.Username))
            yield return null;


        // ask lobby manager to assign the correct team
        int assignedTeam = LobbyManager.instance.AssignTeamForPlayer();

        var info = new PlayerInfo(
            name: PlayerSession.Username,
            id: PlayerSession.PlayFabID.GetHashCode(),
            teamID: 0,
            isBot: false,
            isReady: true,
            playerRef: player,
            playFabId: PlayerSession.PlayFabID,
            sessionTicket: PlayerSession.SessionTicket
        );

        Debug.Log($"[DEBUG] WaitForLobbyThenInject: Player={info.PlayerName}, Team={assignedTeam}, PlayerRef={player}");

        LobbyManager.instance.AddPlayerCard(info);
        Debug.Log($"[FusionCallbackHandler] Injected PlayerInfo for {player} into lobby");
    }

    private void SpawnPlayerForScene(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[FusionCallbackHandler] SpawnPlayerForScene entered for {player}");

        if (playerPrefab == null)
        {
            Debug.LogError("[FusionCallbackHandler] PlayerPrefab is not assigned.");
            return;
        }


       var info = LobbyManager.instance?.lobbyPlayers.FirstOrDefault(p => p.playerRef == player);
       int teamId = info?.teamID ?? 0;
        
        // use gameSpawner Manager instead of Lobby Manager
        Transform spawnT = GameSpawnerManger.instance?.GetSpawnPointForTeam(teamId);
        if (spawnT == null)
        {
            Debug.LogWarning($"[FusionCallbackHandler] No Spawn points found for team {teamId}, defaulting to Vector 3. zero");

        }
       

        Vector3 spawnPos = spawnT != null ? spawnT.position :Vector3.zero;
        Quaternion spawnRot = spawnT != null ? spawnT.rotation: Quaternion.identity;

        Debug.Log($"[FusionCallbackHandler] Spawning player {player} at {spawnPos}(team {teamId})");

        var obj = runner.Spawn(playerPrefab, spawnPos, spawnRot, player);

        if (obj == null)
        {
            Debug.LogError("[FusionCallbackHandler] runner.Spawn returned NULL");
            return;
        }

        runner.SetPlayerObject(player,obj);
       Debug.Log($"[FusionCallbackHandler] After SetPlayerObject: LocalPlayerObject={runner.GetPlayerObject(runner.LocalPlayer)}");


        //Assign spawn point to PlaneSpawner
        var spawner = obj.GetComponent<PlaneSpawner>();
        if(spawner != null)
        {
            spawner.spawnPoint = spawnT;
            Debug.Log($"[FusionCallbackHandler] PlaneSpawner.spawnPoint = {spawnT.position}");
        }

        // assign plane to CrosshairController and camaera for the local player
        if (player == runner.LocalPlayer)
        { 
            if (crosshair != null)
            {
                crosshair.Plane = obj.transform;
                Debug.Log($"[FusionCallbackHandler] Crosshair bound to {obj.transform.name}");
            }
            if (cineCam != null)
            {
                cineCam.Follow = obj.transform;
                Debug.Log($"[FusionCallbackHandler] Cinemachine Follow set to {obj.transform.name}");
            }
            else
            {
                Debug.LogWarning("[FusionCallbackHandler] CrosshairController and Cinemachine camera not found in scene.");
            }



            
        }

    }

    // Other INetworkRunnerCallbacks unchanged...
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => Debug.Log($"[FusionCallbackHandler] Player left: {player}");
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) => Debug.LogWarning($"[FusionCallbackHandler] Input missing for player {player}");
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) => Debug.LogError($"[FusionCallbackHandler] Shutdown: {shutdownReason}");
    public void OnConnectedToServer(NetworkRunner runner) => Debug.Log("[FusionCallbackHandler] Connected to server");
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) => Debug.Log($"[FusionCallbackHandler] Disconnected from server: {reason}");
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) => Debug.Log("[FusionCallbackHandler] Connect request received");
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) => Debug.LogWarning($"[FusionCallbackHandler] Connect failed from {remoteAddress}: {reason}");
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) => Debug.Log("[FusionCallbackHandler] Simulation message received");
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) => Debug.Log($"[FusionCallbackHandler] Session list updated: {sessionList.Count} sessions found");
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) => Debug.Log("[FusionCallbackHandler] Custom authentication response received");
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) => Debug.Log("[FusionCallbackHandler] Host migration triggered");
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) => Debug.Log($"[FusionCallbackHandler] Reliable data progress from {player}: {progress * 100f}%");
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) => Debug.Log($"[FusionCallbackHandler] Reliable data received from {player}, key: {key}");
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) => Debug.Log($"[FusionCallbackHandler] Object {obj.name} entered AOI for player {player}");
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) => Debug.Log($"[FusionCallbackHandler] Object {obj.name} exited AOI for player {player}");
}
