using UnityEngine;

public class HowtoPlayController : MonoBehaviour
{
    public float acceleration = 3.0f;
    public float deceleration = 3.5f;
    public float maxSpeed = 10f;
    public float walkSpeed = 2f;
    public float rideSpeed = 15f;
    public float dragSensitivity = 0.1f;
    public float horizontalBoundary = 3f;
    public float rideCheckRadius = 2.5f;
    public bool hasJumped=false;

    public Animator animator;
    public Rigidbody rb;
    public Transform radiusVisualizer;
    public LayerMask animalLayer;

    private float currentSpeed = 0f;
    private bool isTouching = false;
    private Vector2 previousTouchPosition;
    private float horizontalOffset = 0f;
    private int? currentFingerId = null;

    public PlayerState currentState = PlayerState.Walking;
    public bool IsHolding { get; private set; }
    public string animalTag = "Animal";
    public string ridingOffsetName = "RidingOffset";
    private Transform currentAnimal = null;
    private Vector3 ridingPositionOffset = Vector3.zero;

    private void Start()
    {
        currentFingerId = null;
        IsHolding = false;
        currentState = PlayerState.Walking;
        DrawRadius();
        radiusVisualizer.gameObject.SetActive(false);
    }

    private void Update()
    {
#if UNITY_EDITOR
        HandleKeyboardInput();
#endif
        HandleInput();
        UpdateStateMachine();
        UpdateMovement();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (currentState == PlayerState.Riding)
        {
            // When riding, movement is handled by the parent animal
            return;
        }

        // Only apply movement when not riding
        Vector3 newPosition = new Vector3(
            horizontalOffset,
            transform.position.y,
            transform.position.z + currentSpeed * Time.fixedDeltaTime
        );
        rb.MovePosition(newPosition);
    }

    // ---------------- Input ----------------

    void HandleInput()
    {
        if (Input.touchCount == 0)
        {
            ResetTouchState();
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            if (currentFingerId == null || touch.fingerId == currentFingerId)
            {
                ProcessTouch(touch);
                break;
            }
        }
    }

    void ProcessTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (currentFingerId == null)
                {
                    currentFingerId = touch.fingerId;
                    previousTouchPosition = touch.position;
                    isTouching = true;
                    IsHolding = true;
                }
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isTouching)
                {
                    IsHolding = true;
                    float dragDelta = (touch.position.x - previousTouchPosition.x) * dragSensitivity;
                    horizontalOffset += dragDelta;
                    horizontalOffset = Mathf.Clamp(horizontalOffset, -horizontalBoundary, horizontalBoundary);
                    previousTouchPosition = touch.position;
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (touch.fingerId == currentFingerId)
                {
                    ResetTouchState();
                }
                break;
        }
    }

    void ResetTouchState()
    {
        IsHolding = false;
        isTouching = false;
        currentFingerId = null;
    }

#if UNITY_EDITOR
    void HandleKeyboardInput()
    {
        IsHolding = Input.GetKey(KeyCode.Space);
        if (IsHolding) isTouching = true;
        else if (Input.GetKeyUp(KeyCode.Space)) isTouching = false;

        float keyboardInput = Input.GetAxis("Horizontal");
        horizontalOffset += keyboardInput * dragSensitivity * 50f * Time.deltaTime;
        horizontalOffset = Mathf.Clamp(horizontalOffset, -horizontalBoundary, horizontalBoundary);
    }
#endif

    // ---------------- State Machine ----------------

    void UpdateStateMachine()
    {
        bool animalNearby = CheckForAnimalsInRadius();

        switch (currentState)
        {
            case PlayerState.Walking:
                hasJumped = false; // Reset jump flag when walking
                if (IsHolding)
                {
                    currentState = PlayerState.Running;
                }
                else if (animalNearby && !hasJumped)
                {
                    PrepareForJump();
                    hasJumped = true;
                }
                break;

            case PlayerState.Running:
                if (!IsHolding)
                {
                    currentState = PlayerState.Walking;
                }
                else if (animalNearby && !hasJumped)
                {
                    PrepareForJump();
                    hasJumped = true;
                }
                break;

            case PlayerState.Riding:
                if (!IsHolding && !hasJumped)
                {
                    PrepareForJump();
                    hasJumped = true;
                }
                break;

            case PlayerState.Jumping:
                if (IsGrounded())
                {
                    if (animalNearby)
                    {
                        StartRiding();
                        hasJumped = false; // Reset for next jump
                    }
                    else
                    {
                        GameOver();
                    }
                }
                break;
        }
    }

    bool CheckForAnimalsInRadius()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, rideCheckRadius, animalLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(animalTag))
            {
                Transform ridingOffset = hitCollider.transform.Find(ridingOffsetName);
                if (ridingOffset != null)
                {
                    currentAnimal = hitCollider.transform;
                    ridingPositionOffset = ridingOffset.localPosition;
                    return true;
                }
            }
        }
        currentAnimal = null;
        return false;
    }


    void PrepareForJump()
    {
        radiusVisualizer.gameObject.SetActive(true);
        currentState = PlayerState.Jumping;
        PerformJump();
    }

    bool IsGrounded()
    {
        // Simple ground check - you might want to improve this
        return Mathf.Abs(rb.velocity.y) < 0.1f;
    }



    public void StartRiding()
    {
        if (currentAnimal != null)
        {
            Transform ridingOffset = currentAnimal.Find(ridingOffsetName);
            if (ridingOffset != null)
            {
                // Reset player physics before parenting
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;

                // Parent to the animal
                transform.position = ridingOffset.position;
                transform.rotation = ridingOffset.rotation;
                transform.SetParent(ridingOffset);
            }
        }

        radiusVisualizer.gameObject.SetActive(false);
        currentState = PlayerState.Riding;
        Debug.Log("Started Riding!");
    }
    


    
    

    void GameOver()
    {
        Debug.Log("Game Over: Fell without mount!");
        // You can implement your fall animation, disable movement, or load scene here
        this.enabled = false;
    }

    // ---------------- Movement & Anim ----------------

    void UpdateMovement()
    {
        float targetSpeed = currentState switch
        {
            PlayerState.Walking => walkSpeed,
            PlayerState.Running => maxSpeed,
            PlayerState.Riding => rideSpeed,
            PlayerState.Jumping => 0f, // No movement during jump
            _ => 0f
        };

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed,
            (IsHolding ? acceleration : deceleration) * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", currentSpeed / maxSpeed);
        animator.SetBool("IsRunning", currentState == PlayerState.Running);
        animator.SetBool("IsRiding", currentState == PlayerState.Riding);
        animator.SetBool("IsJumping", currentState == PlayerState.Jumping);
    }

    void PerformJump()
    {
        Debug.Log("Jumped!");
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }

    void DrawRadius()
    {
        if (!radiusVisualizer) return;

        LineRenderer lr = radiusVisualizer.GetComponent<LineRenderer>();
        if (!lr)
        {
            lr = radiusVisualizer.gameObject.AddComponent<LineRenderer>();
        }

        lr.positionCount = 64;
        lr.loop = true;
        lr.widthMultiplier = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = Color.green;

        float angle = 0f;
        for (int i = 0; i < 64; i++)
        {
            float x = Mathf.Cos(angle) * rideCheckRadius;
            float z = Mathf.Sin(angle) * rideCheckRadius;
            lr.SetPosition(i, new Vector3(x, 0.01f, z));
            angle += 2 * Mathf.PI / 64;
        }
    }

    public enum PlayerState
    {
        Walking,
        Running,
        Riding,
        Jumping
    }
}