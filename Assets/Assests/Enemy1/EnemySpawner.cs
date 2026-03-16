using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    [Header("Limits")]
    public int maxEnemies = 6; // The maximum number of enemies allowed at once!

    [Header("Timing")]
    public float timeBetweenSpawns = 3f;
    private float spawnTimer;

    void Start()
    {
        spawnTimer = 0f;
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= timeBetweenSpawns)
        {
            // Count how many objects currently have the "Enemy" tag
            int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

            // Only spawn a new enemy if we have less than our maximum
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
            else
            {
                Debug.Log("Max enemies reached! Waiting for one to be defeated.");
            }

            // Reset the timer either way
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned to the EnemySpawner!");
            return;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];

        Instantiate(enemyPrefab, chosenSpawnPoint.position, chosenSpawnPoint.rotation);
    }
}