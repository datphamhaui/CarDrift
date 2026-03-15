using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    public Text scoreText;

    public int CurrentScore { get; private set; }
    public int CoinCount { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddCoinScore(int points)
    {
        CoinCount++;
        CurrentScore += points;
        UpdateUI();
    }

    public void StartScoring()
    {
        CurrentScore = 0;
        CoinCount = 0;
        UpdateUI();
    }

    public void StopScoring()
    {
    }

    public void SaveHighScore()
    {
        int levelIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        string key = "HighScore_Level" + levelIndex;
        int highScore = PlayerPrefs.GetInt(key, 0);

        if (CurrentScore > highScore)
        {
            PlayerPrefs.SetInt(key, CurrentScore);
            PlayerPrefs.Save();
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = CurrentScore.ToString();
    }
}
