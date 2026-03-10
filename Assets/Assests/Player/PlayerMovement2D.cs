using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. Get Input
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Jump Input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // 3. Flip Sprite based on direction
        if (horizontalInput > 0 && !isFacingRight)
            Flip();
        else if (horizontalInput < 0 && isFacingRight)
            Flip();

        // 4. Update Animator Parameters
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        // 1. Apply Horizontal Movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // 2. Check if Grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    private void UpdateAnimator()
    {
        // Send our horizontal speed (absolute value so it's always positive)
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        
        // Send our vertical velocity (positive = up, negative = down)
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        
        // Send our grounded status
        anim.SetBool("IsGrounded", isGrounded);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // Draws a circle in the editor to help you position the GroundCheck
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}