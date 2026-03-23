using UnityEngine;
using System.Collections;

/* ==============================================================================
 * PROJECT NOTES: INFOTECH E2
 * * A. Functionality: 
 * Handles the logic for an enemy that shoots projectiles. It detects the player 
 * within a certain range, stops moving to perform a "charge up" animation, fires 
 * a projectile, and then resumes its patrol.
 * * B. New component & functionality learned: 
 * - Learned how to use Instantiate() to spawn new GameObjects (bullets) dynamically 
 * during runtime.
 * - Used Vector2.Distance() to calculate the range between the enemy and the player.
 * - Required the EnemyPatrol script to ensure this enemy can move and shoot.
 * * C. Problems encountered: 
 * - Enemies were shooting backward sometimes. Fixed this by adding the FacePlayer() 
 * method to calculate the X difference and force the enemy to look at the player before firing.
 * * D. What you have tried / not tried: 
 * - Tried making them shoot constantly, but added a Coroutine (ChargeAndShootRoutine) 
 * so there is a visual "wind up" period, giving the player a chance to dodge.
 * - Have not tried making the enemy shoot projectiles that home in on the player.
 * * E. Other important developer notes: 
 * - The forcedMoveDuration forces the enemy to continue walking on patrol for a 
 * brief moment after shooting, preventing them from rapid-firing while standing completely still.
 * ============================================================================== */

// Ensure the enemy has physics and the required patrol logic
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyPatrol))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Targeting Settings")]
    public float sightRange = 8f;
    public float forcedMoveDuration = 1.5f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float chargeDuration = 0.7f; // NEW: How long they "wind up" before shooting

    [Header("Animation")]
    public Animator anim;

    // Internal state tracking
    private float nextFireTime = 0f;
    private float forcedMoveTimer = 0f;
    private bool isCharging = false; // NEW: To prevent movement while charging

    // Component References
    private Rigidbody2D rb;
    private EnemyPatrol patrolScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolScript = GetComponent<EnemyPatrol>();

        if (anim == null) anim = GetComponent<Animator>();

        // Automatically find the player object in the scene using its tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private Transform player;

    // Use FixedUpdate for physics-related checks and consistent timing
    void FixedUpdate()
    {
        // Don't do anything if dead, charging, or knocked back
        if (player == null || patrolScript.isKnockedBack || isCharging) return;

        // Calculate distance between enemy and player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Force the enemy to patrol for a set duration after firing
        if (forcedMoveTimer > 0)
        {
            forcedMoveTimer -= Time.fixedDeltaTime;
            patrolScript.enabled = true;
        }
        // If the player is in range and the cooldown is finished, start shooting sequence
        else if (distanceToPlayer <= sightRange && Time.time >= nextFireTime)
        {
            StartCoroutine(ChargeAndShootRoutine());
        }
        // Default to patrolling
        else
        {
            patrolScript.enabled = true;
        }
    }

    // Sequence for stopping, aiming, and firing
    private IEnumerator ChargeAndShootRoutine()
    {
        isCharging = true;
        patrolScript.enabled = false; // Stop patrol movement
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Halt horizontal momentum

        // Face the player
        FacePlayer();

        // 1. Start Charging Animation
        if (anim != null) anim.SetTrigger("charge"); // Make sure you have a "charge" trigger in Animator

        // 2. Wait for the charging period
        yield return new WaitForSeconds(chargeDuration);

        // 3. Check if still alive/not knocked back before firing
        if (!patrolScript.isKnockedBack)
        {
            FireProjectile();
        }

        // 4. Cleanup and reset timers
        nextFireTime = Time.time + fireRate;
        forcedMoveTimer = forcedMoveDuration;
        isCharging = false;
        patrolScript.enabled = true;
    }

    // Logic for creating and launching the bullet
    private void FireProjectile()
    {
        if (anim != null) anim.SetTrigger("attack"); // The actual firing animation

        if (bulletPrefab != null && firePoint != null)
        {
            // Instantiate creates a copy of the prefab in the scene at the firePoint location
            GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            // Determine direction based on which way the enemy sprite is currently facing
            Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // Grab the script on the new bullet and tell it which way to fly
            EnemyProjectile projectileScript = newBullet.GetComponent<EnemyProjectile>();
            if (projectileScript != null)
            {
                projectileScript.ShootHorizontally(shootDirection);
            }
        }
    }

    // Determines if the player is to the left or right and flips the local scale to match
    private void FacePlayer()
    {
        float xDifference = player.position.x - transform.position.x;
        bool playerOnRight = xDifference > 0;

        if ((playerOnRight && transform.localScale.x < 0) || (!playerOnRight && transform.localScale.x > 0))
        {
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}