using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class RunnerValidator2_06 : MonoBehaviour
{
    [Header("Expected Prefabs (NetworkPrefabRef)")]
    public List<NetworkPrefabRef> ExpectedPrefabs = new();

    [Header("Expected Scene Anchors (optional)")]
    public List<NetworkObject> ExpectedSceneAnchors = new();

    [Header("Logging")]
    public bool Verbose = true;
    private const string TAG = "[Validator] ";

    private NetworkRunner _runner;

    private void Awake()
    {
        _runner = GetComponent<NetworkRunner>();
        if (!_runner) { Warn("NetworkRunner component is MISSING on this GameObject."); return; }

        CheckSceneManager();
        CheckDuplicateCallbacksOnRunner();
        CheckProvideInputWiring();
        CheckSceneAnchors();
    }

    private void Start()
    {
        if (!Application.isPlaying || _runner == null) return;

        CheckRunnerState();
        LogActiveScene();
        LogPrefabFingerprint(_runner);
        CheckPlayersAndReadiness();
        CheckExpectedPrefabsRuntime();
        CheckModeCompatibility();
        CheckDuplicateRunnersInScene();
    }

    // ---- Checks ----

    private void CheckSceneManager()
    {
        var nsm = _runner.GetComponent<NetworkSceneManagerDefault>();
        if (!nsm) Error("NetworkSceneManagerDefault is MISSING. Add it to the same GameObject as the runner.");
        else Info("NetworkSceneManagerDefault present.");
    }

    private void CheckProjectConfig()
    {
        if (_runner.Config == null) Error("Runner.Config is NULL. Assign a NetworkProjectConfig to NetworkRunner.");
        else Info("NetworkProjectConfig assigned.");
    }

    private void CheckPrefabTablePresence()
    {
        if (_runner.Config == null) return;
        var table = _runner.Config.PrefabTable;
        if (table == null) { Error("PrefabTable is NULL on NetworkProjectConfig."); return; }
        Info("PrefabTable present.");
    }

    private void CheckDuplicateCallbacksOnRunner()
    {
        var knownTypes = new[] { typeof(FusionCallbackHandler), typeof(RunnerValidator2_06) };
        var callbacks = GetComponents<MonoBehaviour>().Where(mb => mb is INetworkRunnerCallbacks).ToList();

        bool allKnown = callbacks.All(mb => knownTypes.Contains(mb.GetType()));
        if (callbacks.Count > 1 && !allKnown)
            Warn($"Multiple INetworkRunnerCallbacks on the Runner GameObject: {callbacks.Count}. Can cause competing event handling.");
        else
            Info("INetworkRunnerCallbacks count on Runner GameObject is OK.");
    }

    private void CheckProvideInputWiring()
    {
        Info($"ProvideInput: {_runner.ProvideInput}");
        if (!_runner.ProvideInput) return;

        int inputProviders = 0;
        foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
        {
            var method = mb.GetType().GetMethod("OnInput", new[] { typeof(NetworkRunner), typeof(NetworkInput) });
            if (method != null && method.DeclaringType != typeof(RunnerValidator2_06))
                inputProviders++;
        }

        if (inputProviders == 0)
            Error("ProvideInput is TRUE but no valid input provider implementing OnInput(NetworkRunner, NetworkInput) was found.");
        else if (inputProviders > 1)
            Warn($"ProvideInput is TRUE and there are {inputProviders} input providers. Ensure exactly one to avoid conflicts.");
        else
            Info("Input provider detected.");
    }

    private void CheckSceneAnchors()
    {
        foreach (var anchor in ExpectedSceneAnchors)
        {
            if (anchor == null) { Warn("ExpectedSceneAnchors contains a NULL reference."); continue; }
            if (!anchor.gameObject.scene.IsValid()) Error($"Scene anchor '{anchor.name}' is not part of a valid scene.");
            else if (!anchor.gameObject.activeInHierarchy) Error($"Scene anchor '{anchor.name}' is INACTIVE in hierarchy.");
            else Info($"Scene anchor OK: '{anchor.name}'.");
        }

        var all = FindObjectsOfType<NetworkObject>(true);
        if (all.Length == 0) Warn("No in-scene NetworkObjects found.");
        else Info($"In-scene NetworkObjects detected: {all.Length}");
    }

    private void CheckRunnerState()
    {
        Info($"Runner.IsRunning={_runner.IsRunning}, ProvideInput={_runner.ProvideInput}, " +
      $"GameMode={_runner.GameMode}, SessionName={_runner.SessionInfo?.Name ?? "NULL"}, " +
      $"PrefabTableCount={_runner.Config?.PrefabTable?.Prefabs?.Count ?? -1}");


        if (!_runner.IsRunning)
        {
            Warn("Runner is NOT running at Start(). Confirm StartGame is called.");
        }
    }

    private void LogActiveScene()
    {
       var s = SceneManager.GetActiveScene();
        Info($"Active scene: (buildIndex={s.buildIndex}, path='{s.name}')");
    }

    private void LogPrefabFingerprint(NetworkRunner runner)
    {
        var names = runner.Config?.PrefabTable?.Prefabs?
            .Where(p => p != null)
            .Select(p => p.ToString())
            .ToArray();

        int hash = names?.Aggregate(17, (h, n) => unchecked(h * 31 + n.GetHashCode())) ?? -1;
        int count = names?.Length ?? 0;

        Info($"PrefabTable fingerprint -> Count={count}, Hash={hash}, Names=[{string.Join(", ", names ?? new string[0])}]");
    }



    private void CheckPlayersAndReadiness()
    {
        if (!_runner.IsRunning) return;

        int humans = 0, bots = 0;
        foreach (var p in _runner.ActivePlayers)
        {
            var obj = _runner.GetPlayerObject(p);
            if (obj == null)
            {
                Error($"PlayerRef {p} has NO bound PlayerObject. Ensure Runner.SetPlayerObject is called.");
                continue;
            }

            Info($"PlayerRef {p} bound -> '{obj.name}'");

            var lobbyPlayer = obj.GetComponent<LobbyPlayer>();
            if (lobbyPlayer == null)
            {
                Warn($"PlayerRef {p} object '{obj.name}' missing LobbyPlayer component.");
                continue;
            }

            if (lobbyPlayer.IsBot) bots++; else humans++;
        }

        Info($"Players snapshot -> Humans: {humans} | Bots: {bots} | Total: {humans + bots}");
    }

    private void CheckExpectedPrefabsRuntime()
    {
        if (ExpectedPrefabs == null || ExpectedPrefabs.Count == 0) return;

        foreach (var prefabRef in ExpectedPrefabs)
        {
            if (!prefabRef.IsValid)
                Error("ExpectedPrefabs contains an INVALID NetworkPrefabRef.");
            else
                Info($"Expected prefab ref present: {prefabRef}");
        }
    }

    private void CheckModeCompatibility()
    {
        switch (_runner.GameMode)
        {
            case GameMode.Shared:
                if (!_runner.ProvideInput)
                    Warn("GameMode=Shared but ProvideInput=FALSE. Peers usually need ProvideInput=TRUE.");
                break;

            case GameMode.Host:
            case GameMode.Server:
                if (_runner.ProvideInput)
                    Error($"GameMode={_runner.GameMode} but ProvideInput=TRUE. Hosts/Servers should not provide input.");
                break;

            case GameMode.Client:
                if (!_runner.ProvideInput)
                    Warn("GameMode=Client but ProvideInput=FALSE. Enable ProvideInput if players control characters.");
                break;

            default:
                Info($"GameMode={_runner.GameMode} (no specific compatibility rule).");
                break;
        }
    }

    private void CheckDuplicateRunnersInScene()
    {
        var runners = FindObjectsOfType<NetworkRunner>(true);
        if (runners.Length > 1)
            Warn($"Multiple NetworkRunner instances detected: {runners.Length}. Ensure this is intentional.");
        else
            Info("Single NetworkRunner instance detected.");
    }

   
 

    // ---- Logging helpers ----

    private void Info(string msg)
    {
        if (Verbose) Debug.Log($"{TAG}{msg}", this);
    }

    private void Warn(string msg)
    {
        Debug.LogWarning($"{TAG}{msg}", this);
    }

    private void Error(string msg)
    {
        Debug.LogError($"{TAG}{msg}", this);
    }
}
