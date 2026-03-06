using UnityEngine;

/// <summary>
/// Scales a SpriteRenderer to fill the entire camera view while preserving aspect ratio.
/// Attach to any GameObject with a SpriteRenderer (e.g., Background).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    enum FillMode { Fill, Fit }

    [SerializeField] FillMode fillMode = FillMode.Fill;

    void Start()
    {
        ScaleToCamera();
    }

    void ScaleToCamera()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) return;

        float spriteW = sr.sprite.bounds.size.x;
        float spriteH = sr.sprite.bounds.size.y;

        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth  = camHeight * Screen.width / Screen.height;

        float scaleX = camWidth  / spriteW;
        float scaleY = camHeight / spriteH;

        float scale = fillMode == FillMode.Fill
            ? Mathf.Max(scaleX, scaleY)
            : Mathf.Min(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
