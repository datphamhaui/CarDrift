using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform carTransform;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 15f, -8f);
    [Range(60f, 85f)]
    public float lookDownAngle = 65f;
    [Range(1f, 15f)]
    public float followSpeed = 5f;
    [Range(0f, 5f)]
    public float rotateSpeed = 2f;

    float currentYRotation;

    void LateUpdate()
    {
        if (carTransform == null) return;

        // Smoothly rotate camera to follow car's forward direction
        float targetYRotation = carTransform.eulerAngles.y;
        if (rotateSpeed > 0f)
        {
            currentYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, rotateSpeed * Time.deltaTime);
        }

        // Calculate camera position based on car position + rotated offset
        Quaternion rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        Vector3 rotatedOffset = rotation * offset;
        Vector3 targetPosition = carTransform.position + rotatedOffset;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Look at the car from above
        Quaternion targetRotation = Quaternion.Euler(lookDownAngle, currentYRotation, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
    }

    void Start()
    {
        if (carTransform != null)
        {
            currentYRotation = carTransform.eulerAngles.y;

            // Set initial position directly
            Quaternion rotation = Quaternion.Euler(0f, currentYRotation, 0f);
            Vector3 rotatedOffset = rotation * offset;
            transform.position = carTransform.position + rotatedOffset;
            transform.rotation = Quaternion.Euler(lookDownAngle, currentYRotation, 0f);
        }
    }
}
