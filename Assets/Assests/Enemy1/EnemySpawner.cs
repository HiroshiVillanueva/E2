using UnityEngine;

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Manages the periodic spawning of enemy prefabs at random locations. It tracks 
 * the total number of active enemies in the scene and stops spawning when a 
 * maximum cap is reached to keep the difficulty balanced.
 * * B. New component & functionality learned: 
 * - Learned how to use Arrays (Transform[]) to hold a list of multiple spawn points.
 * - Explored Random.Range() to randomly select an index from that array.
 * - Used GameObject.FindGameObjectsWithTag() to dynamically count how many enemies 
 * are currently alive in the scene.
 * * C. Problems encountered: 
 * - Without a cap, the game would spawn an infinite number of enemies, causing 
 * severe lag and making the level unbeatable. Added the maxEnemies check to fix this.
 * - Forgot to assign spawn points in the Inspector once, causing an "Index out of 
 * range" error. Added an array length check to safely catch this.
 * * D. What you have tried / not tried: 
 * - I tried spawning enemies strictly on a timer, but switched to conditional 
 * spawning based on the active enemy count so the player doesn't get overwhelmed.
 * - Have not tried using an Object Pool pattern instead of Instantiate/Destroy to 
 * save on memory overhead.
 * * E. Other important developer notes: 
 * - For this script to work properly, the enemy prefab MUST be assigned the 
 * "Enemy" tag in the Unity Inspector.
 * ============================================================================== */

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    // An array to hold multiple possible spawn locations
    public Transform[] spawnPoints;

    [Header("Limits")]
    public int maxEnemies = 6; // The maximum number of enemies allowed at once!

    [Header("Timing")]
    public float timeBetweenSpawns = 3f;
    private float spawnTimer;

    void Start()
    {
        // Initialize the timer at 0 when the level begins
        spawnTimer = 0f;
    }

    void Update()
    {
        // Time.deltaTime adds the time passed since the last frame
        spawnTimer += Time.deltaTime;

        // When the timer reaches our target interval, check if we can spawn
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

            // Reset the timer either way so it starts counting to 3 seconds again
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        // Safety check to prevent errors if the array is empty
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned to the EnemySpawner!");
            return;
        }

        // Pick a random number between 0 and the total number of spawn points
        int randomIndex = Random.Range(0, spawnPoints.Length);
        // Grab the specific Transform component from the array using that random number
        Transform chosenSpawnPoint = spawnPoints[randomIndex];

        // Create a clone of the enemy prefab at the chosen location
        Instantiate(enemyPrefab, chosenSpawnPoint.position, chosenSpawnPoint.rotation);
    }
}