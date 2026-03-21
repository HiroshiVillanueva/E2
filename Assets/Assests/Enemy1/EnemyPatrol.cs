using UnityEngine;
using System.Collections; // NEW: Added this to use Coroutines!

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolDistance = 8f;

    [Header("Knockback Settings")] // NEW: Added knockback stats!
    public float knockbackForceX = 0.8f;
    public float knockbackForceY = 0.8f;
    public float knockbackDuration = 0.2f;

    [Header("Personal Space (Stops Jitter)")]
    public float stopDistance = 0.8f; 

    [Header("Targeting Settings")]
    public float sightRange = 6f;
    private Transform player;

    private float startX;
    private Rigidbody2D rb;
    private bool movingRight = true;

    // NEW: A public switch so the attack and health scripts know we are flying backwards
public bool isKnockedBack = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        // NEW: If we are stunned/knocked back, do NOT try to walk or chase!
        if (player == null || isKnockedBack) return; 

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= sightRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        rb.linearVelocity = new Vector2((movingRight ? patrolSpeed : -patrolSpeed), rb.linearVelocity.y);

        if (movingRight && transform.position.x >= startX + patrolDistance)
        {
            Flip();
        }
        else if (!movingRight && transform.position.x <= startX - patrolDistance)
        {
            Flip();
        }
    }

    private void ChasePlayer()
    {
        float xDifference = player.position.x - transform.position.x;

        if (Mathf.Abs(xDifference) > stopDistance) // Uses your stop distance to prevent jitter!
        {
            float directionToPlayer = Mathf.Sign(xDifference);
            rb.linearVelocity = new Vector2(directionToPlayer * chaseSpeed, rb.linearVelocity.y);

            if (directionToPlayer > 0 && !movingRight) Flip();
            else if (directionToPlayer < 0 && movingRight) Flip();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // NEW: The physical push backward
    public IEnumerator ApplyKnockback(Transform attacker)
    {
        isKnockedBack = true;

        // Figure out which side the punch came from
        float knockbackDirection = 1f;
        if (attacker != null && transform.position.x < attacker.position.x)
        {
            knockbackDirection = -1f; // Player is on the right, fly left!
        }

        // Apply the push
        rb.linearVelocity = new Vector2(knockbackForceX * knockbackDirection, knockbackForceY);

        // Wait for the flinch to finish
        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.green;
        Vector3 leftPoint = new Vector3(transform.position.x - patrolDistance, transform.position.y, transform.position.z);
        Vector3 rightPoint = new Vector3(transform.position.x + patrolDistance, transform.position.y, transform.position.z);
        Gizmos.DrawLine(leftPoint, rightPoint);
        Gizmos.DrawSphere(leftPoint, 0.2f);
        Gizmos.DrawSphere(rightPoint, 0.2f);
    }
}