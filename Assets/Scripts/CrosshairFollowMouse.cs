using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairFollowMouse : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Cursor.visible = false;
    }

    private void Update()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        transform.position = mouseWorldPosition;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }
}