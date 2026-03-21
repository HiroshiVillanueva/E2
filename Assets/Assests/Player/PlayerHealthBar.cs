using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/* * A. Functionality: Manages the player's health, updates the UI slider and text, handles saving/loading health between scenes, and restarts the game on death.
 * B. New component & functionality learned: Learned how to use PlayerPrefs to save health across scenes, and SceneManager to reload the game.
 * C. Problems encountered: The death animation wouldn't play if the scene restarted instantly.
 * D. What you have tried / not tried: I added a Coroutine delay (ReloadGameAfterDeath) before loading Scene 0 so the player can actually see the death animation finish before the game resets.
 * E. Other important developer notes: Make sure Scene 1 (Title Screen) is at Index 0 in Build Settings!
 */

public class PlayerHealth : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Check this box ONLY in Level 1 so the player always starts with full health!")]
    public bool isFirstLevel = false;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    private bool isDead = false;

    [Header("UI Elements")]
    public Slider healthSlider;
    public TextMeshProUGUI hpText;

    private Animator anim;
    private PlayerMovement2D movementScript;

    void Start()
    {
        anim = GetComponent<Animator>();
        movementScript = GetComponent<PlayerMovement2D>();

        // --- NEW START LOGIC ---
        if (isFirstLevel == true)
        {
            // If this is Level 1, force health to MAX and wipe any old saves from previous play sessions!
            currentHealth = maxHealth;
            PlayerPrefs.DeleteKey("SavedHealth");
            PlayerPrefs.Save();
        }
        else if (PlayerPrefs.HasKey("SavedHealth"))
        {
            // If it's NOT Level 1, and we have saved health, load it!
            currentHealth = PlayerPrefs.GetFloat("SavedHealth");
        }
        else
        {
            // Fallback just in case
            currentHealth = maxHealth;
        }

        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount, Transform attacker)
    {
        if (isDead) return;

        // Ignore damage if rolling OR if the hurt i-frames are active!
        if (movementScript != null && (movementScript.isRolling || movementScript.isInvincible))
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (anim != null) anim.SetTrigger("Hurt");

            // Call the Flicker and Knockback sequence!
            if (movementScript != null)
            {
                StartCoroutine(movementScript.HurtSequence(attacker));
            }
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        // Updates the visual bar
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Updates the text
        if (hpText != null)
        {
            hpText.text = "HP: " + currentHealth.ToString("0");
        }
    }

    // THE EXIT DOOR WILL CALL THIS BEFORE CHANGING SCENES
    public void SaveHealthForNextScene()
    {
        PlayerPrefs.SetFloat("SavedHealth", currentHealth);
        PlayerPrefs.Save();
        Debug.Log("Health saved at: " + currentHealth);
    }

    private void Die()
    {
        isDead = true;

        if (anim != null) anim.SetTrigger("Death");

        Debug.Log("Player has been defeated!");

        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // Start the timer to reload the game so the death animation has time to play
        StartCoroutine(ReloadGameAfterDeath());
    }

    // Coroutine to delay the scene load
    private System.Collections.IEnumerator ReloadGameAfterDeath()
    {
        // Wait 2 seconds (adjust this number to match your death animation length!)
        yield return new WaitForSeconds(2f);

        // Clear the saved health so they start with 100 HP on their next attempt
        PlayerPrefs.DeleteKey("SavedHealth");
        PlayerPrefs.Save();

        // Load the Title Screen (Scene Index 0 in Build Settings)
        SceneManager.LoadScene(0);
    }
}