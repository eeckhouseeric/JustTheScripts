using Fusion;
using UnityEngine;

public class MinimalBootstrapper : MonoBehaviour
{
    private NetworkRunner runner;

    private void Start()
    {
        runner = GetComponent<NetworkRunner>();
        runner.ProvideInput = true;

        runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            Scene = runner.GetSceneRef("GreyBox"), // Fixed the type mismatch by using GetSceneRef
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            SessionName = "MinimalSession"
        });
    }
}
