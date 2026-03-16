using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMelee : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    public float attackDistance = 1.5f;

    [Header("Melee Settings")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask playerLayer;
    public float damageAmount = 10f;
    public float attackCooldown = 2f;

    [Header("Visual Effects")]
    public Color electricColor = Color.cyan;
    public GameObject electricEffectPrefab;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float nextAttackTime = 0f;
    private EnemyPatrol patrolScript;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolScript = GetComponent<EnemyPatrol>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Automatically find the player by their tag!
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Check how close the player is
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If the player is close enough, and the cooldown is ready, ATTACK!
        if (distanceToPlayer <= attackDistance && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformMeleeAttack());
        }
    }

    private IEnumerator PerformMeleeAttack()
    {
        // Reset the cooldown timer
        nextAttackTime = Time.time + attackCooldown;

        // Stop the enemy from walking while it attacks
        if (patrolScript != null) patrolScript.enabled = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Turn the monitor blue/yellow while it winds up
        if (spriteRenderer != null) spriteRenderer.color = electricColor;

        // Wait a tiny bit for the "wind up" 
        yield return new WaitForSeconds(0.2f);

        // SPAWN THE ELECTRIC SHOCK GRAPHIC
        if (electricEffectPrefab != null && attackPoint != null)
        {
            GameObject shockVFX = Instantiate(electricEffectPrefab, attackPoint.position, Quaternion.identity);
            Destroy(shockVFX, 0.3f);
        }

        // Detect if the player actually got hit
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            // Try to find the PlayerHealth script on the player and damage them
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }

            // Grab the Player's SpriteRenderer to change their color
            SpriteRenderer playerSprite = hit.GetComponentInParent<SpriteRenderer>();
            if (playerSprite != null)
            {
                StartCoroutine(ElectrifyPlayerHit(playerSprite));
            }
        }

        // Wait for the punch animation to finish
        yield return new WaitForSeconds(0.5f);

        // Return monitor to its normal color and let it walk again
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (patrolScript != null) patrolScript.enabled = true;
    }

    // New Coroutine to handle the player's 1-second electric flash
    private IEnumerator ElectrifyPlayerHit(SpriteRenderer playerSprite)
    {
        // We hardcode the reset color to white (the default for Unity sprites)
        // so we don't accidentally save the cyan color if the player gets hit twice quickly!
        Color normalColor = Color.white;

        // Turn the player cyan/electric
        playerSprite.color = electricColor;

        // Keep them electric for exactly 1 second
        yield return new WaitForSeconds(1f);

        // Make sure the player hasn't been destroyed (e.g., if they died) before changing back
        if (playerSprite != null)
        {
            playerSprite.color = normalColor;
        }
    }

    // Draws a red circle in the Scene view so you can see the punch range!
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}