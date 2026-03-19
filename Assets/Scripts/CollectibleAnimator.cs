using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton quản lý animation (xoay + bob) cho TẤT CẢ collectible và obstacle.
/// Thay vì mỗi object có Update() riêng, tất cả đăng ký vào đây → chỉ 1 Update() duy nhất.
///
/// Setup: Không cần làm gì - tự tạo nếu chưa có trong scene.
/// </summary>
[DefaultExecutionOrder(-50)]
public class CollectibleAnimator : MonoBehaviour
{
    public static CollectibleAnimator Instance { get; private set; }

    struct Item
    {
        public Transform transform;
        public Vector3   startPos;
        public float     time;
        public float     rotateSpeed;
        public float     bobAmplitude;
        public float     bobSpeed;
    }

    readonly List<Item>                  _items    = new List<Item>(64);
    readonly Dictionary<Transform, int>  _indexMap = new Dictionary<Transform, int>(64);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── API ────────────────────────────────────────────────────────────────────

    public static void Register(Transform t, float rotateSpeed, float bobAmplitude, float bobSpeed)
    {
        EnsureExists();
        if (Instance._indexMap.ContainsKey(t)) return;

        var item = new Item
        {
            transform    = t,
            startPos     = t.position,
            time         = Random.Range(0f, Mathf.PI * 2f), // phase offset ngẫu nhiên
            rotateSpeed  = rotateSpeed,
            bobAmplitude = bobAmplitude,
            bobSpeed     = bobSpeed,
        };
        Instance._indexMap[t] = Instance._items.Count;
        Instance._items.Add(item);
    }

    public static void Unregister(Transform t)
    {
        if (Instance == null) return;
        if (!Instance._indexMap.TryGetValue(t, out int idx)) return;

        // Swap-remove để tránh shift toàn bộ list
        int last = Instance._items.Count - 1;
        if (idx != last)
        {
            Item swapped = Instance._items[last];
            Instance._items[idx] = swapped;
            Instance._indexMap[swapped.transform] = idx;
        }
        Instance._items.RemoveAt(last);
        Instance._indexMap.Remove(t);
    }

    // ── Update ─────────────────────────────────────────────────────────────────

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return; // paused (timeScale = 0)

        for (int i = _items.Count - 1; i >= 0; i--)
        {
            Item item = _items[i];

            // Dọn object đã bị destroy bên ngoài
            if (item.transform == null)
            {
                SwapRemoveAt(i);
                continue;
            }

            item.time += dt;
            item.transform.Rotate(Vector3.up, item.rotateSpeed * dt, Space.World);

            Vector3 pos = item.startPos;
            pos.y += Mathf.Sin(item.time * item.bobSpeed) * item.bobAmplitude;
            item.transform.position = pos;

            _items[i] = item; // struct — phải ghi lại
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    static void EnsureExists()
    {
        if (Instance != null) return;
        var go = new GameObject("[CollectibleAnimator]");
        go.AddComponent<CollectibleAnimator>();
    }

    void SwapRemoveAt(int idx)
    {
        int last = _items.Count - 1;
        if (idx != last)
        {
            Item swapped = _items[last];
            _items[idx]  = swapped;
            if (swapped.transform != null)
                _indexMap[swapped.transform] = idx;
        }
        // Xóa key của item bị null (transform) khỏi dict nếu có
        _items.RemoveAt(last);
    }
}
