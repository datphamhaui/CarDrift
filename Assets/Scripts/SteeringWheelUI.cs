using UnityEngine;
using UnityEngine.EventSystems;

public class SteeringWheelUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Settings")]
    [Range(60f, 180f)]
    public float maxRotationAngle = 120f;
    [Range(2f, 20f)]
    public float returnSpeed = 10f;
    [Tooltip("How many pixels of horizontal drag equals full steering")]
    [Range(50f, 400f)]
    public float dragRange = 200f;

    public float SteeringAmount { get; private set; }

    RectTransform rectTransform;
    Canvas parentCanvas;
    bool isDragging;
    float currentAngle;
    float targetAngle;
    float pointerDownX;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (!isDragging)
        {
            // Return to center smoothly
            targetAngle = 0f;
        }

        // Smooth the actual angle toward target
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, returnSpeed * Time.deltaTime);
        if (Mathf.Abs(currentAngle) < 0.3f && !isDragging)
            currentAngle = 0f;

        // Rotate the wheel visual
        rectTransform.localEulerAngles = new Vector3(0f, 0f, -currentAngle);

        // Output normalized steering (-1 to 1)
        SteeringAmount = Mathf.Clamp(currentAngle / maxRotationAngle, -1f, 1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        pointerDownX = eventData.position.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = eventData.position.x - pointerDownX;

        // Scale by canvas so it feels consistent across resolutions
        float scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        float normalizedDelta = deltaX / (dragRange * scaleFactor);

        targetAngle = Mathf.Clamp(normalizedDelta * maxRotationAngle, -maxRotationAngle, maxRotationAngle);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
}
