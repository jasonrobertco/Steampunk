using UnityEngine;
using UnityEngine.InputSystem;

public class KnifeThrowTeleport : MonoBehaviour
{
    [Header("Knife Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask stickableLayers;
    [SerializeField] private bool stickToEnemies = false;
    [SerializeField] private float embedOffset = 0.05f;
    [SerializeField] private float minimumStickTravelDistance = 0.35f;
    [SerializeField] private float spawnOffsetFromBody = 0.1f;
    [SerializeField] private float maxKnifeLifetime = 0.5f;

    private GameObject activeKnife;
    private Collider2D playerCollider;
    private Rigidbody2D playerRb;

    private void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        playerRb = GetComponent<Rigidbody2D>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (activeKnife == null)
        {
            return;
        }

        if (!activeKnife)
        {
            activeKnife = null;
        }
    }

    public void OnFire(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (activeKnife != null)
            return;

        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }

        if (knifePrefab == null)
        {
            Debug.LogWarning("KnifeThrowTeleport: knifePrefab is not assigned.");
            return;
        }

        if (throwPoint == null)
        {
            Debug.LogWarning("KnifeThrowTeleport: throwPoint is not assigned.");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("KnifeThrowTeleport: mainCamera is not assigned.");
            return;
        }

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        Vector2 aimDirection = ((Vector2)mouseWorldPosition - (Vector2)throwPoint.position).normalized;
        Vector2 spawnPosition = GetKnifeSpawnPosition(aimDirection);
        Vector2 throwDirection = ((Vector2)mouseWorldPosition - spawnPosition).normalized;

        if (throwDirection.sqrMagnitude < 0.0001f)
        {
            throwDirection = aimDirection;
        }

        activeKnife = Instantiate(
            knifePrefab,
            spawnPosition,
            Quaternion.identity
        );

        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        activeKnife.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Rigidbody2D rb = activeKnife.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogWarning("KnifeThrowTeleport: Knife prefab is missing Rigidbody2D.");
            Destroy(activeKnife);
            activeKnife = null;
            return;
        }

        KnifeProjectile projectile = activeKnife.GetComponent<KnifeProjectile>();

        if (projectile == null)
        {
            Debug.LogWarning("KnifeThrowTeleport: Knife prefab is missing KnifeProjectile.");
            Destroy(activeKnife);
            activeKnife = null;
            return;
        }

        // Pass the thrower and stick settings to the projectile so it can ignore
        // the player and lock itself in place on approved surfaces.
        projectile.Initialize(
            transform,
            stickableLayers,
            stickToEnemies,
            embedOffset,
            throwDirection,
            minimumStickTravelDistance,
            maxKnifeLifetime
        );

        rb.linearVelocity = throwDirection * throwSpeed;
    }

    private Vector2 GetKnifeSpawnPosition(Vector2 throwDirection)
    {
        if (playerCollider == null || throwDirection.sqrMagnitude < 0.0001f)
        {
            return throwPoint != null ? throwPoint.position : transform.position;
        }

        Vector2 bodyEdge = playerCollider.ClosestPoint(
            (Vector2)playerCollider.bounds.center + throwDirection * 2f
        );

        return bodyEdge + (throwDirection.normalized * spawnOffsetFromBody);
    }

    public void OnTeleport(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (activeKnife == null)
            return;

        transform.position = activeKnife.transform.position;

        // Reset carried momentum so gravity starts naturally from the new spot.
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        Destroy(activeKnife);
        activeKnife = null;
    }
}
