using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates coins along a path defined by child waypoints.
/// Works independently from MapGenerator.
///
/// Setup:
///   1. Create empty GameObject, add this script
///   2. Add child GameObjects as waypoints (position them along the road)
///   3. Assign coinPrefab
///   4. Click "Generate Coins" in Inspector
/// </summary>
public class CoinGenerator : MonoBehaviour
{
    [Header("Coin Settings")]
    [Tooltip("Leave empty to auto-create a default coin")]
    public GameObject coinPrefab;
    [Range(5, 200)]
    public int coinCount = 30;
    public float coinHeight = 1f;
    [Tooltip("Max lateral offset from path center")]
    public float spreadWidth = 3f;

    [Header("Default Coin Appearance")]
    public float coinScale = 0.5f;
    public int coinScoreValue = 10;

    [Header("Generated")]
    public Transform generatedRoot;

#if UNITY_EDITOR
    public void GenerateCoins()
    {
        ClearCoins();

        var waypoints = GetWaypoints();
        if (waypoints.Count < 2)
        {
            Debug.LogWarning("[CoinGenerator] Need at least 2 child waypoints.");
            return;
        }

        var rootGO = new GameObject("Generated_Coins");
        rootGO.transform.SetParent(transform);
        rootGO.transform.localPosition = Vector3.zero;
        generatedRoot = rootGO.transform;

        // Calculate total path length
        float totalLength = 0f;
        for (int i = 0; i < waypoints.Count - 1; i++)
            totalLength += Vector3.Distance(waypoints[i], waypoints[i + 1]);

        float spacing = totalLength / (coinCount + 1);
        int placed = 0;
        float accumulated = 0f;
        float nextCoinAt = spacing;

        for (int seg = 0; seg < waypoints.Count - 1; seg++)
        {
            Vector3 segStart = waypoints[seg];
            Vector3 segEnd = waypoints[seg + 1];
            Vector3 segDir = (segEnd - segStart).normalized;
            float segLength = Vector3.Distance(segStart, segEnd);
            Vector3 perp = new Vector3(-segDir.z, 0f, segDir.x);

            float walked = 0f;
            while (walked < segLength && placed < coinCount)
            {
                float remaining = nextCoinAt - accumulated;
                if (remaining > segLength - walked)
                {
                    accumulated += segLength - walked;
                    break;
                }

                walked += remaining;
                accumulated = nextCoinAt;
                nextCoinAt += spacing;

                Vector3 center = segStart + segDir * walked;
                float lateralOffset = Random.Range(-spreadWidth, spreadWidth);
                Vector3 pos = center + perp * lateralOffset;
                pos.y = coinHeight;

                var coin = coinPrefab != null
                    ? (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(coinPrefab)
                    : CreateDefaultCoin();
                coin.transform.SetParent(generatedRoot);
                coin.transform.position = pos;
                coin.tag = "Coin";
                placed++;
            }
        }

        Debug.Log($"[CoinGenerator] Placed {placed} coins along {waypoints.Count} waypoints");
    }

    GameObject CreateDefaultCoin()
    {
        var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.transform.localScale = new Vector3(coinScale, coinScale * 0.1f, coinScale);

        // Gold material
        var renderer = coin.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.84f, 0f); // gold
            mat.SetFloat("_Metallic", 0.8f);
            mat.SetFloat("_Glossiness", 0.7f);
            renderer.sharedMaterial = mat;
        }

        // Replace default collider with trigger sphere
        Object.DestroyImmediate(coin.GetComponent<Collider>());
        var sphere = coin.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 1.5f;

        // Add coin script
        var collectible = coin.AddComponent<CoinCollectible>();

        return coin;
    }

    public void ClearCoins()
    {
        if (generatedRoot != null)
        {
            DestroyImmediate(generatedRoot.gameObject);
            generatedRoot = null;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "Generated_Coins")
                DestroyImmediate(child.gameObject);
        }
    }

    List<Vector3> GetWaypoints()
    {
        var points = new List<Vector3>();
        foreach (Transform child in transform)
        {
            if (child == generatedRoot) continue;
            if (child.name == "Generated_Coins") continue;
            points.Add(child.position);
        }
        return points;
    }

    void OnDrawGizmosSelected()
    {
        var waypoints = GetWaypoints();
        if (waypoints.Count < 2) return;

        // Draw path
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count - 1; i++)
            Gizmos.DrawLine(waypoints[i] + Vector3.up * 0.5f, waypoints[i + 1] + Vector3.up * 0.5f);

        // Draw waypoints
        Gizmos.color = new Color(1f, 0.8f, 0f);
        foreach (var wp in waypoints)
            Gizmos.DrawSphere(wp + Vector3.up * 0.5f, 0.8f);

        // Draw spread width
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 dir = (waypoints[i + 1] - waypoints[i]).normalized;
            Vector3 perp = new Vector3(-dir.z, 0f, dir.x) * spreadWidth;
            Gizmos.DrawLine(waypoints[i] + perp + Vector3.up * 0.5f, waypoints[i + 1] + perp + Vector3.up * 0.5f);
            Gizmos.DrawLine(waypoints[i] - perp + Vector3.up * 0.5f, waypoints[i + 1] - perp + Vector3.up * 0.5f);
        }
    }
#endif
}
