using UnityEngine;

public class ObjectJuice : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobSpeed = 2f;      // How fast it bobs
    public float bobAmount = 0.5f;   // How high/low it goes

    [Header("Pulse Settings")]
    public float pulseSpeed = 3f;    // How fast it fades in/out
    [Range(0f, 1f)]
    public float minOpacity = 0.3f;  // Minimum transparency (0 = invisible)
    [Range(0f, 1f)]
    public float maxOpacity = 1f;    // Maximum transparency (1 = solid)

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Record the starting position so we bob around it
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleBobbing();
        HandlePulse();
    }

    private void HandleBobbing()
    {
        // Calculate the new Y position using a Sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void HandlePulse()
    {
        if (spriteRenderer == null) return;

        // Calculate a value between 0 and 1 using Sine
        // We use (sin + 1) / 2 to convert the -1 to 1 range into 0 to 1
        float sineWave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        
        // Interpolate (Lerp) between our min and max opacity
        float newAlpha = Mathf.Lerp(minOpacity, maxOpacity, sineWave);

        // Apply the new alpha to the sprite's color
        Color tempColor = spriteRenderer.color;
        tempColor.a = newAlpha;
        spriteRenderer.color = tempColor;
    }
}