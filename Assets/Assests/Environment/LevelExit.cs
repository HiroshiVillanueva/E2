using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Needed for Coroutines

public class LevelExit : MonoBehaviour
{
    [Header("Level Loading")]
    public string nextLevelName;
    public bool isFinalLevel = false;

    [Header("Transitions")]
    public Animator transitionAnimator; // Drag your Wipe Image Animator here
    public float transitionTime = 0.2f;    // Match this to the length of your animation

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CheckAndStartTransition(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            CheckAndStartTransition(collider.gameObject);
        }
    }

    private void CheckAndStartTransition(GameObject playerObject)
    {
        // First, check if mission is complete
        if (UIManager.instance != null && UIManager.instance.isMissionComplete)
        {
            StartCoroutine(LoadLevelWithWipe(playerObject));
        }
        else
        {
            Debug.Log("The exit is locked! Defeat all enemies first.");
        }
    }

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
            if (isFinalLevel)
            {
                PlayerPrefs.DeleteKey("SavedHealth");
                PlayerPrefs.Save();
            }
            else
            {
                playerHealth.SaveHealthForNextScene();
            }
        }

        // 3. Wait for the animation to finish
        yield return new WaitForSeconds(transitionTime);

        // 4. Load the scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelName);
    }
}