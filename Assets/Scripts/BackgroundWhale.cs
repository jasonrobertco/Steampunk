using UnityEngine;

public class BackgroundWhale : MonoBehaviour
{
    public float speed = 2f;
    public float height = 3f;
    public float edgePadding = 2f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("BackgroundWhale could not find the Main Camera.");
            enabled = false;
            return;
        }

        MoveToRightSide();
    }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        Vector3 leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, transform.position.z - mainCamera.transform.position.z));

        if (transform.position.x < leftEdge.x - edgePadding)
        {
            MoveToRightSide();
        }
    }

    void MoveToRightSide()
    {
        Vector3 rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, transform.position.z - mainCamera.transform.position.z));
        transform.position = new Vector3(rightEdge.x + edgePadding, height, transform.position.z);
    }
}
