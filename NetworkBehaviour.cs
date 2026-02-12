using Fusion;
using UnityEngine;

public class SceneAnchorBehaviour : NetworkBehaviour
{
    public override void Spawned()
    {
        Debug.Log("[SceneAnchorBehaviour] Spawned on network.");
    }
}
