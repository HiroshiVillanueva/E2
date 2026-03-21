using UnityEngine;
using UnityEngine.SceneManagement; // Needed to load Level 2!

public class LevelExit : MonoBehaviour
{
    [Header("Level Loading")]
    public string nextLevelName = "Level 2"; // Type your EXACT Level 2 scene name in the Inspector!

    // This runs the exact moment something bumps into the wall's collider
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Check if the object that bumped into us is the Player
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. Check if the UIManager says the mission is complete
            if (UIManager.instance != null && UIManager.instance.isMissionComplete == true)
            {
                // Unfreeze time just in case, and load Level 2!
                Time.timeScale = 1f;
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                // Optional: Print a message to the console if they try to leave early
                Debug.Log("The door is locked! Defeat all enemies first.");
            }
        }
    }
}