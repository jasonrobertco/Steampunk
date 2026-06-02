using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform target;
    [SerializeField] private CinemachineVirtualCamera virtualCameraOverride;
    [SerializeField] private CinemachineConfiner2D confinerOverride;

    [Header("Bounds")]
    [SerializeField] private bool confineToLevelSpace = true;
    [SerializeField] private Collider2D levelBoundsOverride;

    [Header("Shake")]
    [SerializeField] private float defaultShakeAmplitude = 1.1f;
    [SerializeField] private float defaultShakeFrequency = 5f;
    [SerializeField] private float defaultShakeDuration = 0.08f;

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineConfiner2D confiner;
    private CinemachineBasicMultiChannelPerlin noise;
    private Camera sceneCamera;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeAmplitude;
    private float shakeFrequency;

    private void Awake()
    {
        sceneCamera = GetComponent<Camera>();
        EnsureBrain();
        ResolveReferences();
        ApplyCameraBindings();
    }

    private void OnEnable()
    {
        ResolveReferences();
        ApplyCameraBindings();
    }

    private void Start()
    {
        RefreshConfinerBounds();
    }

    private void LateUpdate()
    {
        if (target == null || virtualCamera == null)
        {
            ResolveReferences();
            ApplyCameraBindings();
        }

        UpdateShake();
    }

    public void PlayImpactShake()
    {
        PlayImpactShake(defaultShakeAmplitude, defaultShakeFrequency, defaultShakeDuration);
    }

    public void PlayImpactShake(float amplitude, float frequency, float duration)
    {
        if (noise == null)
        {
            ResolveReferences();
            ApplyCameraBindings();
        }

        if (noise == null)
            return;

        shakeAmplitude = Mathf.Max(0f, amplitude);
        shakeFrequency = Mathf.Max(0f, frequency);
        shakeDuration = Mathf.Max(0.01f, duration);
        shakeTimer = shakeDuration;

        noise.m_AmplitudeGain = shakeAmplitude;
        noise.m_FrequencyGain = shakeFrequency;
    }

    private void EnsureBrain()
    {
        CinemachineBrain brain = GetComponent<CinemachineBrain>();
        if (brain == null)
            brain = gameObject.AddComponent<CinemachineBrain>();

        brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
        brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
    }

    private void ResolveReferences()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (virtualCameraOverride != null)
        {
            virtualCamera = virtualCameraOverride;
        }
        else if (virtualCamera == null)
        {
            virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        }

        if (confinerOverride != null)
        {
            confiner = confinerOverride;
        }
        else if (virtualCamera != null)
        {
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        }

        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void ApplyCameraBindings()
    {
        if (virtualCamera == null)
            return;

        if (target != null)
            virtualCamera.Follow = target;

        virtualCamera.LookAt = null;
        SyncVirtualCameraLens();

        if (confineToLevelSpace)
            RefreshConfinerBounds();
        else if (confiner != null)
            confiner.enabled = false;

        if (noise != null && shakeTimer <= 0f)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }

    private void RefreshConfinerBounds()
    {
        if (!confineToLevelSpace || confiner == null)
            return;

        Collider2D boundsShape = levelBoundsOverride != null
            ? levelBoundsOverride
            : confiner.m_BoundingShape2D;

        if (boundsShape == null)
        {
            Debug.LogWarning(
                "CameraFollow: assign a PolygonCollider2D or CompositeCollider2D either on levelBoundsOverride or directly on the CinemachineConfiner2D.",
                this);
            confiner.enabled = false;
            return;
        }

        if (boundsShape is not PolygonCollider2D && boundsShape is not CompositeCollider2D)
        {
            Debug.LogWarning(
                "CameraFollow: confiner bounds must be a PolygonCollider2D or CompositeCollider2D.",
                boundsShape);
            confiner.enabled = false;
            return;
        }

        confiner.enabled = true;
        confiner.m_BoundingShape2D = boundsShape;
        SyncVirtualCameraLens();
        confiner.InvalidateCache();
    }

    private void UpdateShake()
    {
        if (noise == null)
            return;

        if (shakeTimer <= 0f)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
            return;
        }

        shakeTimer -= Time.deltaTime;
        float normalizedTimeRemaining = Mathf.Clamp01(shakeTimer / shakeDuration);
        float easedAmplitude = shakeAmplitude * normalizedTimeRemaining * normalizedTimeRemaining;

        noise.m_AmplitudeGain = easedAmplitude;
        noise.m_FrequencyGain = shakeFrequency;

        if (shakeTimer <= 0f)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }

    private void SyncVirtualCameraLens()
    {
        if (sceneCamera == null || virtualCamera == null)
            return;

        virtualCamera.m_Lens.Orthographic = sceneCamera.orthographic;
        virtualCamera.m_Lens.OrthographicSize = sceneCamera.orthographicSize;
    }
}
