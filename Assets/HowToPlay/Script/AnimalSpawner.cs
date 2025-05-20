using UnityEngine;
using System.Collections.Generic;

public class AnimalSpawner : MonoBehaviour
{
    public GameObject animalPrefab;           // Assign your Animal prefab in inspector
    public Transform[] spawnPoints;   
    public float spawnDistance = 35f; // How far in front of the player to spawn// Add multiple spawn points if you want
    private int lastSpawnIndex = -1;

    public void SpawnAnimalInFront(Transform playerTransform)
    {
        if (animalPrefab == null)
        {
            Debug.LogWarning("Animal prefab not assigned in SpawnerManager!");
            return;
        }

        Vector3 spawnPos = playerTransform.position + playerTransform.forward * spawnDistance;
        Quaternion spawnRot = Quaternion.LookRotation(playerTransform.forward); // Face same direction
        Instantiate(animalPrefab, spawnPos, spawnRot);
    }
   
}
