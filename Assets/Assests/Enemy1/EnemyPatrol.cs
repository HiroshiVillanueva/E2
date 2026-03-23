using UnityEngine;
using System.Collections; // NEW: Added this to use Coroutines!

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Controls the basic AI movement for enemies. Handles patrolling back and forth 
 * within a set distance, chasing the player when they enter line of sight, and 
 * processing physics-based knockback when hit.
 * * B. New component & functionality learned: 
 * - Learned how to use Vector2.Distance() and Mathf.Sign() to determine the 
 * direction and distance of the player relative to the enemy.
 * - Implemented Coroutines (IEnumerator) to handle the temporary knockback state.
 * * C. Problems encountered: 
 * - When the enemy reached the exact same X position as the player, it would rapidly 
 * flip back and forth (jittering). Fixed this by adding a "stopDistance" buffer.
 * - Enemies would try to keep walking forward while taking damage. Added an 
 * isKnockedBack boolean to pause their AI routine while they fly backward.
 * * D. What you have tried / not tried: 
 * - Tried using simple trigger colliders for the detection zone, but calculating 
 * distance in code felt cleaner and didn't clutter the physics engine.
 * - Have not tried implementing advanced pathfinding (like NavMesh or A*) to navigate 
 * around complex platformer terrain.
 * * E. Other important developer notes: 
 * - This script uses FixedUpdate() for movement because manipulating the 
 * Rigidbody2D's velocity should always be tied to the Unity physics step, not the frame rate.
 * ============================================================================== */

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

    // Internal state tracking
    private float startX;
    private Rigidbody2D rb;
    private bool movingRight = true;

    // NEW: A public switch so the attack and health scripts know we are flying backwards
    public bool isKnockedBack = false;

    void Start()
    {
        // Initialize components and starting position
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;

        // Automatically locate the player in the scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Physics update loop
    void FixedUpdate()
    {
        // NEW: If we are stunned/knocked back, do NOT try to walk or chase!
        if (player == null || isKnockedBack) return;

        // Calculate the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Switch states based on line of sight
        if (distanceToPlayer <= sightRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // Moves the enemy back and forth around its starting X position
    private void Patrol()
    {
        // Apply horizontal velocity based on current facing direction
        rb.linearVelocity = new Vector2((movingRight ? patrolSpeed : -patrolSpeed), rb.linearVelocity.y);

        // Reverse direction if we hit the right boundary
        if (movingRight && transform.position.x >= startX + patrolDistance)
        {
            Flip();
        }
        // Reverse direction if we hit the left boundary
        else if (!movingRight && transform.position.x <= startX - patrolDistance)
        {
            Flip();
        }
    }

    // Aggressively moves the enemy toward the player's X position
    private void ChasePlayer()
    {
        float xDifference = player.position.x - transform.position.x;

        if (Mathf.Abs(xDifference) > stopDistance) // Uses your stop distance to prevent jitter!
        {
            // Determine if player is left (-1) or right (1)
            float directionToPlayer = Mathf.Sign(xDifference);
            rb.linearVelocity = new Vector2(directionToPlayer * chaseSpeed, rb.linearVelocity.y);

            // Flip sprite to face the player while chasing
            if (directionToPlayer > 0 && !movingRight) Flip();
            else if (directionToPlayer < 0 && movingRight) Flip();
        }
        else
        {
            // Stop moving if we are right next to the player
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // NEW: The physical push backward
    // Coroutine called by EnemyHealth.cs when damage is taken
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

    // Inverts local scale X to change facing direction
    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // Draws visual helpers in the Unity Scene view for easy level design
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