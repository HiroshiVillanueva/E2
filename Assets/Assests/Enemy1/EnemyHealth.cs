using UnityEngine;
using UnityEngine.UI; 

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 30f;
    private float currentHealth;

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
    currentHealth -= damageAmount;

    if (healthSlider != null) healthSlider.value = currentHealth;

    if (currentHealth <= 0)
    {
        Die();
    }
    else
    {
        if (anim != null) anim.SetTrigger("Hurt");

        // NEW: Stop the charging/shooting routine immediately on hit
        if (shooterScript != null) 
        {
            shooterScript.StopAllCoroutines(); 
            // Reset the shooter's internal charging state so they can act again later
            // Note: You might need to make 'isCharging' public in EnemyShooter for this line:
            // shooterScript.isCharging = false; 
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
        if (anim != null) anim.SetTrigger("Death");

        if (patrolScript != null) patrolScript.enabled = false;
        if (shooterScript != null) shooterScript.enabled = false;

        Destroy(gameObject, 0.33f);
    }
}