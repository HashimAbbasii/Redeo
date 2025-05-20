using UnityEngine;

public class AnimalController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 3f;
    public Transform ridingPoint;
    public float rotationSpeed = 100f;
    public float strafeSpeed = 15f;
    public float strafeLimit = 13f; // Max left/right distance

    public void MoveForward(float inputSpeed)
    {
        transform.Translate(Vector3.forward * inputSpeed * Time.fixedDeltaTime);
    }
    public void Rotate(float horizontalInput)
    {
        float rotationAmount = horizontalInput * rotationSpeed;
        transform.Rotate(Vector3.up * rotationAmount * Time.deltaTime);
    }
    public void Strafe(float horizontalDelta)
    {
        Vector3 targetPosition = transform.position + new Vector3(horizontalDelta * strafeSpeed, 0f, 0f);

        // Clamp X position within limits
        targetPosition.x = Mathf.Clamp(targetPosition.x, -strafeLimit, strafeLimit);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f); // Smooth
    }
}
