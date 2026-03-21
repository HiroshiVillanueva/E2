using UnityEngine;
using System.Collections;

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

    private float nextFireTime = 0f;
    private float forcedMoveTimer = 0f;
    private bool isCharging = false; // NEW: To prevent movement while charging
    
    private Rigidbody2D rb;
    private EnemyPatrol patrolScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolScript = GetComponent<EnemyPatrol>();

        if (anim == null) anim = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private Transform player;

    void FixedUpdate()
    {
        // Don't do anything if dead, charging, or knocked back
        if (player == null || patrolScript.isKnockedBack || isCharging) return; 

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (forcedMoveTimer > 0)
        {
            forcedMoveTimer -= Time.fixedDeltaTime;
            patrolScript.enabled = true;
        }
        else if (distanceToPlayer <= sightRange && Time.time >= nextFireTime)
        {
            StartCoroutine(ChargeAndShootRoutine());
        }
        else
        {
            patrolScript.enabled = true;
        }
    }

    private IEnumerator ChargeAndShootRoutine()
    {
        isCharging = true;
        patrolScript.enabled = false; // Stop patrol movement
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

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

        // 4. Cleanup
        nextFireTime = Time.time + fireRate;
        forcedMoveTimer = forcedMoveDuration;
        isCharging = false;
        patrolScript.enabled = true;
    }

    private void FireProjectile()
    {
        if (anim != null) anim.SetTrigger("attack"); // The actual firing animation

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            EnemyProjectile projectileScript = newBullet.GetComponent<EnemyProjectile>();
            if (projectileScript != null)
            {
                projectileScript.ShootHorizontally(shootDirection);
            }
        }
    }

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