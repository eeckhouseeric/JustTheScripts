using UnityEngine;
using UnityEditor;
using TMPro;

public static class LobbyAuditUtility
{
    [MenuItem("Audit/Lobby Cards/Validate All LobbyPlayerCardUI")]
    public static void ValidateAllLobbyCards()
    {
        var allCards = GameObject.FindObjectsOfType<LobbyPlayerCardUI>(true);
        int errorCount = 0;

        foreach (var card in allCards)
        {
            if (card.playerNameIdText == null)
            {
                Debug.LogError($"[Audit]  Missing playerNameIdText on {card.name}", card);
                errorCount++;
                continue;
            }

            string text = card.playerNameIdText.text;
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogWarning($"[Audit]  Empty name on {card.name}", card);
            }
            else
            {
                Debug.Log($"[Audit]  {card.name} has name: '{text}'");
            }
        }

        Debug.Log($"[Audit] Finished validating {allCards.Length} cards. Errors: {errorCount}");
    }
}
