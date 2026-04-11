using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using System;
using UnityEngine.SocialPlatforms;

public class SpitwadMachineGun : NetworkBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform firePoint;

    [Header("Firing")]
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private float range = 500f;
    [SerializeField] private int damage = 10;

    [Header("Prefabs")]
    [SerializeField] private NetworkObject spitwadVisualPrefab;
    [SerializeField] private NetworkObject spitwadImpactPrefab;



    private float nextFireTime = 0f;

    /// ------------------------------
    /// Fusion Input Loop
    /// -------------------------------

    public override void FixedUpdateNetwork()
    {
        // only player with input authority should read input
        if (!Object.HasInputAuthority)
            return;

        // Read fusion Input
        if (GetInput<PlaneInputData>(out var input))
        {
            if (input.Fire && Runner.SimulationTime >= nextFireTime)
            {
                nextFireTime = Runner.SimulationTime + fireRate;

                if (Object.HasInputAuthority)
                {
                    FireServerRpc();
                }
                else
                {
                    RPC_FireRequest();
                }
            }
        }
    }
    // Client  Server request to fire
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_FireRequest()
    {
        // Send RPC to server to request firing
        FireServerRpc();
    }



    // ----------------------------------------------------------------------
    //  RPC: Fired by input authority, executed by state authority
    //  This is where the projectile is actually spawned
    // ----------------------------------------------------------------------

    //Client send fire intent to server
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void FireServerRpc()
    {

        if (firePoint == null)
        {

            Debug.LogError("[SpitwadMachineGun] ERROR: firePoint is Null");
            return;

        }

        //1 . Raycast for real hit (lag compensated)
        if (Runner.LagCompensation.Raycast(
                firePoint.position,
                firePoint.forward,
                range,
                Object.InputAuthority,
                out var hit)) 
        {
            //2. Apply damage to hit target has PlaneHealth
            if (hit.Hitbox != null)
            {
                var root = hit.Hitbox.Root;
                var health = root.GetComponent<PlaneHealth>();
                if (health != null)
                {
                    //simple path: use TakeDamage
                    health.ServerApplyDamage(damage, Runner.Tick);
                }

            }



            //3. Spwan impact effect at hit point

            if (spitwadImpactPrefab != null)
            {
                Runner.Spawn(
                    spitwadImpactPrefab,
                    hit.Point,
                    Quaternion.LookRotation(hit.Normal)
                    );


            }
        }


        //4. Spawn visual spitwad (fake projectile)
        
        
        if (spitwadVisualPrefab != null)
        {
            var spawnPos = firePoint.position + firePoint.forward * 0.2f; 

            Runner.Spawn(
                spitwadVisualPrefab,
                spawnPos,
                firePoint.rotation
                );
        }

    }
}
