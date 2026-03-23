using UnityEngine;
using UnityEngine.UI; // Required to manipulate the Slider UI element

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Manages the health, damage processing, and death state of enemy characters. 
 * Updates a floating health bar, triggers hurt animations and knockback, and 
 * notifies the UIManager when the enemy is defeated to increment the kill count.
 * * B. New component & functionality learned: 
 * - Learned how to use UI Sliders in World Space to create floating health bars 
 * above the enemy's head.
 * - Learned how to use boolean "locks" (like isDead) to prevent methods from 
 * firing multiple times in a single frame.
 * * C. Problems encountered: 
 * - When the player used a heavy punch, the hitbox would stay active for several 
 * frames. If the first frame killed the enemy, the second frame would hit the 
 * "dead" enemy and trigger the Die() method again, giving the player two kills 
 * for one enemy. The isDead boolean padlock fixed this.
 * * D. What you have tried / not tried: 
 * - Tried destroying the GameObject instantly upon reaching 0 health, but it looked 
 * jarring. Instead, I added a 0.33-second delay to Destroy() so the death animation 
 * has time to play out.
 * - Have not tried making enemies drop health pickups or score items when they die.
 * * E. Other important developer notes: 
 * - This script acts as a central hub for the enemy. When taking damage, it needs 
 * to reach into the EnemyShooter and EnemyPatrol scripts to interrupt their 
 * current actions (like stopping a charge-up sequence).
 * ============================================================================== */

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 30f;
    private float currentHealth;

    // --- RE-ADDED: The Padlock to prevent double-kills! ---
    private bool isDead = false;

    [Header("UI Elements")]
    public Slider healthSlider;

    // References to other components on this specific enemy
    private Animator anim;
    private EnemyPatrol patrolScript;
    private EnemyShooter shooterScript;

    void Start()
    {
        // Initialize health and component references
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        patrolScript = GetComponent<EnemyPatrol>();
        shooterScript = GetComponent<EnemyShooter>();

        // Set up the floating health bar
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // Called primarily by the PlayerMovement2D script when a punch hitbox connects
    public void TakeDamage(float damageAmount, Transform attacker = null)
    {
        // --- RE-ADDED: If they are already dead, ignore the hit entirely! ---
        if (isDead) { return; }

        currentHealth -= damageAmount;

        // Update the visual health bar
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Trigger flinch animation
            if (anim != null) anim.SetTrigger("Hurt");

            // Interrupt any shooting wind-up sequences
            if (shooterScript != null)
            {
                shooterScript.StopAllCoroutines();
            }

            // Interrupt patrol and apply physical knockback
            if (patrolScript != null && attacker != null)
            {
                patrolScript.enabled = true;
                patrolScript.StopAllCoroutines();
                StartCoroutine(patrolScript.ApplyKnockback(attacker));
            }
        }
    }

    // Helper method to turn the shooter script back on after recovering from a hit
    private void EnableShooter() { if (shooterScript != null) shooterScript.enabled = true; }

    // Handles the death sequence
    private void Die()
    {
        // --- RE-ADDED: Lock the door so they can never die twice! ---
        isDead = true;

        if (anim != null) anim.SetTrigger("Death");

        // Immediately disable AI behaviors so a dying enemy doesn't keep shooting or walking
        if (patrolScript != null) patrolScript.enabled = false;
        if (shooterScript != null) shooterScript.enabled = false;

        // --- THE FIX: Tell the UI Manager to add the kill! ---
        // Accesses the Singleton to increment the mission progress globally
        if (UIManager.instance != null)
        {
            UIManager.instance.AddKill();
        }
        // -----------------------------------------------------

        // Destroy the object after a brief delay to allow the death animation to play
        Destroy(gameObject, 0.33f);
    }
}