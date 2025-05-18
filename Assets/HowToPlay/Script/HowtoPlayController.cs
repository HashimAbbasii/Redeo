using UnityEngine;

public class HowtoPlayController : MonoBehaviour
{
    // Movement Parameters
    public float acceleration = 3.0f;
    public float deceleration = 3.5f;
    public float maxSpeed = 10f;
    public float walkSpeed = 2f;
    public float dragSensitivity = 0.1f;
    public float horizontalBoundary = 3f;

    // References
    public Animator animator;
    public Rigidbody rb;

    // Private variables
    private float currentSpeed = 0f;
    private bool isTouching = false;
    private Vector2 previousTouchPosition;
    private float horizontalOffset = 0f;
    private int? currentFingerId = null; // Track specific finger

    public bool IsHolding { get; private set; }

    private void Start()
    {
        IsHolding = false;
        currentFingerId = null;
    }

    void Update()
    {
        HandleInput();
        UpdateMovement();
        UpdateAnimator();
    }

    void HandleInput()
    {
        // Reset if no touches
        if (Input.touchCount == 0)
        {
            if (isTouching) Debug.Log("Touch lost - resetting");
            ResetTouchState();
            return;
        }

        // Process touches
        foreach (Touch touch in Input.touches)
        {
            // Only track one finger at a time
            if (currentFingerId == null || touch.fingerId == currentFingerId)
            {
                ProcessTouch(touch);
                break; // Only process the first relevant touch
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
                    Debug.Log($"Touch began - finger: {touch.fingerId}");
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
                    Debug.Log($"Touch ended - finger: {touch.fingerId}");
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

    void UpdateMovement()
    {
        float targetForwardSpeed = isTouching ? maxSpeed : walkSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetForwardSpeed,
            (isTouching ? acceleration : deceleration) * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        float normalizedSpeed = currentSpeed / maxSpeed;
        animator.SetFloat("Speed", normalizedSpeed);
    }

    void FixedUpdate()
    {
        Vector3 newPosition = new Vector3(
            horizontalOffset,
            transform.position.y,
            transform.position.z + currentSpeed * Time.fixedDeltaTime
        );
        rb.MovePosition(newPosition);
    }
}