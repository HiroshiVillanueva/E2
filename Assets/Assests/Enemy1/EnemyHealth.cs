using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 30f;
    private float currentHealth;

    // --- RE-ADDED: The Padlock to prevent double-kills! ---
    private bool isDead = false;

    [Header("UI Elements")]
    public Slider healthSlider;

    private Animator anim;
    private EnemyPatrol patrolScript;
    private EnemyShooter shooterScript;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        patrolScript = GetComponent<EnemyPatrol>();
        shooterScript = GetComponent<EnemyShooter>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damageAmount, Transform attacker = null)
    {
        // --- RE-ADDED: If they are already dead, ignore the hit entirely! ---
        if (isDead) { return; }

        currentHealth -= damageAmount;

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (anim != null) anim.SetTrigger("Hurt");

            if (shooterScript != null)
            {
                shooterScript.StopAllCoroutines();
            }

            if (patrolScript != null && attacker != null)
            {
                patrolScript.enabled = true;
                patrolScript.StopAllCoroutines();
                StartCoroutine(patrolScript.ApplyKnockback(attacker));
            }
        }
    }

    private void EnableShooter() { if (shooterScript != null) shooterScript.enabled = true; }

    private void Die()
    {
        // --- RE-ADDED: Lock the door so they can never die twice! ---
        isDead = true;

        if (anim != null) anim.SetTrigger("Death");

        if (patrolScript != null) patrolScript.enabled = false;
        if (shooterScript != null) shooterScript.enabled = false;

        // --- THE FIX: Tell the UI Manager to add the kill! ---
        if (UIManager.instance != null)
        {
            UIManager.instance.AddKill();
        }
        // -----------------------------------------------------

        Destroy(gameObject, 0.33f);
    }
}