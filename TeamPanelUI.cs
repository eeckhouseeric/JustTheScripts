using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamPanelUI : MonoBehaviour
{
    [SerializeField] private Transform playerCardContainer;
    [SerializeField] private GameObject playerCardPrefab;

    private List<LobbyPlayerCardUI> cards = new();

    public void AddPlayerCard(PlayerInfo player)
    {
        var cardGO = Instantiate(playerCardPrefab, playerCardContainer);
        var cardUI = cardGO.GetComponent<LobbyPlayerCardUI>();
        cardUI.Setup(player); // Make sure this method exists
        cards.Add(cardUI);
    }
    private List<LobbyPlayerCardUI> staticCards;


    public void RemovePlayerCard(PlayerInfo player)
    {
        var card = cards.FirstOrDefault(c => c.PlayerID == player.playerId);
        if (card != null)
        {
            cards.Remove(card);
            Destroy(card.gameObject);
        }
    }
    public void ClearPanel()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

}