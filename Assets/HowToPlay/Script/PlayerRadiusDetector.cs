using UnityEngine;

public class PlayerRadiusDetector : MonoBehaviour
{
    public float detectionRadius = 5f;
    public LineRenderer lineRenderer; // Optional, for visual radius
    public string sheepTag = "Animal";

    private PlayerController playerController;
    private bool gamePaused = false;


    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (lineRenderer) lineRenderer.loop = true; // Optional: smooth circle
    }

    void Update()
    {
        UpdateRadiusCircle();     // Always draw at player position
       // CheckForSheepInRadius();  // Always check per frame
    }

    void UpdateRadiusCircle()
    {
        if (!lineRenderer) return;

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

    void CheckForSheepInRadius()
    {
      //  if (gamePaused || !playerController.IsRunning()) return;

        GameObject[] allSheep = GameObject.FindGameObjectsWithTag(sheepTag);
        foreach (GameObject sheep in allSheep)
        {
            float distance = Vector3.Distance(transform.position, sheep.transform.position);
            if (distance <= detectionRadius)
            {
                PauseGame();
                break;
            }
        }
    }



   

    void PauseGame()
    {
        Time.timeScale = 0f;
        gamePaused = true;
        Debug.Log("Paused: Sheep entered radius while running");
        // Optional: Show pause menu or event
    }
}
