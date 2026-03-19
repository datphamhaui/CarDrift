using UnityEngine;
using UnityEditor;

public static class GameProgressEditor
{
    [MenuItem("CarDrift/Clear Progress")]
    static void ClearProgress()
    {
        if (EditorUtility.DisplayDialog(
            "Clear Progress",
            "Reset unlocked_level về 1?\nHành động này không thể hoàn tác.",
            "Clear", "Cancel"))
        {
            PlayerPrefs.SetInt("unlocked_level", 1);
            PlayerPrefs.Save();
            Debug.Log("[CarDrift] Progress cleared. unlocked_level = 1");
        }
    }

    [MenuItem("CarDrift/Unlock All Levels")]
    static void UnlockAllLevels()
    {
        int total = EditorUtility.DisplayDialogComplex(
            "Unlock All Levels",
            "Mở khoá tất cả level?",
            "Unlock 10 levels", "Cancel", "");

        if (total == 0)
        {
            PlayerPrefs.SetInt("unlocked_level", 10);
            PlayerPrefs.Save();
            Debug.Log("[CarDrift] Unlocked all levels (unlocked_level = 10)");
        }
    }

    [MenuItem("CarDrift/Show Current Progress")]
    static void ShowProgress()
    {
        int unlocked = PlayerPrefs.GetInt("unlocked_level", 1);
        int selected = PlayerPrefs.GetInt("selected_map", 0);
        EditorUtility.DisplayDialog(
            "Current Progress",
            $"unlocked_level = {unlocked}\nselected_map = {selected} (level {selected + 1})",
            "OK");
    }
}
