using UnityEngine;
using UnityEngine.InputSystem;

public class KnifeThrowTeleport : MonoBehaviour
{
    [Header("Knife Settings")]
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private Camera mainCamera;

    private GameObject activeKnife;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    public void OnFire(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (activeKnife != null)
            return;

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

        Vector2 throwDirection = ((Vector2)mouseWorldPosition - (Vector2)throwPoint.position).normalized;

        activeKnife = Instantiate(
            knifePrefab,
            throwPoint.position,
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

        rb.linearVelocity = throwDirection * throwSpeed;
    }

    public void OnTeleport(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (activeKnife == null)
            return;

        transform.position = activeKnife.transform.position;

        Destroy(activeKnife);
        activeKnife = null;
    }
}