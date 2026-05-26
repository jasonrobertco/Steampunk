using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float flashStepDuration = 0.08f;
    [SerializeField] private float flashLowAlpha = 0.5f;
    [SerializeField] private float knockbackHorizontalForce = 3.5f;
    [SerializeField] private float knockbackVerticalForce = 2f;

    private float invincibilityTimer;
    private SpriteRenderer[] spriteRenderers;
    private PlayerControllerScript playerController;
    private Coroutine flashRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        playerController = GetComponent<PlayerControllerScript>();
    }

    void Update()
    {
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, transform.position);
    }

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (invincibilityTimer > 0)
            return;

        currentHealth -= amount;
        Debug.Log("Damage taken. Current health = " + currentHealth);

        invincibilityTimer = invincibilityDuration;
        ApplyKnockback(damageSourcePosition);
        StartDamageFlash();

        if (currentHealth <= 0)
            Die();
    }

    private void ApplyKnockback(Vector2 damageSourcePosition)
    {
        if (playerController == null)
            return;

        float horizontalDirection = transform.position.x >= damageSourcePosition.x ? 1f : -1f;
        Vector2 knockbackForce = new Vector2(
            horizontalDirection * knockbackHorizontalForce,
            knockbackVerticalForce
        );

        playerController.ApplyKnockback(knockbackForce);
    }

    private void StartDamageFlash()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        float[] alphaSteps = { 1f, flashLowAlpha, 1f, flashLowAlpha, 1f };

        for (int i = 0; i < alphaSteps.Length; i++)
        {
            SetSpriteAlpha(alphaSteps[i]);

            if (i < alphaSteps.Length - 1)
                yield return new WaitForSeconds(flashStepDuration);
        }

        flashRoutine = null;
    }

    private void SetSpriteAlpha(float alpha)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
                continue;

            Color color = spriteRenderers[i].color;
            color.a = alpha;
            spriteRenderers[i].color = color;
        }
    }

    public float HealthPercent => (float)currentHealth / maxHealth;

    void Die() => Debug.Log("Player died");
}
