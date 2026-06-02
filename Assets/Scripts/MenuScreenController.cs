using UnityEngine;

public class MenuScreenController : MonoBehaviour
{
    public void StartGame()
    {
        GameSession.Instance.StartNewGame();
    }

    public void ReturnToStartScreen()
    {
        GameSession.Instance.ReturnToStartScreen();
    }
}
