using UnityEngine;

public class RadiusVisualizer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float radius = 2f;
    public int segments = 100;
    public Color ridingColor = Color.cyan;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = ridingColor;
        lineRenderer.endColor = ridingColor;
        lineRenderer.enabled = false;

        DrawCircle();
    }

    void DrawCircle()
    {
        lineRenderer.positionCount = segments + 1;
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, 0f, z));
            angle += 360f / segments;
        }
    }

    public void SetActive(bool state)
    {
        lineRenderer.enabled = state;
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        DrawCircle();
    }
}
