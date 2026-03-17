using Fusion;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

public class PlaneSpawner : MonoBehaviour
{
    public NetworkRunner runner;
    public GameObject planePrefab;
    public Transform spawnPoint;
    public NetworkObject spawnedPlane;

    // assigning PF_HealthUI prefab
    public GameObject healthUiPrefab;
    public GameObject crossHairPrefab;

    // NEW: HUDCanvas prefab name (must be in Resources folder)
    private const string HUD_CANVAS_PREFAB = "PF_HUDCanvas";
    void Start()
    {
        StartCoroutine(WaitAndSpawn());
        Debug.Log("[PlaneSpawner] Start coroutine to wait and spawn plane");
    }

    private IEnumerator WaitAndSpawn()
    {
        Debug.Log("[PlaneSpawner] Coroutine started");
        Debug.Log($"[DEBUG] PlaneSpawner Start: initial spawnPoint={spawnPoint}");
        var anchor = GetComponent<NetworkObject>();


        // Wait until runner is assigned and running
        while (anchor == null || !anchor.HasInputAuthority || anchor.Runner == null || !anchor.Runner.IsRunning)
        {
            anchor = GetComponent<NetworkObject>();
            yield return null;
        }

        runner = anchor.Runner;
        Debug.Log($"[PlaneSpawner] Runner assigned: {runner.name}");
        Debug.Log($"[PlaneSpawner] InputAuthority = {anchor.InputAuthority}, HasStateAuthority = {anchor.HasStateAuthority}");


        if (planePrefab == null)
        {
            Debug.LogError("[PlaneSpawner] Plane prefab is not assigned!");
            yield break;
        }

        if (spawnPoint == null) 
        { Debug.LogError("[PlaneSpawner] ERROR: spawnPoint was never assigned by FusionCallbackHandler!"); 
            yield break; 
        }

      // spawns the plane
       spawnedPlane=runner.Spawn(
            planePrefab, 
            spawnPoint.position, 
            spawnPoint.rotation, 
            anchor.InputAuthority
            );
        Debug.Log($"[PlaneSpawner] Plane spawned for player {anchor.InputAuthority}");
        Debug.Log("[SPAWNER] Plane spawned with InputAuthority=" + anchor.InputAuthority);


        // Spawn UI only for local player
        if (anchor.HasInputAuthority) 
        { 
            Debug.Log("[PlaneSpawner] Local player detected - spawning HUDCanvas + UI");
            // 1. Spawn HUDCanvas dynamically
                GameObject hudCanvasGO = Instantiate(Resources.Load<GameObject>(HUD_CANVAS_PREFAB));
                if (hudCanvasGO == null) 
                    { Debug.LogError("[PlaneSpawner] ERROR: PF_HUDCanvas not found in Resources!");
                        yield break; 
                    } 
                Canvas hudCanvas = hudCanvasGO.GetComponent<Canvas>();
                if (hudCanvas == null)
                    { 
                        Debug.LogError("[PlaneSpawner] ERROR: PF_HUDCanvas has no Canvas component!"); 
                        yield break; 
                    } 
                Debug.Log("[PlaneSpawner] HUDCanvas instantiated"); 
                
                // 2. Spawn Health UI under HUDCanvas
                var ui = Instantiate(healthUiPrefab, hudCanvas.transform);
                Debug.Log("[PlaneSpawner] Health UI instantiated under HUDCanvas");
                
                var health = spawnedPlane.GetComponent<PlaneHealth>();
                if (health != null)
                    { var controller = ui.GetComponentInChildren<HealthUIController>();
                        controller.Bind(health); Debug.Log("[PlaneSpawner] Health UI successfully bound to PlaneHealth");
                    } 
                else 
                    { 
                        Debug.LogError("[PlaneSpawner] PlaneHealth not found on spawned plane!"); 
                    } 
            // 3. Spawn Crosshair under HUDCanvas
                if (crossHairPrefab != null) 
                {
                var crosshairObj = Instantiate(crossHairPrefab, hudCanvas.transform);
                Debug.Log("[PlaneSpawner] Crosshair instantiated under HUDCanvas");
                
                var crosshair = crosshairObj.GetComponent<Crosshair>();
                var feeder = FindFirstObjectByType<PlaneInputFeeder>();
                if (crosshair == null)
                    Debug.LogError("[PlaneSpawner] ERROR: Crosshair script missing on prefab");
                if (feeder == null) 
                 //   Debug.LogError("[PlaneSpawner] ERROR: PlaneInputFeeder missing on plane");
                if (crosshair != null && feeder != null) { crosshair.BindInputSource(feeder);
                    Debug.Log("[PlaneSpawner] Crosshair bound to PlaneInputFeeder");
                    // 4. Spawn Lead Indicator under HUDCanvas
                    var leadIndicatorPrefab = Resources.Load<GameObject>("PF_LeadIndicator");
                    var leadObj = Instantiate(leadIndicatorPrefab, hudCanvas.transform);
                    var leadIndicator = leadObj.GetComponent<LeadIndicator>();

                    // Attach controller to plane
                    var leadController = spawnedPlane.gameObject.AddComponent<LeadIndicatorController>();
                    //leadController.Initialize(leadIndicator);
                }
            } 
        } 
    }
}
    
