using UnityEngine;

public class OverlapChecker : MonoBehaviour
{
    public GameObject sphere;
    public GameObject cuboid;

    public Material overlapMaterial;
    public Material defaultMaterial;
    public Material selectedMaterial;

    private Renderer sphereRenderer;
    private Renderer cuboidRenderer;

    private GameObject selectedObject;
    private Plane dragPlane;
    private Vector3 offset;

    void Start()
    {
        sphereRenderer = sphere.GetComponent<Renderer>();
        cuboidRenderer = cuboid.GetComponent<Renderer>();
    }

    void Update()
    {
        HandleSelectionAndDragging();
        UpdateMaterials();
    }

    void HandleSelectionAndDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == sphere || hit.collider.gameObject == cuboid)
                {
                    selectedObject = hit.collider.gameObject;
                    dragPlane = new Plane(Vector3.up, selectedObject.transform.position);
                    dragPlane.Raycast(ray, out float enter);
                    offset = selectedObject.transform.position - ray.GetPoint(enter);
                }
            }
        }

        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                selectedObject.transform.position = ray.GetPoint(enter) + offset;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedObject = null;
        }
    }

    void UpdateMaterials()
    {
        bool overlapping = DoesOverlap(sphere, cuboid);

        // If something is selected
        if (selectedObject != null)
        {
            if (overlapping)
            {
                sphereRenderer.material = overlapMaterial;
                cuboidRenderer.material = overlapMaterial;
            }
            else
            {
                if (selectedObject == sphere)
                {
                    sphereRenderer.material = selectedMaterial;
                    cuboidRenderer.material = defaultMaterial;
                }
                else if (selectedObject == cuboid)
                {
                    cuboidRenderer.material = selectedMaterial;
                    sphereRenderer.material = defaultMaterial;
                }
            }
        }
        else
        {
            // Nothing selected
            if (overlapping)
            {
                sphereRenderer.material = overlapMaterial;
                cuboidRenderer.material = overlapMaterial;
            }
            else
            {
                sphereRenderer.material = defaultMaterial;
                cuboidRenderer.material = defaultMaterial;
            }
        }
    }


    bool DoesOverlap(GameObject sphere, GameObject cuboid)
    {
        Vector3 sphereCenter = sphere.transform.position;
        float sphereRadius = 0.5f * sphere.transform.lossyScale.x;

        Vector3 localSphereCenter = cuboid.transform.InverseTransformPoint(sphereCenter);
        Vector3 halfExtents = cuboid.transform.localScale * 0.5f;

        Vector3 closestPoint = new Vector3(
            Mathf.Clamp(localSphereCenter.x, -halfExtents.x, halfExtents.x),
            Mathf.Clamp(localSphereCenter.y, -halfExtents.y, halfExtents.y),
            Mathf.Clamp(localSphereCenter.z, -halfExtents.z, halfExtents.z)
        );

        float distance = Vector3.Distance(localSphereCenter, closestPoint);
        return distance <= sphereRadius;
    }
}
