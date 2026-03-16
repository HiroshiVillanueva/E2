using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

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

    [Header("Audio Settings")]
    public AudioClip punchMissSound;   // Drag your "Swoosh" sound here
    public AudioClip punchHitSound;    // Drag your "Smack" sound here

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

    private bool isRolling = false;
    private bool canRoll = true;

    private bool isAttacking = false;
    private bool isPunchDashing = false;
    private int comboStep = 1;
    private float lastAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isRolling || isAttacking) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

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

        if (horizontalInput > 0 && !isFacingRight)
            Flip();
        else if (horizontalInput < 0 && isFacingRight)
            Flip();

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isRolling && !isAttacking)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        else if (isAttacking && !isPunchDashing)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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

            // --- DETECT ENEMIES & PLAY SOUND INSTANTLY ---
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            if (hitEnemies.Length > 0)
            {
                PlayFastSound(punchHitSound); // FIXED: Now uses the fast method!
                foreach (Collider2D enemy in hitEnemies)
                {
                    enemy.GetComponent<EnemyHealth>().TakeDamage(punch1Damage);
                }
            }
            else
            {
                PlayFastSound(punchMissSound); // FIXED: Now uses the fast method!
            }

            // --- NOW WAIT FOR THE DASH AND ANIMATION TO FINISH ---
            yield return new WaitForSeconds(punchDashDuration);
            isPunchDashing = false;
            yield return new WaitForSeconds(punch1Duration - punchDashDuration);
            comboStep = 2;
        }
        else if (comboStep == 2)
        {
            anim.SetTrigger("Punch2");
            rb.linearVelocity = new Vector2(punchDirection * (punchDashForce * 1.5f), rb.linearVelocity.y);

            // --- DETECT ENEMIES & PLAY SOUND INSTANTLY ---
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            if (hitEnemies.Length > 0)
            {
                PlayFastSound(punchHitSound); // Uses the fast method
                foreach (Collider2D enemy in hitEnemies)
                {
                    enemy.GetComponent<EnemyHealth>().TakeDamage(punch2Damage); // FIXED: Now deals punch 2 damage!
                }
            }
            else
            {
                PlayFastSound(punchMissSound); // Uses the fast method
            }

            // --- NOW WAIT FOR THE DASH AND ANIMATION TO FINISH ---
            yield return new WaitForSeconds(punchDashDuration);
            isPunchDashing = false;
            yield return new WaitForSeconds(punch2Duration - punchDashDuration);
            comboStep = 1;
        }

        lastAttackTime = Time.time;
        isAttacking = false;
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

        // Create an invisible, temporary speaker
        GameObject tempAudio = new GameObject("FastPunchSound");
        tempAudio.transform.position = Camera.main.transform.position;

        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clip;

        // THIS IS THE SPEED CONTROL! 2f = 2x faster (and higher pitch)
        source.pitch = 2f;

        source.Play();

        // Destroy the speaker exactly when the sped-up clip finishes
        Destroy(tempAudio, clip.length / source.pitch);
    }
}