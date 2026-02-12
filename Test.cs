using UnityEngine;

public class TestFly : MonoBehaviour
{
    public float speed = 20f;
    private Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            rb.linearVelocity = transform.forward * speed;
    }
}