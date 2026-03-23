using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/* * A. Functionality: Handles 2D player movement, jumping, a 3-hit attack combo, a dodge roll with invincibility frames, and a hit meter system used for healing.
 * B. New component & functionality learned: Explored the use of Coroutines (IEnumerator) for timed abilities, LayerMasks for selective collision, and Physics2D.OverlapCircleAll for precise melee hitboxes.
 * C. Problems encountered: Balancing the jump force and gravity, as well as syncing the punch dash physics with the animator's transition times.
 * D. What you have tried / not tried: Tried standard trigger colliders for the attack but switched to OverlapCircleAll for better range control. Have not tried implementing double-jumps or wall-sliding yet.
 * E. Other important developer notes: The [RequireComponent] attribute is used to ensure the GameObject always has the essential physics and rendering components attached to prevent runtime errors.
 */
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
        // Initialize component references
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set up the UI slider max value if it exists
        if (hitMeterBar != null)
        {
            hitMeterBar.maxValue = maxHitPoints;
            hitMeterBar.value = 0;
        }
    }

    void Update()
    {
        // Handle meter logic and UI updates every frame
        HandleHitMeterDecay();
        UpdateUI();

        // Check for Heal Ability (E Key)
        if (Input.GetKeyDown(KeyCode.E) && currentHitPoints >= healMeterCost)
        {
            StartCoroutine(PerformHeal());
        }

        // INPUT LOCK: Movement and Attack are disabled if these are true
        if (isKnockedBack || isRolling || isAttacking) return;

        // Get raw input for crisp movement (-1, 0, or 1)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Flip character sprite based on direction
        if (horizontalInput > 0 && !isFacingRight)
            Flip();
        else if (horizontalInput < 0 && isFacingRight)
            Flip();

        // Jump input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Dodge roll input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canRoll)
        {
            StartCoroutine(PerformRoll());
        }

        // Attack input
        if (Input.GetButtonDown("Fire1") && isGrounded)
        {
            StartCoroutine(PerformAttack());
        }

        // Sync variables to Animator
        UpdateAnimator();
    }

    // Manages the visual and logic flow for the heal ability
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

    // Physics updates
    void FixedUpdate()
    {
        // Create a small circle at the ground check point to detect ground layer
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isKnockedBack)
        {
            // Standard horizontal velocity
            if (!isRolling && !isAttacking)
            {
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
            }
            // Stop movement when attacking, unless in the dash phase
            else if (isAttacking && !isPunchDashing)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    // Reduces the hit meter if too much time passes between hits
    private void HandleHitMeterDecay()
    {
        if (Time.time > lastHitTimestamp + decayDelay && currentHitPoints > 0)
        {
            currentHitPoints -= decayRate * Time.deltaTime;
            currentHitPoints = Mathf.Max(currentHitPoints, 0);
        }
    }

    // Applies current hit points to the UI
    private void UpdateUI()
    {
        if (hitMeterBar != null)
        {
            hitMeterBar.value = currentHitPoints;
        }
    }

    // Adds vertical velocity to the rigidbody
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    // Manages combo stages and punch dash mechanics
    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        isPunchDashing = true;

        // Reset combo if window expired
        if (Time.time - lastAttackTime > comboWindow)
        {
            comboStep = 1;
        }

        float punchDirection = isFacingRight ? 1f : -1f;

        // Combo 1
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
        // Combo 2
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
        // Combo 3 (Heavy)
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

    // Detects enemies in attack range and applies damage/meter gains
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

    // Manages dodge physics, i-frames, and UI cooldown
    private IEnumerator PerformRoll()
    {
        canRoll = false;
        isRolling = true;
        anim.SetTrigger("Roll");

        // Make sprite slightly transparent during the roll
        Color transparentColor = spriteRenderer.color;
        transparentColor.a = invincibilityIntensity;
        spriteRenderer.color = transparentColor;

        float rollDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(rollDirection * rollForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(rollDuration);

        // Revert sprite transparency
        Color solidColor = spriteRenderer.color;
        solidColor.a = 1f;
        spriteRenderer.color = solidColor;

        isRolling = false;

        // Handle cooldown radial UI
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

    // Handles knockback mechanics and sprite flickering when taking damage
    public IEnumerator HurtSequence(Transform attacker)
    {
        if (isInvincible) yield break; // If healing or rolling, don't get hurt

        isKnockedBack = true;
        isInvincible = true;

        // Calculate knockback direction
        float knockbackDirection = transform.position.x < attacker.position.x ? -1f : 1f;
        rb.linearVelocity = new Vector2(knockbackForceX * knockbackDirection, knockbackForceY);

        spriteRenderer.color = Color.red;

        // Visual flickering loop for i-frames
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

        // Reset hurt state
        spriteRenderer.enabled = true;
        spriteRenderer.color = Color.white;
        isInvincible = false;
        isKnockedBack = false;
    }

    // Sends velocity and state data to the Animator
    private void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);
    }

    // Inverts local scale X to face left/right
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // Draws spheres in the Unity editor view for debugging ground/attack ranges
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

    // Generates a temporary object to play sounds independently
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