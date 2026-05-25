using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    [SerializeField] private int currentHealth;
    float invincibilityTimer;
    const float INVINCIBILITY_DURATION = 1f;

    void Start() => currentHealth = maxHealth;

    void Update()
    {
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
{
    if (invincibilityTimer > 0) return;

    currentHealth -= amount;
    Debug.Log("Damage taken. Current health = " + currentHealth);

    invincibilityTimer = INVINCIBILITY_DURATION;

    if (currentHealth <= 0) Die();
}

    public float HealthPercent => (float)currentHealth / maxHealth;

    void Die() => Debug.Log("Player died");
}
