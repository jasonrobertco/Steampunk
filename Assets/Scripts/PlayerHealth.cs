using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float flashStepDuration = 0.08f;
    [SerializeField] private float flashLowAlpha = 0.5f;
    [SerializeField] private float knockbackHorizontalForce = 3.5f;
    [SerializeField] private float knockbackVerticalForce = 2f;
    [SerializeField] private int damageParticleCount = 12;
    [SerializeField] private float damageParticleLifetime = 0.35f;
    [SerializeField] private float damageParticleSpeed = 4f;
    [SerializeField] private float damageParticleSize = 0.14f;
    [SerializeField] private Color damageParticleStartColor = new Color(1f, 0.82f, 0.35f, 1f);
    [SerializeField] private Color damageParticleEndColor = new Color(1f, 0.35f, 0.2f, 0f);
    [SerializeField] private float fallDeathY = -8f;
    [SerializeField] private Sprite[] deathExplosionFrames;
    [SerializeField] private float deathExplosionFrameRate = 15f;
    [SerializeField] private Vector3 deathExplosionOffset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private Vector3 deathExplosionScale = new Vector3(2f, 2f, 1f);
    [SerializeField] private int deathExplosionSortingOrderOffset = 3;
    [SerializeField] private float deathReloadDelay = 0.8f;

    private float invincibilityTimer;
    private SpriteRenderer[] spriteRenderers;
    private SpriteRenderer primarySpriteRenderer;
    private PlayerControllerScript playerController;
    private Rigidbody2D playerRigidbody;
    private Collider2D[] playerColliders;
    private Coroutine flashRoutine;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        primarySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerController = GetComponent<PlayerControllerScript>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerColliders = GetComponentsInChildren<Collider2D>(true);
    }

    void Update()
    {
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;

        if (!isDead && transform.position.y < fallDeathY)
            LoseLife();
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, transform.position);
    }

    public void TakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (isDead)
            return;

        if (invincibilityTimer > 0)
            return;

        currentHealth -= amount;
        Debug.Log("Damage taken. Current health = " + currentHealth);

        invincibilityTimer = invincibilityDuration;
        ApplyKnockback(damageSourcePosition);
        SpawnDamageParticles();
        StartDamageFlash();

        if (currentHealth <= 0)
            LoseLife();
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

    private void SpawnDamageParticles()
    {
        GameObject particleObject = new GameObject("PlayerDamageParticles");
        particleObject.transform.position = transform.position;

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = damageParticleLifetime;
        main.startLifetime = damageParticleLifetime;
        main.startSpeed = damageParticleSpeed;
        main.startSize = damageParticleSize;
        main.startColor = new ParticleSystem.MinMaxGradient(damageParticleStartColor, damageParticleEndColor);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)damageParticleCount)
        });

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fadeGradient = new Gradient();
        fadeGradient.SetKeys(
            new[]
            {
                new GradientColorKey(damageParticleStartColor, 0f),
                new GradientColorKey(damageParticleEndColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(damageParticleStartColor.a, 0f),
                new GradientAlphaKey(damageParticleEndColor.a, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fadeGradient);

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        particles.Play();
        Destroy(particleObject, damageParticleLifetime + 0.2f);
    }

    public float HealthPercent => (float)currentHealth / maxHealth;

    public void KillInstantly()
    {
        LoseLife();
    }

    private void LoseLife()
    {
        if (isDead)
            return;

        isDead = true;
        StartCoroutine(LoseLifeRoutine());
    }

    private IEnumerator LoseLifeRoutine()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        SetSpriteAlpha(1f);
        FreezePlayer();
        SpawnDeathExplosion();
        yield return new WaitForSeconds(deathReloadDelay);
        GameSession.Instance.LoseLifeAndReloadRoom();
    }

    private void FreezePlayer()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
            playerRigidbody.simulated = false;
        }

        if (playerColliders == null)
            return;

        for (int i = 0; i < playerColliders.Length; i++)
        {
            if (playerColliders[i] != null)
            {
                playerColliders[i].enabled = false;
            }
        }
    }

    private void SpawnDeathExplosion()
    {
        if (deathExplosionFrames == null || deathExplosionFrames.Length == 0)
            return;

        GameObject effectObject = new GameObject("PlayerDeathExplosion");
        effectObject.transform.position = transform.position + deathExplosionOffset;
        effectObject.transform.localScale = deathExplosionScale;

        SpriteRenderer effectRenderer = effectObject.AddComponent<SpriteRenderer>();
        effectRenderer.sprite = deathExplosionFrames[0];

        if (primarySpriteRenderer != null)
        {
            effectRenderer.sharedMaterial = primarySpriteRenderer.sharedMaterial;
            effectRenderer.sortingLayerID = primarySpriteRenderer.sortingLayerID;
            effectRenderer.sortingOrder = primarySpriteRenderer.sortingOrder + deathExplosionSortingOrderOffset;
        }

        OneShotSpriteEffect effect = effectObject.AddComponent<OneShotSpriteEffect>();
        effect.Initialize(effectRenderer, deathExplosionFrames, deathExplosionFrameRate);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (deathExplosionFrames != null && deathExplosionFrames.Length > 0)
            return;

        const string effectFolder = "Assets/Sprites/Super Pixel Effects Mini Pack 1/PNG/fx1_explosion_small_orange";
        string[] frameGuids = AssetDatabase.FindAssets("t:Sprite", new[] { effectFolder });

        if (frameGuids == null || frameGuids.Length == 0)
            return;

        System.Array.Sort(frameGuids, CompareFrameGuids);
        deathExplosionFrames = new Sprite[frameGuids.Length];

        for (int i = 0; i < frameGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(frameGuids[i]);
            deathExplosionFrames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
    }

    private static int CompareFrameGuids(string leftGuid, string rightGuid)
    {
        string leftPath = AssetDatabase.GUIDToAssetPath(leftGuid);
        string rightPath = AssetDatabase.GUIDToAssetPath(rightGuid);
        return string.CompareOrdinal(leftPath, rightPath);
    }
#endif
}
