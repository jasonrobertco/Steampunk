using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool followY = true;
    [SerializeField] private float verticalDeadZone = 1.5f;

    private float trackedY;

    private void Start()
    {
        trackedY = transform.position.y;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        if (followY)
        {
            float upperLimit = trackedY + verticalDeadZone;
            float lowerLimit = trackedY - verticalDeadZone;

            // Only move vertically once the player leaves a comfortable band.
            if (desiredPosition.y > upperLimit)
            {
                trackedY = desiredPosition.y - verticalDeadZone;
            }
            else if (desiredPosition.y < lowerLimit)
            {
                trackedY = desiredPosition.y + verticalDeadZone;
            }

            desiredPosition.y = trackedY;
        }
        else
        {
            desiredPosition.y = transform.position.y;
        }

        desiredPosition.z = offset.z;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
