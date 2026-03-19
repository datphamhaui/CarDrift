using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("=== GROUND GRID ===")]
    [Tooltip("Prefab used for ground tiles")]
    public GameObject groundPrefab;
    [Tooltip("Number of tiles on X axis")]
    public int gridX = 3;
    [Tooltip("Number of tiles on Z axis")]
    public int gridZ = 3;
    [Tooltip("Desired size of each grid cell in world units (tiles will be scaled to fit)")]
    public float groundTileSize = 40f;
    [Tooltip("Actual detected size of ground prefab mesh (click Detect Tile Size)")]
    [SerializeField] float detectedTileSize = 0f;
    [Tooltip("Use the prefab's built-in rotation (e.g. FBX models rotated X=-90)")]
    public bool useOriginalPrefabTransform = true;

    [Header("=== WALL (Border) ===")]
    [Tooltip("Prefab used for walls around the map border")]
    public GameObject wallPrefab;
    [Tooltip("Spacing between wall objects")]
    public float wallSpacing = 12f;
    [Tooltip("Wall scale multiplier")]
    public Vector3 wallScale = Vector3.one;
    [Tooltip("Height offset for walls (negative = lower)")]
    public float wallYOffset = -2f;
    [Tooltip("Use the prefab's built-in rotation for walls")]
    public bool useOriginalWallTransform = true;

    [Header("=== PATH (Driving Route) ===")]
    [Tooltip("Number of turns/waypoints in the path (more = more winding)")]
    [Range(2, 8)]
    public int pathTurns = 4;
    [Tooltip("Width of the drivable road corridor")]
    public float roadWidth = 18f;
    [Tooltip("How far waypoints can deviate sideways (0-1 of half map width)")]
    [Range(0.2f, 0.9f)]
    public float pathWanderAmount = 0.7f;

    [Header("=== OBSTACLES ===")]
    [Tooltip("List of obstacle prefabs to randomly place")]
    public GameObject[] obstaclePrefabs;
    [Tooltip("Number of obstacles to place")]
    [Range(5, 100)]
    public int obstacleCount = 20;
    [Tooltip("Min distance between obstacles")]
    public float minObstacleSpacing = 8f;
    [Tooltip("Random Y rotation for obstacles")]
    public bool randomRotation = true;
    [Tooltip("Random scale range")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    [Header("=== COINS ===")]
    [Tooltip("Coin prefab to place along the road")]
    public GameObject coinPrefab;
    [Tooltip("Number of coins to place along the path")]
    [Range(5, 100)]
    public int coinCount = 30;
    [Tooltip("Height of coins above ground")]
    public float coinHeight = 1f;
    [Tooltip("How far coins can spread from road center (0-1 of half road width)")]
    [Range(0f, 0.9f)]
    public float coinSpreadRatio = 0.6f;

    [Header("=== SPAWN & GOAL ===")]
    [Tooltip("Drag the car object here so obstacles avoid its position")]
    public Transform carTransform;
    [Tooltip("Goal prefab (trigger zone). Leave empty to create a default one")]
    public GameObject goalPrefab;
    [Tooltip("Clear zone radius around spawn and goal (no obstacles)")]
    public float clearZoneRadius = 15f;

    [Header("=== GENERATED (Do Not Edit) ===")]
    public Transform generatedRoot;
    [SerializeField] List<Vector3> generatedPath = new List<Vector3>();

    // Computed map size from grid
    public float MapWidth => gridX * groundTileSize;
    public float MapLength => gridZ * groundTileSize;

    // Spawn = car's actual position, goal = end of path
    public Vector3 GetSpawnPosition()
    {
        if (carTransform != null)
            return new Vector3(carTransform.position.x, 0f, carTransform.position.z);
        return transform.position + new Vector3(0f, 0f, -MapLength / 2f + clearZoneRadius);
    }

    public Vector3 GetGoalPosition()
    {
        if (generatedPath != null && generatedPath.Count > 0)
            return generatedPath[generatedPath.Count - 1];
        return transform.position + new Vector3(0f, 0f, MapLength / 2f - clearZoneRadius);
    }

#if UNITY_EDITOR
    public void GenerateMap()
    {
        ClearGenerated();

        var rootGO = new GameObject("Generated_Map");
        rootGO.transform.SetParent(transform);
        rootGO.transform.localPosition = Vector3.zero;
        generatedRoot = rootGO.transform;

        GeneratePath();
        GenerateGround();
        GenerateWalls();
        GenerateObstacles();
        GenerateCoins();
        GenerateGoal();

        Debug.Log($"[MapGenerator] Map generated! Size={MapWidth}x{MapLength}, path with {generatedPath.Count} waypoints, {obstacleCount} obstacles");
    }

    public void ClearGenerated()
    {
        if (generatedRoot != null)
        {
            DestroyImmediate(generatedRoot.gameObject);
            generatedRoot = null;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "Generated_Map")
                DestroyImmediate(child.gameObject);
        }

        generatedPath.Clear();
    }

    /// <summary>
    /// Generate a winding path starting from the car's position,
    /// going forward (car's facing direction) through the map.
    /// </summary>
    void GeneratePath()
    {
        generatedPath.Clear();

        float halfW = MapWidth / 2f;
        float halfL = MapLength / 2f;
        float maxWander = halfW * pathWanderAmount;

        // Start from the car's actual position
        Vector3 carPos = GetSpawnPosition();
        generatedPath.Add(carPos);

        // Determine car's forward direction to know which way to build the path
        Vector3 carForward = Vector3.forward;
        if (carTransform != null)
            carForward = carTransform.forward;

        // Figure out which map edge the car is facing towards
        // Use the car forward direction's dominant axis to determine path progression
        bool pathAlongZ = Mathf.Abs(carForward.z) >= Mathf.Abs(carForward.x);
        float forwardSign = pathAlongZ ? Mathf.Sign(carForward.z) : Mathf.Sign(carForward.x);

        // Calculate total path length from car to the far edge
        float pathLength;
        float carMainAxis;
        if (pathAlongZ)
        {
            carMainAxis = carPos.z - transform.position.z;
            pathLength = (forwardSign > 0) ? (halfL - carMainAxis - 5f) : (halfL + carMainAxis - 5f);
        }
        else
        {
            carMainAxis = carPos.x - transform.position.x;
            pathLength = (forwardSign > 0) ? (halfW - carMainAxis - 5f) : (halfW + carMainAxis - 5f);
        }

        pathLength = Mathf.Max(pathLength, 30f);
        float segmentLength = pathLength / (pathTurns + 1);

        float lastPerp = pathAlongZ ? carPos.x : carPos.z;

        for (int i = 0; i < pathTurns; i++)
        {
            float progress = segmentLength * (i + 1);

            // Alternate sides for zig-zag
            float perpMax = pathAlongZ ? maxWander : maxWander;
            float targetPerp;
            if (i % 2 == 0)
                targetPerp = transform.position.x + Random.Range(perpMax * 0.3f, perpMax) * (pathAlongZ ? 1f : 1f);
            else
                targetPerp = transform.position.x + Random.Range(-perpMax, -perpMax * 0.3f) * (pathAlongZ ? 1f : 1f);

            // Ensure alternating sides
            float perpCenter = pathAlongZ ? transform.position.x : transform.position.z;
            if (Mathf.Sign(targetPerp - perpCenter) == Mathf.Sign(lastPerp - perpCenter))
                targetPerp = perpCenter - (targetPerp - perpCenter);

            lastPerp = targetPerp;

            Vector3 waypoint;
            if (pathAlongZ)
            {
                float z = carPos.z + forwardSign * progress;
                z = Mathf.Clamp(z, transform.position.z - halfL + 5f, transform.position.z + halfL - 5f);
                float x = Mathf.Clamp(targetPerp, transform.position.x - halfW + 10f, transform.position.x + halfW - 10f);
                waypoint = new Vector3(x, 0f, z);
            }
            else
            {
                float x = carPos.x + forwardSign * progress;
                x = Mathf.Clamp(x, transform.position.x - halfW + 5f, transform.position.x + halfW - 5f);
                float z = Mathf.Clamp(targetPerp, transform.position.z - halfL + 10f, transform.position.z + halfL - 10f);
                waypoint = new Vector3(x, 0f, z);
            }

            generatedPath.Add(waypoint);
        }

        // End: near the far edge
        Vector3 end;
        if (pathAlongZ)
        {
            float endZ = transform.position.z + forwardSign * (halfL - 8f);
            float endX = transform.position.x + Random.Range(-maxWander * 0.3f, maxWander * 0.3f);
            end = new Vector3(endX, 0f, endZ);
        }
        else
        {
            float endX = transform.position.x + forwardSign * (halfW - 8f);
            float endZ = transform.position.z + Random.Range(-maxWander * 0.3f, maxWander * 0.3f);
            end = new Vector3(endX, 0f, endZ);
        }
        generatedPath.Add(end);
    }

    /// <summary>
    /// Get the minimum distance from a point to the path polyline (XZ plane only).
    /// </summary>
    float DistanceToPath(Vector3 point)
    {
        float minDist = float.MaxValue;

        for (int i = 0; i < generatedPath.Count - 1; i++)
        {
            Vector3 a = generatedPath[i];
            Vector3 b = generatedPath[i + 1];

            // Project onto XZ plane
            Vector2 p = new Vector2(point.x, point.z);
            Vector2 a2 = new Vector2(a.x, a.z);
            Vector2 b2 = new Vector2(b.x, b.z);

            float dist = DistanceToLineSegment(p, a2, b2);
            if (dist < minDist)
                minDist = dist;
        }

        return minDist;
    }

    float DistanceToLineSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float sqrLen = ab.sqrMagnitude;
        if (sqrLen < 0.001f) return Vector2.Distance(p, a);

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / sqrLen);
        Vector2 proj = a + t * ab;
        return Vector2.Distance(p, proj);
    }

    GameObject PlacePrefab(GameObject prefab, Transform parent, Vector3 worldPos,
        Quaternion extraRotation, bool preserveOriginalTransform)
    {
        var go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(parent);

        if (preserveOriginalTransform)
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.transform.position = worldPos;
            go.transform.rotation = extraRotation;
        }
        else
        {
            go.transform.position = worldPos;
            go.transform.rotation = extraRotation;
            go.transform.localScale = Vector3.one;
        }

        return go;
    }

    void GenerateGround()
    {
        if (groundPrefab == null) return;

        var groundParent = new GameObject("Ground");
        groundParent.transform.SetParent(generatedRoot);
        groundParent.transform.localPosition = Vector3.zero;

        if (detectedTileSize <= 0f)
            detectedTileSize = DetectTileSize(groundPrefab);

        float actualSize = detectedTileSize > 0f ? detectedTileSize : groundTileSize;
        float scaleFactor = groundTileSize / actualSize;

        Vector3 boundsOffset = DetectBoundsCenter(groundPrefab);

        float tileSize = groundTileSize;
        float startX = -(gridX * tileSize) / 2f + tileSize / 2f;
        float startZ = -(gridZ * tileSize) / 2f + tileSize / 2f;

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                Vector3 gridCenter = transform.position + new Vector3(
                    startX + x * tileSize,
                    0f,
                    startZ + z * tileSize
                );

                Vector3 pos = gridCenter - boundsOffset * scaleFactor;

                var tile = PlacePrefab(groundPrefab, groundParent.transform, pos,
                    Quaternion.identity, useOriginalPrefabTransform);

                if (Mathf.Abs(scaleFactor - 1f) > 0.01f)
                    tile.transform.localScale = new Vector3(scaleFactor, 1f, scaleFactor);
            }
        }

        Debug.Log($"[MapGenerator] Ground: {gridX}x{gridZ} tiles, actual={actualSize:F1}, target={groundTileSize}, scale={scaleFactor:F2}x");
    }

    Vector3 DetectBoundsCenter(GameObject prefab)
    {
        var temp = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        temp.transform.position = Vector3.zero;
        temp.transform.rotation = Quaternion.identity;
        temp.transform.localScale = Vector3.one;

        var renderers = temp.GetComponentsInChildren<Renderer>();
        Vector3 center = Vector3.zero;
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            center = bounds.center;
        }

        DestroyImmediate(temp);
        return center;
    }

    void GenerateWalls()
    {
        if (wallPrefab == null) return;

        var wallParent = new GameObject("Walls");
        wallParent.transform.SetParent(generatedRoot);
        wallParent.transform.localPosition = Vector3.zero;

        float spacing = wallSpacing > 0 ? wallSpacing : 12f;
        float halfW = MapWidth / 2f;
        float halfL = MapLength / 2f;
        float y = wallYOffset;

        PlaceWallLine(wallParent.transform,
            transform.position + new Vector3(-halfW, y, -halfL),
            transform.position + new Vector3(halfW, y, -halfL),
            spacing, 0f);

        PlaceWallLine(wallParent.transform,
            transform.position + new Vector3(-halfW, y, halfL),
            transform.position + new Vector3(halfW, y, halfL),
            spacing, 180f);

        PlaceWallLine(wallParent.transform,
            transform.position + new Vector3(-halfW, y, -halfL),
            transform.position + new Vector3(-halfW, y, halfL),
            spacing, 90f);

        PlaceWallLine(wallParent.transform,
            transform.position + new Vector3(halfW, y, -halfL),
            transform.position + new Vector3(halfW, y, halfL),
            spacing, -90f);

        foreach (Transform wall in wallParent.transform)
            SetTagRecursive(wall.gameObject, "Wall");
    }

    void PlaceWallLine(Transform parent, Vector3 start, Vector3 end, float spacing, float yRotation)
    {
        float distance = Vector3.Distance(start, end);
        int count = Mathf.Max(1, Mathf.CeilToInt(distance / spacing));
        Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);

        for (int i = 0; i <= count; i++)
        {
            float t = (float)i / count;
            Vector3 pos = Vector3.Lerp(start, end, t);
            var wall = PlacePrefab(wallPrefab, parent, pos, rotation, useOriginalWallTransform);
            wall.transform.localScale = wallScale;
        }
    }

    void GenerateObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
        if (generatedPath.Count < 2) return;

        var obstacleParent = new GameObject("Obstacles");
        obstacleParent.transform.SetParent(generatedRoot);
        obstacleParent.transform.localPosition = Vector3.zero;

        Vector3 spawnWorld = GetSpawnPosition();
        Vector3 goalWorld = GetGoalPosition();

        float halfRoad = roadWidth / 2f;
        List<Vector3> placedPositions = new List<Vector3>();
        int placed = 0;

        // === PHASE 1: Place obstacles along BOTH EDGES of the road to form corridor walls ===
        int edgeCount = Mathf.CeilToInt(obstacleCount * 0.7f); // 70% of obstacles for edges
        int edgePlaced = 0;

        for (int seg = 0; seg < generatedPath.Count - 1; seg++)
        {
            Vector3 segStart = generatedPath[seg];
            Vector3 segEnd = generatedPath[seg + 1];
            Vector3 segDir = (segEnd - segStart).normalized;
            float segLength = Vector3.Distance(segStart, segEnd);

            // Perpendicular direction (left/right of road)
            Vector3 perp = new Vector3(-segDir.z, 0f, segDir.x);

            // How many obstacles along this segment
            int obstaclesPerSide = Mathf.Max(2, Mathf.CeilToInt(segLength / minObstacleSpacing));

            for (int i = 0; i <= obstaclesPerSide && edgePlaced < edgeCount; i++)
            {
                float t = (float)i / obstaclesPerSide;
                Vector3 centerPoint = Vector3.Lerp(segStart, segEnd, t);

                // Place on both sides of the road
                for (int side = -1; side <= 1; side += 2)
                {
                    if (edgePlaced >= edgeCount) break;

                    // Offset from road edge + small random variation
                    float edgeOffset = halfRoad + Random.Range(0f, minObstacleSpacing * 0.5f);
                    Vector3 pos = centerPoint + perp * side * edgeOffset;
                    pos.y = 0f;

                    // Skip if too close to spawn or goal
                    if (Vector3.Distance(pos, spawnWorld) < clearZoneRadius) continue;
                    if (Vector3.Distance(pos, goalWorld) < clearZoneRadius) continue;

                    // Skip if outside map bounds
                    float halfW = MapWidth / 2f - 3f;
                    float halfL = MapLength / 2f - 3f;
                    if (Mathf.Abs(pos.x - transform.position.x) > halfW) continue;
                    if (Mathf.Abs(pos.z - transform.position.z) > halfL) continue;

                    // Skip if too close to existing obstacle
                    bool tooClose = false;
                    foreach (var existing in placedPositions)
                    {
                        if (Vector3.Distance(pos, existing) < minObstacleSpacing * 0.7f)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    PlaceObstacle(obstacleParent.transform, pos, placedPositions);
                    edgePlaced++;
                    placed++;
                }
            }
        }

        // === PHASE 2: Fill remaining obstacles randomly outside the road ===
        int fillCount = obstacleCount - placed;
        int maxAttempts = fillCount * 15;
        float halfWFill = MapWidth / 2f - 5f;
        float halfLFill = MapLength / 2f - 5f;

        for (int attempt = 0; attempt < maxAttempts && placed < obstacleCount; attempt++)
        {
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-halfWFill, halfWFill),
                0f,
                Random.Range(-halfLFill, halfLFill)
            );

            // Must be outside road corridor
            float distToPath = DistanceToPath(pos);
            if (distToPath < halfRoad + 2f) continue;

            if (Vector3.Distance(pos, spawnWorld) < clearZoneRadius) continue;
            if (Vector3.Distance(pos, goalWorld) < clearZoneRadius) continue;

            bool tooClose = false;
            foreach (var existing in placedPositions)
            {
                if (Vector3.Distance(pos, existing) < minObstacleSpacing)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            PlaceObstacle(obstacleParent.transform, pos, placedPositions);
            placed++;
        }

        Debug.Log($"[MapGenerator] Placed {placed} obstacles ({edgePlaced} edge + {placed - edgePlaced} fill)");
    }

    void PlaceObstacle(Transform parent, Vector3 pos, List<Vector3> placedPositions)
    {
        var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        var obstacle = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        obstacle.transform.SetParent(parent);
        obstacle.transform.position = pos;

        if (randomRotation)
            obstacle.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        else
            obstacle.transform.rotation = Quaternion.identity;

        float scale = Random.Range(scaleRange.x, scaleRange.y);
        obstacle.transform.localScale = Vector3.one * scale;

        SetTagRecursive(obstacle, "Obstacle");
        placedPositions.Add(pos);
    }

    void GenerateCoins()
    {
        if (coinPrefab == null || generatedPath.Count < 2) return;

        var coinParent = new GameObject("Coins");
        coinParent.transform.SetParent(generatedRoot);
        coinParent.transform.localPosition = Vector3.zero;

        Vector3 spawnWorld = GetSpawnPosition();
        Vector3 goalWorld = GetGoalPosition();
        float halfRoad = roadWidth / 2f * coinSpreadRatio;

        // Calculate total path length
        float totalLength = 0f;
        for (int i = 0; i < generatedPath.Count - 1; i++)
            totalLength += Vector3.Distance(generatedPath[i], generatedPath[i + 1]);

        float spacing = totalLength / (coinCount + 1);
        int placed = 0;
        float accumulated = 0f;
        float nextCoinAt = spacing;

        for (int seg = 0; seg < generatedPath.Count - 1; seg++)
        {
            Vector3 segStart = generatedPath[seg];
            Vector3 segEnd = generatedPath[seg + 1];
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

                // Skip coins too close to spawn or goal
                if (Vector3.Distance(center, spawnWorld) < clearZoneRadius) continue;
                if (Vector3.Distance(center, goalWorld) < clearZoneRadius) continue;

                // Random lateral offset within road
                float lateralOffset = Random.Range(-halfRoad, halfRoad);
                Vector3 pos = center + perp * lateralOffset;
                pos.y = coinHeight;

                var coin = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(coinPrefab);
                coin.transform.SetParent(coinParent.transform);
                coin.transform.position = pos;
                coin.tag = "Coin";
                placed++;
            }
        }

        Debug.Log($"[MapGenerator] Placed {placed} coins along path");
    }

    void GenerateGoal()
    {
        Vector3 goalPos = GetGoalPosition();

        // Calculate rotation to face the direction the car arrives from
        Vector3 lastDir = Vector3.forward;
        if (generatedPath.Count >= 2)
        {
            Vector3 prev = generatedPath[generatedPath.Count - 2];
            Vector3 last = generatedPath[generatedPath.Count - 1];
            lastDir = (last - prev).normalized;
        }
        float goalYRotation = Mathf.Atan2(lastDir.x, lastDir.z) * Mathf.Rad2Deg;

        GameObject goal;
        if (goalPrefab != null)
        {
            goal = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(goalPrefab);
        }
        else
        {
            goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goal.name = "LevelGoal";
            goal.transform.localScale = new Vector3(roadWidth, 5f, 2f);

            var collider = goal.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;

            var renderer = goal.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0f, 1f, 0f, 0.3f);
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                renderer.material = mat;
            }

            goal.AddComponent<LevelGoal>();
        }

        goal.transform.SetParent(generatedRoot);
        goal.transform.position = goalPos;
        // Rotate goal perpendicular to the path direction
        goal.transform.rotation = Quaternion.Euler(0f, goalYRotation + 90f, 0f);
    }

    public float DetectTileSize(GameObject prefab)
    {
        if (prefab == null) return 0f;

        var temp = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        temp.transform.position = Vector3.zero;
        temp.transform.rotation = Quaternion.identity;
        temp.transform.localScale = Vector3.one;

        var renderers = temp.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            DestroyImmediate(temp);
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        DestroyImmediate(temp);

        float size = Mathf.Max(bounds.size.x, bounds.size.z);
        detectedTileSize = size;
        Debug.Log($"[MapGenerator] Detected tile size: {size} (bounds: {bounds.size})");
        return size;
    }

    void SetTagRecursive(GameObject go, string tag)
    {
        go.tag = tag;
        foreach (Transform child in go.transform)
            SetTagRecursive(child.gameObject, tag);
    }

    void OnDrawGizmosSelected()
    {
        // Draw map bounds
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(MapWidth, 0.1f, MapLength));

        // Draw grid lines
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        float halfW = MapWidth / 2f;
        float halfL = MapLength / 2f;
        for (int x = 0; x <= gridX; x++)
        {
            float xPos = -halfW + x * groundTileSize;
            Gizmos.DrawLine(
                transform.position + new Vector3(xPos, 0.1f, -halfL),
                transform.position + new Vector3(xPos, 0.1f, halfL));
        }
        for (int z = 0; z <= gridZ; z++)
        {
            float zPos = -halfL + z * groundTileSize;
            Gizmos.DrawLine(
                transform.position + new Vector3(-halfW, 0.1f, zPos),
                transform.position + new Vector3(halfW, 0.1f, zPos));
        }

        // Draw path
        if (generatedPath != null && generatedPath.Count >= 2)
        {
            // Draw road corridor
            for (int i = 0; i < generatedPath.Count - 1; i++)
            {
                Vector3 a = generatedPath[i];
                Vector3 b = generatedPath[i + 1];
                Vector3 dir = (b - a).normalized;
                Vector3 perp = new Vector3(-dir.z, 0f, dir.x) * roadWidth / 2f;

                // Road edges
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
                Gizmos.DrawLine(a + perp + Vector3.up * 0.2f, b + perp + Vector3.up * 0.2f);
                Gizmos.DrawLine(a - perp + Vector3.up * 0.2f, b - perp + Vector3.up * 0.2f);

                // Center line
                Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
                Gizmos.DrawLine(a + Vector3.up * 0.2f, b + Vector3.up * 0.2f);
            }

            // Draw waypoints
            Gizmos.color = Color.yellow;
            foreach (var wp in generatedPath)
                Gizmos.DrawSphere(wp + Vector3.up * 0.3f, 1.5f);
        }

        // Draw spawn point
        Gizmos.color = Color.green;
        Vector3 spawn = GetSpawnPosition();
        Gizmos.DrawWireSphere(spawn, clearZoneRadius);
        Gizmos.DrawCube(spawn + Vector3.up * 0.5f, Vector3.one * 3f);

        // Draw goal point
        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
        Vector3 goal = GetGoalPosition();
        Gizmos.DrawWireSphere(goal, clearZoneRadius);
        Gizmos.DrawCube(goal + Vector3.up * 0.5f, Vector3.one * 3f);
    }
#endif
}
