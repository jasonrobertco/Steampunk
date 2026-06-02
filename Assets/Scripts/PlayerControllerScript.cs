using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerControllerScript : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 12f;
    [SerializeField] private float knockbackControlLockDuration = 0.12f;
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashRestoreCooldown = 0.15f;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private bool facesRightByDefault = true;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] [Range(0f, 1f)] private float groundedNormalThreshold = 0.35f;

    private float moveX;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private bool isGrounded;
    private float knockbackControlLockTimer;
    private float dashTimer;
    private float dashRestoreTimer;
    private bool hasDash = true;
    private bool dashResetPending;
    private Vector2 dashDirection = Vector2.right;
    private bool isFacingRight = true;
    private bool wasGrounded;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
    private ContactFilter2D groundContactFilter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        groundContactFilter = new ContactFilter2D
        {
            useTriggers = false
        };

        if (visualRoot == null)
        {
            Animator animator = GetComponentInChildren<Animator>();
            visualRoot = animator != null ? animator.transform : transform;
        }

        isFacingRight = facesRightByDefault;
        dashDirection = GetFacingDirection();
        isGrounded = CheckGrounded();
        wasGrounded = isGrounded;
        ApplyFacingDirection();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        moveX = moveInput.x;

        if (moveX < -0.01f)
        {
            isFacingRight = false;
            ApplyFacingDirection();
        }
        else if (moveX > 0.01f)
        {
            isFacingRight = true;
            ApplyFacingDirection();
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            dashDirection = GetCardinalDirection(moveInput);
        }
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        bool groundedNow = CheckGrounded();

        if (!wasGrounded && groundedNow)
        {
            ScheduleDashRestore();
        }

        isGrounded = groundedNow;
        wasGrounded = groundedNow;

        if (knockbackControlLockTimer > 0f)
        {
            knockbackControlLockTimer -= Time.fixedDeltaTime;
            return;
        }

        if (dashRestoreTimer > 0f)
        {
            dashRestoreTimer -= Time.fixedDeltaTime;
        }

        if (!hasDash && dashResetPending && dashRestoreTimer <= 0f)
        {
            hasDash = true;
            dashResetPending = false;
        }

        if (dashTimer > 0f)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = dashDirection * dashSpeed;
            return;
        }

        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }

    void OnDash(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (!hasDash)
            return;

        // Dash in the strongest held direction, or fall back to facing when idle.
        dashDirection = moveInput.sqrMagnitude > 0.01f
            ? GetCardinalDirection(moveInput)
            : GetFacingDirection();
        hasDash = false;
        dashTimer = dashDuration;
        dashResetPending = false;
        dashRestoreTimer = 0f;

        if (isGrounded)
        {
            ScheduleDashRestore();
        }

        rb.linearVelocity = dashDirection * dashSpeed;
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        dashTimer = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        knockbackControlLockTimer = knockbackControlLockDuration;
    }

    public void NotifyTeleported()
    {
        ScheduleDashRestore();
    }

    private Vector2 GetCardinalDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
        {
            return input.x >= 0f ? Vector2.right : Vector2.left;
        }

        return input.y >= 0f ? Vector2.up : Vector2.down;
    }

    private Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }

    private void ApplyFacingDirection()
    {
        if (visualRoot == null)
            return;

        Vector3 scale = visualRoot.localScale;
        float baseXScale = Mathf.Abs(scale.x);
        scale.x = (isFacingRight == facesRightByDefault ? 1f : -1f) * baseXScale;
        visualRoot.localScale = scale;
    }

    private void ScheduleDashRestore()
    {
        if (hasDash)
        {
            return;
        }

        dashResetPending = true;
        dashRestoreTimer = dashRestoreCooldown;
    }

    private bool CheckGrounded()
    {
        if (bodyCollider == null)
        {
            return false;
        }

        int hitCount = bodyCollider.Cast(Vector2.down, groundContactFilter, groundHits, groundCheckDistance);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = groundHits[i];

            if (hit.collider == null || !hit.collider.CompareTag("Ground"))
            {
                continue;
            }

            if (hit.normal.y >= groundedNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
