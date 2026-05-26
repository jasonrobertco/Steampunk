using UnityEngine;

/// <summary>
/// Handles the thrown knife's collision behavior so it can stick in place
/// without affecting the player's throw/teleport flow.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class KnifeProjectile : MonoBehaviour
{
    private const string GroundTag = "Ground";
    private const string StickableTag = "Stickable";
    private const string EnemyTag = "Enemy";
    private const string EnemyStickableTag = "EnemyStickable";

    private Rigidbody2D rb;
    private Collider2D knifeCollider;
    private Transform ownerRoot;
    private LayerMask stickableLayers;
    private bool stickToEnemies;
    private float embedOffset;
    private Vector2 lastMoveDirection = Vector2.right;
    private bool hasStuck;
    private Transform stuckTransform;
    private Vector3 stuckLocalPosition;
    private Quaternion stuckLocalRotation;
    private Vector3 spawnPosition;
    private float minimumStickTravelDistance;

    public bool HasStuck => hasStuck;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        knifeCollider = GetComponent<Collider2D>();
    }

    public void Initialize(
        Transform owner,
        LayerMask allowedStickableLayers,
        bool allowEnemyStick,
        float impactEmbedOffset,
        Vector2 initialDirection,
        float minStickDistance)
    {
        ownerRoot = owner;
        stickableLayers = allowedStickableLayers;
        stickToEnemies = allowEnemyStick;
        embedOffset = impactEmbedOffset;
        minimumStickTravelDistance = Mathf.Max(0f, minStickDistance);
        spawnPosition = transform.position;

        if (initialDirection.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = initialDirection.normalized;
        }

        IgnoreOwnerCollisions();
    }

    private void FixedUpdate()
    {
        if (hasStuck)
        {
            return;
        }

        // Keep tracking the latest travel direction so the knife can remain
        // visually aligned when it embeds into a surface.
        if (rb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = rb.linearVelocity.normalized;
        }
    }

    private void LateUpdate()
    {
        if (!hasStuck || stuckTransform == null)
        {
            return;
        }

        // Follow the struck object without becoming its child so the knife does
        // not inherit non-uniform scale and appear flattened.
        transform.position = stuckTransform.TransformPoint(stuckLocalPosition);
        transform.rotation = stuckTransform.rotation * stuckLocalRotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasStuck || collision.contactCount == 0)
        {
            return;
        }

        if (!CanStickYet())
        {
            return;
        }

        Collider2D hitCollider = collision.collider;

        if (hitCollider == null || IsOwner(hitCollider))
        {
            return;
        }

        if (TryDestroyEnemy(hitCollider))
        {
            return;
        }

        if (!IsStickableTarget(hitCollider))
        {
            return;
        }

        StickIntoSurface(collision);
    }

    private bool CanStickYet()
    {
        return ((Vector2)(transform.position - spawnPosition)).sqrMagnitude >=
            minimumStickTravelDistance * minimumStickTravelDistance;
    }

    private bool IsStickableTarget(Collider2D hitCollider)
    {
        GameObject hitObject = hitCollider.attachedRigidbody != null
            ? hitCollider.attachedRigidbody.gameObject
            : hitCollider.gameObject;

        if (hitObject == null)
        {
            return false;
        }

        string hitTag = hitObject.tag;

        if (hitTag == EnemyStickableTag)
        {
            return true;
        }

        if (hitTag == EnemyTag)
        {
            return stickToEnemies;
        }

        int hitLayerMask = 1 << hitObject.layer;
        bool isLayerStickable = (stickableLayers.value & hitLayerMask) != 0;

        // Tag fallback keeps the current sample scene working even before new
        // layers are assigned in the Inspector.
        return isLayerStickable
            || hitTag == GroundTag
            || hitTag == StickableTag;
    }

    private bool IsOwner(Collider2D hitCollider)
    {
        if (ownerRoot == null)
        {
            return false;
        }

        Transform hitRoot = hitCollider.attachedRigidbody != null
            ? hitCollider.attachedRigidbody.transform.root
            : hitCollider.transform.root;

        return hitRoot == ownerRoot.root;
    }

    private bool TryDestroyEnemy(Collider2D hitCollider)
    {
        GameObject hitObject = hitCollider.attachedRigidbody != null
            ? hitCollider.attachedRigidbody.gameObject
            : hitCollider.gameObject;

        if (hitObject == null)
        {
            return false;
        }

        RobotV1Script robot = hitObject.GetComponentInParent<RobotV1Script>();
        GameObject enemyObject = robot != null ? robot.gameObject : hitObject;

        string hitTag = hitObject.tag;
        string enemyTag = enemyObject.tag;
        bool isTaggedEnemy = enemyTag == EnemyTag || enemyTag == EnemyStickableTag;
        bool isRobotEnemy = robot != null && (
            hitTag == EnemyTag
            || hitTag == EnemyStickableTag
            || enemyTag == EnemyTag
            || enemyTag == EnemyStickableTag
        );

        if (!isTaggedEnemy && !isRobotEnemy)
        {
            return false;
        }

        Debug.Log($"Knife hit robot: {enemyObject.name}");
        Destroy(enemyObject);
        Destroy(gameObject);
        return true;
    }

    private void StickIntoSurface(Collision2D collision)
    {
        hasStuck = true;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 embedDirection = lastMoveDirection.sqrMagnitude > 0.0001f
            ? lastMoveDirection
            : (Vector2)transform.right;

        // Position the knife at the impact point with a small forward offset so
        // it looks embedded instead of hovering.
        Vector2 stuckPosition = contact.point + (embedDirection.normalized * embedOffset);
        transform.position = stuckPosition;

        if (embedDirection.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(embedDirection.y, embedDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Transform hitTransform = collision.rigidbody != null
            ? collision.rigidbody.transform
            : collision.transform;

        if (hitTransform != null)
        {
            stuckTransform = hitTransform;
            stuckLocalPosition = hitTransform.InverseTransformPoint(transform.position);
            stuckLocalRotation = Quaternion.Inverse(hitTransform.rotation) * transform.rotation;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Keep the rigidbody available for teleport position tracking, but turn
        // off further contact resolution so the knife cannot slide or retrigger.
        if (knifeCollider != null)
        {
            knifeCollider.enabled = false;
        }
    }

    private void IgnoreOwnerCollisions()
    {
        if (ownerRoot == null || knifeCollider == null)
        {
            return;
        }

        Collider2D[] ownerColliders = ownerRoot.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics2D.IgnoreCollision(knifeCollider, ownerCollider, true);
            }
        }
    }
}
