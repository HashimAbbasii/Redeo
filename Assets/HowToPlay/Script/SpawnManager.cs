using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static HowtoPlayController;


public class SpawnManager : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private GameObject[] animalPrefabs; // Assign in Inspector
    [SerializeField] private float spawnDelay = 5f;
    [SerializeField] private float minSpawnDistance = 15f;
    [SerializeField] private float maxSpawnDistance = 25f;
    [SerializeField] private float spawnAngleVariance = 30f; // Degrees left/right
    [SerializeField] private int maxActiveAnimals = 10;

    [Header("References")]
    [SerializeField] private Transform player; // Assign player transform
    [SerializeField] private HowtoPlayController playerMovement; // For hold check

    private List<Queue<GameObject>> animalPools;
    private List<GameObject> activeAnimals;
    private float nextSpawnTime;
    public float horizontalBoundary = 10f;

    void Start()
    {
        InitializePools();
        activeAnimals = new List<GameObject>();
        playerMovement=FindObjectOfType<HowtoPlayController>();
    }

    void Update()
    {
        if (ShouldSpawn())
        {
            Debug.Log("Its Spawn or not");
            SpawnAnimal();
            nextSpawnTime = Time.time + spawnDelay;
        }
        CleanupFarAnimals();
    }

    // ===== CORE FUNCTIONALITY =====
    private void InitializePools()
    {
        animalPools = new List<Queue<GameObject>>();
        foreach (var prefab in animalPrefabs)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            // Preload 3 instances per animal type
            for (int i = 0; i < 3; i++)
            {
                GameObject animal = Instantiate(prefab);
                animal.SetActive(false);
                pool.Enqueue(animal);
            }
            animalPools.Add(pool);
        }
    }

    private bool ShouldSpawn()
    {
        bool canSpawn = playerMovement != null && 
                        playerMovement.IsHolding && 
                        Time.time > nextSpawnTime && 
                        activeAnimals.Count < maxActiveAnimals;
    
        if (!canSpawn)
        {
            Debug.Log($"Can't spawn. Conditions - " +
                      $"HasRef:{playerMovement != null}, " +
                      $"Holding:{playerMovement?.IsHolding}, " +
                      $"Time:{Time.time > nextSpawnTime}, " +
                      $"Capacity:{activeAnimals.Count < maxActiveAnimals}");
        }
    
        return canSpawn;
    }

    private void SpawnAnimal()
    {
       
        int randomIndex = Random.Range(0, animalPrefabs.Length);
        GameObject animal = GetAnimalFromPool(randomIndex);
        if(PlayerState.Jumping == playerMovement.currentState  && PlayerState.Riding == playerMovement.currentState)
        {
            animal.SetActive(true);
            activeAnimals.Add(animal);return;
        }
        // Position ahead of player
        float spawnZ = player.position.z + Random.Range(minSpawnDistance, maxSpawnDistance); 
        float spawnX = Random.Range(-horizontalBoundary, horizontalBoundary);
        Vector3 spawnPos = new Vector3(spawnX, 0, spawnZ);
    
        animal.transform.position = spawnPos;
        animal.transform.rotation = Quaternion.LookRotation(Vector3.forward);
    
        // Enable and let AnimalMovement handle the rest
        animal.SetActive(true); 
        activeAnimals.Add(animal);
        
    }

    private Vector3 CalculateSpawnPosition()
    {
        // Forward direction with random left/right angle
        float randomAngle = Random.Range(-spawnAngleVariance, spawnAngleVariance);
        Vector3 spawnDir = Quaternion.Euler(0, randomAngle, 0) * player.forward;

        // Random distance along this direction
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        return player.position + spawnDir * distance;
    }

    // ===== POOL MANAGEMENT =====
    private GameObject GetAnimalFromPool(int prefabIndex)
    {
        if (animalPools[prefabIndex].Count > 0)
        {
            return animalPools[prefabIndex].Dequeue();
        }
        else
        {
            // Instantiate new if pool is empty
            GameObject newAnimal = Instantiate(animalPrefabs[prefabIndex]);
            newAnimal.SetActive(false);
            return newAnimal;
        }
    }

    public void ReturnAnimalToPool(GameObject animal)
    {
        animal.SetActive(false);
        for (int i = 0; i < animalPrefabs.Length; i++)
        {
            if (animal.name.StartsWith(animalPrefabs[i].name))
            {
                animalPools[i].Enqueue(animal);
                activeAnimals.Remove(animal);
                break;
            }
        }
    }

    // ===== MAINTENANCE =====
    private void CleanupFarAnimals()
    {
        for (int i = activeAnimals.Count - 1; i >= 0; i--)
        {
            float distBehind = player.position.z - activeAnimals[i].transform.position.z;
            if (distBehind > maxSpawnDistance * 1.5f)
            {
                ReturnAnimalToPool(activeAnimals[i]);
            }
        }
    }

    private void PlaySpawnVFX(Vector3 position)
    {
        // Implement your preferred VFX system here
        // Example: Instantiate(prefab, position, Quaternion.identity);
    }

    // ===== DEBUG VISUALIZATION =====
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        Gizmos.color = Color.green;
        // Draw spawn area boundaries
        Vector3 leftBound = Quaternion.Euler(0, -spawnAngleVariance, 0) * player.forward * maxSpawnDistance;
        Vector3 rightBound = Quaternion.Euler(0, spawnAngleVariance, 0) * player.forward * maxSpawnDistance;
        
        Gizmos.DrawLine(player.position, player.position + leftBound);
        Gizmos.DrawLine(player.position, player.position + rightBound);
        Gizmos.DrawWireSphere(player.position + player.forward * minSpawnDistance, 1f);
        Gizmos.DrawWireSphere(player.position + player.forward * maxSpawnDistance, 1f);
    }
}
