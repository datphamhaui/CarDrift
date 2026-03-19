using UnityEngine;

/// <summary>
/// Adds a gentle bobbing (dập dìu) animation to a UI element.
/// Attach to any RectTransform (Text, Image, etc.).
/// </summary>
public class UIBobAnimation : MonoBehaviour
{
    [Header("Bobbing")]
    [Tooltip("How far up/down it moves in pixels")]
    public float bobAmount = 15f;
    [Tooltip("How fast it bobs")]
    public float bobSpeed = 2f;

    [Header("Scale Pulse")]
    public bool enablePulse = false;
    [Range(0f, 0.1f)]
    public float pulseAmount = 0.05f;
    public float pulseSpeed = 2f;

    [Header("Offset")]
    [Tooltip("Phase offset so multiple elements don't bob in sync")]
    public float phaseOffset = 0f;

    Vector3 startPos;
    Vector3 startScale;

    void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;
    }

    void Update()
    {
        float t = Time.unscaledTime;

        // Bob up/down
        float yOffset = Mathf.Sin((t * bobSpeed) + phaseOffset) * bobAmount;
        transform.localPosition = startPos + new Vector3(0f, yOffset, 0f);

        // Scale pulse
        if (enablePulse)
        {
            float s = 1f + Mathf.Sin((t * pulseSpeed) + phaseOffset) * pulseAmount;
            transform.localScale = startScale * s;
        }
    }
}
