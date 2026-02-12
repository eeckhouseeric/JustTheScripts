using UnityEngine;

public class LobbyTestHarness : MonoBehaviour
{
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private int botCount = 4;

    private void Start()
    {
        if (lobbyManager == null)
        {
            Debug.LogError(" LobbyManager reference is missing.");
            return;
        }

        for (int i = 0; i < botCount; i++)
        {
            string botName = BotGeneratorName.generateName(); // Direct static call
            int teamId = i % 2;

            var botInfo = new PlayerInfo(botName, i, i % 2, true);
            lobbyManager.AddPlayerCard(botInfo); //  Your actual method
        }

        Debug.Log($" Injected {botCount} bots using BotGeneratorName");
    }

}
