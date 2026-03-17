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

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float nextAttackTime = 0f;
    private EnemyPatrol patrolScript;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolScript = GetComponent<EnemyPatrol>();
        anim = GetComponent<Animator>(); 

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
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

    private IEnumerator PerformMeleeAttack()
    {
        nextAttackTime = Time.time + attackCooldown;

        if (patrolScript != null) patrolScript.enabled = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (spriteRenderer != null) spriteRenderer.color = electricColor;
        
        if (anim != null) anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.2f);

        if (electricEffectPrefab != null && attackPoint != null)
        {
            GameObject shockVFX = Instantiate(electricEffectPrefab, attackPoint.position, Quaternion.identity);
            Destroy(shockVFX, 0.3f);
        }

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (Collider2D hit in hitPlayers)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount, transform);
            }

            SpriteRenderer playerSprite = hit.GetComponentInParent<SpriteRenderer>();
            if (playerSprite != null)
            {
                StartCoroutine(ElectrifyPlayerHit(playerSprite));
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (patrolScript != null) patrolScript.enabled = true;
    }

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

    public void ResetMelee()
    {
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (patrolScript != null) patrolScript.enabled = true;
    }
}