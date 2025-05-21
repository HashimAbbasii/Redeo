using UnityEngine;

public class PlayerRadiusDetector : MonoBehaviour
{
    public float detectionRadius = 5f;
    public LineRenderer lineRenderer; // Optional, for visual radius
    public string sheepTag = "Animal";

    private PlayerController playerController;
    private bool gamePaused = false;
    private bool hasValidTarget = false;
    public Material greenColor;
    public Material RedColor;


    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (lineRenderer)
        {
            lineRenderer.loop = true;
            lineRenderer.enabled = false; // Start with disabled
        }
    }

    void Update()
    {
        UpdateRadiusCircle();     // Always draw at player position
       // CheckForSheepInRadius();  // Always check per frame
    }

    public void ToggleRadiusDisplay(bool show)
    {
        if (lineRenderer)
        {
            lineRenderer.enabled = show;
            UpdateRadiusColor();
        }
    }
    public void UpdateRadiusVisuals()
    {
        if (!lineRenderer || !lineRenderer.enabled) return;

        hasValidTarget = CheckForSheepInRadius();
        UpdateRadiusColor();
        UpdateRadiusCircle();
    }
    void UpdateRadiusColor()
    {
        lineRenderer.material = hasValidTarget ? greenColor : RedColor;
        lineRenderer.material = hasValidTarget ? greenColor : RedColor;
    }
    bool CheckForSheepInRadius()
    {
        GameObject[] allSheep = GameObject.FindGameObjectsWithTag(sheepTag);

        foreach (GameObject sheep in allSheep)
        {
            if (sheep == playerController.lastJumpedFromAnimal) continue;

            Vector3 directionToSheep = sheep.transform.position - transform.position;
            float distance = directionToSheep.magnitude;
            float dotProduct = Vector3.Dot(transform.forward, directionToSheep.normalized);

            if (distance <= detectionRadius && dotProduct > 0.3f)
            {
                return true;
            }
        }
        return false;
    }

    void UpdateRadiusCircle()
    {
        int segments = 50;
        lineRenderer.positionCount = segments + 1;
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * detectionRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * detectionRadius;
            Vector3 point = new Vector3(x, 0.01f, z) + transform.position;
            lineRenderer.SetPosition(i, point);
            angle += 360f / segments;
        }
    }


}
