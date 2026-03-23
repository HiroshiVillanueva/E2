using UnityEngine;
using System.Collections;

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Controls the logic for a melee-focused enemy. It includes two forms of attack: 
 * a heavy electric punch when the player is in range, and a constant "contact aura" 
 * that damages the player if they touch the enemy's body.
 * * B. New component & functionality learned: 
 * - Learned how to instantiate visual effect prefabs (like electricity sparks) 
 * at a specific Transform point and then destroy them automatically.
 * - Used OnDrawGizmosSelected() to draw different colored debug spheres to clearly 
 * differentiate between the attack range (red) and the contact aura (yellow).
 * * C. Problems encountered: 
 * - Originally used OnCollisionStay2D for contact damage, but it caused jittery 
 * physics and sometimes stopped the enemy from patrolling. Switched to a silent 
 * OverlapCircleAll check in Update() to apply damage without affecting physics.
 * * D. What you have tried / not tried: 
 * - Tried applying a color tint (electricColor) to the player's sprite when hit to 
 * sell the electric effect, using a Coroutine to revert it back to normal.
 * - Have not tried adding a lunging dash attack when the player gets just outside 
 * of the melee range.
 * * E. Other important developer notes: 
 * - GetComponentInParent<>() is used when checking the player because the player's 
 * collision hitboxes might be on child objects rather than the main root object 
 * where the PlayerHealth script lives.
 * ============================================================================== */

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMelee : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    public float attackDistance = 1.5f;

    [Header("Melee Settings")]
    public Transform attackPoint;
    public float attackRadius = 0.75f;
    public LayerMask playerLayer;
    public float damageAmount = 10f;
    public float attackCooldown = 1f;

    // --- NEW: Contact Damage Settings (Always Active Aura) ---
    [Header("Contact Damage")]
    public bool dealContactDamage = true;
    public float contactRadius = 0.4f; // Adjust this in the Inspector to fit the enemy's body!
    public float contactDamageAmount = 5f;
    public float contactCooldown = 1f;
    private float nextContactTime = 0f;

    [Header("Visual Effects")]
    public Color electricColor = Color.cyan;
    public GameObject electricEffectPrefab;

    // Component references and tracking variables
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float nextAttackTime = 0f;
    private EnemyPatrol patrolScript;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        patrolScript = GetComponent<EnemyPatrol>();
        anim = GetComponent<Animator>();

        // Save the enemy's default color so it can be restored after electric attacks
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Automatically find the player upon spawning
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Halt logic if player is dead or the enemy is currently being knocked back
        if (player == null || (patrolScript != null && patrolScript.isKnockedBack)) return;

        // 1. ALWAYS check for body contact damage first!
        if (dealContactDamage && Time.time >= nextContactTime)
        {
            CheckContactDamage();
        }

        // 2. Then check if we are close enough to do the full Melee Punch
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackDistance && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformMeleeAttack());
        }
    }

    // --- NEW: The Silent Damage Check (Replaces OnCollisionStay2D) ---
    private void CheckContactDamage()
    {
        // Draw an invisible circle around the enemy's CENTER point (transform.position)
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, contactRadius, playerLayer);

        foreach (Collider2D hit in hitPlayers)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Damage them and push them back, but do NOT stop the enemy from walking!
                playerHealth.TakeDamage(contactDamageAmount, transform);
                nextContactTime = Time.time + contactCooldown;
            }
        }
    }

    // Sequence for stopping patrol, playing animation, spawning VFX, and dealing heavy damage
    private IEnumerator PerformMeleeAttack()
    {
        // Set cooldown immediately so it doesn't try to fire again while animating
        nextAttackTime = Time.time + attackCooldown;

        // Halt patrol and movement
        if (patrolScript != null) patrolScript.enabled = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Visual feedback before hit
        if (spriteRenderer != null) spriteRenderer.color = electricColor;

        if (anim != null) anim.SetTrigger("Attack");

        // Wait for the animation to reach the punch frame
        yield return new WaitForSeconds(0.2f);

        // Spawn VFX
        if (electricEffectPrefab != null && attackPoint != null)
        {
            GameObject shockVFX = Instantiate(electricEffectPrefab, attackPoint.position, Quaternion.identity);
            Destroy(shockVFX, 0.3f);
        }

        // Check for player in the punch radius
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount, transform);
            }

            // Visually electrify the player if they get hit
            SpriteRenderer playerSprite = hit.GetComponentInParent<SpriteRenderer>();
            if (playerSprite != null)
            {
                StartCoroutine(ElectrifyPlayerHit(playerSprite));
            }
        }

        // Wait for animation to fully conclude before resuming patrol
        yield return new WaitForSeconds(0.5f);

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (patrolScript != null) patrolScript.enabled = true;
    }

    // Quick coroutine to flash the player cyan when struck by the electric punch
    private IEnumerator ElectrifyPlayerHit(SpriteRenderer playerSprite)
    {
        Color normalColor = Color.white;
        playerSprite.color = electricColor;
        yield return new WaitForSeconds(0.2f);
        if (playerSprite != null)
        {
            playerSprite.color = normalColor;
        }
    }

    // Draws debug spheres in the editor to help visually balance ranges
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        // Draw the new body contact range in yellow!
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }

    // Utility method called by the EnemyHealth script to reset state if interrupted by damage
    public void ResetMelee()
    {
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (patrolScript != null) patrolScript.enabled = true;
    }
}