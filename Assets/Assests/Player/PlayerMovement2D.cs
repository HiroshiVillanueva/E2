using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Hurt & Knockback Settings")]
    public float knockbackForceX = 5f;
    public float knockbackForceY = 5f;
    public float knockbackDuration = 0.2f; 
    public float iFrameDuration = 1.5f; 
    public float flickerInterval = 0.1f; 

    [Header("Roll Settings")]
    public float rollForce = 15f;
    public float rollDuration = 0.4f;
    public float rollCooldown = 1f;
    public float invincibilityIntensity = 0.90f;

    [Header("Attack Settings")]
    public float punch1Duration = 0.3f;
    public float punch2Duration = 0.6f;
    public float comboWindow = 1.5f;
    public float punchDashForce = 4f;
    public float punchDashDuration = 0.1f;

    [Header("Hitbox & Damage Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public float punch1Damage = 10f;
    public float punch2Damage = 30f;

    [Header("Hit Meter (Visual Bar)")]
    public Slider hitMeterBar;         
    public float maxHitPoints = 100f;  
    public float pointsPerHit = 10f;   
    public float decayRate = 5f;       
    public float decayDelay = 2f;      

    [Header("Special Abilities")]
    public float healAmount = 25f;
    public float healMeterCost = 5f;
    public float healGlowDuration = 0.5f; // How long the green glow lasts
    public Color healGlowColor = Color.green;

    [Header("Audio Settings")]
    public AudioClip punchMissSound;
    public AudioClip punchHitSound;
    public AudioClip healSound; // Assign this in the Inspector

    [Header("UI Elements")]
    public Image cooldownIndicator;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;

    public bool isRolling = false;
    public bool isKnockedBack = false;
    public bool isInvincible = false; 

    private bool canRoll = true;
    private bool isAttacking = false;
    private bool isPunchDashing = false;
    private int comboStep = 1;
    private float lastAttackTime = 0f;

    private float currentHitPoints = 0f;
    private float lastHitTimestamp = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (hitMeterBar != null)
        {
            hitMeterBar.maxValue = maxHitPoints;
            hitMeterBar.value = 0;
        }
    }

    void Update()
    {
        HandleHitMeterDecay();
        UpdateUI();

        // Check for Heal Ability (E Key)
        if (Input.GetKeyDown(KeyCode.E) && currentHitPoints >= healMeterCost)
        {
            StartCoroutine(PerformHeal());
        }

        // INPUT LOCK: Movement and Attack are disabled if these are true
        if (isKnockedBack || isRolling || isAttacking) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0 && !isFacingRight)
            Flip();
        else if (horizontalInput < 0 && isFacingRight)
            Flip();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canRoll)
        {
            StartCoroutine(PerformRoll());
        }

        if (Input.GetButtonDown("Fire1") && isGrounded)
        {
            StartCoroutine(PerformAttack());
        }

        UpdateAnimator();
    }

    private IEnumerator PerformHeal()
    {
        currentHitPoints -= healMeterCost;
        isInvincible = true; // Turn on I-Frames

        // Visual Feedback
        spriteRenderer.color = healGlowColor;
        
        // Audio Feedback
        if (healSound != null) PlayFastSound(healSound);

        // Logic to increase health
        var healthScript = GetComponent<PlayerHealth>(); 
        if (healthScript != null)
        {
            healthScript.Heal(healAmount);
        }

        // Wait for the glow/invincibility to finish
        yield return new WaitForSeconds(healGlowDuration);

        // Reset
        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isKnockedBack)
        {
            if (!isRolling && !isAttacking)
            {
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
            }
            else if (isAttacking && !isPunchDashing)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    private void HandleHitMeterDecay()
    {
        if (Time.time > lastHitTimestamp + decayDelay && currentHitPoints > 0)
        {
            currentHitPoints -= decayRate * Time.deltaTime;
            currentHitPoints = Mathf.Max(currentHitPoints, 0); 
        }
    }

    private void UpdateUI()
    {
        if (hitMeterBar != null)
        {
            hitMeterBar.value = currentHitPoints;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        isPunchDashing = true;

        if (Time.time - lastAttackTime > comboWindow)
        {
            comboStep = 1;
        }

        float punchDirection = isFacingRight ? 1f : -1f;

        if (comboStep == 1)
        {
            anim.SetTrigger("Punch1");
            rb.linearVelocity = new Vector2(punchDirection * punchDashForce, rb.linearVelocity.y);
            CheckHitbox(punch1Damage); 
            yield return new WaitForSeconds(punchDashDuration);
            isPunchDashing = false;
            yield return new WaitForSeconds(punch1Duration - punchDashDuration);
            comboStep = 2;
        }
        else if (comboStep == 2)
        {
            anim.SetTrigger("Punch1"); 
            rb.linearVelocity = new Vector2(punchDirection * punchDashForce, rb.linearVelocity.y);
            CheckHitbox(punch1Damage); 
            yield return new WaitForSeconds(punchDashDuration);
            isPunchDashing = false;
            yield return new WaitForSeconds(punch1Duration - punchDashDuration);
            comboStep = 3;
        }
        else if (comboStep == 3)
        {
            anim.SetTrigger("Punch2"); 
            rb.linearVelocity = new Vector2(punchDirection * (punchDashForce * 1.5f), rb.linearVelocity.y);
            CheckHitbox(punch2Damage); 
            yield return new WaitForSeconds(punchDashDuration);
            isPunchDashing = false;
            yield return new WaitForSeconds(punch2Duration - punchDashDuration);
            comboStep = 1; 
        }

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private void CheckHitbox(float damageAmount)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        if (hitEnemies.Length > 0)
        {
            PlayFastSound(punchHitSound); 
            currentHitPoints += pointsPerHit;
            currentHitPoints = Mathf.Min(currentHitPoints, maxHitPoints); 
            lastHitTimestamp = Time.time; 

            foreach (Collider2D enemy in hitEnemies)
            {
                var enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damageAmount, transform);
                }
            }
        }
        else
        {
            PlayFastSound(punchMissSound); 
        }
    }

    private IEnumerator PerformRoll()
    {
        canRoll = false;
        isRolling = true;
        anim.SetTrigger("Roll");

        Color transparentColor = spriteRenderer.color;
        transparentColor.a = invincibilityIntensity;
        spriteRenderer.color = transparentColor;

        float rollDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(rollDirection * rollForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(rollDuration);

        Color solidColor = spriteRenderer.color;
        solidColor.a = 1f;
        spriteRenderer.color = solidColor;

        isRolling = false;

        float cooldownTimer = 0f;
        if (cooldownIndicator != null) cooldownIndicator.fillAmount = 0f;

        while (cooldownTimer < rollCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownIndicator != null) cooldownIndicator.fillAmount = cooldownTimer / rollCooldown;
            yield return null;
        }

        if (cooldownIndicator != null) cooldownIndicator.fillAmount = 1f;
        canRoll = true;
    }

    public IEnumerator HurtSequence(Transform attacker)
    {
        if (isInvincible) yield break; // If healing or rolling, don't get hurt

        isKnockedBack = true;
        isInvincible = true;

        float knockbackDirection = transform.position.x < attacker.position.x ? -1f : 1f;
        rb.linearVelocity = new Vector2(knockbackForceX * knockbackDirection, knockbackForceY);

        spriteRenderer.color = Color.red;

        float elapsedTime = 0f;
        while (elapsedTime < iFrameDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flickerInterval);
            elapsedTime += flickerInterval;

            if (elapsedTime >= knockbackDuration)
            {
                isKnockedBack = false;
            }
        }

        spriteRenderer.enabled = true;
        spriteRenderer.color = Color.white;
        isInvincible = false;
        isKnockedBack = false;
    }

    private void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    private void PlayFastSound(AudioClip clip)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("FastSoundEffect");
        tempAudio.transform.position = Camera.main.transform.position;

        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = 2f;
        source.Play();

        Destroy(tempAudio, clip.length / source.pitch);
    }
}