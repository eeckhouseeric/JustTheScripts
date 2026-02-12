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

    // assigning PF_HealthUI prefab
    public GameObject healthUiPrefab;
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
        var planeObj=runner.Spawn(
            planePrefab, 
            spawnPoint.position, 
            spawnPoint.rotation, 
            anchor.InputAuthority
            );
        Debug.Log($"[PlaneSpawner] Plane spawned for player {anchor.InputAuthority}");
        Debug.Log("[SPAWNER] Plane spawned with InputAuthority=" + anchor.InputAuthority); 


        // spawns ui only for local player
        if (anchor.HasInputAuthority)
        { 
            
            Debug.Log("[PlaneSpawner] Local player detected - spawning Health UI");

            var ui = Instantiate(healthUiPrefab);

            Debug.Log($"[PlaneSpawner] Health UI instantiated {ui.name}");
            Debug.Log($"[PlaneSpawner] UI activeInHierachy = {ui.activeInHierarchy}");
            Debug.Log($"[PlaneSpawner] UI postion = {ui.transform.position}" );


            var health = planeObj.GetComponent<PlaneHealth>();
            Debug.Log($"[PlaneSpawner] PlaneHealth component found {health}");

            if (health != null)
            {
                var controller = ui.GetComponentInChildren<HealthUIController>();
                controller.Bind(health);
                Debug.Log("[PlaneSpawner] Health UI sucessfully bound to PlaneHealth");
            }
            else
            {
                Debug.LogError("[PlaneSpawner] PlaneHealth not found on spawned plane!");
            }


        }
    }
}
