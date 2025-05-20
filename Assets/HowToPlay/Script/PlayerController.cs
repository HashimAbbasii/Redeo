using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private Vector2 dragStartPos;
    private float dragSensitivity = 0.5f; // Tweak for faster/slower turn
    private AnimalController currentAnimal;
    public float walkSpeed = 2f;
    public float runSpeed = 10f;

    private Rigidbody rb;
    private Animator animator;

    private float currentSpeed = 0f;
    private float targetBlendSpeed = 0.3f;

    private bool isRunning = false;
    public float detectionRadius = 5f;
    public Vector3 ridingOffset = new Vector3(0, 1, 0);
    // private bool isRunning = false;
    public float jumpDuration = 2f;
    private bool isJumping = false;
    public float jumpHeight = 2f;
    public string sheepTag = "Animal";
    private bool hasJumpedOffAnimal = false;
    private GameObject lastJumpedFromAnimal = null;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        UpdateAnimator();

        // Don't allow new jumps if already jumping
        if (isJumping) return;

        // 🚀 CASE 1: Riding and player RELEASES hold — jump off current animal
        if (isRiding && !isRunning && !hasJumpedOffAnimal)
        {
            Debug.Log("Released hold while riding. Jumping to next animal...");
            hasJumpedOffAnimal = true; // ✅ Mark that jump has been triggered
            lastJumpedFromAnimal = currentAnimal != null ? currentAnimal.gameObject : null;
            isRiding = false;
            isJumping = true;

            transform.SetParent(null);
            rb.isKinematic = false;
            StartCoroutine(JumpArcAndSnap());
        }

        // 🐑 CASE 2: Not riding and player is holding — find target sheep and jump
        if (!isRiding && isRunning)
        {
            GameObject sheep = GetSheepInRadius();
            if (sheep != null)
            {
                StartCoroutine(JumpArcAndSnap());
            }
        }
    }
    void FixedUpdate()
    {
        if (isRiding && currentAnimal != null)
        {
            // When riding, forward movement is controlled through the animal
            float inputSpeed = isRunning ? runSpeed : walkSpeed;
            currentAnimal.MoveForward(inputSpeed);
        }
        else
        {
            // Regular player movement
            MovePlayer();
        }
    }
    void HandleInput()
    {
#if UNITY_EDITOR
        isRunning = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

        if (isRiding && Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
        }
        else if (isRiding && Input.GetMouseButton(0))
        {
            Vector2 dragDelta = (Vector2)Input.mousePosition - dragStartPos;
            float horizontalDelta = dragDelta.x * dragSensitivity * Time.deltaTime;
            currentAnimal?.Strafe(horizontalDelta); // ✅ Call strafe method
            dragStartPos = Input.mousePosition;
        }
#else
    isRunning = Input.touchCount > 0;

    if (isRiding && Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            dragStartPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            Vector2 dragDelta = touch.position - dragStartPos;
            float horizontalDelta = dragDelta.x * dragSensitivity * Time.deltaTime;
            currentAnimal?.Strafe(horizontalDelta);
            dragStartPos = touch.position;
        }
    }
#endif

        targetBlendSpeed = isRunning ? 1f : 0.3f;
    }
    void UpdateAnimator()
    {
        if (isRiding)
        {
            animator.SetFloat("Speed", 0);  // Stop movement blend
            animator.SetBool("Riding", true); // Ensure riding animation is active
            return;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetBlendSpeed, Time.deltaTime * 3f);
        animator.SetFloat("Speed", currentSpeed);
    }

    void MovePlayer()
    {
        float speed = Mathf.Lerp(walkSpeed, runSpeed, currentSpeed);
        rb.velocity = transform.forward * speed;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game Paused - Radius-based detection triggered");
    }

    IEnumerator JumpArcAndSnap()
    {
        transform.SetParent(null);      // Ensure not parented to previous animal
        rb.isKinematic = false;         // Re-enable physics
        isJumping = true;
       

        // Trigger jump animation
      //  animator.SetBool("IsJumping", true);

        // Slow motion start
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + transform.forward * 3f;
        float elapsed = 0f;
        float halfDuration = jumpDuration / 2f;
        bool snapped = false;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime / Time.timeScale;

            float normalizedTime = elapsed / jumpDuration;
            float height = 4 * jumpHeight * normalizedTime * (1 - normalizedTime);
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, normalizedTime);
            currentPos.y += height;
            transform.position = currentPos;

            if (!snapped && elapsed >= halfDuration)
            {
                GameObject sheep = GetSheepInRadius();
                if (sheep != null && sheep != lastJumpedFromAnimal)
                {
                    SnapToSheep(sheep.transform);
                    snapped = true;

                    Time.timeScale = originalTimeScale;
                    Time.fixedDeltaTime = 0.02f;
                    break;
                }
            }

            yield return null;
            hasJumpedOffAnimal = false;
        }

        if (!snapped)
        {
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = 0.02f;

            Debug.Log("Missed jump. Game Over!");
            PauseGame(); // ✅ Game over here!
        }

        // End jump animation
        // animator.SetBool("IsJumping", false);

        isJumping = false;
    }


    bool SnapToSheep(Transform sheep)
    {
        if (sheep.gameObject == lastJumpedFromAnimal)
        {
            Debug.Log("Skipping re-attachment to the same animal.");
            return false;
        }

        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        Transform ridingPoint = sheep.Find("RidingOffset");

        if (ridingPoint != null)
        {
            transform.SetParent(ridingPoint);
            transform.localPosition = Vector3.zero;
            currentAnimal = sheep.GetComponent<AnimalController>();
        }
        else
        {
            Debug.LogWarning("RidingOffset not found on sheep. Using fallback offset.");
            transform.SetParent(sheep);
            transform.position = sheep.position + ridingOffset;
            currentAnimal = sheep.GetComponent<AnimalController>();
        }

        isRiding = true;
        isJumping = false;
        hasJumpedOffAnimal = false;
        lastJumpedFromAnimal = null;

        animator.SetBool("Riding", true);
        Debug.Log("Mounted on animal and riding started.");

        return true;
    }

    public bool isRiding=false;
    GameObject GetSheepInRadius()
    {
        GameObject[] allSheep = GameObject.FindGameObjectsWithTag(sheepTag);
        foreach (GameObject sheep in allSheep)
        {
            float distance = Vector3.Distance(transform.position, sheep.transform.position);
            if (distance <= detectionRadius)
            {
                return sheep;
            }
        }
        return null;
    }

}
