using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerScript : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    [SerializeField] private float knockbackControlLockDuration = 0.12f;

    private float moveX;
    private Rigidbody2D rb;
    private bool isGrounded;
    private float knockbackControlLockTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveX = input.x;
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

        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        knockbackControlLockTimer = knockbackControlLockDuration;
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
