using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleMenuScreen : MonoBehaviour
{
    [SerializeField] private bool createEventSystemIfMissing = true;

    private void Awake()
    {
        if (!createEventSystemIfMissing)
            return;

        EnsureEventSystemExists();
    }

    public void StartGame()
    {
        GameSession.Instance.StartNewGame();
    }

    public void ReturnToStartScreen()
    {
        GameSession.Instance.ReturnToStartScreen();
    }

    private static void EnsureEventSystemExists()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();

        Type inputSystemModuleType = Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"
        );

        if (inputSystemModuleType != null)
        {
            eventSystemObject.AddComponent(inputSystemModuleType);
            return;
        }

        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}
