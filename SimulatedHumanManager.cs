using Fusion;
using System.Collections;
using UnityEngine;
using static Unity.Collections.Unicode;

public class SimulatedHumanManager : MonoBehaviour
{
    //[SerializeField] private NetworkObject simulatedHumanPrefab;
    private bool _alreadyInjected;
    private void Start()
    {
#if UNITY_EDITOR
        //StartCoroutine(WaitForRunnerThenInject());
#endif
        }
private IEnumerator WaitForRunnerThenInject()
    {
        Debug.Log("[SimHuman] Coroutine started.");

        while (NetworkBootstrapper.Runner == null|| !NetworkBootstrapper.Runner.IsRunning)
        {
            Debug.Log($"[SimHuman] Waiting... Runner exists={NetworkBootstrapper.Runner != null}, " +
                      $"IsRunning={(NetworkBootstrapper.Runner?.IsRunning.ToString() ?? "N/A")}");
            yield return null;
        }
        
        var runner = NetworkBootstrapper.Runner;
        var localPlayer = runner.LocalPlayer;

        if (LobbyManager.instance.hasPlayer(localPlayer))
        {
            Debug.Log("[SimulatedHumanManager] Local player already injected - skipping.");
            yield break;
        }

            var simulateHumanInfo = new PlayerInfo(
            name: "SimulatedHuman",
            id: 999,
            teamID: 0,
            isBot: false,
            isReady: true,
            playerRef: localPlayer
            );
        Debug.Log("[SimulatedHumanManager] Creating SimulatedHuman");
        LobbyManager.instance.AddPlayerCard(simulateHumanInfo);
        Debug.Log("[SimulatedHumanManager] Injected fake human into lobby");
    

        // Check if already injected
     
    }

   

}



