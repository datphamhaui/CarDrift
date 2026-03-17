using UnityEngine;

/// <summary>
/// Chướng ngại vật hình lập phương di chuyển theo quy luật.
///
/// Setup:
///   - Attach vào GameObject có Collider (isTrigger = true)
///   - Tag: "Obstacle"
///   - Chọn MoveType trong Inspector
///
/// MoveType:
///   - Horizontal: đi qua lại từ trái sang phải
///   - Vertical:   nhô lên hụp xuống trên mặt đường
/// </summary>
public class MovingObstacle : MonoBehaviour
{
    public enum MoveType { Horizontal, Vertical }

    [Header("Loại di chuyển")]
    public MoveType moveType = MoveType.Horizontal;

    [Header("Chung")]
    [Tooltip("Tốc độ di chuyển")]
    public float speed = 3f;

    [Header("Horizontal (trái - phải)")]
    [Tooltip("Khoảng cách di chuyển mỗi bên tính từ vị trí ban đầu")]
    public float horizontalRange = 4f;

    [Header("Vertical (lên - xuống)")]
    [Tooltip("Chiều cao nhô lên tính từ vị trí ban đầu")]
    public float verticalRange = 1.5f;
    [Tooltip("Dừng lại dưới đất bao lâu (giây)")]
    public float groundPauseDuration = 0.5f;

    [Header("Va chạm")]
    public int damage = 1;

    // ── private state ──────────────────────────────────────────────
    Vector3 _startPos;
    float   _time;
    float   _pauseTimer;
    bool    _pausing;

    void Start()
    {
        _startPos = transform.position;
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        switch (moveType)
        {
            case MoveType.Horizontal: MoveHorizontal(); break;
            case MoveType.Vertical:   MoveVertical();   break;
        }
    }

    // ── Horizontal ─────────────────────────────────────────────────
    // Dùng Mathf.Sin để chuyển động mượt (ease-in / ease-out)
    void MoveHorizontal()
    {
        _time += Time.deltaTime * speed;
        float offset = Mathf.Sin(_time) * horizontalRange;
        Vector3 pos = _startPos;
        pos.x += offset;
        transform.position = pos;
    }

    // ── Vertical ───────────────────────────────────────────────────
    // Nhô lên bằng Sin dương, khi trở về y=0 thì dừng một chút rồi lên tiếp
    void MoveVertical()
    {
        if (_pausing)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f)
            {
                _pausing = false;
                _time = 0f; // bắt đầu chu kỳ mới từ 0
            }
            return;
        }

        _time += Time.deltaTime * speed;

        // Abs(Sin) → chỉ đi từ 0 lên rồi về 0, không xuống âm
        float t      = Mathf.Abs(Mathf.Sin(_time));
        float offset = t * verticalRange;

        Vector3 pos = _startPos;
        pos.y += offset;
        transform.position = pos;

        // Khi Sin về gần 0 (đã hoàn thành nửa chu kỳ π), dừng lại
        if (_time >= Mathf.PI && !_pausing)
        {
            transform.position = _startPos; // snap xuống đất
            _pausing   = true;
            _pauseTimer = groundPauseDuration;
        }
    }

    // ── Va chạm ────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        GameManager.Instance.TakeDamage(damage);
    }
}
