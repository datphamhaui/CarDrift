using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CoinGenerator))]
public class CoinGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CoinGenerator generator = (CoinGenerator)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Count waypoints
        int waypointCount = 0;
        foreach (Transform child in generator.transform)
        {
            if (child.name != "Generated_Coins" && child != generator.generatedRoot)
                waypointCount++;
        }

        EditorGUILayout.HelpBox(
            $"Waypoints: {waypointCount}\nCoins: {generator.coinCount}",
            MessageType.Info);

        if (waypointCount < 2)
            EditorGUILayout.HelpBox("Add at least 2 child GameObjects as waypoints!", MessageType.Warning);
        if (generator.coinPrefab == null)
            EditorGUILayout.HelpBox("Assign a Coin Prefab!", MessageType.Warning);

        EditorGUILayout.Space(5);

        GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
        if (GUILayout.Button("GENERATE COINS", GUILayout.Height(40)))
        {
            Undo.RegisterCompleteObjectUndo(generator, "Generate Coins");
            generator.GenerateCoins();
            EditorUtility.SetDirty(generator);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("CLEAR COINS", GUILayout.Height(30)))
        {
            Undo.RegisterCompleteObjectUndo(generator, "Clear Coins");
            generator.ClearCoins();
            EditorUtility.SetDirty(generator);
        }

        GUI.backgroundColor = Color.white;
    }
}
