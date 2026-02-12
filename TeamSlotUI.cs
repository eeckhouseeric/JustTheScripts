using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image teamColor;

    public void AssignPlayer(PlayerInfo player)
    {
        playerNameText.text = player.PlayerName;
        teamColor.color = player.teamColor;
    }

    public void ClearSlot()
    {
        playerNameText.text = "Open Slot";
    }

    public void Setup(PlayerInfo info)
    {
        playerNameText.text = info.PlayerName;
      
    }


}
