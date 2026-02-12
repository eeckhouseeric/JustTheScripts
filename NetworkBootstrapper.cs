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


    private void Start()
    {
        if (Instance != this || _hasStarted) return;
        _hasStarted = true;

        Debug.Log($"[Bootstrapper] Start called. Runner={Runner?.name}, HasStarted={_hasStarted}");

        // Call ValidateConfig before starting async flow
        if (!ValidateConfig())
        {
            Debug.LogError("[Bootstrapper] Config validation failed. Not starting runner.");
            return;
        }
        _ = InitializeAsync();// Fire and forget

    }

    private async Task InitializeAsync()
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
        var prefabCount = table?.Prefabs?.Count ?? 0;
        if (prefabCount == 0)
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



        // Prefab table fingerprinting
        var names = table.Prefabs.Where(p => p != null).Select(p => p.ToString()).ToArray();
        int hash = names.Aggregate(17, (h, n) => unchecked(h * 31 + n.GetHashCode()));
       
        Debug.Log("[Bootstrapper] --- CONFIG VALIDATION ---");
        Debug.Log("[Bootstrapper] Using Global NetworkProjectConfig");
        Debug.Log($"Runner.ProvideInput={Runner.ProvideInput}");
        Debug.Log($"PrefabTable count={prefabCount}");
        Debug.Log($"PrefabTable hash={hash} | names=[{string.Join(", ", names)}]");
        Debug.Log("[Bootstrapper] -------------------------");

      
        // Build unique session name

        string unique = System.DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string version = Application.version;
        string region = "US"; // Placeholder for region logic if needed
        string uniqueToken = System.Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
       //string sceneName = $"Match_US_{Application.version}_{unique}";


        string sessionName = $"session_{region}_{unique}_{version}_{uniqueToken}";

        // StartGame arguments
        var startArgs = new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            // Don't tue runner to current scene - keep Presistent scene active
            Scene = SceneRef.FromIndex(1),
            SceneManager = Runner.GetComponent<NetworkSceneManagerDefault>(),
            Config = config,
        };

        FusionCallbackHandler.SetSceneIndex(1); // StartMenu scene index
        Debug.Log($"[Bootstrapper] StartGameArgs -> Mode={startArgs.GameMode}, " +
                  $"Session={startArgs.SessionName}, SceneIndex={startArgs.Scene}, " +
                  $"PrefabCount={prefabCount}");
        
        // Start the runner
        var result = await Runner.StartGame(startArgs);

        var localPlayerRef = Runner.LocalPlayer; // PlayerRef
        var localPlayerObj = Runner.GetPlayerObject(localPlayerRef);
        Debug.Log($"[Bootstrapper] StartGame result: Ok={result.Ok}, Reason={result.ShutdownReason}");
        Debug.Log($"[Bootstrapper] ActivePlayers={Runner.ActivePlayers.Count()} " +
                  $"LocalPlayerObject={(localPlayerObj != null ? localPlayerObj.name : "NULL")}");

        //Only log failure if result ok. Okay is false
        if (!result.Ok)
        {
            Debug.LogError($"[Bootstrapper] StartGame failed: {result.ShutdownReason}");

            // Dump global config at failure for comparison
            var global = NetworkProjectConfig.Global;
            if (global?.PrefabTable != null)
            {
                foreach (var p in global.PrefabTable.Prefabs)
                {   Debug.Log($"[Bootstrapper] Global PrefabTable entry at failure: {p}"); }
            }
            return;
        }

        // Load initial scene (index 1)
        await Runner.LoadScene(SceneRef.FromIndex(1));


        // Runner.Config is now valid - dump what the runner actually has
        var runnerNames = Runner.Config.PrefabTable.Prefabs
            .Where(p => p != null)
            .Select(p => p.ToString())
            .ToArray();
        int runnerHash = runnerNames.Aggregate(17, (h, n) => unchecked(h * 31 + n.GetHashCode())) -1;

        Debug.Log ($"[DEBUG Bootstrapper] Runner PrefabTable -> Count= {runnerNames?.Length ?? 0}" +
                   $"Hash= {runnerNames?.Aggregate(17,(h, n) => unchecked(h * 31 + n.GetHashCode()))}");
                  
        Debug.Log($"[Bootstrapper] Fusion running |" +
                  $" Mode={Runner.GameMode} |" +
                  $" Session='{Runner.SessionInfo?.Name}' |" +
                  $" Players={Runner.ActivePlayers.Count()} |" +
                  $" ProvideInput={Runner.ProvideInput} |" +
                  $"PrefabTable={(Runner.Config?.PrefabTable != null ? "SET" : "MISSING")} | " +
                  $" Scene={SceneManager.GetActiveScene().name}");
        Debug.Log("[Bootstrapper] Fusion started successfully.");
       
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


