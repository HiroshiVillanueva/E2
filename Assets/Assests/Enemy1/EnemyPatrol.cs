using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolDistance = 8f;

    [Header("Personal Space (Stops Jitter)")]
    public float stopDistance = 0.8f; // The enemy will stop this far away from your center!

    [Header("Targeting Settings")]
    public float sightRange = 6f;
    private Transform player;

    private float startX;
    private Rigidbody2D rb;
    private bool movingRight = true;

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
        if (player == null) return;

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

        // A tiny 0.2 deadzone! It will run right up next to you, but won't overshoot and jitter.
        if (Mathf.Abs(xDifference) > 0.2f)
        {
            // Run at the player!
            float directionToPlayer = Mathf.Sign(xDifference);
            rb.linearVelocity = new Vector2(directionToPlayer * chaseSpeed, rb.linearVelocity.y);

            // Face the correct direction
            if (directionToPlayer > 0 && !movingRight) Flip();
            else if (directionToPlayer < 0 && movingRight) Flip();
        }
        else
        {
            // We are standing right beside the player. Stop moving so we don't flip out!
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
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