using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerReady : NetworkBehaviour
{
    [Networked] public bool IsReady { get; set; }
    [Networked] public bool IsBot { get; set; }

    public void SetReady(bool ready)
    {
        if (!HasStateAuthority)
        {
            Debug.LogWarning($"[PlayerReady] Rejecting SetReady from non-authority.");
            return;
        }

        IsReady = ready;
        Debug.Log($"[PlayerReady] Set IsReady = {ready}");
    }

    public void SetBot(bool bot)
    {
        if (!HasStateAuthority)
        {
            Debug.LogWarning($"[PlayerReady] Rejecting SetBot from non-authority.");
            return;
        }

        IsBot = bot;
        Debug.Log($"[PlayerReady] Set IsBot = {bot}");
    }

    public override void Spawned()
    {
        Debug.Log($"[PlayerReady] Spawned | IsBot={IsBot} | IsReady={IsReady}");
    }
}
