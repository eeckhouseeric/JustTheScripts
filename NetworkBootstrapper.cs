using Fusion;
using Fusion.Menu;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;



[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkRunner))]
public class NetworkBootstrapper : MonoBehaviour
{
    public static NetworkRunner Runner { get; private set; }
    public static NetworkBootstrapper Instance { get; private set; }

    [Header("Fusion Configuration")]
    [SerializeField] private NetworkPrefabTable networkPrefabTable; // optional override
   // [SerializeField] public NetworkProjectConfig networkConfig;// Optional inspector assignment

    private bool _hasStarted;
    private void Awake()
    {
        //Singleton guard
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Bootstrapper] Duplicate instance detected. Destroying this GameObject.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[Bootstrapper] Awake called. GameObject set to not destroy on load.");

        //  runner 
        Runner = GetComponent<NetworkRunner>();
        if (Runner == null)
        {
            Debug.LogError("[Bootstrapper] NetworkRunner component is missing on this GameObject. Please attach it.");
            return;
        }


        // Ensure callback handler exists
        if (!Runner.GetComponent<FusionCallbackHandler>())
        {
            Runner.gameObject.AddComponent<FusionCallbackHandler>();
            Debug.Log("[Bootstrapper] FusionCallbackHandler added to Runner.");
        }

        // Add lobby callbacks if available
        if (LobbyManager.instance)
        {
           // Runner.AddCallbacks(LobbyManager.instance);
            Debug.Log("[Bootstrapper] LobbyManager callbacks registered.");
        }

        LogDiagnostics();

    }


    public async Task StartRunner(GameMode mode, string sessionName,SceneRef scene)
    {
        if (Instance != this)
        { 
        
            Debug.LogWarning("[Bootstrapper] StartRunner called on non-singleton instance.");
            return;
        }

        if (_hasStarted)
        {
            Debug.LogWarning("[Bootstrapper] StartRunner called but runner has already started.");
            return;
        }
        _hasStarted = true;

        if (!ValidateConfig())
        { 
            Debug.LogError("[Bootstrapper] Configuration validation failed. Aborting StartRunner.");
            return;
        }

        await InitializeAsync(mode,sessionName,scene);

    }

    private async Task InitializeAsync(GameMode mode, string sessionName, SceneRef scene)
    {

        Debug.Log("[Bootstrapper] InitializeAsync entered.");

        if (Runner == null)
        {
            Debug.LogError("[Bootstrapper] Runner is null in InitializeAsync. Aborting startup.");
            return;
        }

        Debug.Log($"[Bootstrapper] Runner.IsRunning={Runner.IsRunning}");

        //Early exit if already running
        if (Runner.IsRunning)
        {
            Debug.LogWarning("[Bootstrapper] Runner already running — skipping StartGame.");
            return;
        }


        Runner.ProvideInput = true; // Ensure input is provided for Shared mode

        // Ensure NetworkSceneManagerDefault exists
        var sceneManager = Runner.GetComponent<NetworkSceneManagerDefault>() ??
                           Runner.gameObject.AddComponent<NetworkSceneManagerDefault>();


        //Resolve config
        var config = ResolveProjectConfig();
        if (config == null)
        {
            Debug.LogError("[Bootstrapper] No valid NetworkProjectConfig found. Aborting startup.");
            return;
        }

        // Prefab table guard
        var table = config.PrefabTable;
        if(table == null || table.Prefabs == null || table.Prefabs.Count == 0)
        {
            Debug.LogError("[Bootstrapper] PrefabTable is EMPTY — aborting startup.");
            return;
        }

        //Proof log - runner must now see the same prefab count as resolved config
        foreach (var source in table.Prefabs)
        {
            if (source != null)
            {
                Debug.Log($"[Bootstrapper] Prefab entry: {source.ToString()} (Type={source.GetType().Name})");
            }
            else
            {
                Debug.Log("[Bootstrapper] Prefab entry: NULL");
            }
        }
        // FIX #1 — DO NOT AUTO-LOAD ANY SCENE HERE
        // SceneRef.None prevents Fusion from overriding Unity's scene loading.
        // StartGame arguments
        var startArgs = new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            // Don't tue runner to current scene - keep Presistent scene active
            Scene = SceneRef.None,
            SceneManager = Runner.GetComponent<NetworkSceneManagerDefault>(),
            Config = config,
        };

        // Tell FusionCallbackHandler what scene will load NEXT
        FusionCallbackHandler.SetSceneIndex(scene.AsIndex); // StartMenu scene index
        Debug.Log($"[Bootstrapper] StartGameArgs -> Mode={startArgs.GameMode}, " +
                  $"Session={startArgs.SessionName}, SceneIndex={startArgs.Scene}");
        
        // Start the runner
        var result = await Runner.StartGame(startArgs);
        
        
        // FIX #2 — Correct success check
        if (!result.Ok)
         {
            Debug.Log($"[Bootstrapper] StartGame failed: Mode={mode}, Session= {sessionName},SceneIndex={scene.AsIndex}");
            return;
        }

        // FIX #3 — Load the scene passed in (Lobby)
        await Runner.LoadScene(scene);
        
        Debug.Log($"[Bootstrapper] Fusion started successfully. Scene= {scene.AsIndex}");
        PostStartupFlow();

    }

    private NetworkProjectConfig ResolveProjectConfig()
    {
        var config = NetworkProjectConfig.Global;
        if (config == null)
        {
            Debug.LogError("[Bootstrapper] No NetworkProjectConfig.Global found. " +
                           "Make sure you have one in Resources/Fusion/");
            return null;
        }

        if (config.PrefabTable == null || config.PrefabTable.Prefabs == null || config.PrefabTable.Prefabs.Count == 0)
        {
            Debug.LogError("[Bootstrapper] Global config has no prefab table entries. " +
                           "Open Resources/Fusion/NetworkProjectConfig and assign your NetworkPrefabTable.");
        }
        else
        {
            Debug.Log($"[Bootstrapper] Using Global config with PrefabTable count={config.PrefabTable.Prefabs.Count}");
        }

        return config;
    }




    private void LogDiagnostics()
    {
        var bootstraps = FindObjectsOfType<NetworkBootstrapper>(true);
        Debug.Log($"[Bootstrapper] Found {bootstraps.Length} NetworkBootstrapper instances.");
        foreach (var b in bootstraps)
            Debug.Log($"[Bootstrapper] Instance: {b.name} in scene '{b.gameObject.scene.name}'");

        var runners = FindObjectsOfType<NetworkRunner>(true);
        Debug.Log($"[Bootstrapper] Found {runners.Length} NetworkRunner instances.");
        foreach (var r in runners)
            Debug.Log($"[Bootstrapper] Runner: {r.name} in scene '{r.gameObject.scene.name}'");
    }

    private void PostStartupFlow()
    {
        // Optional: inject bots, transition scenes, or trigger gameplay logic
        Debug.Log("[Bootstrapper] PostStartupFlow triggered.");
    }
    // Optional: explicit config validation hook (if you want a preflight in Start)
    private bool ValidateConfig()
    {
        var config = ResolveProjectConfig();
        if (config == null)
        {
            Debug.LogError("[Bootstrapper] No NetworkProjectConfig available.");
            return false;
        }

        if (config.PrefabTable == null || config.PrefabTable.Prefabs == null || config.PrefabTable.Prefabs.Count == 0)
        {
            Debug.LogError("[Bootstrapper] PrefabTable is missing or empty.");
            return false;
        }

        Debug.Log("[Bootstrapper] NetworkProjectConfig and PrefabTable are assigned correctly.");
        return true;
    }

}


