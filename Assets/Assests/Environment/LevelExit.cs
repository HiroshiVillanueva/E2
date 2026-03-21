using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Level Loading")]
    [Tooltip("Type the exact name of the scene this door should load next.")]
    public string nextLevelName;

    [Tooltip("Check this box ONLY for the stairs in Level 3 that go to the Ending Scene.")]
    public bool isFinalLevel = false;

    // 1. THIS HANDLES SOLID WALLS (Like Level 1 & 2)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryLoadNextLevel(collision.gameObject);
        }
    }

    // 2. --- NEW: THIS HANDLES PASS-THROUGH ZONES (Like Level 3) ---
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            TryLoadNextLevel(collider.gameObject);
        }
    }

    // 3. The actual logic to change scenes (shared by both methods above)
    private void TryLoadNextLevel(GameObject playerObject)
    {
        if (UIManager.instance != null && UIManager.instance.isMissionComplete == true)
        {
            PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (isFinalLevel == true)
                {
                    // If this is the end of the game, delete the saved health!
                    PlayerPrefs.DeleteKey("SavedHealth");
                    PlayerPrefs.Save();
                }
                else
                {
                    // Normal level transition: save the health!
                    playerHealth.SaveHealthForNextScene();
                }
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.Log("The exit is locked! Defeat all enemies first.");
        }
    }
}