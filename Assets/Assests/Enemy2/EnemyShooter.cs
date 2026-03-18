using UnityEngine;

/* * A. Functionality: Enemy patrols left and right within a set distance. When player is in sight, it stops, faces the player, and shoots. When player leaves, it resumes patrolling.
 * B. New component & functionality learned: Learned how to smoothly switch between a moving Patrol state and a stationary Attack state using Vector2.Distance.
 * C. Problems encountered: Enemy wasn't moving properly due to overly complex logic.
 * D. What you have tried / not tried: Stripped down the logic to directly mirror the working EnemyPatrol script, replacing the Chase logic with Shoot logic.
 * E. Other important developer notes: Ensure patrolSpeed is greater than 0 in the Unity Inspector!
 */
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 5f;

    [Header("Targeting Settings")]
    public float sightRange = 8f;
    private Transform player;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;

    private float nextFireTime = 0f;
    private float startX;
    private Rigidbody2D rb;

    // We match your working script exactly by using movingRight!
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
        // If the player is missing or dead, just keep walking!
        if (player == null)
        {
            Patrol();
            return;
        }

        // Check how close the player is
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= sightRange)
        {
            StopAndShoot();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        // 1. Move horizontally (Exact copy of your working script)
        rb.linearVelocity = new Vector2((movingRight ? patrolSpeed : -patrolSpeed), rb.linearVelocity.y);

        // 2. Turn around if we go too far right
        if (movingRight && transform.position.x >= startX + patrolDistance)
        {
            Flip();
        }
        // 3. Turn around if we go too far left
        else if (!movingRight && transform.position.x <= startX - patrolDistance)
        {
            Flip();
        }
    }

    private void StopAndShoot()
    {
        // 1. Instantly stop walking
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // 2. Face the player's direction
        float xDifference = player.position.x - transform.position.x;
        if (xDifference > 0 && !movingRight)
        {
            Flip();
        }
        else if (xDifference < 0 && movingRight)
        {
            Flip();
        }

        // 3. Shoot bullets on a timer
        if (Time.time >= nextFireTime)
        {
            if (bulletPrefab != null && firePoint != null)
            {
                // Spawn the bullet
                GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

                // Shoot whichever way the enemy is currently facing
                Vector2 shootDirection = movingRight ? Vector2.right : Vector2.left;

                EnemyProjectile projectileScript = newBullet.GetComponent<EnemyProjectile>();
                if (projectileScript != null)
                {
                    projectileScript.ShootHorizontally(shootDirection);
                }
            }

            // Reset the firing cooldown
            nextFireTime = Time.time + fireRate;
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
        // Draws the yellow sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draws the Green Patrol Line exactly like your Enemy1 script!
        Gizmos.color = Color.green;
        Vector3 leftPoint = new Vector3(transform.position.x - patrolDistance, transform.position.y, transform.position.z);
        Vector3 rightPoint = new Vector3(transform.position.x + patrolDistance, transform.position.y, transform.position.z);

        if (Application.isPlaying)
        {
            leftPoint.x = startX - patrolDistance;
            rightPoint.x = startX + patrolDistance;
        }

        Gizmos.DrawLine(leftPoint, rightPoint);
        Gizmos.DrawSphere(leftPoint, 0.2f);
        Gizmos.DrawSphere(rightPoint, 0.2f);
    }
}