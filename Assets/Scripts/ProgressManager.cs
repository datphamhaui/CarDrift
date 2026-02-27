using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager instance;

    const string UNLOCKED_LEVEL_KEY = "unlocked_level";

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsLevelUnlocked(int level)
    {
        // Level 1 is always unlocked
        if (level <= 1) return true;
        return level <= GetUnlockedLevel();
    }

    public int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
    }

    public void UnlockLevel(int level)
    {
        int current = GetUnlockedLevel();
        if (level > current)
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, level);
            PlayerPrefs.Save();
        }
    }

    public void CompleteLevel(int level)
    {
        UnlockLevel(level + 1);
    }

    // For debug
    public void ResetProgress()
    {
        PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, 1);
        PlayerPrefs.Save();
    }

    public void UnlockAll(int totalLevels)
    {
        PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, totalLevels);
        PlayerPrefs.Save();
    }
}
