using Fusion;
using UnityEngine;

public class RuntimeRunnerConfigurator : MonoBehaviour, INetworkRunnerConfigurator
{
    [SerializeField] private NetworkProjectConfig configAsset;

    public void ConfigureRunner(NetworkRunner runner)
    {
        runner.ProvideInput = true;
        runner.AddCallbacks(new FusionCallbackHandler());

        // Config is already assigned via Inspector — no need to set it manually
        Debug.Log("[RuntimeRunnerConfigurator] Runner configured with input and callbacks.");
    }
}
