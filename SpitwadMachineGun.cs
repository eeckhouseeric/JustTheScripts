using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class SpitwadMachineGun : NetworkBehaviour
{
    [Header("Spitwad Settings")]
    [SerializeField] private GameObject spitwadPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private float spitwadSpeed = 50f;

    [Header("Input")]
    [SerializeField] private InputActionReference fireAction;

    private float nextFireTime = 0f;

    public override void Spawned()
    {
        // Only activate for InputAuthority (e.g., local player)
        if (Object.HasInputAuthority)
        {
            fireAction.action.performed += OnFireInput;
        }
    }

    private void OnFireInput(InputAction.CallbackContext ctx)
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            
            int timestamp = (int)(Runner.SimulationTime * 1000); // Uses milliseconds
            FireSpitwadRPC(timestamp);
        }
    }
    
    //Client send fire intent to server
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void FireSpitwadRPC(int timestamp)
    {
        if (!Object.HasInputAuthority)
            return;

        GameObject spitwad = Instantiate(spitwadPrefab, firePoint.position, firePoint.rotation);
        if (spitwad.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = firePoint.forward * spitwadSpeed;
        }

        // Optional: Add particle FX, sound, or shake
    }

    private void OnDisable()
    {
        if (Object.HasInputAuthority)
        {
            fireAction.action.performed -= OnFireInput;
        }
    }
}
