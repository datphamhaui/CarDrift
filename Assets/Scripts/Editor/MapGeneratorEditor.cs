using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator generator = (MapGenerator)target;

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("MAP GENERATOR TOOLS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Info box
        EditorGUILayout.HelpBox(
            $"Map Size: {generator.MapWidth} x {generator.MapLength} units\n" +
            $"Ground Grid: {generator.gridX} x {generator.gridZ} tiles ({generator.gridX * generator.gridZ} total)\n" +
            $"Tile Size: {generator.groundTileSize}\n" +
            $"Obstacles: {generator.obstacleCount}",
            MessageType.Info);

        // Warnings
        if (generator.groundPrefab == null)
            EditorGUILayout.HelpBox("Ground Prefab is not assigned!", MessageType.Warning);
        if (generator.wallPrefab == null)
            EditorGUILayout.HelpBox("Wall Prefab is not assigned!", MessageType.Warning);
        if (generator.obstaclePrefabs == null || generator.obstaclePrefabs.Length == 0)
            EditorGUILayout.HelpBox("No Obstacle Prefabs assigned!", MessageType.Warning);
        if (generator.carTransform == null)
            EditorGUILayout.HelpBox("Car Transform not assigned! Drag the car (Prometeo) here so the path starts from the car.", MessageType.Error);

        EditorGUILayout.Space(5);

        // Detect tile size button
        GUI.backgroundColor = new Color(0.9f, 0.8f, 0.3f);
        if (GUILayout.Button("DETECT TILE SIZE (from Ground Prefab)", GUILayout.Height(28)))
        {
            if (generator.groundPrefab != null)
            {
                Undo.RecordObject(generator, "Detect Tile Size");
                float detected = generator.DetectTileSize(generator.groundPrefab);
                if (detected > 0)
                {
                    EditorUtility.SetDirty(generator);
                    Debug.Log($"[MapGenerator] Prefab actual size: {detected:F1}. Tiles will be scaled {generator.groundTileSize / detected:F2}x to fill {generator.groundTileSize} cells.");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Assign a Ground Prefab first!", "OK");
            }
        }

        EditorGUILayout.Space(5);

        // Generate button
        GUI.backgroundColor = new Color(0.3f, 0.9f, 0.3f);
        if (GUILayout.Button("GENERATE MAP", GUILayout.Height(45)))
        {
            Undo.RegisterCompleteObjectUndo(generator, "Generate Map");
            generator.GenerateMap();
            EditorUtility.SetDirty(generator);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(5);

        // Randomize obstacles only
        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.9f);
        if (GUILayout.Button("RANDOMIZE OBSTACLES ONLY", GUILayout.Height(32)))
        {
            Undo.RegisterCompleteObjectUndo(generator, "Randomize Obstacles");
            RegenerateObstaclesOnly(generator);
            EditorUtility.SetDirty(generator);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(5);

        // Clear button
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("CLEAR MAP", GUILayout.Height(32)))
        {
            if (EditorUtility.DisplayDialog("Clear Map",
                "Are you sure you want to clear the generated map?", "Yes", "Cancel"))
            {
                Undo.RegisterCompleteObjectUndo(generator, "Clear Map");
                generator.ClearGenerated();
                EditorUtility.SetDirty(generator);
            }
        }

        GUI.backgroundColor = Color.white;
    }

    void RegenerateObstaclesOnly(MapGenerator generator)
    {
        if (generator.generatedRoot == null)
        {
            Debug.LogWarning("[MapGenerator] No generated map found. Generate a full map first.");
            return;
        }

        // Find and destroy existing obstacles and goal
        var toDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in generator.generatedRoot)
        {
            if (child.name == "Obstacles" || child.name == "LevelGoal" ||
                child.GetComponent<LevelGoal>() != null)
                toDestroy.Add(child.gameObject);
        }
        foreach (var go in toDestroy)
            DestroyImmediate(go);

        // Call private methods via reflection
        var obstacleMethod = typeof(MapGenerator).GetMethod("GenerateObstacles",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        obstacleMethod?.Invoke(generator, null);

        var goalMethod = typeof(MapGenerator).GetMethod("GenerateGoal",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        goalMethod?.Invoke(generator, null);

        Debug.Log("[MapGenerator] Obstacles randomized!");
    }
}
