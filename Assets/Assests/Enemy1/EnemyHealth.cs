using UnityEngine;
using UnityEngine.UI; 

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("UI Elements")]
    public Slider healthSlider; 

    // NEW: Connecting the other components
    private Animator anim;
    private EnemyPatrol patrolScript;
    private EnemyMelee meleeScript;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Grab the components
        anim = GetComponent<Animator>();
        patrolScript = GetComponent<EnemyPatrol>();
        meleeScript = GetComponent<EnemyMelee>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // NEW: Added "Transform attacker" so we know which way to fly!
    public void TakeDamage(float damageAmount, Transform attacker = null)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage!");

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 1. Play the Flinch Animation
            if (anim != null) anim.SetTrigger("Hurt");

            // 2. Interrupt their attack! (If they were winding up a punch, cancel it)
            if (meleeScript != null) meleeScript.StopAllCoroutines();

            // 3. Push them backwards!
            if (patrolScript != null && attacker != null)
            {
                // We MUST stop the patrol's old routines before starting a knockback
                patrolScript.StopAllCoroutines(); 
                StartCoroutine(patrolScript.ApplyKnockback(attacker));
            }
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has been defeated!");

        // Play the death animation
        if (anim != null) anim.SetTrigger("Death");

        // Turn off their brain and legs so they fall to the ground dead
        if (patrolScript != null) patrolScript.enabled = false;
        if (meleeScript != null) meleeScript.enabled = false;

        if (UIManager.instance != null)
        {
            UIManager.instance.AddKill();
        }

        // Destroy the body after 2 seconds instead of instantly
        Destroy(gameObject, 0.33f);
    }
}