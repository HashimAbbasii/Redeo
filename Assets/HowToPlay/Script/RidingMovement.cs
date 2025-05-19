using UnityEngine;

public class RidingMovement : MonoBehaviour
{
    public float rideMoveSpeed = 3f;
    public float horizontalBoundary = 3f;
    private bool isHolding = false;
    private float horizontalOffset = 0f;
    private Transform ridingOffset;

    private void Start()
    {
        ridingOffset = transform.Find("RidingOffset");
    }

    private void Update()
    {
        if (ridingOffset == null || ridingOffset.childCount == 0) return;

        HandleTouchInput();

        // Apply movement
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + horizontalOffset * Time.deltaTime, -horizontalBoundary, horizontalBoundary);
        transform.position = pos;
    }

    void HandleTouchInput()
    {
        isHolding = false;
        horizontalOffset = 0f;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            isHolding = true;

            if (touch.phase == TouchPhase.Moved)
            {
                horizontalOffset = touch.deltaPosition.x * Time.deltaTime * rideMoveSpeed;
            }
        }

#if UNITY_EDITOR
        // For editor testing
        isHolding = Input.GetKey(KeyCode.Space);
        horizontalOffset = Input.GetAxis("Horizontal") * rideMoveSpeed;
#endif

        // If player released finger, detach and jump
        if (!isHolding)
        {
            UnparentAndJump();
        }
    }

    void UnparentAndJump()
    {
        Transform player = ridingOffset.childCount > 0 ? ridingOffset.GetChild(0) : null;
        if (player == null) return;

        HowtoPlayController playerController = player.GetComponent<HowtoPlayController>();
        if (playerController)
        {
            player.SetParent(null);
            playerController.rb.isKinematic = false;
            playerController.transform.position += Vector3.up * 0.5f; // prevent overlap
            playerController.enabled = true; // re-enable player control
            this.enabled = false; // disable this script
            playerController.PrepareForJump();
        }
    }
}
