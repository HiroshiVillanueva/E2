using UnityEngine;
using UnityEngine.UI; // Required for the Slider

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("UI Elements")]
    public Slider healthSlider; // Drag your new EnemyHealthSlider here!

    void Start()
    {
        currentHealth = maxHealth;

        // Set up the health bar at the start
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage!");

        // Update the floating health bar
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has been defeated!");
        Destroy(gameObject);
    }
}