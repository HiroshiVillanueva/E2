using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Needed for Coroutines

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Manages the end-of-level sequence. It checks if the player has completed the 
 * level's objectives, triggers a screen wipe animation, saves the player's current 
 * health to carry over, and smoothly loads the next scene.
 * * B. New component & functionality learned: 
 * - Learned how to use PlayerPrefs to save data (like player health) persistently 
 * across different scenes.
 * - Used both OnCollisionEnter2D and OnTriggerEnter2D to make the exit zone flexible.
 * * C. Problems encountered: 
 * - Initially, the scene loaded instantly before the wipe animation could play. 
 * Using a Coroutine and yield return new WaitForSeconds fixed the timing issue.
 * * D. What you have tried / not tried: 
 * - I tried hard-coding the next scene index, but switched to a public string variable 
 * (nextLevelName) to easily set the destination in the Unity Inspector.
 * - Have not tried adding an unlocking particle effect when the objective is met yet.
 * * E. Other important developer notes: 
 * - The UIManager.instance.isMissionComplete flag prevents players from skipping 
 * the level without defeating enemies or completing the objective.
 * ============================================================================== */

public class LevelExit : MonoBehaviour
{
    [Header("Level Loading")]
    public string nextLevelName;
    public bool isFinalLevel = false;

    [Header("Transitions")]
    public Animator transitionAnimator; // Drag your Wipe Image Animator here
    public float transitionTime = 0.2f;    // Match this to the length of your animation

    // Detects when the player physically bumps into the exit (if Is Trigger is FALSE)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CheckAndStartTransition(collision.gameObject);
        }
    }

    // Detects when the player walks through the exit zone (if Is Trigger is TRUE)
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            CheckAndStartTransition(collider.gameObject);
        }
    }

    // Verifies the objective is complete before allowing the player to leave
    private void CheckAndStartTransition(GameObject playerObject)
    {
        // First, check if mission is complete via the UIManager singleton
        if (UIManager.instance != null && UIManager.instance.isMissionComplete)
        {
            StartCoroutine(LoadLevelWithWipe(playerObject));
        }
        else
        {
            Debug.Log("The exit is locked! Defeat all enemies first.");
        }
    }

    // Coroutine to handle the sequence of animation, saving, and loading
    private IEnumerator LoadLevelWithWipe(GameObject playerObject)
    {
        // 1. Play the animation
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("StartWipe");
        }

        // 2. Handle health saving logic
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // If this is the final level, clear the saved health so new games start fresh
            if (isFinalLevel)
            {
                PlayerPrefs.DeleteKey("SavedHealth");
                PlayerPrefs.Save();
            }
            // Otherwise, save the current health so it carries into the next scene
            else
            {
                playerHealth.SaveHealthForNextScene();
            }
        }

        // 3. Wait for the animation to finish before snapping to the next scene
        yield return new WaitForSeconds(transitionTime);

        // 4. Load the scene and ensure time is running normally
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelName);
    }
}