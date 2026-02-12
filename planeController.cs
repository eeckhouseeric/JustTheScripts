using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlaneController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float thrustSpeed = 50f;
    public float pitchSpeed = 90f;
    public float yawSpeed = 90f;
    public float rollSpeed = 120f;
    public float currentThrust = 1f;
    public float accelerationRate = 10f;
    public float decelerationRate = 8f;
    public float minThrust = 0.3f;
    public float maxThrust = 1f;






    [Header("Shooting Settings")]
    public GameObject primaryBulletPrefab;
    public Transform primaryFirePoint;
    public float primaryShootCooldown = 0.3f;

    public GameObject secondaryBulletPrefab;
    public Transform secondaryFirePoint;
    public float secondaryShootCooldown = 1.5f;

    private Rigidbody rb;
    private float primaryShootTimer;
    private float secondaryShootTimer;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>() ?? GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on plane or its children.");
            return;
        }

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Debug.Log("PlaneController Spawned � Rigidbody initialized.");
    }

    public override void FixedUpdateNetwork()
    {

 
        Debug.Log("FixedUpdateNetwork running Authority: " + Object.InputAuthority);

        if (!GetInput<PlaneInputData>(out var input))
        {
            Debug.LogWarning("No input received in FixedUpdateNetwork.");
            return;
        }

        Debug.Log($"[PlaneController] Input received: Thrust={input.Thrust}, Pitch={input.Pitch}, Yaw={input.Yaw}, Roll={input.Roll}");

        if (rb != null)
        {
            // Adjust throttle based on input
            if (input.Thrust > 0f)
            {
                currentThrust += accelerationRate * Runner.DeltaTime;
            }
            else
            {
                currentThrust -= decelerationRate * Runner.DeltaTime;
            }

            currentThrust = Mathf.Clamp(currentThrust, minThrust, maxThrust);

            // Apply velocity
            rb.linearVelocity = transform.forward * currentThrust * thrustSpeed;

            // Apply rotation via physics
            float pitch = input.Pitch * pitchSpeed * Runner.DeltaTime;
            float yaw = input.Yaw * yawSpeed * Runner.DeltaTime;
            float roll = input.Roll * rollSpeed * Runner.DeltaTime;

            Quaternion deltaRotation = Quaternion.Euler(-pitch, yaw, -roll);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
        else
        {
            // Fallback: Direct transform movement for testing
            Debug.LogWarning("No Rigidbody found � using transform-based fallback movement.");
            transform.position += transform.forward * input.Thrust * thrustSpeed * Runner.DeltaTime;
            transform.Rotate(-input.Pitch * pitchSpeed * Runner.DeltaTime,
                             input.Yaw * yawSpeed * Runner.DeltaTime,
                            -input.Roll * rollSpeed * Runner.DeltaTime);
        }

        // Handle shooting
        primaryShootTimer -= Runner.DeltaTime;
        secondaryShootTimer -= Runner.DeltaTime;

        if (input.Fire && primaryShootTimer <= 0f)
        {
            Shoot(primaryBulletPrefab, primaryFirePoint);
            primaryShootTimer = primaryShootCooldown;
        }

        if (input.SecondaryFire && secondaryShootTimer <= 0f)
        {
            Shoot(secondaryBulletPrefab, secondaryFirePoint);
            secondaryShootTimer = secondaryShootCooldown;
        }
    }

    private void Shoot(GameObject prefab, Transform firePoint)
    {
        if (prefab == null || firePoint == null)
        {
            Debug.LogWarning("Bullet prefab or fire point is missing!");
            return;
        }

        var bullet = Runner.Spawn(prefab, firePoint.position, firePoint.rotation, Object.InputAuthority);
        if (bullet.TryGetComponent<Rigidbody>(out var bulletRb))
            bulletRb.AddForce(firePoint.forward * 100f, ForceMode.Impulse);
    }
}