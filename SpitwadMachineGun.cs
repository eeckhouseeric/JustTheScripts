using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class SpitwadMachineGun : NetworkBehaviour
{
    [Header("Spitwad Settings")]
    [SerializeField] private NetworkObject spitwadProjectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private float spitwadSpeed = 50f;



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
                FireSpitwadRPC();
            }
        }
    }





    // ----------------------------------------------------------------------
    //  RPC: Fired by input authority, executed by state authority
    //  This is where the projectile is actually spawned
    // ----------------------------------------------------------------------

    //Client send fire intent to server
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void FireSpitwadRPC()
    {

        if (spitwadProjectilePrefab == null)
        { 
        
           Debug.LogError("[SpitwadMachineGun] ERROR: spitwadProjectile is Null");
           return;

        }

        if (firePoint == null)
        { 
        
            Debug.LogError("[SpitwadMachineGun] ERROR: firePoint is Null");
            return;

        }

        // State authority spawns the pojectile
        NetworkObject spitwad = Runner.Spawn(
                spitwadProjectilePrefab,
                firePoint.position,
                firePoint.rotation
            );

        Debug.Log($"[SpitwadMachineGun]\n" +
                        $"FirePoint Forward: {firePoint.forward}" +
                        $"Projectile Rotation: {spitwad.transform.rotation.eulerAngles}" +
                        $"Projectile Forward: {spitwad.transform.forward}" +
                        $"Projectile Position: {spitwad.transform.position}"
            );




        //ingore collisions between the projectile and the player who fired it
        if (spitwad.TryGetComponent<Collider>(out var projCol))
        {
            var planeCol = firePoint.root.GetComponentInChildren<Collider>();
            if (planeCol != null)
            {
                Physics.IgnoreCollision(projCol, planeCol);
            }

        }


        //apply forward velocity 
        if (spitwad.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 inheritedVelocity = firePoint.root.GetComponent<Rigidbody>().linearVelocity ;
            Vector3 muzzleVelocity = firePoint.forward * spitwadSpeed;

            rb.linearVelocity = inheritedVelocity + muzzleVelocity;
            Debug.Log($"[SpitwadMachineGun] Applied velocity: {rb.linearVelocity}");
        }

        // Optional: Add particle FX, sound, or shake
    }

}
