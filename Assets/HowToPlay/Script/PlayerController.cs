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
    public AnimalSpawner spawner; // Assign via inspector

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
    public float jumpHeight = 4f;
    public string sheepTag = "Animal";
    private bool hasJumpedOffAnimal = false;
    private GameObject lastJumpedFromAnimal = null;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    private float spawnCooldown = 8f; // 1 second between spawns
    private float lastSpawnTime = 0f;
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
            animator.SetTrigger("Jump");

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
        
        if (isRiding && !isJumping)
        {
            if (Time.time - lastSpawnTime > spawnCooldown)
            {
                spawner.SpawnAnimalInFront(transform);
                lastSpawnTime = Time.time;
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
        animator.SetBool("Riding",false);
        animator.SetBool("Jump",false);
        animator.SetBool("Death",true);
       
        //Time.timeScale = 0f;
        Debug.Log("Game Paused - Radius-based detection triggered");
    }

IEnumerator JumpArcAndSnap()
{
    Debug.Log("[JUMP START] Initializing jump sequence...");
    
    // Immediately clear parent and prepare for jump
    transform.SetParent(null);
    rb.isKinematic = false;
    isJumping = true;
    isRiding = false;
    animator.SetBool("Jump", true);
    animator.SetBool("Riding", false);

    Debug.Log($"[STATE] isJumping={isJumping}, isRiding={isRiding}, hasJumpedOffAnimal={hasJumpedOffAnimal}");

    // Slow motion effect
    float originalTimeScale = Time.timeScale;
    Time.timeScale = 0.3f;
    Time.fixedDeltaTime = 0.02f * Time.timeScale;
    Debug.Log("[TIME] Slow motion activated");

    Vector3 startPos = transform.position;
    Vector3 forwardDirection = transform.forward;
    
    // Calculate jump parameters
    float jumpDistance = 5f;
    float peakHeight = 2f;
    
    Vector3 endPos = startPos + (forwardDirection * jumpDistance);
    endPos.y = startPos.y;
    
    Debug.Log($"[JUMP PARAMS] Start: {startPos}, End: {endPos}, Direction: {forwardDirection}");

    float elapsed = 0f;
    bool snapped = false;
    GameObject targetAnimal = null;

    // Initial animal detection
    targetAnimal = GetSheepInRadius();
    if (targetAnimal != null)
    {
        Debug.Log($"[TARGET FOUND] Initial target: {targetAnimal.name} " + 
                 $"(Distance: {Vector3.Distance(transform.position, targetAnimal.transform.position):F2}m)");
        
        if (targetAnimal == lastJumpedFromAnimal)
        {
            Debug.Log("[TARGET REJECTED] Same as last jumped from animal");
            targetAnimal = null;
        }
    }
    else
    {
        Debug.Log("[TARGET] No initial target found");
    }

    while (elapsed < jumpDuration)
    {
        elapsed += Time.deltaTime / Time.timeScale;
        float normalizedTime = elapsed / jumpDuration;
        
        // Quadratic curve for arc
        float verticalProgress = 4 * normalizedTime * (1 - normalizedTime);
        float currentHeight = peakHeight * verticalProgress;
        
        Vector3 horizontalProgress = Vector3.Lerp(startPos, endPos, normalizedTime);
        
        Vector3 currentPos = new Vector3(
            horizontalProgress.x,
            startPos.y + currentHeight,
            horizontalProgress.z
        );
        
        transform.position = currentPos;

        Debug.DrawLine(startPos, currentPos, Color.green, 2f); // Visual debug

        // Snap window (30-70% of jump)
        if (!snapped && normalizedTime > 0.3f && normalizedTime < 0.7f)
        {
            // If we didn't find a target at start, try again
            if (targetAnimal == null)
            {
                targetAnimal = GetSheepInRadius();
                if (targetAnimal != null)
                {
                    Debug.Log($"[TARGET FOUND] Mid-air target: {targetAnimal.name} " +
                             $"(Distance: {Vector3.Distance(transform.position, targetAnimal.transform.position):F2}m)");
                }
            }

            if (targetAnimal != null && targetAnimal != lastJumpedFromAnimal)
            {
                Debug.Log($"[ATTEMPTING SNAP] To: {targetAnimal.name} at {normalizedTime*100:F0}% of jump");
                if (SnapToSheep(targetAnimal.transform))
                {
                    snapped = true;
                    Debug.Log("[SNAP SUCCESS] Mounted on new animal");
                    break;
                }
                else
                {
                    Debug.Log("[SNAP FAILED] Could not mount animal");
                }
            }
        }

        yield return null;
    }

    // Final attempt right before landing
    if (!snapped)
    {
        Debug.Log("[FINAL ATTEMPT] Checking for animals before landing...");
        targetAnimal = GetSheepInRadius();
        if (targetAnimal != null && targetAnimal != lastJumpedFromAnimal)
        {
            Debug.Log($"[FINAL TARGET] Found: {targetAnimal.name}");
            if (SnapToSheep(targetAnimal.transform))
            {
                snapped = true;
                Debug.Log("[LAST-MOMENT SNAP] Successfully mounted");
            }
        }
    }

    if (!snapped)
    {
        Debug.LogWarning("[JUMP FAILED] No valid target found. Game over!");
        PauseGame();
    }

    // Clean up
    Time.timeScale = originalTimeScale;
    Time.fixedDeltaTime = 0.02f;
    isJumping = false;
    hasJumpedOffAnimal = false;
    animator.SetBool("Jump", false);

    Debug.Log($"[JUMP END] Jump sequence completed. snapped={snapped}, isJumping={isJumping}");
}


    bool SnapToSheep(Transform sheep)
{
    Debug.Log("=== SNAP TO SHEEP PROCESS STARTED ===");
    Debug.Log($"Attempting to snap to sheep: {(sheep != null ? sheep.name : "NULL TRANSFORM")}");

    // Null check
    if (sheep == null)
    {
        Debug.LogError("Snap failed: Sheep transform is null!");
        return false;
    }

    // Same animal check
    if (sheep.gameObject == lastJumpedFromAnimal)
    {
        Debug.Log($"Snap rejected: {sheep.name} is the same animal we jumped from (lastJumpedFromAnimal)");
        return false;
    }
    else if (lastJumpedFromAnimal != null)
    {
        Debug.Log($"Last jumped from: {lastJumpedFromAnimal.name}, current target: {sheep.name} - OK to proceed");
    }

    // Animation states
    Debug.Log("Setting animation states: Jump->false, Riding->true");
    animator.SetBool("Jump", false);
    animator.SetBool("Riding", true);

    // Physics setup
    Debug.Log("Resetting physics - velocity to zero, kinematic to true");
    rb.velocity = Vector3.zero;
    rb.isKinematic = true;

    // Find riding point
    Debug.Log($"Looking for RidingOffset on {sheep.name}...");
    Transform ridingPoint = sheep.Find("RidingOffset");

    if (ridingPoint != null)
    {
        Debug.Log($"Found RidingOffset on {sheep.name}. Parenting player to it.");
        Debug.Log($"Before parenting - Player position: {transform.position}, RidingOffset position: {ridingPoint.position}");
        
        transform.SetParent(ridingPoint);
        transform.localPosition = Vector3.zero;
        Debug.Log($"After parenting - Local position: {transform.localPosition}, World position: {transform.position}");

        currentAnimal = sheep.GetComponent<AnimalController>();
        Debug.Log(currentAnimal != null 
            ? $"Found AnimalController on {sheep.name}" 
            : $"WARNING: No AnimalController on {sheep.name}");
    }
    else
    {
        Debug.LogWarning($"RidingOffset not found on {sheep.name}. Using fallback offset.");
        Debug.Log($"Before parenting - Player position: {transform.position}, Sheep position: {sheep.position}");
        
        transform.SetParent(sheep);
        transform.position = sheep.position + ridingOffset;
        Debug.Log($"After parenting - World position: {transform.position}");

        currentAnimal = sheep.GetComponent<AnimalController>();
        Debug.Log(currentAnimal != null 
            ? $"Found AnimalController on {sheep.name}" 
            : $"WARNING: No AnimalController on {sheep.name}");
    }

    // State management
    Debug.Log("Updating player states:");
    Debug.Log($"- isRiding: {isRiding} -> true");
    Debug.Log($"- isJumping: {isJumping} -> false");
    Debug.Log($"- hasJumpedOffAnimal: {hasJumpedOffAnimal} -> false");
    
    isRiding = true;
    isJumping = false;
    hasJumpedOffAnimal = false;

    Debug.Log($"Clearing lastJumpedFromAnimal (was: {lastJumpedFromAnimal?.name ?? "null"})");
    lastJumpedFromAnimal = null;

    // Final animation check
    Debug.Log("Final animation state check:");
    Debug.Log($"- Jump: {animator.GetBool("Jump")}");
    Debug.Log($"- Riding: {animator.GetBool("Riding")}");

    Debug.Log($"=== SUCCESSFULLY MOUNTED {sheep.name} ===");
    return true;
}

    public bool isRiding=false;
    GameObject GetSheepInRadius()
    {
        GameObject[] allSheep = GameObject.FindGameObjectsWithTag(sheepTag);
        GameObject closestSheep = null;
        float closestDistance = Mathf.Infinity;
    
        foreach (GameObject sheep in allSheep)
        {
            if (sheep == lastJumpedFromAnimal) 
            {
                Debug.Log($"[DETECT] Skipping last jumped animal: {sheep.name}");
                continue;
            }
        
            Vector3 directionToSheep = sheep.transform.position - transform.position;
            float distance = directionToSheep.magnitude;
            float dotProduct = Vector3.Dot(transform.forward, directionToSheep.normalized);
        
            Debug.DrawLine(transform.position, sheep.transform.position, 
                distance <= detectionRadius && dotProduct > 0.3f ? Color.green : Color.red, 
                0.5f);

            if (distance <= detectionRadius && dotProduct > 0.3f)
            {
                Debug.Log($"[DETECT] Valid sheep: {sheep.name} " +
                          $"(Distance: {distance:F2}m, Dot: {dotProduct:F2})");
            
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSheep = sheep;
                }
            }
        }

        if (closestSheep != null)
        {
            Debug.Log($"[DETECT] Closest sheep: {closestSheep.name} ({closestDistance:F2}m)");
        }
        else
        {
            Debug.Log("[DETECT] No valid sheep in radius");
        }

        return closestSheep;
    }
}
