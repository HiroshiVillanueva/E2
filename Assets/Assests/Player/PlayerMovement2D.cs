using UnityEngine;
using System.Collections;
using UnityEngine.UI; // 1. We MUST add this to talk to UI elements!

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

    [Header("UI Elements")]
    public Image cooldownIndicator; // 2. The slot for our new UI pie chart

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    void Update()
    {
        if (isRolling) return; 

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canRoll)
        {
            StartCoroutine(PerformRoll());
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

        if (!isRolling)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    private IEnumerator PerformRoll()
    {
        canRoll = false;
        isRolling = true;
        anim.SetTrigger("Roll"); 

        Color transparentColor = spriteRenderer.color; 
        transparentColor.a = 0.5f; 
        spriteRenderer.color = transparentColor; 

        float rollDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(rollDirection * rollForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(rollDuration);

        Color solidColor = spriteRenderer.color;
        solidColor.a = 1f; 
        spriteRenderer.color = solidColor; 

        isRolling = false;

        // 3. THE NEW COOLDOWN LOGIC
        float cooldownTimer = 0f;
        
        // Empty the UI image instantly
        if (cooldownIndicator != null) 
            cooldownIndicator.fillAmount = 0f; 

        // Smoothly fill it back up over time
        while (cooldownTimer < rollCooldown)
        {
            cooldownTimer += Time.deltaTime; // Add the time passed since last frame
            
            if (cooldownIndicator != null)
            {
                // Calculate percentage (0.0 to 1.0) and apply to UI
                cooldownIndicator.fillAmount = cooldownTimer / rollCooldown; 
            }
            
            yield return null; // Wait for the next frame before looping again
        }

        // Ensure it is completely full and unlock the roll
        if (cooldownIndicator != null) 
            cooldownIndicator.fillAmount = 1f; 
            
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
    }
}