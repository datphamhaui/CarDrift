using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Spawn lưới coin 2D: rows hàng × coinsPerRow coin mỗi hàng.
/// Hướng chính (coinsPerRow) = direction chọn trong Inspector.
/// Hướng phụ (rows)          = rowDirection chọn trong Inspector.
///
/// Setup:
///   1. Tạo empty GameObject, đặt đúng vị trí góc đầu tiên
///   2. Gắn script này vào
///   3. Gán Coin Prefab
///   4. Chỉnh Rows, Coins Per Row, Spacing, Row Spacing rồi bấm Generate Grid
/// </summary>
public class CoinRow : MonoBehaviour
{
    [Header("Coin")]
    public GameObject coinPrefab;

    [Header("Grid Settings")]
    [Tooltip("Số hàng (chiều phụ)")]
    [Min(1)] public int rows = 1;

    [Tooltip("Số coin mỗi hàng (chiều chính)")]
    [Min(1)] public int coinsPerRow = 10;

    [Tooltip("Khoảng cách giữa coin trong cùng hàng")]
    public float spacing = 1.5f;

    [Tooltip("Khoảng cách giữa các hàng")]
    public float rowSpacing = 1.5f;

    [Tooltip("Độ cao coin so với vị trí GameObject (Y)")]
    public float height = 1f;

    public enum RowDirection { Forward, Right, Back, Left, Custom }

    [Header("Hướng coin (chiều chính)")]
    public RowDirection direction = RowDirection.Forward;
    [Tooltip("Hướng tùy chỉnh (local) khi chọn Custom")]
    public Vector3 customDirection = Vector3.forward;

    [Header("Hướng hàng (chiều phụ)")]
    public RowDirection rowDirection = RowDirection.Right;
    [Tooltip("Hướng hàng tùy chỉnh (local) khi chọn Custom")]
    public Vector3 customRowDirection = Vector3.right;

    [Header("Generated (không chỉnh tay)")]
    public Transform generatedRoot;

    // ── Gizmos preview ────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Vector3 coinDir = GetWorldDir(direction, customDirection);
        Vector3 rowDir  = GetWorldDir(rowDirection, customRowDirection);
        Vector3 origin  = transform.position + Vector3.up * height;

        Gizmos.color = new Color(1f, 0.84f, 0f, 0.8f);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < coinsPerRow; c++)
            {
                Vector3 pos = origin + rowDir * (rowSpacing * r) + coinDir * (spacing * c);
                Gizmos.DrawWireSphere(pos, 0.25f);
            }
        }

        // Mũi tên hướng chính
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, coinDir * spacing * 2f);

        // Mũi tên hướng hàng
        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, rowDir * rowSpacing * 2f);
    }

    Vector3 GetWorldDir(RowDirection dir, Vector3 custom)
    {
        return dir switch
        {
            RowDirection.Forward => transform.forward,
            RowDirection.Right   => transform.right,
            RowDirection.Back    => -transform.forward,
            RowDirection.Left    => -transform.right,
            RowDirection.Custom  => transform.TransformDirection(custom.normalized),
            _                    => transform.forward,
        };
    }

#if UNITY_EDITOR
    public void GenerateRow()
    {
        ClearRow();

        if (coinPrefab == null)
        {
            Debug.LogWarning("[CoinRow] Chưa gán Coin Prefab!");
            return;
        }

        var root = new GameObject("CoinRow_Generated");
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;
        generatedRoot = root.transform;

        Vector3 coinDir = GetWorldDir(direction, customDirection);
        Vector3 rowDir  = GetWorldDir(rowDirection, customRowDirection);
        Vector3 origin  = transform.position + Vector3.up * height;

        int idx = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < coinsPerRow; c++)
            {
                Vector3 pos = origin + rowDir * (rowSpacing * r) + coinDir * (spacing * c);
                var coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab, root.transform);
                if (coin == null) coin = (GameObject)Instantiate(coinPrefab, root.transform);
                coin.transform.position = pos;
                coin.name = $"Coin_r{r}_c{c}";
                idx++;
            }
        }

        Debug.Log($"[CoinRow] Spawned {idx} coins ({rows} rows × {coinsPerRow} per row).");
        EditorUtility.SetDirty(this);
    }

    public void ClearRow()
    {
        // Chỉ destroy nếu generatedRoot là con trực tiếp của GameObject này
        if (generatedRoot != null && generatedRoot.parent == transform)
        {
            Undo.DestroyObjectImmediate(generatedRoot.gameObject);
        }
        generatedRoot = null;

        // Dọn thêm phòng hờ (dùng snapshot trước vì childCount thay đổi khi destroy)
        var toDelete = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child != null && child.name == "CoinRow_Generated")
                toDelete.Add(child.gameObject);
        }
        foreach (var go in toDelete)
            Undo.DestroyObjectImmediate(go);
    }
#endif
}

// ═════════════════════════════════════════════════════════════════════════════
//  Custom Inspector
// ═════════════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
[CustomEditor(typeof(CoinRow))]
public class CoinRowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (CoinRow)target;

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField(
            $"Tổng: {t.rows} hàng × {t.coinsPerRow} coin = {t.rows * t.coinsPerRow} coin",
            EditorStyles.helpBox);

        EditorGUILayout.Space(6);

        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button("▶  Generate Grid", GUILayout.Height(32)))
            t.GenerateRow();

        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        if (GUILayout.Button("✕  Clear", GUILayout.Height(24)))
            t.ClearRow();

        GUI.backgroundColor = Color.white;
    }
}
#endif
