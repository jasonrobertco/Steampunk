using UnityEngine;

public class InstantDeathHazard : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        RemoveKnife(collision.collider);
        KillPlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RemoveKnife(other);
        KillPlayer(other);
    }

    private static void RemoveKnife(Collider2D collider)
    {
        if (collider == null)
            return;

        KnifeProjectile knifeProjectile = collider.GetComponentInParent<KnifeProjectile>();
        if (knifeProjectile == null)
            return;

        Object.Destroy(knifeProjectile.gameObject);
    }

    private static void KillPlayer(Collider2D collider)
    {
        if (collider == null)
            return;

        PlayerHealth playerHealth = collider.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            return;

        playerHealth.KillInstantly();
    }
}
