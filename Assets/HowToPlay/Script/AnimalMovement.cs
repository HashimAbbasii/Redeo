using UnityEngine;

public class AnimalMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float runSpeed = 5f;
    public bool startRunningImmediately = true;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnEnable()
    {
        if (startRunningImmediately)
        {
            StartRunning();
        }
    }

    public void StartRunning()
    {
        rb.velocity = Vector3.forward * runSpeed;
    }

    void FixedUpdate()
    {
        // Maintain constant forward velocity
        if (rb.velocity.z < runSpeed)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, runSpeed);
        }
    }
}