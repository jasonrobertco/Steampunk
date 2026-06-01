using Cinemachine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool followY = true;
    [SerializeField] private float verticalDeadZone = 1.5f;
    [SerializeField] private bool confineToLevelSpace = true;
    [SerializeField] private string levelBoundsTag = "Ground";
    [SerializeField] private Collider2D levelBoundsOverride;
    [SerializeField] private float horizontalDamping = 0.35f;
    [SerializeField] private float verticalDamping = 0.45f;
    [Header("Shake")]
    [SerializeField] private float defaultShakeAmplitude = 1.1f;
    [SerializeField] private float defaultShakeFrequency = 5f;
    [SerializeField] private float defaultShakeDuration = 0.08f;

    private const string FollowAnchorName = "__CameraFollowAnchor";
    private const string VirtualCameraName = "__CinemachineVirtualCamera";
    private const string BoundsName = "__GeneratedCameraBounds";

    private Camera sceneCamera;
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineConfiner2D confiner;
    private CinemachineBasicMultiChannelPerlin noise;
    private Transform followAnchor;
    private BoxCollider2D generatedBoundsCollider;
    private float trackedY;
    private float lockedY;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeAmplitude;
    private float shakeFrequency;

    private void Start()
    {
        sceneCamera = GetComponent<Camera>();

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        lockedY = transform.position.y + offset.y;
        trackedY = lockedY;

        EnsureCinemachineRig();
        RefreshConfinerBounds();
        UpdateFollowAnchor();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                EnsureCinemachineRig();
                RefreshConfinerBounds();
            }
        }

        if (target == null || followAnchor == null)
            return;

        UpdateFollowAnchor();
        UpdateShake();
    }

    private void EnsureCinemachineRig()
    {
        if (sceneCamera == null)
            sceneCamera = GetComponent<Camera>();

        if (sceneCamera == null)
            return;

        CinemachineBrain brain = GetComponent<CinemachineBrain>();
        if (brain == null)
            brain = gameObject.AddComponent<CinemachineBrain>();

        brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
        brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;

        GameObject existingAnchor = GameObject.Find(FollowAnchorName);
        followAnchor = existingAnchor != null ? existingAnchor.transform : null;
        if (followAnchor == null)
        {
            GameObject anchorObject = new GameObject(FollowAnchorName);
            followAnchor = anchorObject.transform;
        }

        GameObject existingVirtualCamera = GameObject.Find(VirtualCameraName);
        Transform virtualCameraTransform = existingVirtualCamera != null ? existingVirtualCamera.transform : null;
        if (virtualCameraTransform == null)
        {
            GameObject virtualCameraObject = new GameObject(VirtualCameraName);
            virtualCameraTransform = virtualCameraObject.transform;
        }

        virtualCamera = virtualCameraTransform.GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera == null)
            virtualCamera = virtualCameraTransform.gameObject.AddComponent<CinemachineVirtualCamera>();

        virtualCamera.Priority = 100;
        virtualCamera.Follow = followAnchor;
        virtualCamera.LookAt = null;
        virtualCamera.m_Lens.Orthographic = sceneCamera.orthographic;
        virtualCamera.m_Lens.OrthographicSize = sceneCamera.orthographicSize;

        CinemachineFramingTransposer framing = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framing == null)
            framing = virtualCamera.AddCinemachineComponent<CinemachineFramingTransposer>();

        framing.m_TrackedObjectOffset = Vector3.zero;
        framing.m_CameraDistance = Mathf.Max(Mathf.Abs(offset.z), 0.01f);
        framing.m_XDamping = horizontalDamping > 0f ? horizontalDamping : ConvertLegacySmoothSpeed(smoothSpeed);
        framing.m_YDamping = verticalDamping > 0f ? verticalDamping : ConvertLegacySmoothSpeed(smoothSpeed);
        framing.m_ZDamping = 0f;
        framing.m_DeadZoneWidth = 0f;
        framing.m_DeadZoneHeight = 0f;
        framing.m_SoftZoneWidth = 0.8f;
        framing.m_SoftZoneHeight = 0.8f;
        framing.m_UnlimitedSoftZone = false;

        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null)
            noise = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;

        confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        if (confineToLevelSpace)
        {
            if (confiner == null)
                confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner2D>();
        }
        else if (confiner != null)
        {
            confiner.enabled = false;
        }
    }

    private void UpdateFollowAnchor()
    {
        Vector3 anchorPosition = target.position;
        anchorPosition.x += offset.x;
        anchorPosition.z = 0f;

        if (followY)
        {
            float desiredY = target.position.y + offset.y;
            float upperLimit = trackedY + verticalDeadZone;
            float lowerLimit = trackedY - verticalDeadZone;

            // Only move vertically once the player leaves a comfortable band.
            if (desiredY > upperLimit)
            {
                trackedY = desiredY - verticalDeadZone;
            }
            else if (desiredY < lowerLimit)
            {
                trackedY = desiredY + verticalDeadZone;
            }

            anchorPosition.y = trackedY;
        }
        else
        {
            anchorPosition.y = lockedY;
        }

        followAnchor.position = anchorPosition;
    }

    private void RefreshConfinerBounds()
    {
        if (!confineToLevelSpace || virtualCamera == null || confiner == null)
            return;

        Collider2D boundingShape = levelBoundsOverride != null
            ? levelBoundsOverride
            : BuildLevelBoundsCollider();

        if (boundingShape == null)
        {
            confiner.enabled = false;
            return;
        }

        confiner.enabled = true;
        confiner.m_BoundingShape2D = boundingShape;
        confiner.m_Damping = 0f;
        confiner.InvalidateCache();
    }

    public void PlayImpactShake()
    {
        PlayImpactShake(defaultShakeAmplitude, defaultShakeFrequency, defaultShakeDuration);
    }

    public void PlayImpactShake(float amplitude, float frequency, float duration)
    {
        if (noise == null)
            EnsureCinemachineRig();

        if (noise == null)
            return;

        shakeAmplitude = Mathf.Max(0f, amplitude);
        shakeFrequency = Mathf.Max(0f, frequency);
        shakeDuration = Mathf.Max(0.01f, duration);
        shakeTimer = shakeDuration;

        noise.m_AmplitudeGain = shakeAmplitude;
        noise.m_FrequencyGain = shakeFrequency;
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

    private Collider2D BuildLevelBoundsCollider()
    {
        Collider2D[] colliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        Bounds combinedBounds = default;
        bool hasBounds = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider2D = colliders[i];
            if (collider2D == null || !collider2D.enabled || collider2D.isTrigger)
                continue;

            if (!string.IsNullOrEmpty(levelBoundsTag) && !collider2D.CompareTag(levelBoundsTag))
                continue;

            if (target != null && collider2D.transform.IsChildOf(target))
                continue;

            if (!hasBounds)
            {
                combinedBounds = collider2D.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(collider2D.bounds);
            }
        }

        if (!hasBounds)
            return null;

        if (generatedBoundsCollider == null)
        {
            GameObject boundsObject = new GameObject(BoundsName);
            generatedBoundsCollider = boundsObject.AddComponent<BoxCollider2D>();
            generatedBoundsCollider.isTrigger = true;
        }

        generatedBoundsCollider.transform.position = combinedBounds.center;
        generatedBoundsCollider.size = new Vector2(
            Mathf.Max(combinedBounds.size.x, 1f),
            Mathf.Max(combinedBounds.size.y, 1f)
        );

        return generatedBoundsCollider;
    }

    private float ConvertLegacySmoothSpeed(float legacySpeed)
    {
        if (legacySpeed <= 0f)
            return 0f;

        return 1f / legacySpeed;
    }
}
