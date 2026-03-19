using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    public string[] levelSceneNames;

    public int CurrentLevelIndex
    {
        get => PlayerPrefs.GetInt("CurrentLevel", 0);
        set => PlayerPrefs.SetInt("CurrentLevel", value);
    }

    public int HighestUnlockedLevel
    {
        get => PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
        set
        {
            PlayerPrefs.SetInt("HighestUnlockedLevel", value);
            PlayerPrefs.Save();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levelSceneNames.Length) return;
        if (index > HighestUnlockedLevel) return;

        CurrentLevelIndex = index;
        SceneManager.LoadScene(levelSceneNames[index]);
    }

    public void LoadNextLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;
        if (nextIndex < levelSceneNames.Length)
        {
            LoadLevel(nextIndex);
        }
    }

    public void UnlockNextLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;
        if (nextIndex > HighestUnlockedLevel && nextIndex < levelSceneNames.Length)
        {
            HighestUnlockedLevel = nextIndex;
        }
    }

    public bool IsLevelUnlocked(int index)
    {
        return index <= HighestUnlockedLevel;
    }

    public int TotalLevels => levelSceneNames != null ? levelSceneNames.Length : 0;
}
