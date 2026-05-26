using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RobotV1Script : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float detectionRange = 6f;
    [SerializeField] float overlapPadding = 0.02f;
    [SerializeField] float damageInterval = 1f;
    [SerializeField] int damageAmount = 1;

    Rigidbody2D rb;
    Collider2D[] robotColliders;
    Collider2D bodyCollider;
    Transform playerTarget;
    PlayerHealth playerHealth;
    Collider2D[] playerColliders;
    float damageTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        robotColliders = GetComponents<Collider2D>();

        AcquirePlayerReferences();
        IgnorePlayerCollisions();
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (playerTarget == null)
        {
            AcquirePlayerReferences();
            IgnorePlayerCollisions();

            if (playerTarget == null)
                return;
        }

        float deltaX = playerTarget.position.x - transform.position.x;
        float absX = Mathf.Abs(deltaX);
        float chaseX = 0f;

        if (absX <= detectionRange && ShouldKeepClosing(deltaX))
            chaseX = Mathf.Sign(deltaX) * moveSpeed;

        rb.linearVelocity = new Vector2(chaseX, rb.linearVelocity.y);

        if (chaseX != 0f)
        {
            Vector3 localScale = transform.localScale;
            // Sprite's default art faces left, so invert sign to face the player.
            localScale.x = -Mathf.Sign(deltaX) * Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }

        HandleContactDamage();
    }

    bool ShouldKeepClosing(float deltaX)
    {
        if (bodyCollider == null || playerColliders == null || playerColliders.Length == 0)
            return true;

        Bounds robotBounds = bodyCollider.bounds;
        Bounds playerBounds = playerColliders[0].bounds;

        for (int i = 1; i < playerColliders.Length; i++)
            playerBounds.Encapsulate(playerColliders[i].bounds);

        if (deltaX > 0f)
            return robotBounds.max.x < playerBounds.min.x + overlapPadding;

        return robotBounds.min.x > playerBounds.max.x - overlapPadding;
    }

    void AcquirePlayerReferences()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerTarget = playerHealth.transform;
        playerColliders = playerHealth.GetComponentsInChildren<Collider2D>(true);

        if (bodyCollider == null)
        {
            for (int i = 0; i < robotColliders.Length; i++)
            {
                if (!robotColliders[i].isTrigger)
                {
                    bodyCollider = robotColliders[i];
                    break;
                }
            }

            if (bodyCollider == null && robotColliders.Length > 0)
                bodyCollider = robotColliders[0];
        }
    }

    void IgnorePlayerCollisions()
    {
        if (playerColliders == null || robotColliders == null)
            return;

        for (int i = 0; i < robotColliders.Length; i++)
        {
            if (robotColliders[i].isTrigger)
                continue;

            for (int j = 0; j < playerColliders.Length; j++)
                Physics2D.IgnoreCollision(robotColliders[i], playerColliders[j], true);
        }
    }

    void HandleContactDamage()
    {
        if (playerHealth == null)
            return;

        if (damageTimer > 0f)
            damageTimer -= Time.fixedDeltaTime;

        if (damageTimer > 0f || !IsOverlappingPlayer())
            return;

        playerHealth.TakeDamage(damageAmount, transform.position);
        damageTimer = damageInterval;
    }

    bool IsOverlappingPlayer()
    {
        if (robotColliders == null || playerColliders == null)
            return false;

        for (int i = 0; i < robotColliders.Length; i++)
        {
            Bounds robotBounds = robotColliders[i].bounds;

            for (int j = 0; j < playerColliders.Length; j++)
            {
                if (robotBounds.Intersects(playerColliders[j].bounds))
                    return true;
            }
        }

        return false;
    }
}
