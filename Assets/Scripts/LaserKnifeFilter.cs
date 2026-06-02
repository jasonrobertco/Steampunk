using UnityEngine;

/// <summary>
/// Makes a trigger collider act like a non-solid laser filter for thrown knives.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LaserKnifeFilter : MonoBehaviour
{
    private void Reset()
    {
        SetColliderTriggerState();
    }

    private void Awake()
    {
        SetColliderTriggerState();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        KnifeProjectile knifeProjectile = other.GetComponentInParent<KnifeProjectile>();

        if (knifeProjectile == null)
        {
            return;
        }

        knifeProjectile.AbsorbIntoLaser();
    }

    private void SetColliderTriggerState()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
