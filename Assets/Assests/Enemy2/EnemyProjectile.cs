using UnityEngine;

/* * A. Functionality: Controls a bullet that flies strictly horizontally (left or right) and damages the player on impact.
 * B. New component & functionality learned: Learned how to use Rigidbody2D.velocity to push an object in a straight 2D line and OnTriggerEnter2D for hit detection.
 * C. Problems encountered: The bullet would push the player physically instead of just passing through them.
 * D. What you have tried / not tried: Fixed by making the bullet's BoxCollider2D a "Trigger". Have not tried adding an explosion animation on impact yet.
 * E. Other important developer notes: Ensure the Rigidbody2D gravity scale is 0 so the bullet doesn't fall to the ground.
 */
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 15f;
    public float lifeTime = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Destroy the bullet after 3 seconds so it doesn't fly off to infinity and cause lag
        Destroy(gameObject, lifeTime);
    }

    // The enemy will call this and pass either Vector2.left or Vector2.right
    public void ShootHorizontally(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform);
            }
            Destroy(gameObject); // Destroy bullet on hit
        }
        else if (collision.CompareTag("Walls"))
        {
            Destroy(gameObject); // Destroy bullet if it hits a wall
        }
    }
}