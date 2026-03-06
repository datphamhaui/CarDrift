using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Ready, Playing, Paused, GameOver, Win }

    [Header("UI Panels")]
    public GameObject readyPanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;

    [Header("HP")]
    public int maxHP = 3;

    public GameState CurrentState { get; private set; } = GameState.Ready;
    public bool IsPaused => CurrentState == GameState.Paused;
    public int CurrentHP { get; private set; }

    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnGameWin;
    public event Action OnGamePause;
    public event Action OnGameResume;
    public event Action<int, int> OnHPChanged; // (currentHP, maxHP)

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
        Time.timeScale = 1f;
        SetState(GameState.Ready);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayMusic(AudioManager.instance.gameMusic);
    }

    void Update()
    {
        if (CurrentState == GameState.Ready && Input.anyKeyDown)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        CurrentHP = maxHP;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
        SetState(GameState.Playing);
        OnGameStart?.Invoke();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.Paused);
        Time.timeScale = 0f;
        OnGamePause?.Invoke();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;

        SetState(GameState.Playing);
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
    }

    public void TakeDamage(int damage = 1)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        OnHPChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0)
            TriggerGameOver();
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.GameOver);
        OnGameOver?.Invoke();

        if (AudioManager.instance != null)
            AudioManager.instance.PlayLose();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.StopScoring();
    }

    public void TriggerWin()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.Win);
        OnGameWin?.Invoke();

        if (AudioManager.instance != null)
            AudioManager.instance.PlayWin();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.StopScoring();
            ScoreManager.Instance.SaveHighScore();
        }

        if (ProgressManager.instance != null)
        {
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            ProgressManager.instance.CompleteLevel(currentLevel);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            LoadSelectScene();
    }

    public void LoadSelectScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SelectScene");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    void SetState(GameState newState)
    {
        CurrentState = newState;

        if (readyPanel != null)    readyPanel.SetActive(newState == GameState.Ready);
        if (hudPanel != null)      hudPanel.SetActive(newState == GameState.Playing);
        if (pausePanel != null)    pausePanel.SetActive(newState == GameState.Paused);
        if (gameOverPanel != null) gameOverPanel.SetActive(newState == GameState.GameOver);
        if (winPanel != null)      winPanel.SetActive(newState == GameState.Win);
    }
}
