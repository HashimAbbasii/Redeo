using UnityEngine;

public class AnimalMovement : MonoBehaviour 
{
    [Header("Movement Settings")]
    public float runSpeed = 2f; // Set this higher than player's maxSpeed
    public bool startRunningImmediately = true;
    
    private Rigidbody rb;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable() {
        if(startRunningImmediately) {
            StartRunning();
        }
    }

    public void StartRunning() {
        // Set constant velocity
        rb.velocity = transform.forward * runSpeed;
    }

    void FixedUpdate() {
        // Maintain constant forward speed while preventing y-axis rotation
        rb.velocity = new Vector3(0, rb.velocity.y, runSpeed);
        rb.angularVelocity = Vector3.zero;
    }
}