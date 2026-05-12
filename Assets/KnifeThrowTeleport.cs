using UnityEngine;
using UnityEngine.InputSystem;

public class KnifeThrowTeleport : MonoBehaviour
{
    [Header("Knife Settings")]
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwSpeed = 10f;

    private GameObject activeKnife;

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

        activeKnife = Instantiate(
            knifePrefab,
            throwPoint.position,
            throwPoint.rotation
        );

        Rigidbody2D rb = activeKnife.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogWarning("KnifeThrowTeleport: Knife prefab is missing Rigidbody2D.");
            Destroy(activeKnife);
            activeKnife = null;
            return;
        }

        rb.linearVelocity = transform.right * throwSpeed;
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
