using UnityEngine;
using UnityEngine.UI; // Required for Slider
using TMPro; // This tells Unity we want to use TextMeshPro!
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Elements")]
    public Slider healthSlider; // Now we use a Slider instead of an Image

    [Header("UI Elements")]
    public TextMeshProUGUI hpText; // This will hold our text on the screen

    void Start()
    {
        currentHealth = maxHealth;

        // Ensure the slider matches our health stats at the start
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
        if (hpText != null)
        {
            // The "0" formatting makes sure you don't get ugly decimals like HP: 89.9999
            hpText.text = "HP: " + currentHealth.ToString("0");
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        // Directly set the slider's value to the current health
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player has been defeated!");
    }

}