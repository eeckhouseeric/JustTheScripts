using UnityEngine;
using Fusion;

public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] private float lifetime = 3f;
    private float spawnTime;   



    public override void Spawned()
    {
        spawnTime = Runner.SimulationTime;
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.SimulationTime - spawnTime >= lifetime)
        {
            Runner.Despawn(Object);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Runner != null && Object != null)
        { 
        
            Runner.Despawn(Object);


        }
    }


}
