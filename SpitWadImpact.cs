using UnityEngine;
using Fusion;

public class SpitWadImpact : NetworkBehaviour
{
    // How long the impact puff stays visible
    [SerializeField] private float lifeTime = 0.4f;
    
    
    private float spawnTime;

    public override void Spawned()
    {
        spawnTime = Runner.SimulationTime;
    }

    public override void FixedUpdateNetwork()
    {
        // Despawn after lifetime expires
        if (Runner.SimulationTime > spawnTime + lifeTime)
            Runner.Despawn(Object);
    }

}
