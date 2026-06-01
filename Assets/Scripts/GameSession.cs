using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps the player's life count across scene reloads so a room can fully reset
/// while the run's remaining lives stay intact.
/// </summary>
public class GameSession : MonoBehaviour
{
    public const int StartingLives = 7;
    public const string StartSceneName = "StartScreen";
    public const string GameplaySceneName = "SampleScene";
    public const string GameOverSceneName = "GameOverScreen";

    private static GameSession instance;

    [SerializeField] private int currentLives = StartingLives;

    public static GameSession Instance
    {
        get
        {
            EnsureInstanceExists();
            return instance;
        }
    }

    public int CurrentLives => currentLives;
    public bool ShouldShowLivesHud
    {
        get
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            return activeSceneName != StartSceneName && activeSceneName != GameOverSceneName;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstanceExists();
    }

    private static void EnsureInstanceExists()
    {
        if (instance != null)
            return;

        GameObject sessionObject = new GameObject(nameof(GameSession));
        instance = sessionObject.AddComponent<GameSession>();
        DontDestroyOnLoad(sessionObject);
    }

    public void LoseLifeAndReloadRoom()
    {
        currentLives = Mathf.Max(0, currentLives - 1);

        if (currentLives <= 0)
        {
            SceneManager.LoadScene(GameOverSceneName);
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void StartNewGame()
    {
        currentLives = StartingLives;
        SceneManager.LoadScene(GameplaySceneName);
    }

    public void ReturnToStartScreen()
    {
        SceneManager.LoadScene(StartSceneName);
    }
}
