using UnityEngine;
using Fusion;

public class SpitWadVisual : NetworkBehaviour
{
    [SerializeField] private float speed = 200f;
    // Visual projectile speed
    [SerializeField] private float lifeTime = 0.25f;
    // How long the visual projectile exists

    private float spawnTime;

    public override void Spawned()
    {
        // Record when this projectile was created
        spawnTime = Runner.SimulationTime;
    }

    public override void FixedUpdateNetwork()
    {
        // Move forward visually (not used for hit detection)
        transform.position += transform.forward * speed * Runner.DeltaTime;


        // Despawn after lifetime expires
        if (Runner.SimulationTime > spawnTime + lifeTime)
        {
           
            if(Runner.SimulationTime > spawnTime + lifeTime)
                Runner.Despawn(Object);

        }
    }

}
