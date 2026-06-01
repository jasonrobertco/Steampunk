using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a simple persistent HUD label that shows the player's remaining lives.
/// </summary>
public class LivesHUD : MonoBehaviour
{
    private static LivesHUD instance;

    private Canvas livesCanvas;
    private Text livesText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstanceExists();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
            return;

        GameObject hudObject = new GameObject(nameof(LivesHUD));
        instance = hudObject.AddComponent<LivesHUD>();
        DontDestroyOnLoad(hudObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        CreateHud();
    }

    private void Update()
    {
        if (livesText == null)
        {
            CreateHud();
        }

        if (livesCanvas != null)
        {
            livesCanvas.enabled = GameSession.Instance.ShouldShowLivesHud;
        }

        if (livesText != null)
        {
            livesText.text = $"Lives: {GameSession.Instance.CurrentLives}";
        }
    }

    private void CreateHud()
    {
        Canvas existingCanvas = GetComponentInChildren<Canvas>();

        if (existingCanvas == null)
        {
            GameObject canvasObject = new GameObject("LivesCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            existingCanvas = canvas;
        }

        livesCanvas = existingCanvas;

        GameObject textObject = new GameObject("LivesText");
        textObject.transform.SetParent(existingCanvas.transform, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-32f, -24f);
        rectTransform.sizeDelta = new Vector2(260f, 44f);

        livesText = textObject.AddComponent<Text>();
        livesText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        livesText.fontSize = 28;
        livesText.alignment = TextAnchor.UpperRight;
        livesText.horizontalOverflow = HorizontalWrapMode.Overflow;
        livesText.verticalOverflow = VerticalWrapMode.Overflow;
        livesText.color = new Color(0.96f, 0.96f, 0.96f, 1f);
        livesText.text = $"Lives: {GameSession.StartingLives}";
    }
}
