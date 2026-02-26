using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Ready, Playing, GameOver, Win }

    [Header("UI Panels")]
    public GameObject readyPanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject hudPanel;

    public GameState CurrentState { get; private set; } = GameState.Ready;

    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnGameWin;

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
        SetState(GameState.Ready);
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
        SetState(GameState.Playing);
        OnGameStart?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.GameOver);
        OnGameOver?.Invoke();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.StopScoring();
    }

    public void TriggerWin()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.Win);
        OnGameWin?.Invoke();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.StopScoring();
            ScoreManager.Instance.SaveHighScore();
        }

        if (LevelManager.Instance != null)
            LevelManager.Instance.UnlockNextLevel();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadNextLevel();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    void SetState(GameState newState)
    {
        CurrentState = newState;

        if (readyPanel != null) readyPanel.SetActive(newState == GameState.Ready);
        if (gameOverPanel != null) gameOverPanel.SetActive(newState == GameState.GameOver);
        if (winPanel != null) winPanel.SetActive(newState == GameState.Win);
        if (hudPanel != null) hudPanel.SetActive(newState == GameState.Playing);
    }
}
