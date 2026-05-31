using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerScript : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 12f;
    [SerializeField] private float knockbackControlLockDuration = 0.12f;
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashRestoreCooldown = 1f;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private bool facesRightByDefault = true;

    private float moveX;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool isGrounded;
    private float knockbackControlLockTimer;
    private float dashTimer;
    private float dashRestoreTimer;
    private bool hasDash = true;
    private Vector2 dashDirection = Vector2.right;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (visualRoot == null)
        {
            Animator animator = GetComponentInChildren<Animator>();
            visualRoot = animator != null ? animator.transform : transform;
        }

        isFacingRight = facesRightByDefault;
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
        if (knockbackControlLockTimer > 0f)
        {
            knockbackControlLockTimer -= Time.fixedDeltaTime;
            return;
        }

        if (dashRestoreTimer > 0f)
        {
            dashRestoreTimer -= Time.fixedDeltaTime;
        }

        // The dash only comes back after the cooldown has finished and the player is grounded.
        if (!hasDash && isGrounded && dashRestoreTimer <= 0f)
        {
            hasDash = true;
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

        if (moveInput.sqrMagnitude <= 0.01f)
            return;

        // Dash in the strongest held direction, then spend the stored dash.
        dashDirection = GetCardinalDirection(moveInput);
        hasDash = false;
        dashTimer = dashDuration;
        dashRestoreTimer = dashRestoreCooldown;
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

    private Vector2 GetCardinalDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
        {
            return input.x >= 0f ? Vector2.right : Vector2.left;
        }

        return input.y >= 0f ? Vector2.up : Vector2.down;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
