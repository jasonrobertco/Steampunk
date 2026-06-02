using UnityEngine;

public class InstantDeathHazard : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        KillPlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        KillPlayer(other);
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
