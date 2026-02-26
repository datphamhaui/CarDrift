using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public float driftScorePerSecond = 10f;
    public float comboMultiplierIncreaseRate = 0.5f;
    public float maxComboMultiplier = 5f;

    [Header("UI")]
    public Text scoreText;
    public Text comboText;
    public Text highScoreText;
    public Text finalScoreText;

    public int CurrentScore { get; private set; }
    public float ComboMultiplier { get; private set; } = 1f;

    float driftScoreAccumulator;
    bool isScoring;
    bool wasDrifting;

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

        if (highScoreText != null)
        {
            int levelIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            int highScore = PlayerPrefs.GetInt("HighScore_Level" + levelIndex, 0);
            highScoreText.text = highScore.ToString();
        }
    }

    void Update()
    {
        if (!isScoring) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        // Find the car controller to check drift state
        var car = FindAnyObjectByType<PrometeoCarController>();
        if (car == null) return;

        if (car.isDrifting)
        {
            // Increase combo while drifting
            if (wasDrifting)
            {
                ComboMultiplier = Mathf.Min(ComboMultiplier + comboMultiplierIncreaseRate * Time.deltaTime, maxComboMultiplier);
            }

            // Accumulate drift score
            driftScoreAccumulator += driftScorePerSecond * ComboMultiplier * Time.deltaTime;

            if (driftScoreAccumulator >= 1f)
            {
                int points = Mathf.FloorToInt(driftScoreAccumulator);
                CurrentScore += points;
                driftScoreAccumulator -= points;
            }

            wasDrifting = true;
        }
        else
        {
            // Reset combo when not drifting
            if (wasDrifting)
            {
                ComboMultiplier = 1f;
            }
            wasDrifting = false;
        }

        UpdateUI();
    }

    public void StartScoring()
    {
        isScoring = true;
        CurrentScore = 0;
        ComboMultiplier = 1f;
        driftScoreAccumulator = 0f;
    }

    public void StopScoring()
    {
        isScoring = false;

        if (finalScoreText != null)
            finalScoreText.text = CurrentScore.ToString();
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

        if (comboText != null)
        {
            if (ComboMultiplier > 1.1f)
                comboText.text = "x" + ComboMultiplier.ToString("F1");
            else
                comboText.text = "";
        }
    }
}
