using Fusion;
using UnityEngine;

public class PlayerIdentity : NetworkBehaviour
{
    [Networked] public string Username { get; set; }

    public override void Spawned()
    {
        Debug.Log($"[PlayerIdentity] Spawned Player with DisplayName: {Username}");
    }
}
