using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PlayerHealth : MonoBehaviour
{
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
        currentHealth = maxHealth;
        anim = GetComponent<Animator>(); 
        movementScript = GetComponent<PlayerMovement2D>(); 

        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount, Transform attacker)
    {
        if (isDead) return; 

        // UPDATED: Now we ignore damage if rolling OR if the hurt i-frames are active!
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
            
            // UPDATED: Call the new Flicker and Knockback sequence!
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
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text = "HP: " + currentHealth.ToString("0");
        }
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
    }
}